using System.Threading.Tasks;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class NumberExtensionsTests
    {
        [Test]
        public void can_convert_sec_to_millisec()
        {
            Assert.AreEqual(1000, 1.Seconds());
            Assert.AreEqual(5000, 5.Seconds());
            Assert.AreEqual(500, .5.Seconds());
        }

        [Test]
        [Timeout(1000)]
        public async Task can_await_on_int()
        {
            await .5.Seconds();
        }
    }
}
