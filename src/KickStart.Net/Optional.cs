using System;

namespace KickStart.Net
{
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

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }

        public static Optional<T> OfNullable(T value)
        {
            return value == null ? Empty() : Of(value);
        }

        public T Value
        {
            get
            {
                if (_value == null)
                    throw new InvalidOperationException("No value present");
                return _value;
            }
        }

        public bool IsPresent => _value != null;

        public T Or(T other)
        {
            return _value ?? other;
        }

        public Optional<T> Or(Optional<T> other)
        {
            return _value != null ? this : other;
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
