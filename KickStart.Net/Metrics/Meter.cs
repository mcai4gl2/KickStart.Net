using System.Threading;
using KickStart.Net.Cache;

namespace KickStart.Net.Metrics
{
    public class Meter : IMetered
    {
        private readonly LongAdder _count = new LongAdder();
        private readonly long _startTime;
        private long _lastTick;
        private readonly IClock _clock;

        private static readonly long _tickInternal = TimeUnits.Seconds.ToTicks(5);

        private readonly Ewma _m1Rate = Ewma.OneMinuteEwma();
        private readonly Ewma _m5Rate = Ewma.FiveMinuteEwma();
        private readonly Ewma _m15Rate = Ewma.FifteenMintueEwma();

        public Meter()
            : this(Clocks.Default)
        {
            
        }

        public Meter(IClock clock)
        {
            _clock = clock;
            _startTime = _clock.Tick;
            _lastTick = _startTime;
        }

        public void Mark() => Mark(1);

        public void Mark(long n)
        {
            TickIfNecessary();
            _count.Add(n);
            _m1Rate.Update(n);
            _m5Rate.Update(n);
            _m15Rate.Update(n);
        }

        private void TickIfNecessary()
        {
            var oldTick = Interlocked.Read(ref _lastTick);
            var newTick = _clock.Tick;
            var age = newTick - oldTick;
            if (age > _tickInternal)
            {
                var newIntervalStartTick = newTick - age%_tickInternal;
                if (Interlocked.CompareExchange(ref _lastTick, newIntervalStartTick, oldTick) == oldTick)
                {
                    var ticks = age/_tickInternal;
                    for (var i = 0; i < ticks; i++)
                    {
                        _m1Rate.Tick();
                        _m5Rate.Tick();
                        _m15Rate.Tick();
                    }
                }
            }
        }

        public long Count => _count.Sum();

        public double FifteenMinutesRate
        {
            get
            {
                TickIfNecessary();
                return _m15Rate.GetRate(TimeUnits.Seconds);
            }
        }

        public double FiveMinutesRate
        {
            get
            {
                TickIfNecessary();
                return _m5Rate.GetRate(TimeUnits.Seconds);
            }
        }

        public double OneMinuteRate
        {
            get
            {
                TickIfNecessary();
                return _m1Rate.GetRate(TimeUnits.Seconds);
            }
        }

        public double MeanRate
        {
            get
            {
                if (Count == 0)
                    return 0.0;
                double elapsed = _clock.Tick - _startTime;
                return Count/elapsed*TimeUnits.Seconds.ToTicks(1);
            }
        }
    }
}
