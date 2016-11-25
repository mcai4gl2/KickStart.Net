using System.IO;

namespace KickStart.Net.Metrics
{
    public abstract class Snapshot
    {
        public abstract double GetValue(double quantile);
        public abstract long[] GetValues();
        public abstract int Size();
        public double GetMedian() => GetValue(0.5);
        public double Get75thPercentile() => GetValue(0.75);
        public double Get95thPercentile() => GetValue(0.95);
        public double Get98thPercentile() => GetValue(0.98);
        public double Get99thPercentile() => GetValue(0.99);
        public double Get999thPercentile() => GetValue(0.999);
        public abstract long GetMax();
        public abstract double GetMean();
        public abstract long GetMin();
        public abstract double GetStdDev();
    }
}
