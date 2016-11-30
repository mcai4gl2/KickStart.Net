using System;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class WeightedSnapshotTests
    {
        private Snapshot _snapshot;

        [SetUp]
        public void SetUp()
        {
            _snapshot = new WeightedSnapshot(WeightedSample.From(
                new long[] {5,1,2,3,4}, new double[] {1,2,3,2,2}));
        }

        [Test]
        public void quantile_0_equals_to_first_value()
        {
            Assert.AreEqual(1, _snapshot.GetValue(0));
        }

        [Test]
        public void quantile_1_equals_to_last_value()
        {
            Assert.AreEqual(5, _snapshot.GetValue(1));
        }

        [TestCase(1.1)]
        [TestCase(-0.1)]
        public void test_invalid_input_to_quantitle_calculation(double quantitle)
        {
            Assert.Throws<ArgumentException>(() => _snapshot.GetValue(quantitle));
        }

        [Test]
        public void test_median()
        {
            Assert.AreEqual(3, _snapshot.GetMedian());
        }

        [Test]
        public void test_75_percent_percentile()
        {
            Assert.AreEqual(4, _snapshot.Get75thPercentile());
        }

        [Test]
        public void test_95_percent_percentile()
        {
            Assert.AreEqual(5.0, _snapshot.Get95thPercentile());
        }

        [Test]
        public void test_98_percent_percentile()
        {
            Assert.AreEqual(5.0, _snapshot.Get98thPercentile());
        }

        [Test]
        public void test_99_percent_percentile()
        {
            Assert.AreEqual(5.0, _snapshot.Get99thPercentile());
        }

        [Test]
        public void test_999_percent_percentile()
        {
            Assert.AreEqual(5.0, _snapshot.Get999thPercentile());
        }

        [Test]
        public void test_get_values()
        {
            Assert.AreEqual(new long[] { 1, 2, 3, 4, 5 }, _snapshot.GetValues());
        }

        [Test]
        public void test_size()
        {
            Assert.AreEqual(5, _snapshot.Size());
        }

        [Test]
        public void test_min_value()
        {
            Assert.AreEqual(1, _snapshot.GetMin());
        }

        [Test]
        public void test_max_value()
        {
            Assert.AreEqual(5, _snapshot.GetMax());
        }

        [Test]
        public void test_mean_value()
        {
            Assert.AreEqual(2.7, _snapshot.GetMean());
        }

        [Test]
        public void test_std_dev()
        {
            Assert.AreEqual(1.2688, _snapshot.GetStdDev(), 0.0001);
        }

        [Test]
        public void can_calc_min_of_empty_snapshot()
        {
            Assert.AreEqual(0, new UniformSnapshot().GetMin());
        }

        [Test]
        public void can_calc_max_of_empty_snapshot()
        {
            Assert.AreEqual(0, new UniformSnapshot().GetMax());
        }

        [Test]
        public void can_calc_std_dev_of_empty_snapshot()
        {
            Assert.AreEqual(0, new UniformSnapshot().GetStdDev());
        }

        [Test]
        public void can_calc_std_dev_of_snapshot_with_single_value()
        {
            Assert.AreEqual(0, new UniformSnapshot(1).GetStdDev());
        }
    }
}
