using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class MetricRegistryTests
    {
        private MetricRegistry _metricRegistry;

        [SetUp]
        public void SetUp()
        {
            _metricRegistry = new MetricRegistry();
        }

        [Test]
        public void can_register_a_meter()
        {
            var meter = _metricRegistry.Meter("meter1");
            Assert.IsNotNull(meter);
            var meter2 = _metricRegistry.Meter("meter1");
            Assert.AreSame(meter, meter2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);

            Assert.IsTrue(_metricRegistry.Remove("meter1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }

        [Test]
        public void can_register_a_counter()
        {
            var counter = _metricRegistry.Counter("counter1");
            Assert.IsNotNull(counter);
            var counter2 = _metricRegistry.Counter("counter1");
            Assert.AreSame(counter, counter2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);

            Assert.IsTrue(_metricRegistry.Remove("counter1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }
    }
}
