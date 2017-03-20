using System;

namespace KickStart.Net
{
    /// <summary>
    /// Optional may or may not hold a non-null value.
    /// </summary>
    /// <typeparam name="T">Type needs to be nullable.</typeparam>
    public abstract class Optional<T>
    {
        private static readonly Optional<T> _empty = new EmptyValue();

        private class HasValue : Optional<T>
        {
            private readonly T _value;

            public HasValue(T value)
            {
                _value = value;
            }

            public override bool IsPresent => true;
            public override T Value => _value;

            public override T Or(T defaultValue)
            {
                return _value;
            }

            public override T Or(Func<T> defaultValueSupplier)
            {
                return _value;
            }

            public override Optional<T> Or(Optional<T> defaultValue)
            {
                return this;
            }

            public override Optional<T> Or(Func<Optional<T>> defaultValueSupplier)
            {
                return this;
            }

            public override T OrNull()
            {
                return _value;
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is HasValue))
                    return false;
                var other = (HasValue)obj;
                return Objects.SafeEquals(_value, other._value);
            }

            public override string ToString()
            {
                return $"Optional[{_value}]";
            }
        }

        private class EmptyValue : Optional<T>
        {
            public override bool IsPresent => false;
            public override T Value
            {
                get { throw new InvalidOperationException("No value present"); }
            }

            public override T Or(T defaultValue)
            {
                return defaultValue;
            }

            public override T Or(Func<T> defaultValueSupplier)
            {
                return defaultValueSupplier();
            }

            public override Optional<T> Or(Optional<T> defaultValue)
            {
                return defaultValue;
            }

            public override Optional<T> Or(Func<Optional<T>> defaultValueSupplier)
            {
                return defaultValueSupplier();
            }

            public override T OrNull()
            {
                return default(T);
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override bool Equals(object obj)
            {
                return obj is EmptyValue;
            }

            public override string ToString()
            {
                return "Optional.Empty";
            }
        }

        /// <summary>
        /// Creates an Optional containing non null value
        /// </summary>
        /// <param name="value">Value shall not be null</param>
        /// <returns>An Optional which contains value</returns>
        public static Optional<T> Of(T value)
        {
            if (Objects.SafeEquals(value, default(T)))
                throw new InvalidOperationException("value cannot be empty");
            return new HasValue(value);
        }

        /// <summary>
        /// Creates an Optional containing either null or non null value
        /// </summary>
        /// <param name="value">Value can either be null or not null</param>
        /// <returns>An Optional which contains value</returns>
        public static Optional<T> OfNullable(T value)
        {
            if (Objects.SafeEquals(value, default(T)))
                return Empty();
            return Of(value);
        }

        public static Optional<T> Empty()
        {
            return _empty;
        }

        /// <summary>
        /// Returns if the value is present or not
        /// </summary>
        public abstract bool IsPresent { get; }

        /// <summary>
        /// Returns the value if present or exception
        /// </summary>
        public abstract T Value { get; }

        /// <summary>
        /// Returns the value or other if value is null
        /// </summary>
        /// <param name="defaultValue">the default return value when value is null</param>
        /// <returns>value if value is not null or other</returns>
        public abstract T Or(T defaultValue);

        /// <summary>
        /// Returns the value or the result of defaultValueSupplier function
        /// </summary>
        /// <param name="defaultValueSupplier">the function which provides the default value</param>
        /// <remarks>This allows default value to be lazy evaluated.</remarks>
        public abstract T Or(Func<T> defaultValueSupplier);

        /// <summary>
        /// Returns the value or the result of defaultValue function
        /// </summary>
        /// <param name="defaultValue">the function which provides the default value</param>
        /// <remarks>This allows default value to be lazy evaluated.</remarks>
        public abstract Optional<T> Or(Optional<T> defaultValue);

        /// <summary>
        /// Returns the value or the result of defaultValueSupplier function
        /// </summary>
        /// <param name="defaultValueSupplier">the function which provides the default value</param>
        /// <remarks>This allows default value to be lazy evaluated.</remarks>
        public abstract Optional<T> Or(Func<Optional<T>> defaultValueSupplier);

        /// <summary>
        /// Returns the value
        /// </summary>
        public abstract T OrNull();

        public static bool operator ==(Optional<T> left, object right) => left.Equals(right);

        public static bool operator !=(Optional<T> left, object right) => !left.Equals(right);

        public static implicit operator Optional<T>(T value) => OfNullable(value);

        public static explicit operator T(Optional<T> value) => value.Value;
    }
}
