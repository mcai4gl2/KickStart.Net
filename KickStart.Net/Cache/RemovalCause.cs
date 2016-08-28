namespace KickStart.Net.Cache
{
    public enum RemovalCause
    {
        Explicit,
        Replaced,
        Collected,
        Expired,
        Size
    }

    public static class RemovalCauseExtensions
    {
        public static bool WasEvicted(this RemovalCause cause)
        {
            switch (cause)
            {
                case RemovalCause.Explicit:
                case RemovalCause.Replaced:
                    return false;
                case RemovalCause.Collected:
                case RemovalCause.Expired:
                case RemovalCause.Size:
                    return true;
            }
            return false;
        }
    }
}
