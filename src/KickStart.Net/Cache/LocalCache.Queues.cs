using System;
using System.Collections;
using System.Collections.Generic;

namespace KickStart.Net.Cache
{
    partial class LocalCache<K, V>
    {
        [VisibleForTesting]
        internal class AccessQueue<K, V> : IEnumerable<IReferenceEntry<K, V>>
        {
            readonly IReferenceEntry<K, V> _head = new AccessReferenceEntry<K, V>();

            public bool Offer(IReferenceEntry<K, V> entry)
            {
                Queues.ConnectAccessOrder(entry.PreviousInAccessQueue, entry.NextInAccessQueue);
                Queues.ConnectAccessOrder(_head.PreviousInAccessQueue, entry);
                Queues.ConnectAccessOrder(entry, _head);
                return true;
            }

            public IReferenceEntry<K, V> Peek()
            {
                var next = _head.NextInAccessQueue;
                return (next == _head) ? null : next;
            }

            public IReferenceEntry<K, V> Poll()
            {
                var next = _head.NextInAccessQueue;
                if (next == _head) return null;
                Remove(next);
                return next;
            }

            public bool Remove(IReferenceEntry<K, V> e)
            {
                var previous = e.PreviousInAccessQueue;
                var next = e.NextInAccessQueue;
                Queues.ConnectAccessOrder(previous, next);
                Queues.NullifyAccessOrder(e);
                return !(next is NullReferenceEntry<K, V>);
            }

            public bool Contains(IReferenceEntry<K, V> e)
            {
                return !(e.NextInAccessQueue is NullReferenceEntry<K, V>);
            }

            public bool IsEmpty()
            {
                return _head.NextInAccessQueue == _head;
            }

            public int Count()
            {
                int size = 0;
                for (var e = _head.NextInAccessQueue; e != _head; e = e.NextInAccessQueue)
                    size++;
                return size;
            }

            public void Clear()
            {
                var e = _head.NextInAccessQueue;
                while (e != _head)
                {
                    var next = e.NextInAccessQueue;
                    Queues.NullifyAccessOrder(e);
                    e = next;
                }
                _head.NextInAccessQueue = _head;
                _head.PreviousInAccessQueue = _head;
            }

            public IEnumerator<IReferenceEntry<K, V>> GetEnumerator()
            {
                return new AccessEnumerator<K, V>(_head, _head);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class AccessEnumerator<K, V> : IEnumerator<IReferenceEntry<K, V>>
        {
            private IReferenceEntry<K, V> _head;
            private IReferenceEntry<K, V> _next;
            private IReferenceEntry<K, V> _start;

            public AccessEnumerator(IReferenceEntry<K, V> head, IReferenceEntry<K, V> start)
            {
                _head = head;
                _next = start;
            }

            public void Dispose()
            {
                _next = _head;
            }

            public bool MoveNext()
            {
                _next = _next.NextInAccessQueue;
                return _next != _head;
            }

            public void Reset()
            {
                _next = _start;
            }

            public IReferenceEntry<K, V> Current => _next;

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        [VisibleForTesting]
        internal class WriteQueue<K, V>
        {
            readonly IReferenceEntry<K, V> _head = new WriteReferenceEntry<K, V>();

            public bool Offer(IReferenceEntry<K, V> entry)
            {
                Queues.ConnectWriteOrder(entry.PreviousInWriteQueue, entry.NextInWriteQueue);
                Queues.ConnectWriteOrder(_head.PreviousInWriteQueue, entry);
                Queues.ConnectWriteOrder(entry, _head);
                return true;
            }

            public IReferenceEntry<K, V> Peek()
            {
                var next = _head.NextInWriteQueue;
                return (next == _head) ? null : next;
            }

            public IReferenceEntry<K, V> Poll()
            {
                var next = _head.NextInWriteQueue;
                if (next == _head) return null;
                Remove(next);
                return next;
            }

            public bool Remove(IReferenceEntry<K, V> e)
            {
                var previous = e.PreviousInWriteQueue;
                var next = e.NextInWriteQueue;
                Queues.ConnectWriteOrder(previous, next);
                Queues.NullifyWriteOrder(e);
                return !(next is NullReferenceEntry<K, V>);
            }

            public bool Contains(IReferenceEntry<K, V> e)
            {
                return !(e.NextInWriteQueue is NullReferenceEntry<K, V>);
            }

            public bool IsEmpty()
            {
                return _head.NextInWriteQueue == _head;
            }

            public int Count()
            {
                int size = 0;
                for (var e = _head.NextInWriteQueue; e != _head; e = e.NextInWriteQueue)
                    size++;
                return size;
            }

            public void Clear()
            {
                var e = _head.NextInWriteQueue;
                while (e != _head)
                {
                    var next = e.NextInWriteQueue;
                    Queues.NullifyWriteOrder(e);
                    e = next;
                }
                _head.NextInWriteQueue = _head;
                _head.PreviousInWriteQueue = _head;
            }
        }

        static class Queues
        {
            public static void ConnectAccessOrder<K, V>(IReferenceEntry<K, V> previous, IReferenceEntry<K, V> next)
            {
                previous.NextInAccessQueue = next;
                next.PreviousInAccessQueue = previous;
            }

            public static void NullifyAccessOrder<K, V>(IReferenceEntry<K, V> nulled)
            {
                var nullEntry = new NullReferenceEntry<K, V>();
                nulled.NextInAccessQueue = nullEntry;
                nulled.PreviousInAccessQueue = nullEntry;
            }

            public static void ConnectWriteOrder<K, V>(IReferenceEntry<K, V> previous, IReferenceEntry<K, V> next)
            {
                previous.NextInWriteQueue = next;
                next.PreviousInWriteQueue = previous;
            }

            public static void NullifyWriteOrder<K, V>(IReferenceEntry<K, V> nulled)
            {
                var nullEntry = new NullReferenceEntry<K, V>();
                nulled.NextInWriteQueue = nullEntry;
                nulled.PreviousInWriteQueue = nullEntry;
            }
        }

        class AccessReferenceEntry<K, V> : IReferenceEntry<K, V>
        {
            public AccessReferenceEntry()
            {
                NextInAccessQueue = this;
                PreviousInAccessQueue = this;
            }

            public IValueReference<K, V> ValueReference
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> Next
            {
                get { throw new NotImplementedException(); }
            }

            public int Hash
            {
                get { throw new NotImplementedException(); }
            }

            public K Key
            {
                get { throw new NotImplementedException(); }
            }

            public long AccessTime
            {
                get { return long.MaxValue; }
                set { }
            }

            public IReferenceEntry<K, V> NextInAccessQueue { get; set; }
            public IReferenceEntry<K, V> PreviousInAccessQueue { get; set; }

            public long WriteTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> NextInWriteQueue
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> PreviousInWriteQueue
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        class WriteReferenceEntry<K, V> : IReferenceEntry<K, V>
        {
            public WriteReferenceEntry()
            {
                NextInWriteQueue = this;
                PreviousInWriteQueue = this;
            }

            public IValueReference<K, V> ValueReference
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> Next
            {
                get { throw new NotImplementedException(); }
            }

            public int Hash
            {
                get { throw new NotImplementedException(); }
            }

            public K Key
            {
                get { throw new NotImplementedException(); }
            }

            public long WriteTime
            {
                get { return long.MaxValue; }
                set { }
            }

            public IReferenceEntry<K, V> NextInWriteQueue { get; set; }
            public IReferenceEntry<K, V> PreviousInWriteQueue { get; set; }

            public long AccessTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> NextInAccessQueue
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IReferenceEntry<K, V> PreviousInAccessQueue
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }
    }
}
