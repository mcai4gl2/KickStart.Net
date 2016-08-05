using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KickStart.Net.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>Waits for task to return IEnumerable and then convert it to List</summary>
        /// <remarks>Avoids the brackets on await</remarks>
        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> awaitable)
        {
            return (await awaitable).ToList();
        }

        /// <summary>
        /// Await for task to finish or timed out
        /// </summary>
        /// <param name="task">the task to await</param>
        /// <param name="timeoutInMillisec">timeout in millisec</param>
        public static Task TimeoutAfter(this Task task, int timeoutInMillisec)
        {
            return Task.WhenAny(task, Task.Delay(timeoutInMillisec)).Unwrap();
        }

        /// <summary>
        /// Await for task to finish or timed out. When timed out, return the default value
        /// </summary>
        /// <param name="task">the task to await</param>
        /// <param name="timeoutInMillisec">timeout in millisec</param>
        /// <param name="default">the default value to return when timed out</param>
        /// <returns>value of the task if task finishes before timeout or default value</returns>
        public static Task<T> TimeoutAfter<T>(this Task<T> task, int timeoutInMillisec,
            T @default = default(T))
        {
            return Task.WhenAny(task, Task.Delay(timeoutInMillisec).ContinueWith(_ => @default)).Unwrap();
        }

        /// <summary>
        /// Await for task to finish or timed out. When timed out, return the output of the callback function
        /// </summary>
        /// <param name="task">the task to await</param>
        /// <param name="timeoutInMillisec">timeout in millisec</param>
        /// <param name="callback">the callback function which is called on timeout</param>
        /// <returns>value of the task if task finishes before timeout or result of callback function</returns>
        /// <remarks>If task returns before callback function, the value of the task returns</remarks>
        public static Task<T> TimeoutAfter<T>(this Task<T> task, int timeoutInMillisec, Func<T> callback)
        {
            return Task.WhenAny(task, Task.Delay(timeoutInMillisec).ContinueWith(_ => callback())).Unwrap();
        }
    }
}
