using KickStart.Net.Cache;

namespace KickStart.Net.Metrics
{
    public class Counter : IMetric, ICounting
    {
        private readonly LongAdder _count = new LongAdder();

        public long Count => _count.Sum();

        public void Increment()
        {
            _count.Increment();
        }

        public void Increment(long n)
        {
            _count.Add(n);
        }

        public void Decrement()
        {
            _count.Add(-1);
        }

        public void Decrement(long n)
        {
            _count.Add(-n);
        }
    }
}
