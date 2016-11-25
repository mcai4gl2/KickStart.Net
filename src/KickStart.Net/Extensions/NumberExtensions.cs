using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>Converts an integer second to millisecond</summary>
        /// <param name="second">number of seconds to convert</param>
        /// <returns>milliseconds equivalent</returns>
        public static int Seconds(this int second)
        {
            return second*1000;
        }

        public static int Seconds(this double second)
        {
            return (int) (second*1000);
        }

        internal static TaskAwaiter GetAwaiter(this int millisecs)
        {
            return Task.Delay(millisecs).GetAwaiter();
        }

        public static IEnumerable<int> Range(this int toNumberExclusive)
        {
            return Enumerable.Range(0, toNumberExclusive);
        }

        public static IEnumerable<int?> Range(this int? toNumberExclusive)
        {
            return Enumerable.Range(0, toNumberExclusive.Value).Cast<int?>();
        }

        /// <summary>
        /// Returns the int nearest in value to long value.
        /// </summary>
        /// <remarks>Ported from Guava Ints.saturatedCast</remarks>
        public static int SaturatedCast(this long value)
        {
            if (value > int.MaxValue)
            {
                return int.MaxValue;
            }
            if (value < int.MinValue)
            {
                return int.MinValue;
            }
            return (int) value;
        }
    }
}
