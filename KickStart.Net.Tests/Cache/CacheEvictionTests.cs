using System;
using System.Linq;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using KickStart.Net.Tests.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]    
    public class CacheEvictionTests
    {
        [Test]
        public void test_max_segment_size()
        {
            foreach (var i in 1000.Range())
            {
                var j = i + 1;
                var cache = CacheBuilder<object, object>.NewBuilder()
                    .WithMaximumSize(j)
                    .Build(new IdentityLoader<object>());
                var total = ((LocalLoadingCache<object, object>) cache)._localCache.Segments.Sum(s => s.MaxSegmentWeight);
                Assert.AreEqual(j, total);
            }
        }

        [Test]
        public void test_max_segment_weight()
        {
            foreach (var i in 1000.Range())
            {
                var j = i + 1;
                var cache = CacheBuilder<object, object>.NewBuilder()
                    .WithMaximumWeight(j)
                    .WithWeigher(Weighers.One<object, object>())
                    .Build(new IdentityLoader<object>());
                var total = ((LocalLoadingCache<object, object>)cache)._localCache.Segments.Sum(s => s.MaxSegmentWeight);
                Assert.AreEqual(j, total);
            }
        }

        [Test]
        public void test_max_size_on_one_segment()
        {
            var maxSize = 1000;
            var cache = CacheBuilder<object, object>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumSize(maxSize)
                .Build(new IdentityLoader<object>());
            foreach (var i in (2 * maxSize).Range())
            {
                cache.Get(i);
                Assert.AreEqual(Math.Min(i + 1, maxSize), cache.Size());
            }

            Assert.AreEqual(maxSize, cache.Size());
        }

        [Test]
        public void test_max_weight_one_segment()
        {
            var maxSize = 1000;
            var cache = CacheBuilder<object, object>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(2*maxSize)
                .WithWeigher(Weighers.Constant<object, object>(2))
                .Build(new IdentityLoader<object>());
            foreach (var i in (2*maxSize).Range())
            {
                cache.Get(i);
                Assert.AreEqual(Math.Min(i + 1, maxSize), cache.Size());
            }

            Assert.AreEqual(maxSize, cache.Size());
        }
    }

    public static class LocalCacheExtensions
    {
        
    }
}
