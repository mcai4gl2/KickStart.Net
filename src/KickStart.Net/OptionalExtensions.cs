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
        public static Optional<TK> Map<T, TK>(this Optional<T> input,  Func<T, TK> func) where T : class where TK : class
        {
            if (input.IsPresent)
                return Optional<TK>.OfNullable(func.Invoke(input.Value));
            return Optional<TK>.Empty();
        }

        /// <summary>
        /// Filter Optional based on a predicate.
        /// </summary>
        /// <returns>Returns Optional.Empty when input is empty or predicate evaluates false</returns>
        /// <remarks>Cannot do input.Map(i => predicate.Invoke(i) ? input : Optional.Empty()) because Optional is a struct</remarks>
        public static Optional<T> Where<T>(this Optional<T> input, Predicate<T> predicate) where T : class
        {
            if (input.IsPresent && predicate.Invoke(input.Value))
                return input;
            return Optional<T>.Empty();
        }
    }
}
