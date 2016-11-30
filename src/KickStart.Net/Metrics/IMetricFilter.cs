namespace KickStart.Net.Metrics
{
    public interface IMetricFilter
    {
        bool Matches(string name, IMetric metric);
    }

    public class MetricFilters
    {
        public static readonly IMetricFilter All = new AllMetricFilter();

        class AllMetricFilter : IMetricFilter
        {
            public bool Matches(string name, IMetric metric)
            {
                return true;
            }
        }
    }
}
