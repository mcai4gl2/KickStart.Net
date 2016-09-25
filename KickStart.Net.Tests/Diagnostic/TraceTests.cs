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
            Assert.AreEqual(12, trace.LineNumber);
            Assert.AreEqual(nameof(test_trace), trace.MemberName);        
        }
    }
}
