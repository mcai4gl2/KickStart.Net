using System;
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Extensions;

namespace KickStart.Net.Metrics
{
    public struct WeightedSample
    {
        public long Value { get; }
        public double Weight { get; }

        public WeightedSample(long value, double weight)
        {
            Value = value;
            Weight = weight;
        }

        public static IEnumerable<WeightedSample> From(long[] values, double[] weights)
        {
            if (values.Length != weights.Length)
                throw new ArgumentException("value and weight length not matched");
            return values.Length.Range().Select(i => new WeightedSample(values[i], weights[i]));
        }
    }

    class WeightedSampleComparer : IComparer<WeightedSample>
    {
        public int Compare(WeightedSample x, WeightedSample y)
        {
            return (int)(x.Value - y.Value);
        }
    }

    public class WeightedSnapshot : Snapshot
    {
        private readonly long[] _values;
        private readonly double[] _normWeights;
        private readonly double[] _quantiles;

        public WeightedSnapshot(IEnumerable<WeightedSample> values)
        {
            var copy = values.ToArray();
            Array.Sort(copy, new WeightedSampleComparer());
            _values = new long[copy.Length];
            _normWeights = new double[copy.Length];
            _quantiles = new double[copy.Length];
            var weightSum = copy.Sum(s => s.Weight);
            foreach (var i in copy.Length.Range())
            {
                _values[i] = copy[i].Value;
                _normWeights[i] = copy[i].Weight/weightSum;
            }
            foreach (var i in copy.Length.Range().Skip(1))
            {
                _quantiles[i] = _quantiles[i - 1] + _normWeights[i - 1];
            }
        }

        public WeightedSnapshot(params WeightedSample[] values)
            : this(values.ToList())
        {
            
        }

        public override double GetValue(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0)
                throw new ArgumentException($"{quantile} is not between 0 and 1");
            if (_values.Length == 0)
                return 0.0;
            var posx = Array.BinarySearch(_quantiles, quantile);
            if (posx < 0)
                posx = ((-posx) - 1) - 1;
            if (posx < 1)
                return _values[0];
            if (posx >= _values.Length)
                return _values[_values.Length - 1];
            return _values[posx];
        }

        public override long[] GetValues() => (long[])_values.Clone();
        public override int Size() => _values.Length;

        public override long GetMax()
        {
            if (_values.Length == 0)
                return 0;
            return _values[_values.Length - 1];
        }

        public override double GetMean()
        {
            if (_values.Length == 0)
                return 0;
            return _values.Length.Range().Sum(i => _values[i]*_normWeights[i]);
        }

        public override long GetMin()
        {
            if (_values.Length == 0)
                return 0;
            return _values[0];
        }

        public override double GetStdDev()
        {
            if (_values.Length <= 1)
                return 0;

            var mean = GetMean();
            var variance = 0.0;

            foreach (var i in _values.Length.Range())
            {
                var diff = _values[i] - mean;
                variance += _normWeights[i]*diff*diff;
            }

            return Math.Sqrt(variance);
        }
    }
}
