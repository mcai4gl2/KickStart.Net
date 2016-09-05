namespace KickStart.Net.Cache
{
    interface ILongAddable
    {
        void Increment();
        void Add(long x);
        long Sum();
    }

    static class LongAddables
    {
        public static ILongAddable Create()
        {
            return new LongAdder();
        }
    }
}
