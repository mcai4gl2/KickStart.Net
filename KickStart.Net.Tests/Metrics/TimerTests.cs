﻿using System.Threading.Tasks;
using KickStart.Net.Metrics;
using NSubstitute;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class TimerTests
    {
        private IReservoir _reservoir;
        private IClock _clock;
        private Timer _timer;

        [SetUp]
        public void SetUp()
        {
            _reservoir = Substitute.For<IReservoir>();
            _clock = new MockClock();
            _timer = new Timer(_reservoir, _clock);
        }

        [Test]
        public void test_rates()
        {
            Assert.AreEqual(0, _timer.Count);
            Assert.AreEqual(0, _timer.MeanRate);
            Assert.AreEqual(0, _timer.OneMinuteRate);
            Assert.AreEqual(0, _timer.FiveMinutesRate);
            Assert.AreEqual(0, _timer.FifteenMinutesRate);
        }

        [Test]
        public void updates_the_count_on_updates()
        {
            Assert.AreEqual(0, _timer.Count);
            _timer.Update(1, TimeUnits.Seconds);
            Assert.AreEqual(1, _timer.Count);
        }

        [Test]
        public void tests_times_sync()
        {
            var value = _timer.Time(() => "one");
            Assert.AreEqual(1, _timer.Count);
            Assert.AreEqual("one", value);
            _reservoir.Received().Update(500000);

            _timer.Time(() =>
            {
                
            });
            Assert.AreEqual(2, _timer.Count);
        }

        [Test]
        public async Task tests_times_async()
        {
            var value = await _timer.Time(() => Task.FromResult("one"));
            Assert.AreEqual(1, _timer.Count);
            Assert.AreEqual("one", value);
            _reservoir.Received().Update(500000);
        }

        [Test]
        public void tests_times_via_context()
        {
            _timer.Time().Stop();
            Assert.AreEqual(1, _timer.Count);
            _reservoir.Received().Update(500000);

            using (_timer.Time())
            {
                Assert.AreEqual(1, _timer.Count);
            }
            Assert.AreEqual(2, _timer.Count);
        }

        [Test]
        public void test_returning_snapshot()
        {
            var snapshot = Substitute.For<Snapshot>();
            _reservoir.GetSnapshot().Returns(snapshot);
            Assert.AreEqual(snapshot, _reservoir.GetSnapshot());
        }

        [Test]
        public void update_ignores_negative_value()
        {
            _timer.Update(-1);
            Assert.AreEqual(0, _timer.Count);
        }
    }

    class MockClock : IClock
    {
        private long _value = 0;

        public long Tick
        {
            get
            {
                _value += TimeUnits.Milliseconds.ToTicks(50);
                return _value;
            }
        }
    }
}
