using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KickStart.Net.Extensions
{
    public static class HttpMessageExtensions
    {
        public static void WithContent(this HttpRequestMessage request, Stream content, string mediaType)
        {
            request.Content = new StreamContent(content);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        }

        public static void WithContent(this HttpRequestMessage request, string content, string mediaType, Encoding encoding = null)
        {
#if !NET_CORE
            request.Content = new StringContent(content, encoding??Encoding.Default, mediaType);
#endif
#if NET_CORE
            request.Content = new StringContent(content, encoding??Encoding.UTF8, mediaType);
#endif
        }

        public static async Task<Stream> ContentAsStream(this HttpResponseMessage response)
        {
            var content = new MemoryStream();
            await response.Content.CopyToAsync(content);
            content.Seek(0, SeekOrigin.Begin);
            return content;
        }
    }
}
