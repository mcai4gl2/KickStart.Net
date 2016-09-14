using System.Threading;

namespace KickStart.Net.Cache
{
    class LongAdder : ILongAddable
    {
        private long _acc;

        public LongAdder() { }

        public void Increment()
        {
            Interlocked.Increment(ref _acc);
        }

        public void Add(long x)
        {
            Interlocked.Add(ref _acc, x);
        }

        public long Sum()
        {
            return Interlocked.Read(ref _acc);
        }

        public long SumAndReset()
        {
            return Interlocked.Exchange(ref _acc, 0);
        }
    }
}
