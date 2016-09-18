using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Metrics
{
    public class Meter : IMetered
    {
        

        public long Count { get; }
        public double FifteenMinutesRate { get; }
        public double FiveMinutesRate { get; }
        public double OneMinuteRate { get; }
        public double MeanRate { get; }
    }
}
