using System;
using System.Diagnostics.Contracts;

namespace KickStart.Net.Cache
{
    public struct CacheStats
    {
        private readonly long _hitCount;
        private readonly long _missCount;
        private readonly long _loadSuccessCount;
        private readonly long _loadExceptionCount;
        private readonly long _totalLoadTime;
        private readonly long _evictionCount;

        public CacheStats(long hitCount, long missCount, long loadSuccessCount,
            long loadExceptionCount, long totalLoadTime, long evictionCount)
        {
            Contract.Assert(hitCount >= 0);
            Contract.Assert(missCount >= 0);
            Contract.Assert(loadSuccessCount >= 0);
            Contract.Assert(loadExceptionCount >= 0);
            Contract.Assert(totalLoadTime >= 0);
            Contract.Assert(evictionCount >= 0);
            _hitCount = hitCount;
            _missCount = missCount;
            _loadSuccessCount = loadSuccessCount;
            _loadExceptionCount = loadExceptionCount;
            _totalLoadTime = totalLoadTime;
            _evictionCount = evictionCount;
        }

        public long RequestCount => _hitCount + _missCount;

        public long HitCount => _hitCount;

        public double HitRate => RequestCount == 0 ? 1.0 : (double) _hitCount/RequestCount;

        public long MissCount => _missCount;

        public double MissRate => RequestCount == 0 ? 0.0 : (double) _missCount/RequestCount;

        public long LoadCount => _loadSuccessCount + _loadExceptionCount;

        public long LoadSuccessCount => _loadSuccessCount;

        public long LoadExceptionCount => _loadExceptionCount;

        public double LoadExceptionRate => LoadCount == 0 ? .0 : (double) _loadExceptionCount/LoadCount;

        public long TotalLoadTime => _totalLoadTime;

        public double AverageLoadPenalty => LoadCount == 0 ? .0 : (double) _totalLoadTime/LoadCount;

        public long EvictionCount => _evictionCount;

        public CacheStats Minus(CacheStats other)
        {
            return new CacheStats(
                Math.Max(0, _hitCount - other._hitCount), 
                Math.Max(0, _missCount - other._missCount),
                Math.Max(0, _loadSuccessCount - other._loadSuccessCount),
                Math.Max(0, _loadExceptionCount - other._loadExceptionCount),
                Math.Max(0, _totalLoadTime - other._totalLoadTime),
                Math.Max(0, _evictionCount - other._evictionCount)
                );
        }

        public CacheStats Plus(CacheStats other)
        {
            return new CacheStats(
                _hitCount + other._hitCount,
                _missCount + other._missCount,
                _loadSuccessCount + other._loadSuccessCount,
                _loadExceptionCount + other._loadExceptionCount,
                _totalLoadTime + other._totalLoadTime,
                _evictionCount + other._evictionCount
                );
        }

        public override bool Equals(object obj)
        {
            if (obj is CacheStats)
            {
                var other = (CacheStats)obj;
                return _hitCount == other._hitCount
                       && _missCount == other._missCount
                       && _loadSuccessCount == other._loadSuccessCount
                       && _loadExceptionCount == other._loadExceptionCount
                       && _totalLoadTime == other._totalLoadTime
                       && _evictionCount == other._evictionCount;
            }
            return false;
        }

        public override string ToString()
        {
            return Objects.ToStringHelper(this)
                .Add("hitCount", _hitCount)
                .Add("missCount", _missCount)
                .Add("loadSuccessCount", _loadSuccessCount)
                .Add("loadExceptionCount", _loadExceptionCount)
                .Add("totalLoadTime", _totalLoadTime)
                .Add("evictionCount", _evictionCount)
                .ToString();
        }
    }
}
