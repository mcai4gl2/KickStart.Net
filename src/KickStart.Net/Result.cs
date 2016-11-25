using System;
using System.Collections.Generic;
using System.Linq;

namespace KickStart.Net
{
    class Demo
    {
        void Test()
        {
            var possibleNumbers = new string[] { "1", "two", "3" };
            var goodValues = possibleNumbers.Attempt(t => int.Parse(t)).Where(res => res.IsOk);
        }
    }

    public static class Result
    {
        /// <summary>Try to call a function, returns the result or the exception that occurred</summary>
        /// <remarks>Allows LINQ over functions that might throw an exception</remarks>
        public static Result<T, Exception> Attempt<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>Try to call a function, returns the result or the exception that occurred</summary>
        /// <remarks>Allows LINQ over functions that might throw an exception</remarks>
        /// <remarks>Optimization that may avoid the creation of a closure, reducing garbage creation</remarks>
        public static Result<TOut, Exception> Attempt<TIn, TOut>(TIn input, Func<TIn, TOut> func)
        {
            try
            {
                return func(input);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        
        /// <summary>Try to call a function, returns the result or the exception that occurred</summary>
        /// <remarks>Allows LINQ over functions that might throw an exception</remarks>
        /// <remarks>Optimization that may avoid the creation of a closure, reducing garbage creation</remarks>
        public static IEnumerable<Result<TOut, Exception>> Attempt<TIn, TOut>(this IEnumerable<TIn> input, Func<TIn, TOut> func)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (func == null) throw new ArgumentNullException(nameof(func));

            foreach (TIn item in input)
            {
                yield return Attempt(item, func);
            }
        }
    }

    /// <summary>Represents a result or some sort of error</summary>
    /// <remarks>Allows LINQ over functions that might throw an exception by using <see cref="Result.Attempt{T}(Func{T})"/></remarks>
    public struct Result<T, TError>
    {
        readonly bool _ok;
        readonly T _value;
        readonly TError _error;

        /// <summary>Is the result successful, does it have a <see cref="Value"/>?</summary>
        public bool IsOk => _ok;

        /// <summary>Is the result a failure, does it have a <see cref="Error"/>?</summary>
        public bool IsError => !_ok;

        /// <summary>Successful result</summary>
        public T Value => _value;

        /// <summary>The error that occurred</summary>
        public TError Error => _error;

        /// <summary>Implicit conversion from a successful value</summary>
        public static implicit operator Result<T, TError>(T value) => new Result<T, TError>(value);
        
        /// <summary>Implicit conversion from an error value</summary>
        public static implicit operator Result<T, TError>(TError error) => new Result<T, TError>(error);

        /// <summary>A new result that <see cref="IsOk"/></summary>
        public Result(T value)
        {
            _value = value;
            _error = default(TError);
            _ok = true;
        }

        /// <summary>A new result that <see cref="IsError"/></summary>
        public Result(TError error)
        {
            _value = default(T);
            _error = error;
            _ok = false;
        }
        public override string ToString() => _ok ? Value?.ToString() : Error?.ToString();
    }
}
