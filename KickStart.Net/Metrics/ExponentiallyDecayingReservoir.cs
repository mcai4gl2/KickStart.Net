using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KickStart.Net.Metrics
{
    public class ExponentiallyDecayingReservoir : IReservoir
    {
        private const int DefaultSize = 1028;
        private const double DefaultAlpha = 0.015;
        private readonly long _rescaleThreshold = TimeUnits.Hours.ToTicks(1);

        private SortedSet<Tuple<double, WeightedSample>> _values;
        private readonly object _lock;
        private readonly double _alpha;
        private readonly int _size;
        private long _count;
        private long _startTime;
        private long _nextScaleTime;
        private readonly IClock _clock;
        private readonly Random _random;
        private readonly IEqualityComparer<Tuple<double, WeightedSample>> _equalityComparer;

        public ExponentiallyDecayingReservoir()
            : this(DefaultSize, DefaultAlpha)
        {
            
        }

        public ExponentiallyDecayingReservoir(int size, double alpha)
            : this(size, alpha, Clocks.Default)
        {
            
        }

        public ExponentiallyDecayingReservoir(int size, double alpha, IClock clock)
        {
            _values = new SortedSet<Tuple<double, WeightedSample>>(new Comparer());
            _lock = new object();
            _alpha = alpha;
            _size = size;
            _clock = clock;
            _count = 0;
            _startTime = CurrentTimeInSeconds();
            _nextScaleTime = _clock.Tick + _rescaleThreshold;
            _random = new Random();
            _equalityComparer = new EquityComparer();
        }

        public void Update(long value)
        {
            Update(value, CurrentTimeInSeconds());
        }

        public void Update(long value, long timestamp)
        {
            RescaleIfNeeded();
            lock (_lock)
            {
                var weight = Math.Exp(_alpha*(timestamp - _startTime));
                var sample = new WeightedSample(value, weight);
                var priority = weight/_random.NextDouble();
                if (_values.Count < _size)
                {
                    _values.Add(new Tuple<double, WeightedSample>(priority, sample));
                }
                else
                {
                    var first = _values.First();
                    var newValue = new Tuple<double, WeightedSample>(priority, sample);
                    if (first.Item1 < priority &&
                        !_values.Contains(newValue, _equalityComparer))
                    {
                        _values.Remove(first);
                        _values.Add(newValue);
                    }
                }
            }
        }

        private long CurrentTimeInSeconds()
        {
            return TimeUnits.Ticks.ToSeconds(_clock.Tick);
        }

        private void RescaleIfNeeded()
        {
            var now = _clock.Tick;
            var next = Interlocked.Read(ref _nextScaleTime);
            if (now >= next)
                Rescale(now, next);
        }

        private void Rescale(long now, long next)
        {
            if (Interlocked.CompareExchange(ref _nextScaleTime, now + _rescaleThreshold, next) == next)
            {
                lock (_lock)
                {
                    var oldStartTime = _startTime;
                    _startTime = CurrentTimeInSeconds();
                    var scalingFactor = Math.Exp(-_alpha*(_startTime - oldStartTime));
                    var newValues = new SortedSet<Tuple<double, WeightedSample>>(new Comparer());
                    foreach (var tuple in _values)
                    {
                        var newTuple = new Tuple<double, WeightedSample>(tuple.Item1 * scalingFactor, 
                            new WeightedSample(tuple.Item2.Value, tuple.Item2.Weight * scalingFactor));
                        newValues.Add(newTuple);
                    }
                    _values = newValues;
                }
            }
        }

        public int Size()
        {
            lock (_lock)
            {
                return _values.Count;
            }
        }

        public Snapshot GetSnapshot()
        {
            lock (_lock)
            {
                return new WeightedSnapshot(_values.Select(v => v.Item2));
            }
        }

        class Comparer : IComparer<Tuple<Double, WeightedSample>>
        {
            public int Compare(Tuple<double, WeightedSample> x, Tuple<double, WeightedSample> y)
            {
                return x.Item1 < y.Item1 ? -1 : x.Item1 > y.Item1 ? 1 : 0;
            }
        }

        class EquityComparer : IEqualityComparer<Tuple<Double, WeightedSample>>
        {
            public bool Equals(Tuple<double, WeightedSample> x, Tuple<double, WeightedSample> y)
            {
                return x.Item1 == y.Item1;
            }

            public int GetHashCode(Tuple<double, WeightedSample> obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
