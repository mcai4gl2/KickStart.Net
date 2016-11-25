using System;
using System.Threading;
using KickStart.Net.Cache;

namespace KickStart.Net.Metrics
{
    public class Ewma
    {
        private const int Interval = 5;
        private const double SecondsPerMinute = 60.0;
        private const int OneMinute = 1;
        private const int FiveMinutes = 5;
        private const int FifteenMinutes = 15;
        private static readonly double M1Alpha = 1 - Math.Exp(-Interval/SecondsPerMinute/OneMinute);
        private static readonly double M5Alpha = 1 - Math.Exp(-Interval/SecondsPerMinute/FiveMinutes);
        private static readonly double M15Alpha = 1 - Math.Exp(-Interval/SecondsPerMinute/FifteenMinutes);

        private volatile bool _initialised = false;
        private double _rate = 0.0;

        private readonly LongAdder _uncounted = new LongAdder();
        private readonly double _alpha;
        private readonly double _interval;

        public Ewma(double alpha, long interval, ITimeUnit intervalUnit)
        {
            _interval = intervalUnit.ToTicks(interval);
            _alpha = alpha;
        }

        public void Update(long n)
        {
            _uncounted.Add(n);
        }

        public void Tick()
        {
            var count = _uncounted.SumAndReset();
            var instantRate = count/_interval;
            if (_initialised)
            {
                var rate = Volatile.Read(ref _rate);
                Volatile.Write(ref _rate, rate + _alpha * (instantRate - rate));
            }
            else
            {
                Volatile.Write(ref _rate, instantRate);
                _initialised = true;
            }
        }

        public double GetRate(ITimeUnit rateUnit)
        {
            return _rate*rateUnit.ToTicks(1);
        }

        public static Ewma OneMinuteEwma() => new Ewma(M1Alpha, Interval, TimeUnits.Seconds);
        public static Ewma FiveMinuteEwma() => new Ewma(M5Alpha, Interval, TimeUnits.Seconds);
        public static Ewma FifteenMintueEwma() => new Ewma(M15Alpha, Interval, TimeUnits.Seconds);
    }
}
