using KickStart.Net.Diagnostic;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Diagnostic
{
    [TestFixture]
    public class TraceTests
    {
        [Test]
        public void test_trace()
        {
            var trace = Trace.Here();
            trace.P();
            Assert.AreEqual(13, trace.LineNumber);
            Assert.AreEqual(nameof(test_trace), trace.MemberName);        
        }
    }
}
