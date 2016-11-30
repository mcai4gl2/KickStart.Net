using System;
using System.Threading.Tasks;

namespace KickStart.Net
{
    public static class Constants
    {
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);

        public static readonly DateTime MinSqlServerDate = new DateTime(1753, 1, 1);

        /// <summary>
        /// Returns yesterday's date relative to input datetime
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime Yesterday(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            return now.AddDays(-1);
        }

        /// <summary>
        /// Returns tomorrow's date relative to input datetime
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime Tomorrow(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            return now.AddDays(1);
        }

        /// <summary>
        /// Returns the next Sunday's date relative to input datetime
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime ThisSunday(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            if (now.DayOfWeek == DayOfWeek.Sunday)
                return now;
            return now.AddDays(7 - (int)now.DayOfWeek);
        }

        /// <summary>
        /// Returns the closest Saturday relative to input datetime.
        /// If input is Sunday, this returns the Saturday just passed.
        /// Otherwise returns the comming Saturday.
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime ThisSaturday(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            if (now.DayOfWeek == DayOfWeek.Sunday)
                return now.AddDays(-1);
            return now.AddDays(6 - (int)now.DayOfWeek);
        }

        /// <summary>
        /// Returns the closest Friday relative to input datetime.
        /// If input is Sunday or Saturday, this returns the Firday just passed.
        /// Otherwise returns the comming Friday.
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime ThisFriday(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            if (now.DayOfWeek == DayOfWeek.Sunday)
                return now.AddDays(-2);
            return now.AddDays(5 - (int)now.DayOfWeek);
        }

        /// <summary>
        /// Returns the last day of the year relative to input datetime.
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        /// <remarks>Can be used during unit test for testing edge cases</remarks>
        public static DateTime LastDayOfThisYear(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            return new DateTime(now.Year, 12, 31);
        }

        /// <summary>
        /// Returns the last day of the month relative to input datetime.
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        public static DateTime LastDayOfThisMonth(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            return new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        }

        /// <summary>
        /// Returns the first day of the next month relative to input datetime.
        /// </summary>
        /// <param name="now">input datetime, default to UtcNow</param>
        public static DateTime FirstDayOfNextMonth(DateTime now = default(DateTime))
        {
            now = now == default(DateTime) ? DateTime.UtcNow.Date : now.Date;
            return LastDayOfThisMonth(now).AddDays(1);
        }
    }
}
