namespace KickStart.Net.Metrics
{
    public interface IReservoir
    {
        int Size();
        void Update(long value);
        Snapshot GetSnapshot();
    }
}
