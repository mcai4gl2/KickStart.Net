using System.Threading.Tasks;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class CachedGaugeTests
    {
        private LongAdder _value;
        private IGauge<long> _gauge;

        [SetUp]
        public void SetUp()
        {
            _value = new LongAdder();
            _gauge = new DelegatingCachedGauge<long>(() => 
            {
                _value.Increment();
                return _value.Sum();
            }, 100, TimeUnits.Milliseconds);
        }

        [Test]
        public void can_cache_value_for_a_given_period()
        {
            Assert.AreEqual(1, _gauge.GetValue());
            Assert.AreEqual(1, _gauge.GetValue());
        }

        [Test]
        public async Task reloads_after_timeout()
        {
            Assert.AreEqual(1, _gauge.GetValue());
            await 0.15.Seconds();
            Assert.AreEqual(2, _gauge.GetValue());
        }
    }
}
