using System;

namespace KickStart.Net
{
    /// <summary>
    /// Optional may or may not hold a non-null value.
    /// </summary>
    /// <typeparam name="T">Type needs to be nullable.</typeparam>
    public struct Optional<T> where T : class
    {
        private static readonly Optional<T> _empty = new Optional<T>();
        private readonly T _value;

        public static Optional<T> Empty()
        {
            return _empty;
        }

        private Optional(T value)
        {
            if (value == null)
                throw new InvalidOperationException("value cannot be null");
            _value = value;
        }

        /// <summary>
        /// Creates an Optional containing non null value
        /// </summary>
        /// <param name="value">Value shall not be null</param>
        /// <returns>An Optional which contains value</returns>
        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Creates an Optional containing either null or non null value
        /// </summary>
        /// <param name="value">Value can either be null or not null</param>
        /// <returns>An Optional which contains value</returns>
        public static Optional<T> OfNullable(T value)
        {
            return value == null ? Empty() : Of(value);
        }


        /// <summary>
        /// Returns the value if present or exception
        /// </summary>
        public T Value
        {
            get
            {
                if (!IsPresent)
                    throw new InvalidOperationException("No value present");
                return _value;
            }
        }

        /// <summary>
        /// Returns if the value is present or not
        /// </summary>
        public bool IsPresent => _value != null;

        /// <summary>
        /// Returns the value or other if value is null
        /// </summary>
        /// <param name="other">the default return value when value is null</param>
        /// <returns>value if value is not null or other</returns>
        public T Or(T other)
        {
            return _value ?? other;
        }

        /// <summary>
        /// Returns the value or the result of defaultValue function
        /// </summary>
        /// <param name="defaultValue">the function which provides the default value</param>
        /// <remarks>This allows default value to be lazy evaluated.</remarks>
        public T Or(Func<T> defaultValue)
        {
            return _value ?? defaultValue.Invoke();
        }

        public Optional<T> Or(Optional<T> other)
        {
            return IsPresent ? this : other;
        }

        /// <summary>
        /// Returns the value or the result of defaultValue function
        /// </summary>
        /// <param name="defaultValue">the function which provides the default value</param>
        /// <remarks>This allows default value to be lazy evaluated.</remarks>
        public Optional<T> Or(Func<Optional<T>> defaultValue)
        {
            return IsPresent ? this : defaultValue.Invoke();
        }

        public T OrNull()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Optional<T>))
                return false;
            var other = (Optional<T>) obj;
            return Objects.SafeEquals(_value, other._value);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _value != null ? $"Optional[{_value}]" : "Optional.Empty";
        }

        public static bool operator ==(Optional<T> left, object right) => left.Equals(right);

        public static bool operator !=(Optional<T> left, object right) => !left.Equals(right);

        public static implicit operator Optional<T>(T value) => OfNullable(value);

        public static explicit operator T(Optional<T> value) => value.Value;
    }
}
