using System;

namespace KickStart.Net.Metrics
{
    // TODO: Moving this to upper level namespace and make cache and this to use the new one
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
