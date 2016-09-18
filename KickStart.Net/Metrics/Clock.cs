using System;

namespace KickStart.Net.Metrics
{
    public interface IClock
    {
        long Tick { get; }
    }

    public class DefaultClock : IClock
    {
        public long Tick => DateTime.UtcNow.Ticks;
    }

    public static class Clocks
    {
        public static readonly IClock Default = new DefaultClock();
    }
}
