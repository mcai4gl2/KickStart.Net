using System;
using System.Threading.Tasks;

namespace KickStart.Net.Metrics
{
    public class Timer : IMetered, ISampling
    {
        public class Context : IDisposable
        {
            private readonly Timer _timer;
            private readonly IClock _clock;
            private readonly long _startTime;

            public Context(Timer timer, IClock clock)
            {
                _timer = timer;
                _clock = clock;
                _startTime = clock.Tick;
            }

            public long Stop()
            {
                var elapsed = _clock.Tick - _startTime;
                _timer.Update(elapsed, TimeUnits.Ticks);
                return elapsed;
            }

            public void Dispose()
            {
                Stop();
            }
        }

        private readonly Meter _meter;
        private readonly Histogram _histogram;
        private readonly IClock _clock;

        public Timer()
            : this(new ExponentiallyDecayingReservoir())
        {
            
        }

        public Timer(IReservoir reservoir)
            : this(reservoir, Clocks.Default)
        {
            
        }

        public Timer(IReservoir reservoir, IClock clock)
        {
            _meter = new Meter(clock);
            _clock = clock;
            _histogram = new Histogram(reservoir);
        }

        public long Count => _histogram.Count;
        public double FifteenMinutesRate => _meter.FifteenMinutesRate;
        public double FiveMinutesRate => _meter.FiveMinutesRate;
        public double OneMinuteRate => _meter.OneMinuteRate;
        public double MeanRate => _meter.MeanRate;
        public Snapshot GetSnapshot()
        {
            return _histogram.GetSnapshot();
        }

        public void Update(long duration, ITimeUnit unit)
        {
            Update(unit.ToTicks(duration));
        }

        public void Update(long duration)
        {
            if (duration > 0)
            {
                _histogram.Update(duration);
                _meter.Mark();
            }
        }

        public Context Time() => new Context(this, _clock);

        public T Time<T>(Func<T> func)
        {
            var startTime = _clock.Tick;
            try
            {
                return func();
            }
            finally
            {
                Update(_clock.Tick - startTime);
            }
        }

        public void Time(Action action)
        {
            var startTime = _clock.Tick;
            try
            {
                action();
            }
            finally
            {
                Update(_clock.Tick - startTime);
            }
        }

        public async Task<T> Time<T>(Func<Task<T>> func)
        {
            var startTime = _clock.Tick;
            try
            {
                return await func();
            }
            finally
            {
                Update(_clock.Tick - startTime);
            }
        }

        public async Task Time(Func<Task> func)
        {
            var startTime = _clock.Tick;
            try
            {
                await func();
            }
            finally
            {
                Update(_clock.Tick - startTime);
            }
        }
    }
}
