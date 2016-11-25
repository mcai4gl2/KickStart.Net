using KickStart.Net.Metrics;

namespace KickStart.Net.Tests.Metrics
{
    public class ManualClock : IClock
    {
        private long _ticks = 0;

        public long Tick => _ticks;

        public void AddTicks(long ticks) => _ticks += ticks;
        public void AddMilliSeconds(long millisecs) => _ticks += TimeUnits.Milliseconds.ToTicks(millisecs);
        public void AddSeconds(long seconds) => _ticks += TimeUnits.Seconds.ToTicks(seconds);
        public void AddHours(long hours) => _ticks += TimeUnits.Hours.ToTicks(hours);
    }
}
