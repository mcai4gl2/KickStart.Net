using System.Linq;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class SimpleStatsTests
    {
        [Test]
        public void test_empty_simple_stats()
        {
            var counter = new SimpleStatsCounter();
            var stats = counter.Snapshot();
            Assert.AreEqual(0, stats.RequestCount);
            Assert.AreEqual(0, stats.HitCount);
            Assert.AreEqual(1.0, stats.HitRate);
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(.0, stats.MissRate);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.LoadCount);
            Assert.AreEqual(0, stats.TotalLoadTime);
            Assert.AreEqual(.0, stats.AverageLoadPenalty);
            Assert.AreEqual(0, stats.EvictionCount);
        }

        [Test]
        public void test_single_simple_stats()
        {
            var counter = new SimpleStatsCounter();
            foreach (var _ in Enumerable.Range(0, 11)) 
                counter.RecordHits(1);
            foreach (var i in Enumerable.Range(0, 13))
                counter.RecordLoadSuccess(i);
            foreach (var i in Enumerable.Range(0, 17))
                counter.RecordLoadException(i);
            foreach (var _ in Enumerable.Range(0, 23))
                counter.RecordMisses(1);
            foreach (var _ in Enumerable.Range(0, 27))
                counter.RecordEviction();
            var stats = counter.Snapshot();
            Assert.AreEqual(11 + 23, stats.RequestCount);
            Assert.AreEqual(11, stats.HitCount);
            Assert.AreEqual(11.0 / (11 + 23), stats.HitRate);
            Assert.AreEqual(23, stats.MissCount);
            Assert.AreEqual(23.0 / (11 + 23), stats.MissRate);
            Assert.AreEqual(13, stats.LoadSuccessCount);
            Assert.AreEqual(17, stats.LoadExceptionCount);
            Assert.AreEqual(13 + 17, stats.LoadCount);
            Assert.AreEqual(214, stats.TotalLoadTime);
            Assert.AreEqual(214.0 / (13 + 17), stats.AverageLoadPenalty);
            Assert.AreEqual(27, stats.EvictionCount);
        }

        [Test]
        public void test_simple_stats_increment_by()
        {
            var loadTime = 0;
            var counter1 = new SimpleStatsCounter();
            foreach (var _ in 11.Range())
                counter1.RecordHits(1);
            foreach (var i in 13.Range())
            {
                counter1.RecordLoadSuccess(i);
                loadTime += i;
            }
            foreach (var i in 17.Range())
            {
                counter1.RecordLoadException(i);
                loadTime += i;
            }
            foreach (var _ in 19.Range())
                counter1.RecordMisses(1);
            foreach (var _ in 23.Range())
                counter1.RecordEviction();
            
            var counter2 = new SimpleStatsCounter();
            foreach (var _ in 27.Range())
                counter2.RecordHits(1);
            foreach (var i in 31.Range())
            {
                counter2.RecordLoadSuccess(i);
                loadTime += i;
            }
            foreach (var i in 37.Range())
            {
                counter2.RecordLoadException(i);
                loadTime += i;
            }
            foreach (var _ in 41.Range())
                counter2.RecordMisses(1);
            foreach (var _ in 43.Range())
                counter2.RecordEviction();

            counter1.IncrementBy(counter2);
            Assert.AreEqual(new CacheStats(38, 60, 44, 54, loadTime, 66), counter1.Snapshot());
        }
    }
}
