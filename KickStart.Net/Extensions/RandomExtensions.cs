using System;

namespace KickStart.Net.Extensions
{
    public static class RandomExtensions
    {
        public static ulong NextLong(this Random random)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }
    }
}
