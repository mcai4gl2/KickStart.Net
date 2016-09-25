using System;
using System.Collections.Generic;
using System.Linq;

namespace KickStart.Net.Metrics
{
    public class UniformSnapshot : Snapshot
    {
        private readonly long[] _values;

        public UniformSnapshot(IEnumerable<long> values)
        {
            _values = values.ToArray();
            Array.Sort(_values);
        }

        public UniformSnapshot(params long[] values) : this(values.ToList())
        {
            
        }

        public override double GetValue(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0)
                throw new ArgumentException($"{quantile} is not between 0 and 1");
            if (_values.Length == 0)
                return 0.0;
            var pos = quantile*(_values.Length + 1);
            var index = (int) pos;
            if (index < 1)
                return _values[0];
            if (index >= _values.Length)
                return _values[_values.Length - 1];
            var lower = _values[index - 1];
            var upper = _values[index];
            return lower + (pos - Math.Floor(pos))*(upper - lower);
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
            return (double)_values.Sum() / _values.Length;
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
            var sum = _values.Aggregate(0.0, (s, v) => s + (v - mean)*(v - mean));
            var variance = sum/(_values.Length - 1);
            return Math.Sqrt(variance);
        }
    }
}
