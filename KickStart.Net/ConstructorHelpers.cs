using System.Collections.Generic;

namespace KickStart.Net
{
    public static class Dictionary
    {
        public static Dictionary<K, V> Of<K, V>()
        {
            return new Dictionary<K, V>();
        }

        public static Dictionary<K, V> Of<K, V>(K k1, V v1)
        {
            return new Dictionary<K, V> {{k1, v1}};
        }

        public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2)
        {
            return new Dictionary<K, V> {{k1, v1}, {k2, v2}};
        }

        public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2, K k3, V v3)
        {
            return new Dictionary<K, V> {{k1, v1}, {k2, v2}, {k3, v3}};
        }

        public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2, K k3, V v3, K k4, V v4)
        {
            return new Dictionary<K, V> {{k1, v1}, {k2, v2}, {k3, v3}, {k4, v4}};
        }

        public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2, K k3, V v3, K k4, V v4, K k5, V v5)
        {
            return new Dictionary<K, V> {{k1, v1}, {k2, v2}, {k3, v3}, {k4, v4}, {k5, v5}};
        }
    }

    public static class List
    {
        public static List<T> Of<T>()
        {
            return new List<T>();
        }

        public static List<T> Of<T>(T v1)
        {
            return new List<T> {v1};
        }

        public static List<T> Of<T>(T v1, T v2)
        {
            return new List<T> {v1, v2};
        }

        public static List<T> Of<T>(T v1, T v2, T v3)
        {
            return new List<T> {v1, v2, v3};
        }

        public static List<T> Of<T>(params T[] values)
        {
            return new List<T>(values);
        }
    }
}
