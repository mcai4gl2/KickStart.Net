using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    partial class LocalCache<K, V>
    {
        internal class Segment<K, V>
        {
            readonly LocalCache<K, V> _map;
            volatile int _count;
            long _totalWeight;
            int _modCount;
            int _threshold;
            volatile IReferenceEntry<K, V>[] _table;
            readonly long _maxSegmentWeight;
            volatile int _readCount;
            ConcurrentQueue<IReferenceEntry<K, V>> _recencyQueue;
            WriteQueue<K, V> _writeQueue;
            AccessQueue<K, V> _accessQueue;
            readonly IStatsCounter _statsCounter;
            private readonly object _lock;
            public long Count => _count;
            public int ModCount => _modCount;
            public IReferenceEntry<K, V>[] Table => _table;
            public IStatsCounter StatsCounter => _statsCounter;

            [VisibleForTesting]
            internal AccessQueue<K, V> AccessQueue => _accessQueue;

            [VisibleForTesting]
            internal WriteQueue<K, V> WriteQueue => _writeQueue;

            [VisibleForTesting]
            internal long MaxSegmentWeight => _maxSegmentWeight;

            public Segment(LocalCache<K, V> map, int initialCapacity, long maxSegmentWeight, IStatsCounter statsCounter)
            {
                _map = map;
                _maxSegmentWeight = maxSegmentWeight;
                _statsCounter = statsCounter;
                _lock = new object();
                InitTable(new IReferenceEntry<K, V>[initialCapacity]);
                
            }

            private void InitTable(IReferenceEntry<K, V>[] newTable)
            {
                _threshold = newTable.Length * 3 / 4;
                if (!_map.CustomWeigher() && _threshold == _maxSegmentWeight)
                    _threshold++;
                _table = newTable;

                _recencyQueue = new ConcurrentQueue<IReferenceEntry<K, V>>();
                _accessQueue = new AccessQueue<K, V>();
                _writeQueue = new WriteQueue<K, V>();
            }

            private IReferenceEntry<K, V> NewEntry(K key, int hash, IReferenceEntry<K, V> next)
            {
                // For now only use strong entry, worry about weak entry later on
                return new StrongAccessWriteEntry<K, V>(key, hash, next);
            }

            private IReferenceEntry<K, V> CopyEntry(Segment<K, V> segment, IReferenceEntry<K, V> original, IReferenceEntry<K, V> newNext)
            {
                var newEntry = NewEntry(original.Key, original.Hash, newNext);
                return newEntry;
            }

            private IReferenceEntry<K, V> CopyEntry(IReferenceEntry<K, V> original, IReferenceEntry<K, V> newNext)
            {
                if (original.Key == null) return null; // key collected
                var valueReference = original.ValueReference;
                var value = valueReference.Value;
                if ((value == null) && valueReference.IsActive()) return null; // value collected
                var newEntry = CopyEntry(this, original, newNext);

                newEntry.AccessTime = original.AccessTime;
                Queues.ConnectAccessOrder(original.PreviousInAccessQueue, newEntry);
                Queues.ConnectAccessOrder(newEntry, original.NextInAccessQueue);
                Queues.NullifyAccessOrder(original);
                newEntry.WriteTime = original.WriteTime;
                Queues.ConnectWriteOrder(original.PreviousInWriteQueue, newEntry);
                Queues.ConnectWriteOrder(newEntry, original.NextInWriteQueue);
                Queues.NullifyWriteOrder(original);

                newEntry.ValueReference = valueReference.Copy(value, newEntry);
                return newEntry;
            }

            void SetValue(IReferenceEntry<K, V> entry, K key, V value, long now)
            {
                var previous = entry.ValueReference;
                int weight = _map._weigher.Weigh(key, value);
                Contract.Assert(weight >= 0);
                var valueReference = weight == 1 ? new StrongValueReference<K, V>(value) : new WeightedStrongValueReference<K, V>(value, weight); // Only use strong reference for now, not even weighted one
                entry.ValueReference = valueReference;
                RecordWrite(entry, weight, now);
                previous.NotifyNewValue(value);
            }

            public V Get(K key, int hash, ICacheLoader<K, V> loader)
            {
                Contract.Assert(key != null);
                Contract.Assert(loader != null);
                try
                {
                    if (_count != 0)
                    {
                        var e = GetEntry(key, hash);
                        if (e != null)
                        {
                            var now = _map._ticker.Read();
                            var value = GetLiveValue(e, now);
                            if (value != null)
                            {
                                RecordRead(e, now);
                                _statsCounter.RecordHits(1);
                                return ScheduleRefresh(e, key, hash, value, now, loader);
                            }
                            var valueReference = e.ValueReference;
                            if (valueReference.IsLoading())
                            {
                                return WaitForLoadingValue(e, key, valueReference);
                            }
                        }
                    }

                    // At this point e is either null or expired
                    return LockedGetOrLoad(key, hash, loader);
                }
                finally
                {
                    PostReadCleanup();
                }
            }

            public V Get(K key, int hash)
            {
                try
                {
                    if (_count != 0)
                    {
                        var now = _map._ticker.Read();
                        var e = GetLiveEntry(key, hash, now);
                        if (e == null) return default(V);
                        var value = e.ValueReference.Value;
                        if (value != null)
                        {
                            RecordRead(e, now);
                            return ScheduleRefresh(e, e.Key, hash, value, now, _map._defaultLoader);
                        }
                        TryDrainReferenceQueues();
                    }
                    return default(V);
                }
                finally
                {
                    PostReadCleanup();
                }
            }

            V LockedGetOrLoad(K key, int hash, ICacheLoader<K, V> loader)
            {
                IReferenceEntry<K, V> e;
                IValueReference<K, V> valueReference = null;
                LoadingValueReference<K, V> loadingValueReference = null;
                bool createNewEntry = true;

                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var newCount = _count - 1;
                    IReferenceEntry<K, V>[] table = _table;
                    int index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            valueReference = e.ValueReference;
                            if (valueReference.IsLoading())
                                createNewEntry = false;
                            else
                            {
                                var value = valueReference.Value;
                                if (value == null)
                                {
                                    EnqueueNotification(entryKey, hash, value, valueReference.Weight, RemovalCause.Collected);
                                }
                                else if (_map.IsExpired(e, now))
                                {
                                    EnqueueNotification(entryKey, hash, value, valueReference.Weight, RemovalCause.Expired);
                                }
                                else
                                {
                                    RecordLockedRead(e, now);
                                    _statsCounter.RecordHits(1);
                                    return value;
                                }

                                _writeQueue.Remove(e);
                                _accessQueue.Remove(e);
                                _count = newCount;
                            }
                            break;
                        }
                    }

                    if (createNewEntry)
                    {
                        loadingValueReference = new LoadingValueReference<K, V>();
                        if (e == null)
                        {
                            e = NewEntry(key, hash, first);
                            e.ValueReference = loadingValueReference;
                            Volatile.Write(ref table[index], e);
                        }
                        else
                        {
                            e.ValueReference = loadingValueReference;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }

                if (createNewEntry)
                {
                    try
                    {
                        lock (e)
                        {
                            return LoadSync(key, hash, loadingValueReference, loader);
                        }
                    }
                    finally
                    {
                        _statsCounter.RecordMisses(1);
                    }
                }
                else
                {
                    return WaitForLoadingValue(e, key, valueReference);
                }
            }

            internal void PostReadCleanup()
            {
                if ((Interlocked.Increment(ref _readCount) & _map._drainThreshold) == 0)
                    CleanUp();
            }

            public void CleanUp()
            {
                var now = _map._ticker.Read();
                RunLockedCleanup(now);
                RunUnlockedCleanup();
            }

            V WaitForLoadingValue(IReferenceEntry<K, V> e, K key, IValueReference<K, V> valueReference)
            {
                try
                {
                    var value = valueReference.WaitForValue().Result;
                    if (value == null)
                        throw new Exception($"CacheLoader returned null for key {key}.");
                    var now = _map._ticker.Read();
                    RecordRead(e, now);
                    return value;
                }
                finally
                {
                    _statsCounter.RecordMisses(1);
                }
            }

            V ScheduleRefresh(IReferenceEntry<K, V> entry, K key, int hash, V oldValue, long now, ICacheLoader<K, V> loader)
            {
                if (_map.Refreshes() && (now - entry.WriteTime) > _map._refreshTicks && !entry.ValueReference.IsLoading())
                {
                    var newValue = Refresh(key, hash, loader, true);
                    if (newValue != null)
                    {
                        return newValue;
                    }
                }
                return oldValue;
            }

            public V Refresh(K key, int hash, ICacheLoader<K, V> loader, bool checkTime)
            {
                var loadingValueReference = InsertLoadingValueReference(key, hash, checkTime);
                if (loadingValueReference == null)
                    return default(V);
                var result = LoadAsync(key, hash, loadingValueReference, loader);
                if (result.IsCompleted)
                {
                    return result.Result;
                }
                return default(V);
            }

            V LoadSync(K key, int hash, LoadingValueReference<K, V> loadingValueReference, ICacheLoader<K, V> loader)
            {
                var task = loadingValueReference.LoadAsync(key, loader);
                return GetAndRecordStats(key, hash, loadingValueReference, task.Result);
            }

            V GetAndRecordStats(K key, int hash, LoadingValueReference<K, V> loadingValueReference, Task<V> task)
            {
                var result = default(V);
                try
                {
                    result = task.Result;
                    if (result == null)
                    {
                        throw new Exception($"CacheLoader returned null for key {key}.");
                    }
                    _statsCounter.RecordLoadSuccess(0); // TODO: store load time
                    StoreLoadedValue(key, hash, loadingValueReference, result);
                    return result;
                }
                finally
                {
                    if (result == null)
                    {
                        _statsCounter.RecordLoadException(0);
                        RemoveLoadingValue(key, hash, loadingValueReference);
                    }
                }
            }

            Task<V> LoadAsync(K key, int hash, LoadingValueReference<K, V> loadingValueReference, ICacheLoader<K, V> loader)
            {
                var task = loadingValueReference.LoadAsync(key, loader);
                task.Wait();
                return Task.Factory.StartNew(() => GetAndRecordStats(key, hash, loadingValueReference, task.Result));
            }

            bool StoreLoadedValue(K key, int hash, LoadingValueReference<K, V> oldValueReference, V newValue)
            {
                Monitor.Enter(_lock);
                try
                {
                    long now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    int newCount = this._count + 1;
                    if (newCount > this._threshold)
                    {
                        Expand();
                        newCount = this._count + 1;
                    }

                    var table = this._table;
                    int index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;
                            if (oldValueReference == valueReference ||
                                (entryValue == null && !(valueReference is UnSetValueReference<K, V>)))
                            {
                                ++_modCount;
                                if (oldValueReference.IsActive())
                                {
                                    var cause = entryValue == null ? RemovalCause.Collected : RemovalCause.Replaced;
                                    EnqueueNotification(key, hash, entryValue, oldValueReference.Weight, cause);
                                    newCount--;
                                }
                                SetValue(e, key, newValue, now);
                                this._count = newCount;
                                EvictEntries(e);
                                return true;
                            }

                            EnqueueNotification(key, hash, newValue, 0, RemovalCause.Replaced);
                        }
                    }

                    ++_modCount;
                    var newEntry = NewEntry(key, hash, first);
                    SetValue(newEntry, key, newValue, now);
                    Volatile.Write(ref table[index], newEntry);
                    _count = newCount;
                    EvictEntries(newEntry);
                    return true;
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            bool RemoveLoadingValue(K key, int hash, LoadingValueReference<K, V> valueReference)
            {
                Monitor.Enter(_lock);
                try
                {
                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var v = e.ValueReference;
                            if (v == valueReference)
                            {
                                if (valueReference.IsActive())
                                    e.ValueReference = valueReference.OldValue;
                                else
                                {
                                    var newFirst = RemoveEntryFromChain(first, e);
                                    Volatile.Write(ref table[index], newFirst);
                                }
                                return true;
                            }
                            return false;
                        }
                    }

                    return false;
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            void EvictEntries(IReferenceEntry<K, V> newest)
            {
                if (!_map.EvictsBySize()) return;

                DrainRecencyQueue();

                if (newest.ValueReference.Weight > _maxSegmentWeight)
                {
                    if (!RemoveEntry(newest, newest.Hash, RemovalCause.Size))
                    {
                        Contract.Assert(false);
                    }
                }

                while (_totalWeight > _maxSegmentWeight)
                {
                    var e = NextEvictable();
                    if (!RemoveEntry(e, e.Hash, RemovalCause.Size))
                    {
                        Contract.Assert(false);
                    }
                }
            }

            IReferenceEntry<K, V> NextEvictable()
            {
                foreach (var e in _accessQueue)
                {
                    var weight = e.ValueReference.Weight;
                    if (weight > 0) return e;
                }
                throw new Exception();
            }

            public bool ContainsKey(K key, int hash)
            {
                try
                {
                    if (_count != 0)
                    {
                        var now = _map._ticker.Read();
                        var e = GetLiveEntry(key, hash, now);
                        if (e == null) return false;
                        return e.ValueReference.Value != null;
                    }
                    return false;
                }
                finally
                {
                    PostReadCleanup();
                }
            }

            public bool ContainsValue(K value)
            {
                try
                {
                    if (_count != 0)
                    {
                        var now = _map._ticker.Read();
                        var table = _table;
                        var length = table.Length;
                        for (var i = 0; i < length; ++i)
                        {
                            for (var e = Volatile.Read(ref table[i]); e != null; e = e.Next)
                            {
                                var entryValue = GetLiveValue(e, now);
                                if (entryValue == null) continue;
                                if (entryValue.Equals(value))
                                    return true;
                            }
                        }
                    }
                    return false;
                }
                finally
                {
                    PostReadCleanup();
                }
            }

            public V Put(K key, int hash, V value, bool onlyIfAbsent)
            {
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var newCount = _count + 1;
                    if (newCount > _threshold)
                    {
                        Expand();
                        newCount = _count + 1;
                    }

                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && entryKey.Equals(key))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;

                            if (entryValue == null)
                            {
                                ++_modCount;
                                if (valueReference.IsActive())
                                {
                                    EnqueueNotification(key, hash, entryValue, valueReference.Weight,
                                        RemovalCause.Collected);
                                    SetValue(e, key, value, now);
                                    newCount = _count;
                                }
                                else
                                {
                                    SetValue(e, key, value, now);
                                    newCount = _count + 1;
                                }
                                _count = newCount;
                                EvictEntries(e);
                                return default(V);
                            }
                            else if (onlyIfAbsent)
                            {
                                RecordLockedRead(e, now);
                                return entryValue;
                            }
                            else
                            {
                                ++_modCount;
                                EnqueueNotification(key, hash, entryValue, valueReference.Weight, RemovalCause.Replaced);
                                SetValue(e, key, value, now);
                                EvictEntries(e);
                                return entryValue;
                            }
                        }
                    }
                    ++_modCount;
                    var newEntry = NewEntry(key, hash, first);
                    SetValue(newEntry, key, value, now);
                    Volatile.Write(ref table[index], newEntry);
                    newCount = _count + 1;
                    _count = newCount;
                    EvictEntries(newEntry);
                    return default(V);
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            void Expand()
            {
                var oldTable = _table;
                int oldCapacity = oldTable.Length;
                if (oldCapacity >= _map._maximumCapacity) return;
                int newCount = _count;
                var newTable = new IReferenceEntry<K, V>[oldCapacity << 1];
                _threshold = newTable.Length * 3 / 4;
                int newMask = newTable.Length - 1;
                for (int oldIndex = 0; oldIndex < oldCapacity; ++oldIndex)
                {
                    var head = Volatile.Read(ref oldTable[oldIndex]);

                    if (head != null)
                    {
                        var next = head.Next;
                        int headIndex = head.Hash & newMask;
                        if (next == null)
                        {
                            Volatile.Write(ref newTable[headIndex], head);
                        }
                        else
                        {
                            var tail = head;
                            int tailIndex = headIndex;
                            for (var e = next; e != null; e = e.Next)
                            {
                                int newIndex = e.Hash & newMask;
                                if (newIndex != tailIndex)
                                {
                                    tailIndex = newIndex;
                                    tail = e;
                                }
                            }
                            Volatile.Write(ref newTable[tailIndex], tail);

                            for (var e = head; e != tail; e = e.Next)
                            {
                                int newIndex = e.Hash & newMask;
                                var newNext = Volatile.Read(ref newTable[newIndex]);
                                var newFirst = CopyEntry(e, newNext);
                                if (newFirst != null)
                                    Volatile.Write(ref newTable[newIndex], newFirst);
                                else
                                {
                                    RemoveCollectedEntry(e);
                                    newCount--;
                                }
                            }
                        }
                    }
                }
                _table = newTable;
                _count = newCount;
            }

            public bool Replace(K key, int hash, V oldValue, V newValue)
            {
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;
                            if (entryValue == null)
                            {
                                // If the value disappeared, this entry is partially collected.
                                if (valueReference.IsActive())
                                {
                                    ++_modCount;
                                    var newFirst = RemoveValueFromChain(first, e, entryKey, hash, entryValue,
                                        valueReference, RemovalCause.Collected);
                                    var newCount = _count - 1;
                                    Volatile.Write(ref table[index], newFirst);
                                    _count = newCount;
                                }
                                return false;
                            }

                            if (oldValue.Equals(entryValue))
                            {
                                ++_modCount;
                                EnqueueNotification(key, hash, entryValue, valueReference.Weight, RemovalCause.Replaced);
                                SetValue(e, key, newValue, now);
                                EvictEntries(e);
                                return true;
                            }
                            else
                            {
                                RecordLockedRead(e, now);
                                return false;
                            }
                        }
                    }
                    return false;
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            public V Replace(K key, int hash, V newValue)
            {
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;
                            if (entryValue == null)
                            {
                                // If the value disappeared, this entry is partially collected.
                                if (valueReference.IsActive())
                                {
                                    var newCount = _count - 1;
                                    ++_modCount;
                                    var newFirst = RemoveValueFromChain(first, e, entryKey, hash, entryValue,
                                        valueReference, RemovalCause.Collected);
                                    newCount = _count - 1;
                                    Volatile.Write(ref table[index], newFirst);
                                    _count = newCount;
                                }
                                return default(V);
                            }

                            ++_modCount;
                            EnqueueNotification(key, hash, entryValue, valueReference.Weight, RemovalCause.Replaced);
                            SetValue(e, key, newValue, now);
                            EvictEntries(e);
                            return entryValue;
                        }
                    }
                    return default(V);
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            public V Remove(K key, int hash)
            {
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var newCount = _count - 1;
                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;

                            RemovalCause cause;
                            if (entryValue != null)
                                cause = RemovalCause.Explicit;
                            else if (valueReference.IsActive())
                                cause = RemovalCause.Collected;
                            else
                                return default(V);

                            ++_modCount;
                            var newFirst = RemoveValueFromChain(first, e, entryKey, hash, entryValue, valueReference, cause);
                            newCount = _count - 1;
                            Volatile.Write(ref table[index], newFirst);
                            _count = newCount;
                            return entryValue;
                        }
                    }

                    return default(V);
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            public bool Remove(K key, int hash, V value)
            {
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    var newCount = _count - 1;
                    var table = _table;
                    var index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref table[index]);

                    for (var e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            var entryValue = valueReference.Value;

                            RemovalCause cause;
                            if (value.Equals(entryValue))
                                cause = RemovalCause.Explicit;
                            else if (entryValue == null && valueReference.IsActive())
                                cause = RemovalCause.Collected;
                            else
                                return false; // currently loading

                            ++_modCount;
                            var newFirst = RemoveValueFromChain(first, e, entryKey, hash, entryValue, valueReference, cause);
                            newCount = _count - 1;
                            Volatile.Write(ref table[index], newFirst);
                            _count = newCount;
                            return cause == RemovalCause.Explicit;
                        }
                    }

                    return false;
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            public void Clear()
            {
                if (_count != 0)
                {
                    Monitor.Enter(_lock);
                    try
                    {
                        var now = _map._ticker.Read();
                        PreWriteCleanup(now);

                        var table = _table;
                        for (var i = 0; i < table.Length; ++i)
                        {
                            for (var e = Volatile.Read(ref table[i]); e != null; e = e.Next)
                            {
                                if (e.ValueReference.IsActive())
                                {
                                    var key = e.Key;
                                    var value = e.ValueReference.Value;
                                    var cause = (key == null || value == null) ? RemovalCause.Collected : RemovalCause.Explicit;
                                    EnqueueNotification(key, e.Hash, value, e.ValueReference.Weight, cause);
                                }
                            }
                        }
                        for (var i = 0; i < table.Length; ++i)
                            Volatile.Write(ref table[i], null);
                        ClearReferenceQueues();
                        _writeQueue.Clear();
                        _accessQueue.Clear();
                        _readCount = 0;
                        ++_modCount;
                        _count = 0;
                    }
                    finally
                    {
                        Monitor.Exit(_lock);
                        PostWriteCleanup();
                    }
                }
            }

            void ClearReferenceQueues()
            {
                // Not really need this
            }

            LoadingValueReference<K, V> InsertLoadingValueReference(K key, int hash, bool checkTime)
            {
                IReferenceEntry<K, V> e = null;
                Monitor.Enter(_lock);
                try
                {
                    var now = _map._ticker.Read();
                    PreWriteCleanup(now);

                    IReferenceEntry<K, V>[] table = this._table;
                    int index = hash & (table.Length - 1);
                    var first = Volatile.Read(ref _table[index]);

                    for (e = first; e != null; e = e.Next)
                    {
                        var entryKey = e.Key;
                        if (e.Hash == hash && entryKey != null && key.Equals(entryKey))
                        {
                            var valueReference = e.ValueReference;
                            if (valueReference.IsLoading() || (checkTime && (now - e.WriteTime < _map._refreshTicks)))
                                return null;
                            ++_modCount;
                            var reference = new LoadingValueReference<K, V>(valueReference);
                            e.ValueReference = reference;
                            return reference;
                        }
                    }

                    ++_modCount;
                    var loadingValueReference = new LoadingValueReference<K, V>();
                    e = NewEntry(key, hash, first);
                    e.ValueReference = loadingValueReference;
                    Volatile.Write(ref table[index], e);
                    return loadingValueReference;
                }
                finally
                {
                    Monitor.Exit(_lock);
                    PostWriteCleanup();
                }
            }

            void PreWriteCleanup(long now)
            {
                RunLockedCleanup(now);
            }

            void PostWriteCleanup()
            {
                RunUnlockedCleanup();
            }

            void RunLockedCleanup(long now)
            {
                if (Monitor.TryEnter(_lock))
                {
                    try
                    {
                        DrainReferenceQueues();
                        ExpireEntries(now);
                        _readCount = 0;
                    }
                    finally
                    {
                        Monitor.Exit(_lock);
                    }
                }
            }

            void RunUnlockedCleanup()
            {
                if (!Monitor.IsEntered(_lock))
                    _map.ProcessPendingNotifications();
            }

            void RecordWrite(IReferenceEntry<K, V> entry, int weight, long now)
            {
                DrainRecencyQueue();
                _totalWeight += weight;
                
                if (_map.RecordsAccess())
                    entry.AccessTime = now;
                if (_map.RecordsWrite())
                    entry.WriteTime = now;
                _accessQueue.Offer(entry);
                _writeQueue.Offer(entry);
            }

            void RecordRead(IReferenceEntry<K, V> entry, long now)
            {
                entry.AccessTime = now;
                _recencyQueue.Enqueue(entry);
            }

            void RecordLockedRead(IReferenceEntry<K, V> entry, long now)
            {
                entry.AccessTime = now;
                _accessQueue.Offer(entry);
            }

            [VisibleForTesting]
            internal void DrainRecencyQueue()
            {
                IReferenceEntry<K, V> entry;
                while (_recencyQueue.TryDequeue(out entry))
                {
                    if (_accessQueue.Contains(entry))
                        _accessQueue.Offer(entry);
                }
            }

            IReferenceEntry<K, V> GetFirst(int hash)
            {
                var table = _table; // volatile read, keep local copy
                return Volatile.Read(ref table[hash & (_table.Length - 1)]);
            }

            IReferenceEntry<K, V> GetEntry(K key, int hash)
            {
                for (var e = GetFirst(hash); e != null; e = e.Next)
                {
                    if (e.Hash != hash) continue;
                    var entryKey = e.Key;
                    if (entryKey == null)
                    {
                        TryDrainReferenceQueues();
                        continue;
                    }

                    if (key.Equals(entryKey)) // needs to use IEqualityComparer later
                    {
                        return e;
                    }
                }
                return null;
            }

            IReferenceEntry<K, V> GetLiveEntry(K key, int hash, long now)
            {
                var e = GetEntry(key, hash);
                if (e == null) return null;
                if (_map.IsExpired(e, now))
                {
                    TryExpireEntries(now);
                    return null;
                }
                return e;
            }

            public V GetLiveValue(IReferenceEntry<K, V> entry, long now)
            {
                if (entry.Key == null)
                    return default(V);
                var value = entry.ValueReference.Value;
                if (value == null)
                    return default(V);
                if (_map.IsExpired(entry, now))
                {
                    TryExpireEntries(now);
                    return default(V);
                }
                return value;
            }

            void TryDrainReferenceQueues()
            {
                if (Monitor.TryEnter(_lock))
                {
                    try
                    {
                        DrainReferenceQueues();
                    }
                    finally
                    {
                        Monitor.Exit(_lock);
                    }
                }
            }

            void DrainReferenceQueues()
            {
                // No need to do anything as we don't deal with weak references now and more importantly, C# doesn't support reference queue
                // on weak reference
            }

            void TryExpireEntries(long now)
            {
                if (Monitor.TryEnter(_lock))
                {
                    try
                    {
                        ExpireEntries(now);
                    }
                    finally
                    {
                        Monitor.Exit(_lock);
                    }
                }
            }

            [VisibleForTesting]
            internal void ExpireEntries(long now)
            {
                DrainRecencyQueue();

                IReferenceEntry<K, V> e;
                while ((e = _writeQueue.Peek()) != null && _map.IsExpired(e, now))
                {
                    if (!RemoveEntry(e, e.Hash, RemovalCause.Expired))
                    {
                        Contract.Assert(false);
                    }
                }
                while ((e = _accessQueue.Peek()) != null && _map.IsExpired(e, now))
                {
                    if (!RemoveEntry(e, e.Hash, RemovalCause.Expired))
                    {
                        Contract.Assert(false);
                    }
                }
            }

            bool RemoveEntry(IReferenceEntry<K, V> entry, int hash, RemovalCause cause)
            {
                int newCount = _count - 1;
                var table = _table;
                int index = hash & (_table.Length - 1);
                var first = Volatile.Read(ref table[index]);

                for (var e = table[index]; e != null; e = e.Next)
                {
                    if (e == entry)
                    {
                        ++_modCount;
                        var newFirst = RemoveValueFromChain(first, e, e.Key, hash, e.ValueReference.Value,
                            e.ValueReference, cause);
                        newCount = _count - 1;
                        Volatile.Write(ref table[index], newFirst);
                        _count = newCount;
                        return true;
                    }
                }

                return false;
            }

            IReferenceEntry<K, V> RemoveValueFromChain(IReferenceEntry<K, V> first, IReferenceEntry<K, V> entry, K key,
                int hash, V value, IValueReference<K, V> valueReference, RemovalCause cause)
            {
                EnqueueNotification(key, hash, value, valueReference.Weight, cause);
                _writeQueue.Remove(entry);
                _accessQueue.Remove(entry);
                if (valueReference.IsLoading())
                {
                    valueReference.NotifyNewValue(default(V));
                    return first;
                }
                else
                {
                    return RemoveEntryFromChain(first, entry);
                }
            }

            IReferenceEntry<K, V> RemoveEntryFromChain(IReferenceEntry<K, V> first, IReferenceEntry<K, V> entry)
            {
                int newCount = _count;
                var newFirst = entry.Next;
                for (var e = first; e != entry; e = e.Next)
                {
                    var next = CopyEntry(e, newFirst);
                    if (next != null)
                        newFirst = next;
                    else
                    {
                        RemoveCollectedEntry(e);
                        newCount--;
                    }
                }
                _count = newCount;
                return newFirst;
            }

            void RemoveCollectedEntry(IReferenceEntry<K, V> entry)
            {
                EnqueueNotification(entry.Key, entry.Hash, entry.ValueReference.Value, entry.ValueReference.Weight, RemovalCause.Collected);
                _writeQueue.Remove(entry);
                _accessQueue.Remove(entry);
            }

            void EnqueueNotification(K key, int hash, V value, int weight, RemovalCause cause)
            {
                _totalWeight -= weight;
                if (cause.WasEvicted())
                    _statsCounter.RecordEviction();
                if (_map._removalConcurrentQueue != null)
                {
                    var notification = RemovalNotification<K, V>.Create(key, value, cause);
                    _map._removalConcurrentQueue.Enqueue(notification);
                }
            }
        }
    }
}
