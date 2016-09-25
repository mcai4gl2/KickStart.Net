namespace KickStart.Net.Metrics
{
    public interface IGauge<T> : IMetric
    {
        T GetValue();
    }
}
