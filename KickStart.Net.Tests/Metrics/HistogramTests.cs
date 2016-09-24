using KickStart.Net.Metrics;
using NSubstitute;
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
            var reservior = Substitute.For<IReservoir>();
            var histogram = new Histogram(reservior);
            var snapshot = new UniformSnapshot();
            reservior.GetSnapshot().Returns(snapshot);
            Assert.AreEqual(snapshot, histogram.GetSnapshot());
        }

        [Test]
        public void update_updates_reservoir()
        {
            var reservior = Substitute.For<IReservoir>();
            var histogram = new Histogram(reservior);
            histogram.Update(1);
            reservior.Received().Update(Arg.Is(1L));
        }
    }
}
