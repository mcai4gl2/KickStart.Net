using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public abstract class AbstractLoadingCache<K, V> : AbstractCache<K, V>, ILoadingCache<K, V>
    {
        public AbstractLoadingCache() { }

        public V Get(K key)
        {
            return GetIfPresent(key);
        }

        public virtual void Refresh(K key)
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyDictionary<K, V> GetAll(IEnumerable<K> keys)
        {
            return GetAllPresents(keys);
        }


    }
}
