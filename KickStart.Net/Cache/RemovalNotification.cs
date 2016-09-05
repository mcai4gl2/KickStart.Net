namespace KickStart.Net.Cache
{
    public class RemovalNotification<K, V>
    {
        public K Key { get; }
        public V Value { get; }
        public RemovalCause Cause { get; }

        private RemovalNotification(K key, V value, RemovalCause cause)
        {
            Key = key;
            Value = value;
            Cause = cause;
        } 

        public static RemovalNotification<K, V> Create(K key, V value, RemovalCause cause)
        {
            return new RemovalNotification<K, V>(key, value, cause);
        }
    }
}
