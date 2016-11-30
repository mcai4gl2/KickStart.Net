using System;
using System.Threading;

namespace KickStart.Net.Metrics
{
    public abstract class CachedGauge<T> : IGauge<T>
    {
        private readonly IClock _clock;
        private long _reloadAt;
        private readonly long _timeoutTicks;
        private T _value;

        protected CachedGauge(long timeout, ITimeUnit timeoutUnit)
            : this(Clocks.Default, timeout, timeoutUnit) 
        {
         
        }

        protected CachedGauge(IClock clock, long timeout, ITimeUnit timeoutUnit)
        {
            _clock = clock;
            _reloadAt = 0;
            _timeoutTicks = timeoutUnit.ToTicks(timeout);
        } 

        public T GetValue()
        {
            if (ShouldLoad())
            {
#if !NET_CORE
                Thread.MemoryBarrier();
#endif
#if NET_CORE
                Interlocked.MemoryBarrier();
#endif
                _value = LoadValue();
#if !NET_CORE
                Thread.MemoryBarrier();
#endif
#if NET_CORE
                Interlocked.MemoryBarrier();
#endif
            }
#if !NET_CORE
            Thread.MemoryBarrier();
#endif
#if NET_CORE
            Interlocked.MemoryBarrier();
#endif
            return _value;
        }

        private bool ShouldLoad()
        {
            while (true)
            {
                var time = _clock.Tick;
                var current = Interlocked.Read(ref _reloadAt);
                if (current > time)
                    return false;
                var newValue = time + _timeoutTicks;
                if (Interlocked.CompareExchange(ref _reloadAt, newValue, current) == current)
                    return true;
            }
        }

        protected abstract T LoadValue();
    }

    public class DelegatingCachedGauge<T> : CachedGauge<T>
    {
        private Func<T> _func;
         
        public DelegatingCachedGauge(Func<T> func, long timeout, ITimeUnit timeoutUnit)
            : base(timeout, timeoutUnit)
        {
            _func = func;
        }

        protected override T LoadValue()
        {
            return _func();
        }
    }
}
