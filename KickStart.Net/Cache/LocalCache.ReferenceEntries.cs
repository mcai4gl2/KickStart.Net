using System;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    interface IReferenceEntry<K, V>
    {
        IValueReference<K, V> ValueReference { get; set; }
        IReferenceEntry<K, V> Next { get; }
        int Hash { get; }
        K Key { get; }
        long AccessTime { get; set; }
        IReferenceEntry<K, V> NextInAccessQueue { get; set; }
        IReferenceEntry<K, V> PreviousInAccessQueue { get; set; }
        long WriteTime { get; set; }
        IReferenceEntry<K, V> NextInWriteQueue { get; set; }
        IReferenceEntry<K, V> PreviousInWriteQueue { get; set; }
    }

    class StrongEntry<K, V> : IReferenceEntry<K, V>
    {
        private readonly K _key;
        private readonly int _hash;
        private readonly IReferenceEntry<K, V> _next;
        private volatile IValueReference<K, V> _valueReference = new UnSetValueReference<K, V>();

        public StrongEntry(K key, int hash, IReferenceEntry<K, V> next)
        {
            _key = key;
            _hash = hash;
            _next = next;
        }

        public IValueReference<K, V> ValueReference
        {
            get { return _valueReference; }
            set { _valueReference = value; }
        }

        public IReferenceEntry<K, V> Next => _next;
        public int Hash => _hash;
        public K Key => _key;
        public long AccessTime { get; set; }
        public IReferenceEntry<K, V> NextInAccessQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public IReferenceEntry<K, V> PreviousInAccessQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public long WriteTime { get; set; }
        public IReferenceEntry<K, V> NextInWriteQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public IReferenceEntry<K, V> PreviousInWriteQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
    }

    class StrongAccessEntry<K, V> : StrongEntry<K, V>
    {
        public StrongAccessEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            AccessTime = long.MaxValue;
        }
    }

    class StrongWriteEntry<K, V> : StrongEntry<K, V>
    {
        public StrongWriteEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            WriteTime = long.MaxValue;
        }
    }

    class StrongAccessWriteEntry<K, V> : StrongEntry<K, V>
    {
        public StrongAccessWriteEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            WriteTime = long.MaxValue;
            AccessTime = long.MaxValue;
        }
    }

    class WeakEntry<K, V> : IReferenceEntry<K, V> where K : class
    {
        private readonly WeakReference<K> _key;
        private readonly int _hash;
        private readonly IReferenceEntry<K, V> _next;
        private volatile IValueReference<K, V> _valueReference = new UnSetValueReference<K, V>();

        public WeakEntry(K key, int hash, IReferenceEntry<K, V> next)
        {
            _key = new WeakReference<K>(key);
            _hash = hash;
            _next = next;
        }

        public IValueReference<K, V> ValueReference
        {
            get { return _valueReference; }
            set { _valueReference = value; }
        }

        public IReferenceEntry<K, V> Next => _next;
        public int Hash => _hash;

        public K Key
        {
            get
            {
                K result;
                if (_key.TryGetTarget(out result)) return result;
                return default(K);
            }
        }

        public long AccessTime { get; set; }
        public IReferenceEntry<K, V> NextInAccessQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public IReferenceEntry<K, V> PreviousInAccessQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public long WriteTime { get; set; }
        public IReferenceEntry<K, V> NextInWriteQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
        public IReferenceEntry<K, V> PreviousInWriteQueue { get; set; } = NullReferenceEntry<K, V>.Instance;
    }

    class WeakAccessEntry<K, V> : StrongEntry<K, V>
    {
        public WeakAccessEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            AccessTime = long.MaxValue;
        }
    }

    class WeakWriteEntry<K, V> : StrongEntry<K, V>
    {
        public WeakWriteEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            WriteTime = long.MaxValue;
        }
    }

    class WeakAccessWriteEntry<K, V> : StrongEntry<K, V>
    {
        public WeakAccessWriteEntry(K key, int hash, IReferenceEntry<K, V> next)
            : base(key, hash, next)
        {
            WriteTime = long.MaxValue;
            AccessTime = long.MaxValue;
        }
    }

    interface IValueReference<K, V>
    {
        V Value { get; }
        Task<V> WaitForValue();
        int Weight { get; }
        IReferenceEntry<K, V> Entry { get; }
        IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry);
        void NotifyNewValue(V newValue);
        bool IsLoading();
        bool IsActive();
    }

    class StrongValueReference<K, V> : IValueReference<K, V>
    {
        readonly V _referent;

        public StrongValueReference(V referent)
        {
            _referent = referent;
            Weight = 1;
        }

        public V Value => _referent;

        public Task<V> WaitForValue()
        {
            return Task.FromResult(Value);
        }

        public int Weight { get; protected set; }
        public IReferenceEntry<K, V> Entry { get; }

        public IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry)
        {
            return this;
        }

        public void NotifyNewValue(V newValue)
        {

        }

        public bool IsLoading()
        {
            return false;
        }

        public bool IsActive()
        {
            return true;
        }
    }

    class WeakValueReference<K, V> : IValueReference<K, V> where V : class
    {
        private readonly WeakReference<V> _referent;
        private readonly IReferenceEntry<K, V> _entry;

        public WeakValueReference(V referent, IReferenceEntry<K, V> entry)
        {
            _referent = new WeakReference<V>(referent);
            _entry = entry;
            Weight = 1;
        }

        public V Value
        {
            get
            {
                V result;
                if (_referent.TryGetTarget(out result)) return result;
                return default(V);
            }
        }

        public Task<V> WaitForValue()
        {
            return Task.FromResult(Value);
        }

        public int Weight { get; protected set; }
        public IReferenceEntry<K, V> Entry => _entry;

        public virtual IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry)
        {
            return new WeakValueReference<K, V>(Value, entry);
        }

        public void NotifyNewValue(V newValue)
        {

        }

        public bool IsLoading()
        {
            return false;
        }

        public bool IsActive()
        {
            return true;
        }
    }

    class WeightedWeakValueReference<K, V> : WeakValueReference<K, V> where V : class
    {
        public WeightedWeakValueReference(V referent, IReferenceEntry<K, V> entry, int weight)
            : base(referent, entry)
        {
            Weight = weight;
        }

        public override IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry)
        {
            return new WeightedWeakValueReference<K, V>(Value, entry, Weight);
        }
    }

    class WeightedStrongValueReference<K, V> : StrongValueReference<K, V>
    {
        public WeightedStrongValueReference(V referent, int weight)
            : base(referent)
        {
            Weight = weight;
        }
    }

    class NullReferenceEntry<K, V> : IReferenceEntry<K, V>
    {
        public static IReferenceEntry<K, V> Instance { get; } = new NullReferenceEntry<K, V>(); 

        public IValueReference<K, V> ValueReference { get; set; }
        public IReferenceEntry<K, V> Next { get; }
        public int Hash { get; }
        public K Key { get; }
        public long AccessTime { get; set; }
        public IReferenceEntry<K, V> NextInAccessQueue { get; set; }
        public IReferenceEntry<K, V> PreviousInAccessQueue { get; set; }
        public long WriteTime { get; set; }
        public IReferenceEntry<K, V> NextInWriteQueue { get; set; }
        public IReferenceEntry<K, V> PreviousInWriteQueue { get; set; }
    }

    class LoadingValueReference<K, V> : IValueReference<K, V>
    {
        private volatile IValueReference<K, V> _oldValue;
        private TaskCompletionSource<V> _tcs;

        public LoadingValueReference() : this(new UnSetValueReference<K, V>())
        {

        }

        public LoadingValueReference(IValueReference<K, V> oldValue)
        {
            _oldValue = oldValue;
            _tcs = new TaskCompletionSource<V>();
        }

        public V Value => _oldValue.Value;

        public Task<V> WaitForValue()
        {
            return _tcs.Task;
        }

        public int Weight => _oldValue.Weight;
        public IReferenceEntry<K, V> Entry => null;

        public IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry)
        {
            return this;
        }

        public void NotifyNewValue(V newValue)
        {
            if (newValue != null)
                Set(newValue);
            else
                _oldValue = new UnSetValueReference<K, V>();
        }

        public async Task<Task<V>> LoadAsync(K key, ICacheLoader<K, V> loader)
        {
            try
            {
                var previousValue = OldValue.Value;
                if (previousValue == null)
                {
                    var newValue = await loader.LoadAsync(key);
                    return Set(newValue) ? _tcs.Task : Task.FromResult(newValue);
                }
                var newValue2 = await loader.ReloadAsync(key, previousValue);
                return Set(newValue2) ? _tcs.Task : Task.FromResult(newValue2);
            }
            catch (Exception ex)
            {
                return SetException(ex) ? _tcs.Task : Tasks.FromException<V>(ex);
            }
        }

        public bool Set(V newValue) => _tcs.TrySetResult(newValue);
        public bool SetException(Exception ex) => _tcs.TrySetException(ex);

        public bool IsLoading() => true;

        public bool IsActive() => _oldValue.IsActive();

        public IValueReference<K, V> OldValue => _oldValue;
    }

    class UnSetValueReference<K, V> : IValueReference<K, V>
    {
        public V Value { get; }

        public Task<V> WaitForValue()
        {
            return Task.FromResult(default(V));
        }

        public int Weight { get; }
        public IReferenceEntry<K, V> Entry { get; }

        public IValueReference<K, V> Copy(V value, IReferenceEntry<K, V> entry)
        {
            return this;
        }

        public void NotifyNewValue(V newValue)
        {
        }

        public bool IsLoading()
        {
            return false;
        }

        public bool IsActive()
        {
            return false;
        }
    }
}
