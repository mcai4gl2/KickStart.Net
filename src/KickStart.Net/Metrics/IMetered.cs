namespace KickStart.Net.Metrics
{
    public interface IMetered : IMetric, ICounting
    {
        double FifteenMinutesRate { get; }
        double FiveMinutesRate { get; }
        double OneMinuteRate { get; }
        double MeanRate { get; }
    }
}
