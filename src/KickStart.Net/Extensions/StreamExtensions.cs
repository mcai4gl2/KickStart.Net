using System.IO;
using System.Text;

namespace KickStart.Net.Extensions
{
    public static class StreamExtensions
    {
        public static bool IsNullOrEmpty(this Stream stream)
        {
            if (stream == null) return true;
            return stream.Length == 0;
        }

        public static Stream ToStream(this string input, Encoding @default = null)
        {
#if !NET_CORE
            @default = @default ?? Encoding.Default;
#endif
#if NET_CORE
            @default = @default ?? Encoding.UTF8;
#endif
            var stream = new MemoryStream(@default.GetBytes(input));
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static Stream ToStream(this byte[] inputs)
        {
            var stream = new MemoryStream(inputs);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static string StreamToString(this Stream stream, Encoding @default = null)
        {
#if !NET_CORE
            @default = @default ?? Encoding.Default;
#endif
#if NET_CORE
            @default = @default ?? Encoding.UTF8;
#endif
            using (var reader = new StreamReader(stream, @default))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
