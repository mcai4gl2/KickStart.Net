using System;
using System.Collections.Generic;

namespace KickStart.Net
{
    /// <summary>Class to help with parsing text</summary>
    public static class Parsing
    {
        /// <summary>Delegate used by <see cref="Parse{T}(string, TryParser{T}, T)"/> and <see cref="Parse{T}(IEnumerable{string}, TryParser{T})"/></summary>
        public delegate bool TryParser<T>(string text, out T value);

        /// <summary>Tries to parse a value, returns the default value of T if the value cannot be parsed</summary>
        /// <example>int number = "1".Parse&lt;int&gt;(int.TryParse);</example>
        public static T Parse<T>(this string text, TryParser<T> tryParse, T @default = default(T))
        {
            if (tryParse == null) throw new ArgumentNullException(nameof(tryParse));
            T value;
            return tryParse(text, out value) ? value : @default;
        }

        /// <summary>Returns the values that are parsable, dropping any values that cannot be parsed</summary>
        /// <example>int[] numbers = new [] {"a", "1"}.Choose&lt;int&gt;(int.TryParse).ToArray();</example>
        public static IEnumerable<T> Parse<T>(this IEnumerable<string> source, TryParser<T> tryParse)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (tryParse == null) throw new ArgumentNullException(nameof(tryParse));
            foreach (string text in source)
            {
                T value;
                if (tryParse(text, out value))
                    yield return value;
            }
        }
    }
}
