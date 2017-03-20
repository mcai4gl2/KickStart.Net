using System;

namespace KickStart.Net
{
    public abstract class Either<TL, TR>
    {
        private class LeftValue : Either<TL, TR>
        {
            private readonly TL _value;

            public LeftValue(TL value)
            {
                _value = value;
            }

            public override Either<T, TR> MapLeft<T>(Func<TL, T> func)
            {
                return new Either<T, TR>.LeftValue(func(_value));
            }

            public override Either<TL, T> MapRight<T>(Func<TR, T> func)
            {
                return new Either<TL, T>.LeftValue(_value);
            }

            public override Either<T, TR> FlatMapLeft<T>(Func<TL, Either<T, TR>> func)
            {
                return func(_value);
            }

            public override Either<TL, T> FlatMapRight<T>(Func<TR, Either<TL, T>> func)
            {
                return new Either<TL, T>.LeftValue(_value);
            }

            public override TL LeftOr(TL defaultValue)
            {
                return _value;
            }

            public override TL LeftOr(Func<TL> defaultValue)
            {
                return _value;
            }

            public override TR RightOr(TR defaultValue)
            {
                return defaultValue;
            }

            public override TR RightOr(Func<TR> defaultValue)
            {
                return defaultValue();
            }

            public override Optional<TL> Left()
            {
                return Optional<TL>.OfNullable(_value);
            }

            public override Optional<TR> Right()
            {
                return Optional<TR>.Empty();
            }
        }

        private class RightValue : Either<TL, TR>
        {
            private readonly TR _value;

            public RightValue(TR value)
            {
                _value = value;
            }

            public override Either<T, TR> MapLeft<T>(Func<TL, T> func)
            {
                return new Either<T, TR>.RightValue(_value);
            }

            public override Either<TL, T> MapRight<T>(Func<TR, T> func)
            {
                return new Either<TL, T>.RightValue(func(_value));
            }

            public override Either<T, TR> FlatMapLeft<T>(Func<TL, Either<T, TR>> func)
            {
                return new Either<T, TR>.RightValue(_value);
            }

            public override Either<TL, T> FlatMapRight<T>(Func<TR, Either<TL, T>> func)
            {
                return func(_value);
            }

            public override TL LeftOr(TL defaultValue)
            {
                return defaultValue;
            }

            public override TL LeftOr(Func<TL> defaultValue)
            {
                return defaultValue();
            }

            public override TR RightOr(TR defaultValue)
            {
                return _value;
            }

            public override TR RightOr(Func<TR> defaultValue)
            {
                return _value;
            }

            public override Optional<TL> Left()
            {
                return Optional<TL>.Empty();
            }

            public override Optional<TR> Right()
            {
                return Optional<TR>.OfNullable(_value);
            }
        }

        public static Either<TL, TR> Left(TL value)
        {
            return new LeftValue(value);
        }

        public static Either<TL, TR> Right(TR value)
        {
            return new RightValue(value);
        }

        public abstract Either<T, TR> MapLeft<T>(Func<TL, T> func);
        public abstract Either<TL, T> MapRight<T>(Func<TR, T> func);

        public abstract Either<T, TR> FlatMapLeft<T>(Func<TL, Either<T, TR>> func);
        public abstract Either<TL, T> FlatMapRight<T>(Func<TR, Either<TL, T>> func);

        public abstract TL LeftOr(TL defaultValue);
        public abstract TL LeftOr(Func<TL> defaultValue);

        public abstract TR RightOr(TR defaultValue);
        public abstract TR RightOr(Func<TR> defaultValue);

        public abstract Optional<TL> Left();
        public abstract Optional<TR> Right();
    }
}
