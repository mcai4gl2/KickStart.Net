using System;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class ClockTests
    {
        [Test]
        public void test_clock_default()
        {
            Assert.That(Clocks.Default is DefaultClock);
        }

        [Test]
        public void test_default_clock()
        {
            var clock = Clocks.Default;
            Assert.AreEqual(DateTime.UtcNow.Ticks, clock.Tick, 100000.0);
        }
    }
}
