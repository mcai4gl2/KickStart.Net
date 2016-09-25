using KickStart.Net.Extensions;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class UniformReservoirTests
    {
        [Test]
        public void reservoir_of_100_out_of_1000()
        {
            var reservoir = new UniformReservoir(100);
            foreach (var i in 1000.Range())
                reservoir.Update(i);
            var snapshot = reservoir.GetSnapshot();

            Assert.AreEqual(100, reservoir.Size());
            Assert.AreEqual(100, snapshot.Size());

            foreach (var i in snapshot.GetValues())
            {
                Assert.That(i < 1000 && i >= 0);
            }
        }
    }
}
