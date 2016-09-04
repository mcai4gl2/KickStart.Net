using System.Linq;
using System.Collections;
using System.Collections.Generic;
using KickStart.Net.Extensions;

namespace KickStart.Net.Collections
{
    public class Combinations<T> : IEnumerable<T[]>
    {
        private readonly T[][] _inputs;

        public Combinations(params T[][] inputs)
        {
            _inputs = inputs;
            if (_inputs.Any(i => i.Length != 0))
                foreach (var index in _inputs.Length.Range())
                {
                    if (_inputs[index].Length == 0)
                        _inputs[index] = new T[] {default(T)};
                }
        } 

        public IEnumerator<T[]> GetEnumerator()
        {
            return new CombinationEnumerator<T>(_inputs);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class CombinationEnumerator<T> : IEnumerator<T[]>
    {
        private T[][] _inputs;
        private T[] _next;
        private int[] _nextIndexes;

        public CombinationEnumerator(T[][] inputs)
        {
            _inputs = inputs;
            Reset();
        } 

        public void Dispose()
        {
            _inputs = null;
            _nextIndexes = null;
            _next = null;
        }

        public bool MoveNext()
        {
            for (var index = _nextIndexes.Length - 1; index >= 0; index--)
            {
                if (_nextIndexes[index] < _inputs[index].Length - 1)
                {
                    _next = new T[_inputs.Length];
                    _nextIndexes[index] += 1;
                    for (var index2 = index + 1; index2 < _nextIndexes.Length; index2++)
                        _nextIndexes[index2] = 0;
                    foreach (var i in _inputs.Length.Range())
                        _next[i] = _inputs[i][_nextIndexes[i]];
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            _next = new T[_inputs.Length];
            _nextIndexes = new int[_inputs.Length];
            _nextIndexes.Fill(0);
            _nextIndexes.SetValue(0, -1, false);
        }

        public T[] Current => _next;

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
