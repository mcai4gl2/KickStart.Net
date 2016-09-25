using KickStart.Net.Cache;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class CacheStatsTests
    {
        [Test]
        public void test_empty()
        {
            var stats = new CacheStats(0, 0, 0, 0, 0, 0);
            Assert.AreEqual(0, stats.RequestCount);
            Assert.AreEqual(0, stats.HitCount);
            Assert.AreEqual(1.0, stats.HitRate);
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0.0, stats.MissRate);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0.0, stats.LoadExceptionRate);
            Assert.AreEqual(0, stats.LoadCount);
            Assert.AreEqual(0, stats.TotalLoadTime);
            Assert.AreEqual(0.0, stats.AverageLoadPenalty);
            Assert.AreEqual(0, stats.EvictionCount);
        }

        [Test]
        public void test_single()
        {
            var stats = new CacheStats(11, 13, 17, 19, 23, 27);
            Assert.AreEqual(24, stats.RequestCount);
            Assert.AreEqual(11, stats.HitCount);
            Assert.AreEqual(11.0/24, stats.HitRate);
            Assert.AreEqual(13, stats.MissCount);
            Assert.AreEqual(13.0/24, stats.MissRate);
            Assert.AreEqual(17, stats.LoadSuccessCount);
            Assert.AreEqual(19, stats.LoadExceptionCount);
            Assert.AreEqual(19.0/36, stats.LoadExceptionRate);
            Assert.AreEqual(17+19, stats.LoadCount);
            Assert.AreEqual(23, stats.TotalLoadTime);
            Assert.AreEqual(23.0/(17+19), stats.AverageLoadPenalty);
            Assert.AreEqual(27, stats.EvictionCount);
        }

        [Test]
        public void test_minus()
        {
            var one = new CacheStats(11, 13, 17, 19, 23, 27);
            var two = new CacheStats(53, 47, 43, 41, 37, 31);

            var diff = two.Minus(one);
            Assert.AreEqual(76, diff.RequestCount);
            Assert.AreEqual(42, diff.HitCount);
            Assert.AreEqual(42.0/76, diff.HitRate);
            Assert.AreEqual(34, diff.MissCount);
            Assert.AreEqual(34.0/76, diff.MissRate);
            Assert.AreEqual(26, diff.LoadSuccessCount);
            Assert.AreEqual(22, diff.LoadExceptionCount);
            Assert.AreEqual(22.0/48, diff.LoadExceptionRate);
            Assert.AreEqual(26+22, diff.LoadCount);
            Assert.AreEqual(14, diff.TotalLoadTime);
            Assert.AreEqual(14.0/(26+22), diff.AverageLoadPenalty);

            Assert.AreEqual(new CacheStats(0, 0, 0, 0, 0, 0), one.Minus(two));
        }

        [Test]
        public void test_plus()
        {
            var one = new CacheStats(11, 13, 15, 13, 11, 9);
            var two = new CacheStats(53, 47, 41, 39, 37, 35);

            var sum = two.Plus(one);
            Assert.AreEqual(124, sum.RequestCount);
            Assert.AreEqual(64, sum.HitCount);
            Assert.AreEqual(64.0/124, sum.HitRate);
            Assert.AreEqual(56, sum.LoadSuccessCount);
            Assert.AreEqual(52, sum.LoadExceptionCount);
            Assert.AreEqual(52.0/108, sum.LoadExceptionRate);
            Assert.AreEqual(56+52, sum.LoadCount);
            Assert.AreEqual(48, sum.TotalLoadTime);
            Assert.AreEqual(48, sum.TotalLoadTime);
            Assert.AreEqual(48.0/(56+52), sum.AverageLoadPenalty);
            Assert.AreEqual(44, sum.EvictionCount);

            Assert.AreEqual(sum, one.Plus(two));
        }
    }
}
