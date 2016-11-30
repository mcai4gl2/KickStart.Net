using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace KickStart.Net.Extensions
{
    public static class CollectionExtenions
    {
        /// <summary>Finds the zero-based index of the <paramref name="item"/> within the <paramref name="list"/> using the supplied <see cref="EqualityComparer{T}"/></summary>
        /// <returns>The index if the item was found, or -1 if the item cannot be found</returns>
        public static int IndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> equality) where T : class
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (equality == null) throw new ArgumentNullException(nameof(equality));
            Contract.EndContractBlock();

            for (int i = 0; i < list.Count; i++)
            {
                if (equality.Equals(item, list[i])) return i;
            }
            return -1;
        }

        /// <summary>Returns a string of the <paramref name="items"/> joined with the <paramref name="delimiter"/>, which defaults to a comma</summary>
        public static string ToDelimitedString<T>(this IEnumerable<T> items, string delimiter = ",")
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();

            return string.Join(delimiter, items);
        }

        /// <summary>Same as ToDelimitedString but skip null values</summary>
        public static string ToDelimitedStringSkipNulls<T>(this IEnumerable<T> items, string delimiter = ",")
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();

            return string.Join(delimiter, items.Where(i => i != null));
        }

        public static IDictionary<TKey, TValue> IndexBy<TKey, TValue>(this IEnumerable<TValue> items,
            Func<TValue, TKey> keySelector)
        {
            return items.ToDictionary(keySelector, i => i);
        } 

        /// <summary>
        /// Removes all the <paramref name="keys"/> from the <paramref name="dictionary"/>
        /// </summary>
        /// <param name="dictionary">the dictionary to remove from</param>
        /// <param name="keys">the keys to remove</param>
        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            RemoveRange(dictionary, (IEnumerable<TKey>)keys);
        }

        /// <summary>
        /// Removes all the <paramref name="keys"/> from the <paramref name="dictionary"/>
        /// </summary>
        /// <param name="dictionary">the dictionary to remove from</param>
        /// <param name="keys">the keys to remove</param>
        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            Contract.EndContractBlock();
            foreach (var key in keys)
            {
                dictionary.Remove(key);
            }
        }
        
        public static void RemoveByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, Func<TValue, TValue, bool> predicate = null)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            Contract.EndContractBlock();
            predicate = predicate ?? ((v1, v2) => v1.Equals(v2));
            var keysToRemove = (from kvp in dictionary where predicate(kvp.Value, value) select kvp.Key).ToList();
            dictionary.RemoveRange(keysToRemove);
        }

        /// <summary>
        /// Gets the value in <paramref name="dictionary"/> for the <paramref name="key"/>.  If the <paramref name="key"/> is not present then it returns <paramref name="defaultValue"/>
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            Contract.EndContractBlock();
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Returns a new <see cref="HashSet{T}"/> containing unqiue <paramref name="items"/>
        /// </summary>
        /// <param name="items">The items to add to the returned <see cref="HashSet{T}"/></param>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();
            return new HashSet<T>(items);
        }

        /// <summary>
        /// Adds all the <paramref name="items"/> to the <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">the collection to add <paramref name="items"/> to</param>
        /// <param name="items">The items to add</param>
        public static void AddRange<T>(this ICollection<T> collection, params T[] items)
        {
            AddRange(collection, (IEnumerable<T>)items);
        }

        /// <summary>
        /// Adds all the <paramref name="items"/> to the <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">the collection to add <paramref name="items"/> to</param>
        /// <param name="items">The items to add</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (collection.IsReadOnly) throw new ArgumentException("cannot modify a readonly collection", nameof(collection));
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Adds <paramref name="items"/> not in the <paramref name="collection"/>, replaces <paramref name="items"/> that are in the <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">The collection to modify</param>
        /// <param name="items">the items to add or replace in the <paramref name="collection"/></param>
        public static void SetRange<T>(this ICollection<T> collection, params T[] items)
        {
            SetRange(collection, (IEnumerable<T>)items);
        }

        /// <summary>
        /// Adds <paramref name="items"/> not in the <paramref name="collection"/>, replaces <paramref name="items"/> that are in the <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">The collection to modify</param>
        /// <param name="items">the items to add or replace in the <paramref name="collection"/></param>
        public static void SetRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (collection.IsReadOnly) throw new ArgumentException("cannot modify a readonly collection", nameof(collection));
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();

            foreach (var item in items)
            {
                if (collection.Contains(item))
                    collection.Remove(item);
                collection.Add(item);
            }
        }

        /// <summary>
        /// Adds <paramref name="items"/> not in the <paramref name="list"/>, replaces <paramref name="items"/> that are in the <paramref name="list"/>
        /// </summary>
        /// <param name="list">The collection to modify</param>
        /// <param name="items">the items to add or replace in the <paramref name="list"/></param>
        public static void SetRange<T>(this IList<T> list, params T[] items)
        {
            SetRange(list, (IEnumerable<T>)items);
        }

        /// <summary>
        /// Adds <paramref name="items"/> not in the <paramref name="list"/>, replaces <paramref name="items"/> that are in the <paramref name="list"/>
        /// </summary>
        /// <remarks>Preserves the position of items that are replaced</remarks>
        /// <param name="list">The collection to modify</param>
        /// <param name="items">the items to add or replace in the <paramref name="list"/></param>
        public static void SetRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (list.IsReadOnly) throw new ArgumentException("cannot modify a readonly list", nameof(list));
            if (items == null) throw new ArgumentNullException(nameof(items));
            Contract.EndContractBlock();

            foreach (var item in items)
            {
                int index = list.IndexOf(item);
                if (index >= 0)
                    list[index] = item;
                else
                    list.Add(item);
            }
        }

        public static IEnumerable<TValue> Override<TKey, TValue>(this IEnumerable<TValue> @base, IEnumerable<TValue> @override,
            Func<TValue, TKey> keySelector)
        {
            var byId = @override.IndexBy(keySelector);
            return @base.Select(b => byId.ContainsKey(keySelector(b)) ? byId[keySelector(b)] : b);
        }

        public static IEnumerable<TValue> Override<TKey, TValue>(this IEnumerable<TValue> @base, Func<TValue, TKey> keySelector, params TValue[] @override)
        {
            return Override(@base, @override, keySelector);
        }

        public static bool IsEmpty<T>(this IList<T> list) => list.Count == 0;

        public static bool IsNotEmpty<T>(this IList<T> list) => list.Count != 0;

        public static SplitResult<T> Split<T>(this IList<T> list, Predicate<T> predicate)
        {
            var result = new SplitResult<T>
            {
                True = new List<T>(list.Count),
                False = new List<T>(list.Count)
            };
            foreach (var value in list)
            {
                if (predicate(value)) result.True.Add(value);
                else result.False.Add(value);
            }
            return result;
        }

        public static bool All<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            return source
                .Select((v, i) => new {Value = v, Index = i})
                .All(value => predicate(value.Value, value.Index));
        }

        public static bool Any<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            return source
                .Select((v, i) => new {Value = v, Index = i})
                .Any(value => predicate(value.Value, value.Index));
        }
    }

    public class SplitResult<T>
    {
        public IList<T> True { get; set; }
        public IList<T> False { get; set; } 
    }
}
