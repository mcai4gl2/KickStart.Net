using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using KickStart.Net.Extensions;

namespace KickStart.Net.Cache
{
    partial class LocalCache<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private static readonly int UNSET_INT = -1;

        private readonly int _maximumCapacity = 1 << 30;
        private readonly int _drainThreshold = 0x3F;
        private readonly int _containsValueRetries = 3;
        private readonly int _maxSegments = 1 << 16;
        private readonly int _drainMax = 16;

        private readonly int _segmentMask;
        private readonly int _segmentShift;
        private readonly Segment<K, V>[] _segments;

        private readonly int _concurrencyLevel;
        private readonly long _maxWeight;

        private readonly long _expireAfterAccessTicks;
        private readonly long _expireAfterWriteTicks;
        private long _refreshTicks;
        private IWeigher<K, V> _weigher;
        private ConcurrentQueue<RemovalNotification<K, V>> _removalConcurrentQueue = new ConcurrentQueue<RemovalNotification<K, V>>();
        private readonly IRemovalListener<K, V> _removalListener;
        private ITicker _ticker;

        private readonly IStatsCounter _globalStatsCounter;
        private readonly ICacheLoader<K, V> _defaultLoader;

        public IStatsCounter GlobalStatsCounter => _globalStatsCounter;
        public Segment<K, V>[] Segments => _segments;

        public LocalCache(CacheBuilder<K, V> builder, ICacheLoader<K, V> loader)
        {
            _concurrencyLevel = Math.Min(builder.ConcurrencyLevel, _maxSegments);
            _maxWeight = builder.MaximumWeight;
            _weigher = builder.Weighter;
            _expireAfterWriteTicks = builder.ExpireAfterWriteTicks;
            _expireAfterAccessTicks = builder.ExpireAfterAccessTicks;
            _refreshTicks = builder.RefreshAfterWrite;

            _removalListener = builder.RemovalListener;

            _ticker = builder.Ticker(RecordsTime());
            _globalStatsCounter = builder.IsRecordingStats ? (IStatsCounter)new SimpleStatsCounter() : NullStatsCounter.Instance;
            _defaultLoader = loader;

            var initialCapacity = Math.Min(builder.InitialCapacity, _maximumCapacity);
            if (EvictsBySize() && !CustomWeigher())
                initialCapacity = Math.Min(initialCapacity, (int)_maxWeight);

            var segmentShift = 0;
            var segmentCount = 1;
            while (segmentCount < _concurrencyLevel && (!EvictsBySize() || segmentCount*20 <= _maxWeight))
            {
                ++segmentShift;
                segmentCount <<= 1;
            }
            _segmentShift = 32 - segmentShift;
            _segmentMask = segmentCount - 1;

            _segments = NewSegmentArray(segmentCount);

            var segmentCapacity = initialCapacity/segmentCount;
            if (segmentCapacity*segmentCount < initialCapacity)
                ++segmentCapacity;

            var segmentSize = 1;
            while (segmentSize < segmentCapacity)
                segmentSize <<= 1;

            if (EvictsBySize())
            {
                var maxSegmentWeight = _maxWeight/segmentCount + 1;
                var remainder = _maxWeight%segmentCount;
                for (var i = 0; i < _segments.Length; ++i)
                {
                    if (i == remainder)
                        maxSegmentWeight--;
                    _segments[i] = CreateSegment(segmentSize, maxSegmentWeight, new SimpleStatsCounter());
                }
            }
            else
            {
                for (var i = 0; i < _segments.Length; ++i)
                {
                    _segments[i] = CreateSegment(segmentSize, UNSET_INT, new SimpleStatsCounter());
                }
            }
        }

        /// <summary>
        /// Gets the value from an entry. Returns null if the entry is invalid, partially-collected,
        /// loading, or expired. Unlike the segment version, this method, does not attempt to cleanup 
        /// stale entiries. It shall only be called outside of a segment during iterator iteration.
        /// </summary>
        V GetLiveValue(IReferenceEntry<K, V> entry, long now)
        {
            if (entry.Key == null)
                return default(V);
            var value = entry.ValueReference.Value;
            if (value == null)
                return default(V);
            if (IsExpired(entry, now))
                return default(V);
            return value;
        }

        Segment<K, V>[] NewSegmentArray(int size)
        {
            return new Segment<K, V>[size];
        }

        Segment<K, V> CreateSegment(int initialCapacity, long maxSegmentWeight, IStatsCounter statsCounter)
        {
            return new Segment<K, V>(this, initialCapacity, maxSegmentWeight, statsCounter);
        }

        Segment<K, V> SegmentFor(int hash)
        {
            return _segments[(hash >> _segmentShift) & _segmentMask];
        } 

        bool EvictsBySize() => _maxWeight >= 0;
        bool CustomWeigher() => !(_weigher is OneWeigher<K, V>);
        bool Expires() => ExpiresAfterWrite() || ExpiresAfterAccess();
        bool ExpiresAfterWrite() => _expireAfterWriteTicks > 0;
        bool ExpiresAfterAccess() => _expireAfterAccessTicks > 0;
        bool Refreshes() => _refreshTicks > 0;
        bool UsesAccessQueue() => ExpiresAfterAccess() || EvictsBySize();
        bool UsesWriteQueue() => ExpiresAfterWrite();
        bool RecordsWrite() => ExpiresAfterWrite() || Refreshes();
        bool RecordsAccess() => ExpiresAfterAccess();
        bool RecordsTime() => RecordsWrite() || RecordsAccess();
        bool UsesWriteEntries() => UsesWriteQueue() || RecordsWrite();
        bool UsesAccessEntries() => UsesAccessQueue() || RecordsAccess();

        bool IsExpired(IReferenceEntry<K, V> entry, long now)
        {
            Contract.Assert(entry != null);
            if (ExpiresAfterAccess() && (now - entry.AccessTime) >= _expireAfterAccessTicks)
                return true;
            if (ExpiresAfterWrite() && (now - entry.WriteTime) >= _expireAfterWriteTicks)
                return true;
            return false;
        }

        static int ReHash(int h)
        {
            h += (int)(h << 15 ^ 0xffffcd7d);
            h ^= (int)((uint)h >> 10);
            h += (h << 3);
            h ^= (int)((uint)h >> 6);
            h += (h << 2) + (h << 14);
            return h ^ (int)((uint)h >> 16);
        }

        internal void ProcessPendingNotifications()
        {
            RemovalNotification<K, V> notification;
            while (_removalConcurrentQueue.TryDequeue(out notification))
            {
                try
                {
                    _removalListener.OnRemoval(notification);
                }
                finally
                {
                    
                }
            }
        }

        public void CleanUp()
        {
            foreach (var segment in _segments)
                segment.CleanUp();
        }

        public bool IsEmpty()
        {
            long sum = 0L;
            Segment<K, V>[] segments = _segments;
            foreach (Segment<K, V> t in segments)
            {
                if (t.Count != 0)
                {
                    return false;
                }
                sum += t.ModCount;
            }

            if (sum != 0L)
            {
                // recheck unless no modifications
                foreach (Segment<K, V> t in segments)
                {
                    if (t.Count != 0)
                    {
                        return false;
                    }
                    sum -= t.ModCount;
                }
                if (sum != 0L)
                {
                    return false;
                }
            }
            return true;
        }

        public long LongSize()
        {
            var segments = _segments;
            long sum = 0;
            for (var i = 0; i < segments.Length; i++)
            {
                sum += Math.Max(0, segments[i].Count); // see https://github.com/google/guava/issues/2108
            }
            return sum;
        }

        public int Size()
        {
            return LongSize().SaturatedCast();
        }

        public V Get(K key)
        {
            if (key == null) return default(V);
            int hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Get(key, hash);
        }

        public V GetIfPresent(K key)
        {
            var hash = ReHash(key.GetHashCode());
            var value = SegmentFor(hash).Get(key, hash);
            if (value == null) 
                _globalStatsCounter.RecordMisses(1);
            else
                _globalStatsCounter.RecordHits(1);
            return value;
        }

        public V GetOrDefault(K key, V defaultValue)
        {
            var result = Get(key);
            return result != null ? result : defaultValue;
        }

        public V Get(K key, ICacheLoader<K, V> loader)
        {
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Get(key, hash, loader);
        }

        public V GetOrLoad(K key)
        {
            return Get(key, _defaultLoader);
        }

        public Dictionary<K, V> GetAllPresent(IEnumerable<K> keys)
        {
            var hits = 0;
            var misses = 0;
            
            var results = new Dictionary<K, V>();
            foreach (var key in keys)
            {
                var value = Get(key);
                if (value == null)
                {
                    misses++;
                    continue;
                }
                hits++;
                results.Add(key, value);
            }
            _globalStatsCounter.RecordHits(hits);
            _globalStatsCounter.RecordMisses(misses);
            return results;
        }

        public Dictionary<K, V> GetAll(IEnumerable<K> keys)
        {
            var hits = 0;
            var misses = 0;

            var results = new Dictionary<K, V>();
            var keysToLoad = new HashSet<K>();
            foreach (var key in keys)
            {
                var value = Get(key);
                if (!results.ContainsKey(key))
                {
                    results.Add(key, value);
                    if (value == null)
                    {
                        misses++;
                        keysToLoad.Add(key);
                    }
                    else hits++;
                }
            }

            try
            {
                if (keysToLoad.Count != 0)
                {
                    try
                    {
                        var newEntries = LoadAll(keysToLoad, _defaultLoader);
                        foreach (var key in keysToLoad)
                        {
                            var value = newEntries[key];
                            if (value == null)
                                throw new Exception($"LoadAll failed to return a vlaue for {key}");
                            results[key] = value;
                        }
                    }
                    catch (NotImplementedException ex)
                    {
                        foreach (K key in keysToLoad)
                        {
                            misses--; // get will count this miss
                            results[key] = Get(key, _defaultLoader);
                        }
                    }
                }

                return results;
            }
            finally
            {
                _globalStatsCounter.RecordHits(hits);
                _globalStatsCounter.RecordMisses(misses);
            }
        }

        public void Refresh(K key)
        {
            var hash = ReHash(key.GetHashCode());
            SegmentFor(hash).Refresh(key, hash, _defaultLoader, false);
        }

        Dictionary<K, V> LoadAll(HashSet<K> keys, ICacheLoader<K, V> loader)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(K key)
        {
            if (key == null) return false;
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).ContainsKey(key, hash);
        }

        public bool ContainsValue(V value)
        {
            if (value == null)
                return false;

            // This implementation is patterned after ConcurrentHashMap, but without the locking. The only
            // way for it to return a false negative would be for the target value to jump around in the map
            // such that none of the subsequent iterations observed it, despite the fact that at every point
            // in time it was present somewhere int the map. This becomes increasingly unlikely as
            // CONTAINS_VALUE_RETRIES increases, though without locking it is theoretically possible.
            var now = _ticker.Read();
            var segments = _segments;
            var last = -1L;
            for (var i = 0; i < _containsValueRetries; i++)
            {
                var sum = 0L;
                foreach (var segment in segments)
                {
                    var unused = segment.Count;
                    var table = segment.Table;
                    for (var j = 0; j < table.Length; j++)
                    {
                        for (var e = Volatile.Read(ref table[j]); e != null; e = e.Next)
                        {
                            var v = segment.GetLiveValue(e, now);
                            if (v != null && value.Equals(v))
                                return true;
                        }
                    }
                    sum += segment.ModCount;
                }
                if (sum == last) break;
                last = sum;
            }
            return false;
        }

        public V Put(K key, V value)
        {
            Contract.Assert(key != null);
            Contract.Assert(value != null);
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Put(key, hash, value, false);
        }

        public V PutIfAbsent(K key, V value)
        {
            Contract.Assert(key != null);
            Contract.Assert(value != null);
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Put(key, hash, value, true);
        }

        public void PutAll(IReadOnlyDictionary<K, V> inputs)
        {
            foreach (var kvp in inputs)
                Put(kvp.Key, kvp.Value);
        }

        public V Remove(K key)
        {
            if (key == null)
                return default(V);

            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Remove(key, hash);
        }

        public bool Remove(K key, V value)
        {
            if (key == null || value == null) return false;
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Remove(key, hash, value);
        }

        public bool Replace(K key, V oldValue, V newValue)
        {
            Contract.Assert(key != null);
            Contract.Assert(newValue != null);
            if (oldValue == null)
                return false;
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Replace(key, hash, oldValue, newValue);
        }

        public V Replace(K key, V value)
        {
            Contract.Assert(key != null);
            Contract.Assert(value != null);
            var hash = ReHash(key.GetHashCode());
            return SegmentFor(hash).Replace(key, hash, value);
        }

        public void Clear()
        {
            foreach (var segment in _segments)
                segment.Clear();
        }

        public void InvalidateAll(IEnumerable<K> keys)
        {
            foreach (var key in keys)
                Remove(key);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new KeyValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
