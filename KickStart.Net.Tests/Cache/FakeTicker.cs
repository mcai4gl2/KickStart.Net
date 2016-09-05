using System;
using KickStart.Net.Cache;

namespace KickStart.Net.Tests.Cache
{
    public class FakeTicker : ITicker
    {
        private readonly ILongAddable _adder = new LongAdder();
        private long AutoIncrementStep { get; set; }

        public FakeTicker Advance(long time)
        {
            _adder.Add(time);
            return this;
        }

        public FakeTicker Advance(TimeSpan timeSpan)
        {
            _adder.Add(timeSpan.Ticks);
            return this;
        }

        public long Read()
        {
            var result = _adder.Sum();
            _adder.Add(AutoIncrementStep);
            return result;
        }
    }
}
