using System;

namespace KickStart.Net
{
    /// <summary>
    /// Extension methods on Optional. 
    /// </summary>
    public static class OptionalExtensions
    {
        /// <summary>
        /// Map an Optional using a function.
        /// </summary>
        public static Optional<TK> Map<T, TK>(this Optional<T> input,  Func<T, TK> func)
        {
            if (input.IsPresent)
                return Optional<TK>.OfNullable(func(input.Value));
            return Optional<TK>.Empty();
        }

        /// <summary>
        /// Map an Optional using a function.
        /// </summary>
        public static Optional<TK> FlatMap<T, TK>(this Optional<T> input, Func<T, Optional<TK>> func)
        {
            if (input.IsPresent)
                return func(input.Value);
            return Optional<TK>.Empty();
        }

        /// <summary>
        /// Filter Optional based on a predicate.
        /// </summary>
        /// <returns>Returns Optional.Empty when input is empty or predicate evaluates false</returns>
        public static Optional<T> Where<T>(this Optional<T> input, Predicate<T> predicate)
        {
            return input.FlatMap(i => predicate(i) ? input : Optional<T>.Empty());
        }
    }
}
