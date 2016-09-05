using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace KickStart.Net.Cache
{
    partial class LocalCache<K, V>
    {
        internal abstract class CacheEnumerator<T> : IEnumerator<T>
        {
            private int _nextSegmentIndex;
            private int _nextTableIndex;
            private Segment<K, V> _currentSegment;
            private LocalCache<K, V> _cache; 
            private IReferenceEntry<K, V> _nextEntry;
            protected KeyValuePair<K, V> _nextExternal;

            public CacheEnumerator(LocalCache<K, V> cache)
            {
                _cache = cache;
                _nextSegmentIndex = _cache.Segments.Length - 1;
                _nextTableIndex = -1;
            }

            void Advance()
            {
                _nextExternal = default(KeyValuePair<K, V>);
                if (NextInChain())
                    return;
                if (NextInTable())
                    return;
                while (_nextSegmentIndex >= 0)
                {
                    _currentSegment = _cache.Segments[_nextSegmentIndex--];
                    if (_currentSegment.Count != 0)
                    {
                        _nextTableIndex = _currentSegment.Table.Length - 1;
                        if (NextInTable())
                        {
                            return;
                        }
                    }
                }
            }

            bool NextInChain()
            {
                if (_nextEntry != null)
                {
                    for (_nextEntry = _nextEntry.Next; _nextEntry != null; _nextEntry = _nextEntry.Next)
                    {
                        if (AdvanceTo(_nextEntry)) return true;
                    }
                }
                return false;
            }

            bool NextInTable()
            {
                while (_nextTableIndex >= 0)
                {
                    if ((_nextEntry = Volatile.Read(ref _currentSegment.Table[_nextTableIndex--])) != null)
                    {
                        if (AdvanceTo(_nextEntry) || NextInTable())
                            return true;
                    }
                }
                return false;
            }

            bool AdvanceTo(IReferenceEntry<K, V> entry)
            {
                try
                {
                    var now = _cache._ticker.Read();
                    var key = entry.Key;
                    var value = _cache.GetLiveValue(entry, now);
                    if (value != null)
                    {
                        _nextExternal = new KeyValuePair<K, V>(key, value);
                        return true;
                    }
                    else
                        return false;
                }
                finally
                {
                    _currentSegment.PostReadCleanup();
                }
            }

            public void Dispose()
            {
                _nextSegmentIndex = -1;
            }

            public bool MoveNext()
            {
                Advance();
                return !_nextExternal.Equals(default(KeyValuePair<K, V>));
            }

            public void Reset()
            {
                _nextSegmentIndex = _cache.Segments.Length - 1;
                _nextTableIndex = -1;
            }

            public abstract T Current { get; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        internal class KeyEnumerator : CacheEnumerator<K>
        {
            public KeyEnumerator(LocalCache<K, V> cache)
                : base(cache)
            {

            }

            public override K Current => _nextExternal.Key;
        }

        internal class ValueEnumerator : CacheEnumerator<V>
        {
            public ValueEnumerator(LocalCache<K, V> cache)
                : base(cache)
            {
                
            }

            public override V Current => _nextExternal.Value;
        }

        internal class KeyValueEnumerator : CacheEnumerator<KeyValuePair<K, V>>
        {
            public KeyValueEnumerator(LocalCache<K, V> cache)
                : base(cache)
            {

            }

            public override KeyValuePair<K, V> Current => _nextExternal;
        }
    }
}
