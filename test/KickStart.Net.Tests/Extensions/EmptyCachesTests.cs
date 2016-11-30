using System;
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Cache;
using KickStart.Net.Collections;
using KickStart.Net.Extensions;
using KickStart.Net.Tests.Cache;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class EmptyCachesTests
    {
        [Test]
        public void test_empty()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                Assert.IsTrue(cache.IsEmpty());
                Assert.AreEqual(0, cache.Size());
                Assert.AreEqual(0, cache.ToDictionary().Count);
            }
        }

        [Test]
        public void test_invalidate()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                cache.Get(1);
                cache.Get(2);
                cache.Invalidate(1);
                cache.Invalidate(2);
                cache.Invalidate(0);

                Assert.IsTrue(cache.IsEmpty());
                Assert.AreEqual(0, cache.Size());
                Assert.AreEqual(0, cache.ToDictionary().Count);
            }
        }

        [Test]
        public void test_invalidate_all()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                cache.Get(1);
                cache.Get(2);

                cache.InvalidateAll();

                Assert.IsTrue(cache.IsEmpty());
                Assert.AreEqual(0, cache.Size());
                Assert.AreEqual(0, cache.ToDictionary().Count);
            }
        }

        [Test]
        public void test_get_null()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                Assert.Throws<NullReferenceException>(() => cache.Get(null));

                Assert.IsTrue(cache.IsEmpty());
                Assert.AreEqual(0, cache.Size());
                Assert.AreEqual(0, cache.ToDictionary().Count);
            }
        }

        [Test]
        public void test_remove()
        {
            foreach (var cache in Caches<int?, int?>().Select(b => b.RecordStats().Build(new IdentityLoader<int?>())))
            {
                cache.Get(1);
                cache.Get(2);

                cache.Remove(1);
                cache.Remove(2, 2);

                Assert.IsTrue(cache.IsEmpty());
                Assert.AreEqual(0, cache.Size());
                Assert.AreEqual(0, cache.ToDictionary().Count);
            }
        }

        public IEnumerable<CacheBuilder<K, V>> Caches<K, V>()
        {
            var concurrencyLevels = new[] { 1, 4, 16, 64 };
            var maxSizes = new[] { 0, 1, 10, 100, 1000 };
            var initialCapacities = new[] { 0, 1, 10, 100, 1000 };
            var expireAfterWrites = new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), TimeSpan.FromDays(1) };
            var expireAfterAccesses = new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), TimeSpan.FromDays(1) };
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
    }
}
