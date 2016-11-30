using System.Collections.Generic;
using KickStart.Net.Cache;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class CacheManualTests
    {
        [Test]
        public void test_get_if_present()
        {
            var cache = CacheBuilder<object, object>.NewBuilder().RecordStats().Build();
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            var one = new object();
            var two = new object();

            Assert.Null(cache.GetIfPresent(one));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.Null(cache.GetIfPresent(two));
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Put(one, two);

            Assert.AreSame(two, cache.GetIfPresent(one));
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            Assert.Null(cache.GetIfPresent(two));
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            cache.Put(two, one);

            Assert.AreSame(two, cache.GetIfPresent(one));
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(2, stats.HitCount);

            Assert.AreSame(one, cache.GetIfPresent(two));
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);
        }

        [Test]
        public void test_get_all_present()
        {
            var cache = CacheBuilder<int?, int?>.NewBuilder().RecordStats().Build();
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(new Dictionary<int?, int?>(),
                cache.GetAllPresents(new int?[] {1, 2, 3}));
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Put(2, 22);

            Assert.AreEqual(new Dictionary<int?, int?> {{ 2, 22}},
                cache.GetAllPresents(new int?[] {1, 2, 3}));
            stats = cache.Stats();
            Assert.AreEqual(5, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            cache.Put(3, 33);

            Assert.AreEqual(new Dictionary<int?, int?> { { 2, 22 }, { 3, 33 } },
                cache.GetAllPresents(new int?[] { 1, 2, 3 }));
            stats = cache.Stats();
            Assert.AreEqual(6, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);

            cache.Put(1, 11);

            Assert.AreEqual(new Dictionary<int?, int?> { { 1, 11 }, { 2, 22 }, { 3, 33 } },
                cache.GetAllPresents(new int?[] { 1, 2, 3 }));
            stats = cache.Stats();
            Assert.AreEqual(6, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(6, stats.HitCount);
        }
    }
}
