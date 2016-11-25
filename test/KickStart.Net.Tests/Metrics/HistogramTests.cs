using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class HistogramTests
    {
        [Test]
        public void count_is_updated()
        {
            var reservior = new UniformReservoir();
            var histogram = new Histogram(reservior);

            Assert.AreEqual(0, histogram.Count);

            histogram.Update(1);

            Assert.AreEqual(1, histogram.Count);
        }

        [Test]
        public void snapshot_of_reservoir_is_returned()
        {
            var reservior = new StubReservior();
            var histogram = new Histogram(reservior);
            var snapshot = new UniformSnapshot();
            reservior.Snapshot = snapshot;
            Assert.AreEqual(snapshot, histogram.GetSnapshot());
        }

        [Test]
        public void update_updates_reservoir()
        {
            var reservior = new StubReservior();
            var histogram = new Histogram(reservior);
            histogram.Update(1);
            Assert.AreEqual(1, reservior.UpdateCallCount);
            Assert.AreEqual(1L, reservior.LastUpdateParam);
        }
    }

    class StubReservior : IReservoir
    {
        public Snapshot Snapshot { get; set; }
        public int UpdateCallCount { get; private set; } = 0;
        public long LastUpdateParam { get; private set; }

        public int Size()
        {
            throw new System.NotImplementedException();
        }

        public void Update(long value)
        {
            UpdateCallCount++;
            LastUpdateParam = value;
        }

        public Snapshot GetSnapshot()
        {
            return Snapshot;
        }
    }
}
