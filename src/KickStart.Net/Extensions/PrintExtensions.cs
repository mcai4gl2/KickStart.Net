using System;
using System.Collections.Generic;

namespace KickStart.Net.Extensions
{
    public static class PrintExtensions
    {
        public static void Print(this string input)
        {
            Console.WriteLine(input);
        }

        public static void Print(this object input)
        {
            Console.WriteLine(input);
        }

        public static void P(this object input) => Print(input);
        public static void P(this string input) => Print(input);

        public static void Print<T>(this IEnumerable<T> inputs, int top = 10)
        {
            int count = 0;
            foreach (var input in inputs)
            {
                Console.WriteLine($"{count} - {input}");
                count++;
                if (count >= top)
                {
                    Console.WriteLine("...");
                    break;
                }
            }
        }

        public static void P<T>(this IEnumerable<T> inputs, int top = 10) => Print(inputs, top);

        public static void Print<TK, TV>(this IDictionary<TK, TV> inputs, int top = 10)
        {
            int count = 0;
            foreach (var kvp in inputs)
            {
                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
                count++;
                if (count >= top)
                {
                    Console.WriteLine("...");
                    break;
                }
            }
        }

        public static void P<TK, TV>(this IDictionary<TK, TV> inputs, int top = 10) => Print(inputs, top);
    }
}
