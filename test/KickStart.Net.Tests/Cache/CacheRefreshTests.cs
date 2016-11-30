using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class CacheRefreshTests
    {
        [Test]
        public void test_auto_refresh()
        {
            var ticker = new FakeTicker();
            var loader = new IncrementingLoader();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithRefreshAfterWrite(TimeSpan.FromMilliseconds(3))
                .WithExpireAfterWrite(TimeSpan.FromMilliseconds(6))
                .WithTicker(ticker)
                .Build(loader);

            var expectedLoads = 0;
            var expectedReloads = 0;
            foreach (var i in 3.Range())
            {
                Assert.AreEqual(i, cache.Get(i));
                expectedLoads++;
                Assert.AreEqual(expectedLoads, loader.CountLoad);
                Assert.AreEqual(expectedReloads, loader.CountReload);
                ticker.Advance(TimeSpan.FromMilliseconds(1));
            }

            Assert.AreEqual(0, cache.Get(0));
            Assert.AreEqual(1, cache.Get(1));
            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);

            // Refreshs 0
            ticker.Advance(TimeSpan.FromMilliseconds(1));
            cache.Get(0);
            Task.Delay(500).Wait();
            Assert.AreEqual(1, cache.Get(0));
            expectedReloads++;
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);
            Assert.AreEqual(1, cache.Get(1));
            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);

            // Writing to 1 delays the refresh
            cache.Put(1, -1);
            ticker.Advance(TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(1, cache.Get(0));
            Assert.AreEqual(-1, cache.Get(1));
            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);

            // Refreshs 2
            ticker.Advance(TimeSpan.FromMilliseconds(1));
            cache.Get(2);
            Task.Delay(500).Wait();
            Assert.AreEqual(1, cache.Get(0));
            Assert.AreEqual(-1, cache.Get(1));
            Assert.AreEqual(3, cache.Get(2));
            expectedReloads++;
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);

            ticker.Advance(TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(1, cache.Get(0));
            Assert.AreEqual(-1, cache.Get(1));
            Assert.AreEqual(3, cache.Get(2));
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);

            // Refreshes 0 and 1
            ticker.Advance(TimeSpan.FromMilliseconds(1));
            cache.Get(0);
            cache.Get(1);
            Task.Delay(500).Wait();
            expectedReloads += 2;
            Assert.AreEqual(2, cache.Get(0));
            Assert.AreEqual(0, cache.Get(1));
            Assert.AreEqual(3, cache.Get(2));
            Assert.AreEqual(expectedLoads, loader.CountLoad);
            Assert.AreEqual(expectedReloads, loader.CountReload);
        }
    }

    class IncrementingLoader : ICacheLoader<int?, int?>
    {
        private int _countLoad;
        private int _countReload;
        public int CountLoad => _countLoad;
        public int CountReload => _countReload;

        public Task<int?> LoadAsync(int? key)
        {
            _countLoad++;
            return Task.FromResult(key);
        }

        public Task<int?> ReloadAsync(int? key, int? oldValue)
        {
            _countReload++;
            return Task.FromResult((int?) oldValue.Value + 1);
        }

        public IReadOnlyDictionary<int?, int?> LoadAllAsync(IReadOnlyCollection<int?> keys)
        {
            throw new NotImplementedException();
        }
    }
}
