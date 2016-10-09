using System;
using System.Threading;
using KickStart.Net.Extensions;

namespace KickStart.Net.Metrics
{
    public class UniformReservoir : IReservoir
    {
        private const int DefaultSize = 1028;
        private long _count;
        private readonly long[] _values;
        private ThreadLocal<Random> _random;

        public UniformReservoir() 
            : this(DefaultSize)
        {
            
        }

        public UniformReservoir(int size)
        {
            _values = new long[size];
            foreach (var i in size.Range())
                _values[i] = 0;
        }

        public int Size()
        {
            var count = Interlocked.Read(ref _count);
            if (count > _values.Length)
                return _values.Length;
            return (int) count;
        }

        public void Update(long value)
        {
            var count = Interlocked.Increment(ref _count);
            if (count < _values.Length)
            {
                Interlocked.Exchange(ref _values[count - 1], value);
            }
            else
            {
                if (_random == null)
                    _random = new ThreadLocal<Random>(() => new Random());
                var r = NextLong(_random.Value, (ulong)count);
                if (r < _values.Length)
                {
                    Interlocked.Exchange(ref _values[(int)r], value);
                }
            }
        }

        private static long NextLong(Random random, ulong n)
        {
            var bits = random.NextLong();
            var val = bits % n;
            return (long)val;
        }

        public Snapshot GetSnapshot()
        {
            var size = Size();
            var copy = new long[size];
            foreach (var i in size.Range())
                copy[i] = Interlocked.Read(ref _values[i]);
            return new UniformSnapshot(copy);
        }
    }
}
