using System;
using System.Text;

namespace KickStart.Net
{
    public static class Objects
    {
        public static ToStringHelper ToStringHelper(object self)
        {
            return new ToStringHelper(self.GetType().Name);
        }

        public static ToStringHelper ToStringHelper(Type type)
        {
            return new ToStringHelper(type.Name);
        }

        public static ToStringHelper ToStringHelper(string className)
        {
            return new ToStringHelper(className);
        }

        /// <summary>
        /// Returns true if both inputs are null. Returns false if either inputs are null but not both.
        /// Otherwise forwarding the call to Equals.
        /// </summary>
        public static bool SafeEquals<T>(T value1, T value2)
        {
            if (ReferenceEquals(null, value1))
                return ReferenceEquals(null, value2);
            return value1.Equals(value2);
        }
    }

    public class ToStringHelper
    {
        private readonly string _className;
        private readonly ValueHolder _holderHead = new ValueHolder();
        private ValueHolder _holderTail;
        private bool _omitNullValues = false;

        public ToStringHelper(string className)
        {
            _className = className;
            _holderTail = _holderHead;
        }

        public ToStringHelper OmitNullValues()
        {
            _omitNullValues = true;
            return this;
        }

        public ToStringHelper Add(string name, object value)
        {
            return AddHolder(name, value);
        }

        public ToStringHelper Add(object value)
        {
            return AddHolder(value);
        }

        private ValueHolder AddHolder()
        {
            var valueHolder = new ValueHolder();
            _holderTail = _holderTail.Next = valueHolder;
            return valueHolder;
        }

        private ToStringHelper AddHolder(object value)
        {
            var valueHolder = AddHolder();
            valueHolder.Value = value;
            return this;
        }

        private ToStringHelper AddHolder(string name, object value)
        {
            var valueHolder = AddHolder();
            valueHolder.Value = value;
            valueHolder.Name = name;
            return this;
        }

        public override string ToString()
        {
            var omitNullValuesSnapshot = _omitNullValues;
            var nextSeparator = "";
            var builder = new StringBuilder(32).Append(_className).Append('{');
            for (ValueHolder valueHolder = _holderHead.Next;
                valueHolder != null;
                valueHolder = valueHolder.Next)
            {
                if (!omitNullValuesSnapshot || valueHolder.Value != null)
                {
                    builder.Append(nextSeparator);
                    nextSeparator = ", ";

                    if (valueHolder.Name != null)
                    {
                        builder.Append(valueHolder.Name).Append('=');
                    }
                    builder.Append(valueHolder.Value??"null");
                }
            }
            return builder.Append('}').ToString();
        }

        private class ValueHolder
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ValueHolder Next { get; set; }
        }
    }
}
