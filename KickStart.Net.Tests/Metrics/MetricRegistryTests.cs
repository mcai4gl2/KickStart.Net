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
            Assert.That(MetricBuilders.Meters.Is(meter));
            Assert.IsNotNull(meter);
            var meter2 = _metricRegistry.Meter("meter1");
            Assert.AreSame(meter, meter2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);
            Assert.AreEqual(1, _metricRegistry.Meters().Count);

            Assert.IsTrue(_metricRegistry.Remove("meter1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }

        [Test]
        public void can_register_a_counter()
        {
            var counter = _metricRegistry.Counter("counter1");
            Assert.That(MetricBuilders.Counters.Is(counter));
            Assert.IsNotNull(counter);
            var counter2 = _metricRegistry.Counter("counter1");
            Assert.AreSame(counter, counter2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);
            Assert.AreEqual(1, _metricRegistry.Counters().Count);

            Assert.IsTrue(_metricRegistry.Remove("counter1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }

        [Test]
        public void can_register_a_histogram()
        {
            var histogram = _metricRegistry.Histogram("histogram1");
            Assert.That(MetricBuilders.Histograms.Is(histogram));
            Assert.IsNotNull(histogram);
            var histogram2 = _metricRegistry.Histogram("histogram1");
            Assert.AreSame(histogram, histogram2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);
            Assert.AreEqual(1, _metricRegistry.Histograms().Count);

            Assert.IsTrue(_metricRegistry.Remove("histogram1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }

        [Test]
        public void can_register_a_timer()
        {
            var timer = _metricRegistry.Timer("timer1");
            Assert.That(MetricBuilders.Timers.Is(timer));
            Assert.IsNotNull(timer);
            var timer2 = _metricRegistry.Timer("timer1");
            Assert.AreSame(timer, timer2);
            Assert.AreEqual(1, _metricRegistry.GetMetrics().Count);
            Assert.AreEqual(1, _metricRegistry.Timers().Count);

            Assert.IsTrue(_metricRegistry.Remove("timer1"));
            Assert.AreEqual(0, _metricRegistry.GetMetrics().Count);
        }
    }
}
