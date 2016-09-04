using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        [Test]
        public void test_max_size()
        {
            var maxSize = 1000;
            var listener = new CountingRemovalListener<int?, int?>();
            var loader = new IdentityLoader<int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithMaximumSize(maxSize)
                .WithRemovalListener(listener)
                .Build(loader);
            foreach (var i in (2*maxSize).Range())
            {
                cache.Get(i);
                Assert.That(cache.Size() <= maxSize);
            }

            Assert.AreEqual(maxSize, ((LocalLoadingCache<int?, int?>)cache)._localCache.AccessQueueSize());
            Assert.AreEqual(maxSize, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(maxSize, listener.Count);
        }

        [Test]
        public void test_max_weight()
        {
            var maxSize = 1000;
            var listener = new CountingRemovalListener<int?, int?>();
            var loader = new IdentityLoader<int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithMaximumWeight(2*maxSize)
                .WithWeigher(new ConstantWeigher<int?, int?>(2))
                .WithRemovalListener(listener)
                .Build(loader);
            foreach (var i in (2*maxSize).Range())
            {
                cache.Get(i);
                Assert.That(cache.Size() <= maxSize);
            }

            Assert.AreEqual(maxSize, ((LocalLoadingCache<int?, int?>)cache)._localCache.AccessQueueSize());
            Assert.AreEqual(maxSize, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(maxSize, listener.Count);
        }

        [Test]
        public void zero_weight_will_not_be_evicted()
        {
            var listener = new CountingRemovalListener<int?, int?>();
            var loader = new IdentityLoader<int?>();
            var weigher = Weighers.From<int?, int?>((k, v) => k.Value % 2);

            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(0)
                .WithWeigher(weigher)
                .WithRemovalListener(listener)
                .Build(loader);

            // 1 won't be cached
            Assert.AreEqual(1, cache.Get(1));
            Assert.AreEqual(0, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(1, listener.Count);

            // 2 will be cached
            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(1, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(1, listener.Count);

            // 4 will be cached
            Assert.AreEqual(4, cache.Get(4));
            Assert.AreEqual(2, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(1, listener.Count);

            // 5 won't be cached
            Assert.AreEqual(5, cache.Get(5));
            Assert.AreEqual(2, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(2, listener.Count);
        }

        [Test]
        public void single_entry_weight_bigger_than_max_weight()
        {
            var listener = new CountingRemovalListener<int?, int?>();
            var loader = new IdentityLoader<int?>();
            var weigher = Weighers.From<int?, int?>((k, v) => v.Value);

            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(4)
                .WithWeigher(weigher)
                .WithRemovalListener(listener)
                .Build(loader);

            // 2 will be cached
            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(1, cache.Size());

            // 3 will be cached and 2 will be evicted
            Assert.AreEqual(3, cache.Get(3));
            Assert.AreEqual(1, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(1, listener.Count);
            Assert.AreEqual(2, listener.RemovalNotifications.Select(n => n.Value.Value).First());

            // 5 will not be cached
            Assert.AreEqual(5, cache.Get(5));
            Assert.AreEqual(1, cache.Size());
            ((LocalLoadingCache<int?, int?>)cache)._localCache.ProcessPendingNotifications();
            Assert.AreEqual(2, listener.Count);
            Assert.AreEqual(new[] {2, 5}, listener.RemovalNotifications.Select(n => n.Value.Value).ToArray());

            // 1 will be cached, no eviction
            Assert.AreEqual(1, cache.Get(1));
            Assert.AreEqual(2, listener.Count);
            Assert.AreEqual(2, cache.Size());
            Assert.AreEqual(new[] { 2, 5 }, listener.RemovalNotifications.Select(n => n.Value.Value).ToArray());

            // 4 will be cached and 1, 3 will be evicted
            Assert.AreEqual(4, cache.Get(4));
            Assert.AreEqual(4, listener.Count);
            Assert.AreEqual(1, cache.Size());
            Assert.AreEqual(new[] { 2, 5, 3, 1 }, listener.RemovalNotifications.Select(n => n.Value.Value).ToArray());
        }

        [Test]
        public void test_max_weight_overflow()
        {
            var listener = new CountingRemovalListener<ObjectWithHash, int?>();
            var loader = new ConstantLoader<ObjectWithHash, int?>(1);
            var weigher = new ConstantWeigher<ObjectWithHash, int?>(int.MaxValue); // 2147483647

            var cache = CacheBuilder<ObjectWithHash, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(1L << 31) // 2147483648
                .WithWeigher(weigher)
                .WithRemovalListener(listener)
                .Build(loader);

            cache.Get(new ObjectWithHash(0));
            Assert.AreEqual(0, listener.Count);
            cache.Get(new ObjectWithHash(0));
            Assert.AreEqual(1, listener.Count);
        }

        [Test]
        public void test_size_eviction_lru()
        {
            var loader = new IdentityLoader<int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumSize(10)
                .Build(loader);

            // warm up
            foreach (var i in 10.Range())
            {
                cache.Get(i);
            }
            var keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(10, keys.Count);
            Assert.AreEqual(new SortedSet<int?>(((int?)10).Range()), new SortedSet<int?>(keys));

            cache.Get(0);
            cache.Get(1);
            cache.Get(2);

            // 3, 4, 5 shall be evicted
            cache.Get(10);
            cache.Get(11);
            cache.Get(12);

            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(10, keys.Count);
            Assert.IsFalse(keys.Contains(3));
            Assert.IsFalse(keys.Contains(4));
            Assert.IsFalse(keys.Contains(5));

            cache.Get(6);
            cache.Get(7);
            cache.Get(8);

            // 9, 0, 1 shall be evicted
            cache.Get(13);
            cache.Get(14);
            cache.Get(15);

            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(10, keys.Count);
            Assert.IsFalse(keys.Contains(9));
            Assert.IsFalse(keys.Contains(0));
            Assert.IsFalse(keys.Contains(1));
        }

        [Test]
        public void test_weight_eviction_lru()
        {
            var loader = new IdentityLoader<int?>();
            var weigher = new ForwardingWeigher<int?, int?>((k, v) => k.Value);
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(45)
                .WithWeigher(weigher)
                .Build(loader);

            // warm up
            foreach (var i in 10.Range())
            {
                cache.Get(i);
            }
            var keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(10, keys.Count);
            Assert.AreEqual(new SortedSet<int?>(((int?)10).Range()), new SortedSet<int?>(keys));

            cache.Get(0);
            cache.Get(1);
            cache.Get(2);

            // 3, 4, 5 shall be evicted
            cache.Get(10);

            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(8, keys.Count);
            Assert.IsFalse(keys.Contains(3));
            Assert.IsFalse(keys.Contains(4));
            Assert.IsFalse(keys.Contains(5));

            cache.Get(6);
            cache.Get(7);
            cache.Get(8);

            // 9, 1, 2, 10 shall be evicted
            cache.Get(15);

            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(5, keys.Count);
            Assert.IsFalse(keys.Contains(9));
            Assert.IsFalse(keys.Contains(10));
            Assert.IsFalse(keys.Contains(1));
            Assert.IsFalse(keys.Contains(2));

            cache.Get(9);
            ((LocalLoadingCache<int?, int?>)cache)._localCache.DrainRecencyQueues();
            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(6, keys.Count);
            Assert.IsTrue(keys.Contains(9));

            // 6 evicted
            cache.Get(1);
            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(6, keys.Count);
            Assert.IsTrue(keys.Contains(9));
            Assert.IsFalse(keys.Contains(6));
            Assert.IsTrue(keys.Contains(1));
        }

        [Test]
        public void test_weight_eviction_on_overweight_entry()
        {
            var loader = new IdentityLoader<int?>();
            var weigher = new ForwardingWeigher<int?, int?>((k, v) => k.Value);
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumWeight(45)
                .WithWeigher(weigher)
                .Build(loader);

            // warm up
            foreach (var i in 10.Range())
            {
                cache.Get(i);
            }
            var keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(10, keys.Count);
            Assert.AreEqual(new SortedSet<int?>(((int?)10).Range()), new SortedSet<int?>(keys));

            cache.Get(45);
            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains(45));

            cache.Get(46);
            keys = cache.ToDictionary().Keys.ToList();
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains(45));
            Assert.IsTrue(keys.Contains(0));
        }

        [Test]
        public void test_invalidate_all_reset_size()
        {
            var loader = new IdentityLoader<int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithConcurrencyLevel(1)
                .WithMaximumSize(10)
                .Build(loader);

            var keys = cache.ToDictionary().Keys;
            Assert.AreEqual(0, keys.Count());

            foreach (var i in 5.Range())
                cache.Get(i);
            keys = cache.ToDictionary().Keys;
            Assert.AreEqual(5, keys.Count());

            cache.InvalidateAll();
            keys = cache.ToDictionary().Keys;
            Assert.AreEqual(0, keys.Count());

            for (int i = 5; i <= 12; i++)
                cache.Get(i);
            keys = cache.ToDictionary().Keys;
            Assert.AreEqual(8, keys.Count());
        }
    }

    static class LocalCacheExtensions
    {
        public static int AccessQueueSize<K, V>(this LocalCache<K, V> cache)
        {
            return cache.Segments.Sum(s => s.AccessQueue.Count());
        }

        public static void DrainRecencyQueues<K, V>(this LocalCache<K, V> cache)
        {
            foreach (var segment in cache.Segments)
            {
                segment.DrainRecencyQueue();
            }
        }
    }

    class ObjectWithHash
    {
        private readonly int _hash;

        public ObjectWithHash(int hash)
        {
            _hash = hash;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}

