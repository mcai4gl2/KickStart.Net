using System;
using System.Threading.Tasks;

namespace KickStart.Net
{
    public static class Constants
    {
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);

        public static DateTime Yesterday(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow : now;
            return now.AddDays(-1);
        }

        public static DateTime Tomorrow(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow : now;
            return now.AddDays(1);
        }
    }
}
