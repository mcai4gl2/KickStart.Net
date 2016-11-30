using System;
using System.Threading.Tasks;

namespace KickStart.Net.Extensions
{
    public static class Tasks
    {
        /// <summary>
        /// Returns a Task with exception. This would not require in .Net 4.6
        /// </summary>
        public static Task<TResult> FromException<TResult>(Exception exc)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exc);
            return tcs.Task;
        }
    }
}
