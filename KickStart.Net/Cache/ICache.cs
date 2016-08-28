using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public interface ICache<K, V>
    {
        V GetIfPresent(K key);
        V Get(K key, Func<V> loader);
        IReadOnlyDictionary<K, V> GetAllPresents(IEnumerable<K> keys);
        void Put(K key, V value);
        void PutAll(IReadOnlyDictionary<K, V> map);
        void Invalidate(K key);
        void InvalidateAll(IEnumerable<K> keys);
        void InvalidateAll();
        long Size();
        IReadOnlyDictionary<K, V> AsMap();
        void CleanUp();
        CacheStats Stats();
    }
}
