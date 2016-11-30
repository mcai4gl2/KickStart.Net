using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Extensions;

namespace KickStart.Net.Collections
{
    /// <summary>
    /// <see cref="ITable{TR,TC,TV}"/> implementation backed by dictionary that assocates row keys with
    /// column key / value dictionary. This class provides fast access by row key alone or both keys, but not
    /// by column key.
    /// </summary>
    /// <remarks>This implementation is not multi thread safe.</remarks>
    public class HashBasedTable<TR, TC, TV> : ITable<TR, TC, TV>
    {
        private readonly Dictionary<TR, Dictionary<TC, TV>> _backingDictionary;
        
        public HashBasedTable()
        {
            _backingDictionary = new Dictionary<TR, Dictionary<TC, TV>>();
        }

        public bool Contains(TR rowKey, TC columnKey)
        {
            if (!_backingDictionary.SafeContainsKey(rowKey))
                return false;
            var dict = _backingDictionary[rowKey];
            return dict.SafeContainsKey(columnKey);
        }

        public bool ContainsRow(TR rowKey) => _backingDictionary.SafeContainsKey(rowKey);

        public bool ContainsColumn(TC columnKey)
        {
            if (columnKey == null)
                return false;
            return _backingDictionary.Values.Any(dict => dict.SafeContainsKey(columnKey));
        }

        public bool ContainsValue(TV value) => _backingDictionary.Values.Any(dict => dict.ContainsValue(value));

        public TV Get(TR rowKey, TC columnKey)
        {
            if (!Contains(rowKey, columnKey))
                return default(TV);
            return _backingDictionary[rowKey][columnKey];
        }

        public bool IsEmpty() => _backingDictionary.Count == 0;

        public void Clear() => _backingDictionary.Clear();

        public TV Put(TR rowKey, TC columnKey, TV value)
        {
            if (rowKey == null || columnKey == null)
                return default(TV);
            if (!_backingDictionary.SafeContainsKey(rowKey))
                _backingDictionary.SafeAdd(rowKey, new Dictionary<TC, TV>());
            var dict = _backingDictionary.SafeGet(rowKey);
            var result = dict.SafeGet(columnKey);
            dict.SafeSet(columnKey, value);
            return result;
        }

        public void PutAll(ITable<TR, TC, TV> input)
        {
            foreach (var cell in input.CellSet())
            {
                Put(cell.RowKey, cell.ColumnKey, cell.Value);
            }
        }

        public TV Remove(TR rowKey, TC columnKey)
        {
            if (_backingDictionary.SafeContainsKey(rowKey))
            {
                if (_backingDictionary.SafeGet(rowKey).SafeContainsKey(columnKey))
                {
                    var value = _backingDictionary.SafeGet(rowKey).SafeGet(columnKey);
                    _backingDictionary[rowKey].Remove(columnKey);
                    if (_backingDictionary[rowKey].Count == 0)
                        _backingDictionary.Remove(rowKey);
                    return value;
                }
            }
            return default(TV);
        }

        public IReadOnlyDictionary<TC, TV> Row(TR rowKey)
        {
            return _backingDictionary.SafeGet(rowKey);
        }

        public IReadOnlyDictionary<TR, TV> Column(TC columnKey)
        {
            if (columnKey == null) return null;
            var result = new Dictionary<TR, TV>();
            TV value;
            foreach (var kvp in _backingDictionary)
            {
                if (kvp.Value.TryGetValue(columnKey, out value))
                    result.Add(kvp.Key, value);
            }
            return result.Count == 0 ? null : result;
        }

        public IReadOnlyCollection<ICell<TR, TC, TV>> CellSet()
        {
            var results = new List<ICell<TR, TC, TV>>();
            foreach (var key in _backingDictionary.Keys)
                foreach (var kvp in _backingDictionary[key])
                {
                    results.Add(new Cell(key, kvp.Key, kvp.Value));
                }
            return results;
        }

        public IReadOnlyCollection<TR> RowKeySet()
        {
            return _backingDictionary.Keys.ToList();
        }

        public IReadOnlyCollection<TC> ColumnKeySet()
        {
            var sets = new HashSet<TC>();
            foreach (var dict in _backingDictionary.Values)
            {
                sets.AddRange(dict.Keys);
            }
            return sets.ToList();
        }

        public IReadOnlyCollection<TV> Values()
        {
            var results = new List<TV>();
            foreach (var dict in _backingDictionary.Values)
            {
                results.AddRange(dict.Values);
            }
            return results;
        }

        public struct Cell : ICell<TR, TC, TV>
        {
            public TR RowKey { get; }
            public TC ColumnKey { get; }
            public TV Value { get; }

            public Cell(TR rowKey, TC columnKey, TV value)
            {
                RowKey = rowKey;
                ColumnKey = columnKey;
                Value = value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Cell))
                    return false;
                var other = (Cell) obj;
                return Objects.SafeEquals(RowKey, other.RowKey) &&
                       Objects.SafeEquals(ColumnKey, other.ColumnKey) &&
                       Objects.SafeEquals(Value, other.Value);
            }
        }
    }
}
