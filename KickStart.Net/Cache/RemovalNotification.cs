using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public class RemovalNotification<K, V>
    {
        private readonly K _key;
        private readonly V _value;
        private RemovalCause Cause { get; }

        private RemovalNotification(K key, V value, RemovalCause cause)
        {
            _key = key;
            _value = value;
            Cause = cause;
        } 

        public static RemovalNotification<K, V> Create(K key, V value, RemovalCause cause)
        {
            return new RemovalNotification<K, V>(key, value, cause);
        }
    }
}
