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

        public static IWeigher<K, V> Constant<K, V>(int constant)
        {
            return new ConstantWeigher<K, V>(constant);
        }  
    }

    class OneWeigher<K, V> : IWeigher<K, V>
    {
        public int Weigh(K key, V value)
        {
            return 1;
        }
    }

    class ConstantWeigher<K, V> : IWeigher<K, V>
    {
        private readonly int _constant;

        public ConstantWeigher(int constant)
        {
            _constant = constant;
        }  

        public int Weigh(K key, V value)
        {
            return _constant;
        }
    }
}
