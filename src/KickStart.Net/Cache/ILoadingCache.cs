using System.Collections.Generic;

namespace KickStart.Net.Cache
{
    public interface ILoadingCache<K, V> : ICache<K, V>
    {
        V Get(K key);
        IReadOnlyDictionary<K, V> GetAll(IEnumerable<K> keys);
        void Refresh(K key);
    }
}
