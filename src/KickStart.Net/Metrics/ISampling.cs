namespace KickStart.Net.Metrics
{
    public interface ISampling
    {
        Snapshot GetSnapshot();
    }
}
