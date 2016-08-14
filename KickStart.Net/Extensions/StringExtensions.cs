using System.Linq;

namespace KickStart.Net.Extensions
{
    public static class StringExtensions
    {
        public static string JoinSkipNulls(this string delimiter, params string[] parts)
        {
            return string.Join(delimiter, parts.Where(part => !string.IsNullOrEmpty(part)));
        }

        public static string Joins(this string delimiter, params string[] parts)
        {
            return string.Join(delimiter, parts);
        }
    }
}
