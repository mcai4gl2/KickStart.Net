using System.Collections.Generic;

namespace KickStart.Net.Metrics
{
    public interface IMetric
    {
    }

    public interface IMetricSet : IMetric
    {
        IReadOnlyDictionary<string, IMetric> GetMetrics();
    }
}
