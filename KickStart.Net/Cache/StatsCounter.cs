namespace KickStart.Net.Cache
{
    public interface IStatsCounter
    {
        void RecordHits(int count);
        void RecordMisses(int count);
        void RecordLoadSuccess(long loadTime);
        void RecordLoadException(long loadTime);
        void RecordEviction();
        CacheStats Snapshot();
    }

    public class SimpleStatsCounter : IStatsCounter
    {
        private readonly ILongAddable _hitCount = LongAddables.Create();
        private readonly ILongAddable _missCount = LongAddables.Create();
        private readonly ILongAddable _loadSuccessCount = LongAddables.Create();
        private readonly ILongAddable _loadExceptionCount = LongAddables.Create();
        private readonly ILongAddable _totalLoadTime = LongAddables.Create();
        private readonly ILongAddable _evictionCount = LongAddables.Create();

        public void RecordHits(int count)
        {
            _hitCount.Add(count);
        }

        public void RecordMisses(int count)
        {
            _missCount.Add(count);
        }

        public void RecordLoadSuccess(long loadTime)
        {
            _loadSuccessCount.Increment();
            _totalLoadTime.Add(loadTime);
        }

        public void RecordLoadException(long loadTime)
        {
            _loadExceptionCount.Increment();
            _totalLoadTime.Add(loadTime);
        }

        public void RecordEviction()
        {
            _evictionCount.Increment();
        }

        public CacheStats Snapshot()
        {
            return new CacheStats(
                _hitCount.Sum(),
                _missCount.Sum(),
                _loadSuccessCount.Sum(),
                _loadExceptionCount.Sum(),
                _totalLoadTime.Sum(),
                _evictionCount.Sum()
                );
        }

        public void IncrementBy(IStatsCounter other)
        {
            var otherStats = other.Snapshot();
            _hitCount.Add(otherStats.HitCount);
            _missCount.Add(otherStats.MissCount);
            _loadSuccessCount.Add(otherStats.LoadSuccessCount);
            _loadExceptionCount.Add(otherStats.LoadExceptionCount);
            _totalLoadTime.Add(otherStats.TotalLoadTime);
            _evictionCount.Add(otherStats.EvictionCount);
        }
    }

    public class NullStatsCounter : IStatsCounter
    {
        public static readonly NullStatsCounter Instance = new NullStatsCounter();
        private static readonly CacheStats _emptyCacheStats = new CacheStats(0, 0, 0, 0, 0, 0);

        public void RecordHits(int count)
        {
            
        }

        public void RecordMisses(int count)
        {
            
        }

        public void RecordLoadSuccess(long loadTime)
        {
            
        }

        public void RecordLoadException(long loadTime)
        {
            
        }

        public void RecordEviction()
        {
            
        }

        public CacheStats Snapshot()
        {
            return _emptyCacheStats;
        }
    }
}
