using System;

namespace KickStart.Net
{
    public interface ITimeUnit
    {
        long ToTicks(long d);
        long ToMillis(long d);
        long ToSeconds(long d);
        long ToMinutes(long d);
        long ToHours(long d);
        long ToDays(long d);
        long Convert(long d, ITimeUnit u);
        TimeSpan ToTimeSpan(long d);
    }

    public static class TimeUnits
    {
        private const long C0 = 1L;
        private const long C1 = C0*10000L; // millisec
        private const long C2 = C1*1000L; // sec
        private const long C3 = C2*60L; // min
        private const long C4 = C3*60L; // hour
        private const long C5 = C4*24L; // day

        public static readonly ITimeUnit Ticks = new _Ticks();
        public static readonly ITimeUnit Milliseconds = new _Milliseconds();
        public static readonly ITimeUnit Seconds = new _Seconds();
        public static readonly ITimeUnit Minutes = new _Minutes();
        public static readonly ITimeUnit Hours = new _Hours();
        public static readonly ITimeUnit Days = new _Days();

        class _Ticks : ITimeUnit
        {
            public long ToTicks(long d) => d;
            public long ToMillis(long d) => d/C1/C0;
            public long ToSeconds(long d) => d/C2/C0;
            public long ToMinutes(long d) => d/C3/C0;
            public long ToHours(long d) => d/C4/C0;
            public long ToDays(long d) => d/C5/C0;
            public long Convert(long d, ITimeUnit u) => u.ToTicks(d);
            public TimeSpan ToTimeSpan(long d) => new TimeSpan(d);
        }

        class _Milliseconds : ITimeUnit
        {
            public long ToTicks(long d) => X(d, C1/C0, Max/(C1/C0));
            public long ToMillis(long d) => d;
            public long ToSeconds(long d) => d / (C2 / C1);
            public long ToMinutes(long d) => d / (C3 / C1);
            public long ToHours(long d) => d / (C4 / C1);
            public long ToDays(long d) => d / (C5 / C1);
            public long Convert(long d, ITimeUnit u) => u.ToMillis(d);
            public TimeSpan ToTimeSpan(long d) => new TimeSpan(ToTicks(d));
        }

        class _Seconds : ITimeUnit
        {
            public long ToTicks(long d) => X(d, C2 / C0, Max / (C2 / C0));
            public long ToMillis(long d) => X(d, C2 / C1, Max / (C2 / C1));
            public long ToSeconds(long d) => d;
            public long ToMinutes(long d) => d / (C3 / C2);
            public long ToHours(long d) => d / (C4 / C2);
            public long ToDays(long d) => d / (C5 / C2);
            public long Convert(long d, ITimeUnit u) => u.ToSeconds(d);
            public TimeSpan ToTimeSpan(long d) => TimeSpan.FromSeconds(d);
        }

        class _Minutes : ITimeUnit
        {
            public long ToTicks(long d) => X(d, C3 / C0, Max / (C3 / C0));
            public long ToMillis(long d) => X(d, C3 / C1, Max / (C3 / C1));
            public long ToSeconds(long d) => X(d, C3 / C2, Max / (C3 / C2));
            public long ToMinutes(long d) => d;
            public long ToHours(long d) => d / (C4 / C3);
            public long ToDays(long d) => d / (C5 / C3);
            public long Convert(long d, ITimeUnit u) => u.ToMinutes(d);
            public TimeSpan ToTimeSpan(long d) => TimeSpan.FromMinutes(d);
        }

        class _Hours : ITimeUnit
        {
            public long ToTicks(long d) => X(d, C4 / C0, Max / (C4 / C0));
            public long ToMillis(long d) => X(d, C4 / C1, Max / (C4 / C1));
            public long ToSeconds(long d) => X(d, C4 / C2, Max / (C4 / C2));
            public long ToMinutes(long d) => X(d, C4 / C3, Max / (C4 / C3));
            public long ToHours(long d) => d;
            public long ToDays(long d) => d / (C5 / C4);
            public long Convert(long d, ITimeUnit u) => u.ToHours(d);
            public TimeSpan ToTimeSpan(long d) => TimeSpan.FromHours(d);
        }

        class _Days : ITimeUnit
        {
            public long ToTicks(long d) => X(d, C5 / C0, Max / (C5 / C0));
            public long ToMillis(long d) => X(d, C5 / C1, Max / (C5 / C1));
            public long ToSeconds(long d) => X(d, C5 / C2, Max / (C5 / C2));
            public long ToMinutes(long d) => X(d, C5 / C3, Max / (C5 / C3));
            public long ToHours(long d) => X(d, C5 / C4, Max / (C5 / C4));
            public long ToDays(long d) => d;
            public long Convert(long d, ITimeUnit u) => u.ToDays(d);
            public TimeSpan ToTimeSpan(long d) => TimeSpan.FromDays(d);
        }

        private static readonly long Max = long.MaxValue;
        static long X(long d, long m, long over)
        {
            if (d > over) return long.MaxValue;
            if (d < -over) return long.MinValue;
            return d*m;
        }
    }
}
