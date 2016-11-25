using KickStart.Net.Cache;

namespace KickStart.Net.Metrics
{
    public class Histogram : IMetric, ISampling, ICounting
    {
        private readonly IReservoir _reservoir;
        private readonly LongAdder _count;

        public Histogram(IReservoir reservoir)
        {
            _reservoir = reservoir;
            _count = new LongAdder();
        }

        public void Update(int value)
        {
            Update((long) value);
        }

        public void Update(long value)
        {
            _count.Increment();
            _reservoir.Update(value);
        }

        public Snapshot GetSnapshot() => _reservoir.GetSnapshot();

        public long Count => _count.Sum();
    }
}
