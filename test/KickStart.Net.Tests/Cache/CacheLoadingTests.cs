using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KickStart.Net.Cache;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Cache
{
    [TestFixture]
    public class CacheLoadingTests
    {
        [Test]
        public void test_load()
        {
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(new IdentityLoader<object>());
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            var key = new object();
            Assert.AreSame(key, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            key = new object();
            Assert.AreSame(key, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(2, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            key = new object();
            cache.Refresh(key);
            Task.Delay(500).Wait(); // A wait is required for the Load Success count because refresh is async
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(3, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(key, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(3, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            Assert.AreSame(key, cache.Get(key, () => { throw new Exception();})); // This loader will not be called as we had the value cached
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(3, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(2, stats.HitCount);

            var value = new object();
            key = new object();
            Assert.AreSame(value, cache.Get(key, () => value));
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(4, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(2, stats.HitCount);
        }

        [Test]
        public void test_reload()
        {
            var one = new object();
            var two = new object();
            var loader = new DelegatingCacheLoader<object, object>(_ => Task.FromResult(one), (k, o) => Task.FromResult(two));
            var cache = CacheBuilder<object, object>.NewBuilder().RecordStats().Build(loader);

            var key = new object();
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Refresh(key);
            Task.Delay(500).Wait(); // A wait is required for the Load Success count because refresh is async
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(2, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(two, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(2, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);
        }

        [Test]
        [Ignore("Needs to be rewrite as it depends on the speed so the outcome is inconsistent")]
        public void test_refresh()
        {
            var one = new object();
            var two = new object();
            var ticker = new FakeTicker();
            var loader = new DelegatingCacheLoader<object, object>(_ => Task.Delay(100).ContinueWith(t => Task.FromResult(one)).Unwrap(), 
                                                                   (k, o) => Task.Delay(100).ContinueWith(t => Task.FromResult(two)).Unwrap());
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .WithTicker(ticker)
                .WithRefreshAfterWrite(TimeSpan.FromMilliseconds(1).Ticks)
                .Build(loader);
            
            var key = new object();
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            // Now we expired but this round, we will still get the old value
            // as the loading is async
            ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(2, stats.HitCount);

            // Let's wait a little bit for the refresh to done
            Task.Delay(500).Wait();

            Assert.AreSame(two, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(2, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);
        }

        [Test]
        public void test_refresh_get_if_present()
        {
            var one = new object();
            var two = new object();
            var ticker = new FakeTicker();
            var loader = new DelegatingCacheLoader<object, object>(_ => Task.Delay(500).ContinueWith(t => Task.FromResult(one)).Unwrap(), (k, o) => Task.FromResult(two));
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .WithTicker(ticker)
                .WithRefreshAfterWrite(TimeSpan.FromMilliseconds(1).Ticks)
                .Build(loader);

            var key = new object();
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            Assert.AreSame(one, cache.GetIfPresent(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            // Now we expired but this round, we will still get the old value
            // as the loading is async
            ticker.Advance(TimeSpan.FromMilliseconds(1).Ticks);
            Assert.AreSame(one, cache.GetIfPresent(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(2, stats.HitCount);

            // Let's wait a little bit for the refresh to done
            Task.Delay(500).Wait();

            Assert.AreSame(two, cache.GetIfPresent(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(2, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);
        }

        [Test]
        public void test_GetAll_load_multiple()
        {
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(new IdentityLoader<object>());
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(), cache.GetAll(List.Of<object>()));
            stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(1, 1), cache.GetAll(List.Of<object>(1)));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(1, 1, 2, 2, 3, 3, 4, 4), cache.GetAll(List.Of<object>(1, 2, 3, 4)));
            stats = cache.Stats();
            Assert.AreEqual(4, stats.MissCount);
            Assert.AreEqual(4, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(2, 2, 3, 3), cache.GetAll(List.Of<object>(2, 3)));
            stats = cache.Stats();
            Assert.AreEqual(4, stats.MissCount);
            Assert.AreEqual(4, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(4, 4, 5, 5), cache.GetAll(List.Of<object>(4, 5)));
            stats = cache.Stats();
            Assert.AreEqual(5, stats.MissCount);
            Assert.AreEqual(5, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(4, stats.HitCount);
        }

        [Test]
        [Ignore("LoadAll needs to be implemented")]
        public void test_GetAll_load_via_LoadAllAsync()
        {
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(new BulkCacheLoader<object, object>(new IdentityLoader<object>()));
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(), cache.GetAll(List.Of<object>()));
            stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(1, 1), cache.GetAll(List.Of<object>(1)));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(1, 1, 2, 2, 3, 3, 4, 4), cache.GetAll(List.Of<object>(1, 2, 3, 4)));
            stats = cache.Stats();
            Assert.AreEqual(4, stats.MissCount);
            Assert.AreEqual(4, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(2, 2, 3, 3), cache.GetAll(List.Of<object>(2, 3)));
            stats = cache.Stats();
            Assert.AreEqual(4, stats.MissCount);
            Assert.AreEqual(4, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(3, stats.HitCount);

            Assert.AreEqual(Dictionary.Of<object, object>(4, 4, 5, 5), cache.GetAll(List.Of<object>(4, 5)));
            stats = cache.Stats();
            Assert.AreEqual(5, stats.MissCount);
            Assert.AreEqual(5, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(4, stats.HitCount);
        }

        [Test]
        public void test_load_null()
        {
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(new ConstantLoader<object, object>(null));
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            try
            {
                cache.Get(new object());
                Assert.Fail();
            }
            catch (Exception)
            {
                
            }
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(1, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Refresh(new object());
            Task.Delay(500).Wait(); // Let's wait a little bit for the refresh to done
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(2, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            try
            {
                cache.Get(new object(), () => null);
                Assert.Fail();
            }
            catch (Exception)
            {

            }
            stats = cache.Stats();
            Assert.AreEqual(2, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(3, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            try
            {
                cache.GetAll(List.Of(new object()));
                Assert.Fail();
            }
            catch (Exception)
            {

            }
            stats = cache.Stats();
            Assert.AreEqual(3, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(4, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);
        }

        [Test]
        public void test_reload_null()
        {
            var one = new object();
            var loader = new DelegatingCacheLoader<object, object>(_ => Task.FromResult(one), (k, o) => null);
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(loader);
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            var key = new object();
            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Refresh(key);
            Task.Delay(500).Wait(); // Let's wait a little bit for the refresh to done
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(1, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(1, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);
        }

        [Test]
        public void test_reload_null_task()
        {
            var one = new object();
            var loader = new DelegatingCacheLoader<object, object>(_ => Task.FromResult(one), (k, o) => Task.FromResult<object>(null));
            var cache = CacheBuilder<object, object>.NewBuilder()
                .RecordStats()
                .Build(loader);
            var stats = cache.Stats();
            Assert.AreEqual(0, stats.MissCount);
            Assert.AreEqual(0, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            var key = new object();
            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(0, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            cache.Refresh(key);
            Task.Delay(500).Wait(); // Let's wait a little bit for the refresh to done
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(1, stats.LoadExceptionCount);
            Assert.AreEqual(0, stats.HitCount);

            Assert.AreSame(one, cache.Get(key));
            stats = cache.Stats();
            Assert.AreEqual(1, stats.MissCount);
            Assert.AreEqual(1, stats.LoadSuccessCount);
            Assert.AreEqual(1, stats.LoadExceptionCount);
            Assert.AreEqual(1, stats.HitCount);
        }

        [Test]
#if RELEASE
        [Ignore("Fail on CI")]
#endif
        public void test_reload_after_failure()
        {
            var count = 0;
            var loader = CacheLoaders.From<int?, int?>(key =>
            {
                if (count ++ == 0)
                    throw new Exception();
                return key;
            });
            var listener = new CountingRemovalListener<int?, int?>();
            var cache = CacheBuilder<int?, int?>.NewBuilder()
                .WithRemovalListener(listener)
                .Build(loader);

            Assert.Throws<AggregateException>(() => cache.Get(1));

            Assert.AreEqual(1, cache.Get(1));
            Assert.AreEqual(0, listener.Count);

            count = 0;
            cache.Refresh(2);
            Task.Delay(500).Wait();

            Assert.AreEqual(2, cache.Get(2));
            Assert.AreEqual(0, listener.Count);
        }

        [Test]
#if !NET_CORE
        [Timeout(1000)]
#endif
#if RELEASE
        [Ignore("Fail on CI")]
#endif
        public async Task test_concurrent_loading_default()
        {
            var count = 10;
            var adder = new LongAdder();
            var tcs = new TaskCompletionSource<bool>();
            var value = new object();

            var cache = CacheBuilder<string, object>.NewBuilder()
                .Build(CacheLoaders.From<string, object>(key =>
                {
                    adder.Add(1);
                    tcs.Task.Wait();
                    return value;
                }));

            var results = count.Range().Select(_ => Task.Factory.StartNew(() => cache.Get("abc"))).ToList();
            tcs.SetResult(true);
            await Task.WhenAll(results);

            Assert.AreEqual(1, adder.Sum());
            Assert.IsTrue(results.All(r => r.Result == value));
            Assert.AreEqual(count, results.Count);
        }

        [Test]
#if !NET_CORE
        [Timeout(10*000)]
#endif
        [Ignore("Fail on CI")]
        public async Task test_concurrent_loading_null()
        {
            var count = 10;
            var adder = new LongAdder();
            var tcs = new TaskCompletionSource<bool>();

            var cache = CacheBuilder<string, object>.NewBuilder()
                .Build(CacheLoaders.From<string, object>(key =>
                {
                    adder.Add(1);
                    tcs.Task.Wait();
                    return default(object);
                }));

            var results = count.Range().Select(_ => Task.Factory.StartNew(() => cache.Get("abc"))).ToList();
            await Task.Delay(500); // Waiting for all tasks to kick off, otherwise we may receive multiple call to loader because the task above kicked off too late
            tcs.SetResult(true);
            try
            {
                await Task.WhenAll(results);
                Assert.Fail();
            }
            catch (Exception)
            {
               
            }

            Assert.AreEqual(1, adder.Sum());
        }
    } 

    class ConstantLoader<K, V> : ICacheLoader<K, V>
    {
        private readonly V _constant;

        public ConstantLoader(V constant)
        {
            _constant = constant;
        } 

        public Task<V> LoadAsync(K key)
        {
            return Task.FromResult(_constant);
        }

        public Task<V> ReloadAsync(K key, V oldValue)
        {
            return Task.FromResult(_constant);
        }

        public IReadOnlyDictionary<K, V> LoadAllAsync(IReadOnlyCollection<K> keys)
        {
            throw new NotImplementedException();
        }
    }

    class DelegatingCacheLoader<K, V> : ICacheLoader<K, V>
    {
        private readonly Func<K, Task<V>> _loadAsync;
        private readonly Func<K, V, Task<V>> _reloadAsync;

        public DelegatingCacheLoader(Func<K, Task<V>> loadFunc, Func<K, V, Task<V>> reloadFunc)
        {
            _loadAsync = loadFunc;
            _reloadAsync = reloadFunc;
        } 

        public Task<V> LoadAsync(K key)
        {
            return _loadAsync(key);
        }

        public Task<V> ReloadAsync(K key, V oldValue)
        {
            return _reloadAsync(key, oldValue);
        }

        public IReadOnlyDictionary<K, V> LoadAllAsync(IReadOnlyCollection<K> keys)
        {
            throw new NotImplementedException();
        }
    }

    class BulkCacheLoader<K, V> : ICacheLoader<K, V>
    {
        private readonly ICacheLoader<K, V> _cacheLoader;

        public BulkCacheLoader(ICacheLoader<K, V> cacheLoader)
        {
            _cacheLoader = cacheLoader;
        }  

        public Task<V> LoadAsync(K key)
        {
            return _cacheLoader.LoadAsync(key);
        }

        public Task<V> ReloadAsync(K key, V oldValue)
        {
            return _cacheLoader.ReloadAsync(key, oldValue);
        }

        public IReadOnlyDictionary<K, V> LoadAllAsync(IReadOnlyCollection<K> keys)
        {
            return keys.ToDictionary(k => k, k => _cacheLoader.LoadAsync(k).Result);
        }
    }

    class IdentityLoader<K> : ICacheLoader<K, K>
    {
        public Task<K> LoadAsync(K key)
        {
            return Task.FromResult(key);
        }

        public Task<K> ReloadAsync(K key, K oldValue)
        {
            return Task.FromResult(key);
        }

        public IReadOnlyDictionary<K, K> LoadAllAsync(IReadOnlyCollection<K> keys)
        {
            return keys.ToDictionary(k => k, k => k);
        }
    }
}
