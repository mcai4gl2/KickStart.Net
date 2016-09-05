using System;

namespace KickStart.Net.Cache
{
    public interface ITicker
    {
        long Read();
    }

    public class DateTimeTicker : ITicker
    {
        public long Read() => DateTime.UtcNow.Ticks;
    }

    public class NullTicker : ITicker
    {
        public long Read() => 0;
    }
}
