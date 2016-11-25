using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public interface ICacheLoader<K, V>
    {
        Task<V> LoadAsync(K key);
        Task<V> ReloadAsync(K key, V oldValue);
        IReadOnlyDictionary<K, V> LoadAllAsync(IReadOnlyCollection<K> keys);
    }

    public static class CacheLoaders
    {
        public static ICacheLoader<K, V> From<K, V>(Func<K, Task<V>> func)
        {
            return new DelegateCacheLoader<K, V>(func);
        }

        public static ICacheLoader<K, V> From<K, V>(Func<K, V> func)
        {
            return new DelegateCacheLoader<K, V>(k => Task.FromResult(func(k)));
        }

        public static ICacheLoader<K, V> From<K, V>(Func<V> func)
        {
            return new DelegateCacheLoader<K, V>(_ => Task.FromResult(func()));
        }

        class DelegateCacheLoader<K, V> : ICacheLoader<K, V>
        {
            private readonly Func<K, Task<V>> _func;

            public DelegateCacheLoader(Func<K, Task<V>> func)
            {
                _func = func;
            }  

            public Task<V> LoadAsync(K key)
            {
                return _func(key);
            }

            public Task<V> ReloadAsync(K key, V oldValue)
            {
                return LoadAsync(key);
            }

            public IReadOnlyDictionary<K, V> LoadAllAsync(IReadOnlyCollection<K> keys)
            {
                throw new NotImplementedException();
            }
        }   
    }
}
