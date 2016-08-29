using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace KickStart.Net.Cache
{
    public interface IRemovalListener<K, V>
    {
        void OnRemoval(RemovalNotification<K, V> notification);
    }

    public static class RemovalListeners
    {
        public static IRemovalListener<K, V> Asynchronous<K, V>(IRemovalListener<K, V> listener)
        {
            return new AsynchronousRemovalListener<K, V>(listener);
        }

        public static IRemovalListener<K, V> Null<K, V>()
        {
            return new NullRemovalListener<K, V>();
        }

        public static IRemovalListener<K, V> Forwarding<K, V>(Action<RemovalNotification<K, V>> listener)
        {
            return new ForwardingRemovalListener<K, V>(listener);
        } 
    }

    class NullRemovalListener<K, V> : IRemovalListener<K, V>
    {
        public void OnRemoval(RemovalNotification<K, V> notification) { }
    }

    class AsynchronousRemovalListener<K, V> : IRemovalListener<K, V>
    {
        private readonly IRemovalListener<K, V> _listener; 

        public AsynchronousRemovalListener(IRemovalListener<K, V> listener)
        {
            _listener = listener;
        }

        public void OnRemoval(RemovalNotification<K, V> notification)
        {
            Task.Factory.StartNew(() => _listener.OnRemoval(notification));
        }
    }

    class ForwardingRemovalListener<K, V> : IRemovalListener<K, V>
    {
        private readonly Action<RemovalNotification<K, V>> _listener;

        public ForwardingRemovalListener(Action<RemovalNotification<K, V>> listener)
        {
            Contract.Assert(listener != null);
            _listener = listener;
        } 

        public void OnRemoval(RemovalNotification<K, V> notification)
        {
            _listener(notification);
        }
    }
}
