using System;
using System.Diagnostics.Contracts;

namespace KickStart.Net.Cache
{
    public class CacheBuilder<K, V>
    {
        private static readonly int DEFAULT_INITIAL_CAPACITY = 16;
        private static readonly int DEFAULT_CONCURRENCY_LEVEL = 4;
        private static readonly int DEFAULT_EXPIRATION_MILLIS = 0;
        private static readonly int DEFAULT_REFRESH_MILLIS = 0;

        private static readonly int UNSET_INT = -1;
        bool _strictParsing = true;
        int _initialCapacity = UNSET_INT;
        int _concurrencyLevel = UNSET_INT;
        long _maximumSize = UNSET_INT;
        long _maximumWeight = UNSET_INT;
        IWeigher<K, V> _weighter;
        long _expireAfterWriteTicks = UNSET_INT;
        long _expireAfterAccessTicks = UNSET_INT;
        long _refreshTicks = UNSET_INT;
        bool _recordsStats = false;

        public bool IsRecordingStats => _recordsStats;

        IRemovalListener<K, V> _removalListener;
        ITicker _ticker;

        public static CacheBuilder<K, V> NewBuilder()
        {
            return new CacheBuilder<K, V>();
        }

        public CacheBuilder<K, V> WithConcurrencyLevel(int concurrencyLevel)
        {
            Contract.Assert(concurrencyLevel > 0);
            _concurrencyLevel = concurrencyLevel;
            return this;
        }
        public int ConcurrencyLevel => _concurrencyLevel == UNSET_INT ? DEFAULT_CONCURRENCY_LEVEL : _concurrencyLevel;

        public CacheBuilder<K, V> WithInitialCapacity(int initialCapacity)
        {
            Contract.Assert(initialCapacity >= 0);
            _initialCapacity = initialCapacity;
            return this;
        }
        public int InitialCapacity => _initialCapacity == UNSET_INT ? DEFAULT_INITIAL_CAPACITY : _initialCapacity;

        public CacheBuilder<K, V> WithMaximumSize(long size)
        {
            Contract.Assert(_maximumSize == UNSET_INT);
            Contract.Assert(_maximumWeight == UNSET_INT);
            Contract.Assert(_weighter == null);
            Contract.Assert(size >= 0);
            _maximumSize = size;
            return this;
        }

        public CacheBuilder<K, V> WithMaximumWeight(long weight)
        {
            Contract.Assert(_maximumWeight == UNSET_INT);
            Contract.Assert(_maximumSize == UNSET_INT);
            Contract.Assert(weight >= 0);
            _maximumWeight = weight;
            return this;
        }

        public CacheBuilder<K, V> WithWeigher(IWeigher<K, V> weigher)
        {
            Contract.Assert(_maximumSize == UNSET_INT);
            Contract.Assert(weigher != null);
            _weighter = weigher;
            return this;
        }

        public long MaximumWeight => _weighter == null ? _maximumSize : _maximumWeight;

        public IWeigher<K, V> Weighter => _weighter ?? Weighers.One<K, V>();

        public CacheBuilder<K, V> WithExpireAfterWrite(long ticks)
        {
            Contract.Assert(_expireAfterWriteTicks == UNSET_INT);
            Contract.Assert(ticks >= 0);
            _expireAfterWriteTicks = ticks;
            return this;
        }

        public CacheBuilder<K, V> WithExpireAfterWrite(TimeSpan timeSpan) => WithExpireAfterWrite(timeSpan.Ticks); 
        public long ExpireAfterWriteTicks => _expireAfterWriteTicks == UNSET_INT ? DEFAULT_EXPIRATION_MILLIS : _expireAfterWriteTicks;

        public CacheBuilder<K, V> WithExpireAfterAccess(TimeSpan timeSpan) => WithExpireAfterAccess(timeSpan.Ticks);
        public CacheBuilder<K, V> WithExpireAfterAccess(long ticks)
        {
            Contract.Assert(_expireAfterAccessTicks == UNSET_INT);
            Contract.Assert(ticks >= 0);
            _expireAfterAccessTicks = ticks;
            return this;
        }
        public long ExpireAfterAccessTicks => _expireAfterAccessTicks == UNSET_INT ? DEFAULT_EXPIRATION_MILLIS : _expireAfterAccessTicks;

        public CacheBuilder<K, V> WithRefreshAfterWrite(long ticks)
        {
            Contract.Assert(_refreshTicks == UNSET_INT);
            Contract.Assert(ticks > 0);
            _refreshTicks = ticks;
            return this;
        }
        public long RefreshAfterWrite => _refreshTicks == UNSET_INT ? DEFAULT_REFRESH_MILLIS : _refreshTicks;
        public CacheBuilder<K, V> WithRefreshAfterWrite(TimeSpan timeSpan) => WithRefreshAfterWrite(timeSpan.Ticks); 

        public CacheBuilder<K, V> WithTicker(ITicker ticker)
        {
            Contract.Assert(_ticker == null);
            Contract.Assert(ticker != null);
            _ticker = ticker;
            return this;
        }

        public ITicker Ticker(bool recordsTime)
        {
            if (_ticker != null) return _ticker;
            return recordsTime ? new DateTimeTicker() : (ITicker) new NullTicker();
        }

        public CacheBuilder<K, V> RecordTime()
        {
            Contract.Assert(_ticker == null);
            _ticker = new DateTimeTicker();
            return this;
        } 

        public CacheBuilder<K, V> WithRemovalListener(IRemovalListener<K, V> listener)
        {
            Contract.Assert(_removalListener == null);
            Contract.Assert(listener != null);
            _removalListener = listener;
            return this;
        }
        public IRemovalListener<K, V> RemovalListener => _removalListener ?? RemovalListeners.Null<K, V>();

        public CacheBuilder<K, V> RecordStats()
        {
            _recordsStats = true;
            return this;
        }  

        public ICache<K, V> Build()
        {
            Contract.Assert((_weighter == null && _maximumWeight == UNSET_INT) || (_weighter != null && _maximumWeight == UNSET_INT));
            Contract.Assert(_refreshTicks == UNSET_INT);
            return new LocalManualCache<K, V>(this);
        }

        public ILoadingCache<K, V> Build(ICacheLoader<K, V> loader)
        {
            Contract.Assert((_weighter == null && _maximumWeight == UNSET_INT) || (_weighter != null && _maximumWeight != UNSET_INT));
            return new LocalLoadingCache<K, V>(this, loader);
        }
    }
}
