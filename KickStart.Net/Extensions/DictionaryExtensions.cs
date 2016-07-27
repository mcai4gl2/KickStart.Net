using System;
using System.Collections.Generic;
using System.Linq;

namespace KickStart.Net.Extensions
{
    public static class DictionaryExtensions
    {
        public static void RemoveRange<TK, TV>(this Dictionary<TK, TV> dict, params TK[] keysToRemove)
        {
            dict.RemoveRange(keysToRemove.ToList());
        }

        public static void RemoveRange<TK, TV>(this Dictionary<TK, TV> dict, IEnumerable<TK> keysToRemove)
        {
            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }
        }

        public static void RemoveByValue<TK, TV>(this Dictionary<TK, TV> dict, TV value, Func<TV, TV, bool> predicate = null)
        {
            predicate = predicate ?? ((v1, v2) => v1.Equals(v2));
            var keysToRemove = (from kvp in dict where predicate(kvp.Value, value) select kvp.Key).ToList();
            dict.RemoveRange(keysToRemove);
        }

        public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV defaultValue)
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
