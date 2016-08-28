using System;
using System.Collections.Generic;
using System.Linq;

namespace KickStart.Net.Cache
{
    public abstract class AbstractCache<K, V> : ICache<K, V>
    {
        public AbstractCache() { }

        public V GetIfPresent(K key)
        {
            throw new NotImplementedException();
        }

        public virtual V Get(K key, Func<V> loader)
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyDictionary<K, V> GetAllPresents(IEnumerable<K> keys)
        {
            return keys.ToDictionary(key => key, GetIfPresent);
        }

        public virtual void Put(K key, V value)
        {
            throw new NotImplementedException();
        }

        public virtual void PutAll(IReadOnlyDictionary<K, V> map)
        {
            foreach (var kvp in map)
            {
                Put(kvp.Key, kvp.Value);
            }
        }

        public virtual void Invalidate(K key)
        {
            throw new NotImplementedException();
        }

        public virtual void InvalidateAll(IEnumerable<K> keys)
        {
            throw new NotImplementedException();
        }

        public virtual void InvalidateAll()
        {
            throw new NotImplementedException();
        }

        public virtual long Size()
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyDictionary<K, V> AsMap()
        {
            throw new NotImplementedException();
        }

        public virtual void CleanUp() { }
        public CacheStats Stats()
        {
            throw new NotImplementedException();
        }
    }
}
