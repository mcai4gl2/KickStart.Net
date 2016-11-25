using System;
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Cache;
using KickStart.Net.Collections;
using KickStart.Net.Extensions;
using KickStart.Net.Tests.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class PopulatedCachesTests
    {
        private static readonly int _warmupMin = 120;
        private static readonly int _warmupMax = 135;
        private static readonly int _warmupSize = _warmupMax - _warmupMin;

        [Test]
        public void test_size()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                Assert.AreEqual(_warmupSize, cache.Size());
                Assert.AreEqual(_warmupSize, cache.ToDictionary().Count);
            }
        }

        [Test]
        public void test_invalidate()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                Assert.AreEqual(_warmupSize, cache.Size());
                Assert.AreEqual(_warmupSize, cache.ToDictionary().Count);
                cache.InvalidateAll();
                Assert.AreEqual(0, cache.Size());
                Assert.IsTrue(cache.IsEmpty());
            }
        }

        [Test]
        public void test_ContainsKey_and_Value()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                for (var i = _warmupMin; i < _warmupMax; i++)
                {
                    Assert.IsTrue(cache.ToDictionary().ContainsKey(i));
                    Assert.AreEqual(i, cache.ToDictionary()[i]);
                }
                Assert.AreEqual(_warmupSize, cache.Stats().MissCount);
            }
        }

        [Test]
        public void test_Put()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                for (var i = _warmupMin; i < _warmupMax; i++)
                {
                    cache.Put(i, i + 100);
                    Assert.AreEqual(i + 100, cache.Get(i));
                    cache.Put(i + 10000, i + 100);
                    Assert.AreEqual(i + 100, cache.Get(i + 10000)); // This doesn't cause a cache miss
                }
                Assert.AreEqual(_warmupSize, cache.Stats().MissCount);
            }
        }

        [Test]
        public void test_Remove()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                for (var i = _warmupMin; i < _warmupMax; i++)
                {
                    Assert.AreEqual(i, cache.Remove(i));
                }
                Assert.IsTrue(cache.IsEmpty());
            }
        }

        [Test]
        public void test_remove_with_value()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                for (var i = _warmupMin; i < _warmupMax; i++)
                {
                    Assert.IsTrue(cache.Remove(i, i));
                    Assert.IsFalse(cache.Remove(i, i + 1));
                }
                Assert.IsTrue(cache.IsEmpty());
            }
        }

        [Test]
        public void test_put_all()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                WarmUp(cache);
                cache.PutAll(new Dictionary<int?, int?>() { {0, 0}, {-1, -1} });
                Assert.AreEqual(0, cache.Get(0));
                Assert.AreEqual(-1, cache.Get(-1));
                Assert.AreEqual(_warmupSize, cache.Stats().MissCount);
            }
        }

        public IEnumerable<CacheBuilder<K, V>> Caches<K, V>()
        {
            var concurrencyLevels = new [] {1, 4, 16, 64};
            var maxSizes = new [] {400, 1000};
            var initialCapacities = new [] {0, 1, 10, 100, 1000};
            var expireAfterWrites = new [] {TimeSpan.FromSeconds(1), TimeSpan.FromDays(1)};
            var expireAfterAccesses = new [] { TimeSpan.FromSeconds(1), TimeSpan.FromDays(1) };
            var refrehes = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromDays(1) };

            var combinations = new Combinations<int>(
                concurrencyLevels, 
                maxSizes,
                initialCapacities,
                expireAfterWrites.Length.Range().ToArray(),
                expireAfterAccesses.Length.Range().ToArray(),
                refrehes.Length.Range().ToArray());

            return combinations.Select(c => new CacheBuilder<K, V>()
                .WithConcurrencyLevel(c[0])
                .WithMaximumSize(c[1])
                .WithInitialCapacity(c[2])
                .WithExpireAfterWrite(expireAfterWrites[c[3]])
                .WithExpireAfterAccess(expireAfterAccesses[c[4]])
                .WithRefreshAfterWrite(refrehes[c[5]]));
        }

        public void WarmUp(ILoadingCache<int?, int?> cache)
        {
            for (var i = _warmupMin; i < _warmupMax; i++)
            {
                cache.Get(i);
            }
        }
    }
}
