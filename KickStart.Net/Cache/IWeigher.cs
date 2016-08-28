using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public interface IWeigher<K, V>
    {
        int Weigh(K key, V value);
    }

    public static class Weighers
    {
        public static IWeigher<K, V> One<K, V>()
        {
            return new OneWeigher<K, V>();
        } 
    }

    public class OneWeigher<K, V> : IWeigher<K, V>
    {
        public int Weigh(K key, V value)
        {
            return 1;
        }
    }
}
