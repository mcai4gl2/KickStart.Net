using KickStart.Net.Extensions;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class ExponentiallyDecayingReservoirTests
    {
        [Test]
        public void reservoir_of_100_out_of_1000_elements()
        {
            var reservoir = new ExponentiallyDecayingReservoir(100, 0.99);

            foreach (var i in 1000.Range())
            {
                reservoir.Update(i);
            }

            Assert.AreEqual(100, reservoir.Size());

            var snapshot = reservoir.GetSnapshot();

            Assert.AreEqual(100, snapshot.Size());
            foreach (var value in snapshot.GetValues())
            {
                Assert.That(value >= 0 && value < 1000);
            }
        }

        [Test]
        public void reservoir_of_100_out_of_10_elements()
        {
            var reservoir = new ExponentiallyDecayingReservoir(100, 0.99);

            foreach (var i in 10.Range())
            {
                reservoir.Update(i);
            }

            Assert.AreEqual(10, reservoir.Size());

            var snapshot = reservoir.GetSnapshot();

            Assert.AreEqual(10, snapshot.Size());
            foreach (var value in snapshot.GetValues())
            {
                Assert.That(value >= 0 && value < 10);
            }
        }

        [Test]
        public void biased_reservoir_of_100_out_of_1000_elements()
        {
            var reservoir = new ExponentiallyDecayingReservoir(100, 0.01);

            foreach (var i in 1000.Range())
            {
                reservoir.Update(i);
            }

            Assert.AreEqual(100, reservoir.Size());

            var snapshot = reservoir.GetSnapshot();

            Assert.AreEqual(100, snapshot.Size());
            foreach (var value in snapshot.GetValues())
            {
                Assert.That(value >= 0 && value < 1000);
            }
        }

        [Test]
        public void long_periods_of_inactivity_should_not_corrupt_sampling_state()
        {
            var clock = new ManualClock();
            var reservoir = new ExponentiallyDecayingReservoir(10, 0.015, clock);

            // Add 1000 values at the rate of 10 values per second
            foreach (var i in 1000.Range())
            {
                reservoir.Update(1000 + i);
                clock.AddMilliSeconds(100);
            }
            Assert.AreEqual(10, reservoir.GetSnapshot().Size());
            foreach (var value in reservoir.GetSnapshot().GetValues())
                Assert.That(value >= 1000 && value < 2000);

            // Wait for 15 hours and add another value.
            // This shall trigger a rescale. 
            clock.AddHours(15);
            reservoir.Update(2000);
            Assert.AreEqual(2, reservoir.GetSnapshot().Size());
            foreach (var value in reservoir.GetSnapshot().GetValues())
                Assert.That(value >= 1000 && value < 3000);
            reservoir.GetSnapshot().GetValues().P();

            // Add 1000 values at the rate of 10 values per second
            foreach (var i in 1000.Range())
            {
                reservoir.Update(3000 + i);
                clock.AddMilliSeconds(100);
            }
            Assert.AreEqual(10, reservoir.GetSnapshot().Size());
            foreach (var value in reservoir.GetSnapshot().GetValues())
                Assert.That(value >= 3000 && value < 4000);
        }

        [Test]
        public void test_spot_lift()
        {
            var clock = new ManualClock();
            var reservoir = new ExponentiallyDecayingReservoir(1000, 0.015, clock);

            var valuesRatePerMinute = 10;
            var valuesItervalMillis = (int) TimeUnits.Minutes.ToMillis(1)/valuesRatePerMinute;
            // Mode 1: steady regime for 120 mins
            foreach (var _ in (120*valuesRatePerMinute).Range())
            {
                reservoir.Update(177);
                clock.AddMilliSeconds(valuesItervalMillis);
            }

            // Switching to model 2: 10 mintes more with the same rate, but larger value
            foreach (var _ in (10 * valuesRatePerMinute).Range())
            {
                reservoir.Update(9999);
                clock.AddMilliSeconds(valuesItervalMillis);
            }

            Assert.AreEqual(9999, reservoir.GetSnapshot().GetMedian());
        }

        [Test]
        public void test_spot_fall()
        {
            var clock = new ManualClock();
            var reservoir = new ExponentiallyDecayingReservoir(1000, 0.015, clock);

            var valuesRatePerMinute = 10;
            var valuesItervalMillis = (int)TimeUnits.Minutes.ToMillis(1) / valuesRatePerMinute;
            // Mode 1: steady regime for 120 mins
            foreach (var _ in (120 * valuesRatePerMinute).Range())
            {
                reservoir.Update(9999);
                clock.AddMilliSeconds(valuesItervalMillis);
            }

            // Switching to model 2: 10 mintes more with the same rate, but smaller value
            foreach (var _ in (10 * valuesRatePerMinute).Range())
            {
                reservoir.Update(177);
                clock.AddMilliSeconds(valuesItervalMillis);
            }

            Assert.AreEqual(177, reservoir.GetSnapshot().GetMedian());
        }

        [Test]
        public void test_quantiles_is_based_on_weights()
        {
            var clock = new ManualClock();
            var reservoir = new ExponentiallyDecayingReservoir(1000, 0.015, clock);

            foreach (var _ in 40.Range())
            {
                reservoir.Update(177);
            }

            clock.AddSeconds(120);

            foreach (var _ in 10.Range())
            {
                reservoir.Update(9999);
            }

            Assert.AreEqual(50, reservoir.GetSnapshot().Size());

            // the first added 40 items (177) have weights 1 
            // the next added 10 items (9999) have weights ~6 
            // so, it's 40 vs 60 distribution, not 40 vs 10
            Assert.AreEqual(9999, reservoir.GetSnapshot().GetMedian());
            Assert.AreEqual(9999, reservoir.GetSnapshot().Get75thPercentile());
        }
    }
}
