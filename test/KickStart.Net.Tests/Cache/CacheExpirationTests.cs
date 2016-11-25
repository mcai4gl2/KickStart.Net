using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using NSubstitute;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class CacheExpirationTests
    {
        [Test]
        public void test_explicit_expire_after_write()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var loader = Substitute.For<ICacheLoader<int?, int?>>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterWrite(new TimeSpan(0, 0, 0, 1)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .Build(loader);
            loader.LoadAsync(Arg.Any<int?>()).Returns(i => Task.FromResult(i.ArgAt<int?>(0)));

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(i, cache.Get(i));
            }

            loader.Received(10).LoadAsync(Arg.Any<int?>());
            
            foreach (var i in 10.Range())
            {
                loader.ClearReceivedCalls();
                Assert.AreEqual(i, cache.Get(i));
                loader.DidNotReceive().LoadAsync(Arg.Any<int?>()); // All values are cached
            }

            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(10, removalListener.Count);
            Assert.IsTrue(removalListener.RemovalNotifications.All(n => n.Cause == RemovalCause.Expired));

            // Expire again will not expire more
            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(10, removalListener.Count);
        }

        [Test]
        public void test_explicit_expire_after_access()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var loader = Substitute.For<ICacheLoader<int?, int?>>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterAccess(TimeSpan.FromMilliseconds(1)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .RecordStats()
                .Build(loader);
            loader.LoadAsync(Arg.Any<int?>()).Returns(i => Task.FromResult(i.ArgAt<int?>(0)));

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(i, cache.Get(i));
            }

            loader.Received(10).LoadAsync(Arg.Any<int?>());

            foreach (var i in 10.Range())
            {
                loader.ClearReceivedCalls();
                Assert.AreEqual(i, cache.Get(i));
                loader.DidNotReceive().LoadAsync(Arg.Any<int?>()); // All values are cached
            }

            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(10, removalListener.Count);
            Assert.IsTrue(removalListener.RemovalNotifications.All(n => n.Cause == RemovalCause.Expired));

            // Expire again will not expire more
            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(10, removalListener.Count);
        }

        [Test]
        public void test_expire_after_write_via_get()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var loader = Substitute.For<ICacheLoader<int?, int?>>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterWrite(new TimeSpan(0, 0, 0, 1)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .Build(loader);
            loader.LoadAsync(Arg.Any<int?>()).Returns(i => Task.FromResult(i.ArgAt<int?>(0)));

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(i, cache.Get(i));
            }

            loader.Received(10).LoadAsync(Arg.Any<int?>());

            foreach (var i in 10.Range())
            {
                loader.ClearReceivedCalls();
                Assert.AreEqual(i, cache.Get(i));
                loader.DidNotReceive().LoadAsync(Arg.Any<int?>()); // All values are cached
            }

            ticker.Advance(new TimeSpan(0, 0, 0, 1).Ticks);

            cache.Get(11);

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(default(int?), cache.GetIfPresent(i)); // all expired
            }

            Assert.AreEqual(11, cache.GetIfPresent(11)); // 11 is not expired yet

            foreach (var i in 10.Range())
            {
                loader.ClearReceivedCalls();
                Assert.AreEqual(i, cache.Get(i));
                loader.Received(1).LoadAsync(Arg.Any<int?>()); // Values no longer cached
            }

            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(21, removalListener.Count);
            Assert.IsTrue(removalListener.RemovalNotifications.All(n => n.Cause == RemovalCause.Expired));
        }

        [Test]
        public void test_expire_after_access_via_get()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var loader = Substitute.For<ICacheLoader<int?, int?>>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterAccess(new TimeSpan(0, 0, 0, 1)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .Build(loader);
            loader.LoadAsync(Arg.Any<int?>()).Returns(i => Task.FromResult(i.ArgAt<int?>(0)));

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(i, cache.Get(i));
            }

            loader.Received(10).LoadAsync(Arg.Any<int?>());

            foreach (var i in 10.Range())
            {
                loader.ClearReceivedCalls();
                Assert.AreEqual(i, cache.Get(i));
                loader.DidNotReceive().LoadAsync(Arg.Any<int?>()); // All values are cached
            }

            ticker.Advance(new TimeSpan(0, 0, 0, 1).Ticks);

            foreach (var i in 10.Range())
            {
                Assert.AreEqual(default(int?), cache.GetIfPresent(i)); // all expired
            }

            ((LocalManualCache<int?, int?>)cache)._localCache.ExpireEntries(new TimeSpan(0, 0, 0, 1).Ticks, ticker);
            Assert.AreEqual(10, removalListener.Count);
            Assert.IsTrue(removalListener.RemovalNotifications.All(n => n.Cause == RemovalCause.Expired));
        }

        [Test]
        public void test_removal_listener_called_when_expire_after_write()
        {
            var ticker = new FakeTicker();
            var evctionCount = 0;
            var applyCount = 0;
            var totalSum = 0;

            var removalListener = RemovalListeners.Forwarding<int?, LongAdder>(n =>
            {
                if (n.Cause.WasEvicted())
                {
                    evctionCount++;
                    totalSum += (int)n.Value.Sum();
                }
            });
            var loader = CacheLoaders.From<int?, LongAdder>(k =>
            {
                applyCount += 1;
                return new LongAdder();
            });

            var cache = CacheBuilder<int?, LongAdder>.NewBuilder()
                .WithRemovalListener(removalListener)
                .WithExpireAfterWrite(TimeSpan.FromMilliseconds(10))
                .WithTicker(ticker)
                .Build(loader);

            foreach (var i in 100.Range())
            {
                cache.Get(10).Increment();
                ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            }

            cache.Stats().P();

            Assert.AreEqual(evctionCount + 1, applyCount);
            var remaining = cache.Get(10);
            Assert.AreEqual(100, totalSum + remaining.Sum());
        }

        [Test]
        public void test_invalidate_all()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterAccess(new TimeSpan(0, 0, 0, 1)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .Build();
            cache.Put(1, 1);
            ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            cache.InvalidateAll();

            Assert.AreEqual(1, removalListener.Count);
            Assert.That(removalListener.RemovalNotifications.All(n => n.Cause == RemovalCause.Explicit));
        }

        [Test]
        public void test_expiration_order_access()
        {
            var ticker = new FakeTicker();
            var removalListener = new CountingRemovalListener<int?, int?>();
            var loader = Substitute.For<ICacheLoader<int?, int?>>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithExpireAfterAccess(new TimeSpan(0, 0, 0, 11)) // Setting expire after access
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .Build(loader);
            loader.LoadAsync(Arg.Any<int?>()).Returns(i => Task.FromResult(i.ArgAt<int?>(0)));

            foreach (var i in 10.Range())
            {
                cache.Get(i);
                ticker.Advance(TimeSpan.FromMilliseconds(1));
            }

        }
    }

    static class CacheTestingExtensions
    {
        public static void ExpireEntries<K, V>(this LocalCache<K, V> cache, long expiringTime, FakeTicker ticker)
        {
            foreach (var segment in cache.Segments)
            {
                segment.CleanUp();
            }

            ticker.Advance(2*expiringTime);

            var now = ticker.Read();
            foreach (var segment in cache.Segments)
            {
                segment.ExpireEntries(now);
                segment.CleanUp();
            }

            cache.ProcessPendingNotifications();
        }
    }

    class CountingRemovalListener<K, V> : IRemovalListener<K, V>
    {
        private int _count;
        private readonly List<RemovalNotification<K, V>> _removalNotifications;

        public int Count => _count;
        public List<RemovalNotification<K, V>> RemovalNotifications => _removalNotifications; 

        public CountingRemovalListener()
        {
            _count = 0;
            _removalNotifications = new List<RemovalNotification<K, V>>();
        }   

        public void OnRemoval(RemovalNotification<K, V> notification)
        {
            _count += 1;
            _removalNotifications.Add(notification);
        }
    }
}
