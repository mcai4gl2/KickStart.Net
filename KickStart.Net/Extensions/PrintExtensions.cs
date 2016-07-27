using System;

namespace KickStart.Net.Extensions
{
    public static class PrintExtensions
    {
        public static void Print(this object input)
        {
            Console.WriteLine(input);
        }

        public static void P(this object input) => Print(input);
    }
}
