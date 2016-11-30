using System.Linq;
using System.Threading.Tasks;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
#if !NET_CORE
    [Timeout(1000)]
#endif
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
        public async Task can_await_on_int()
        {
            await .5.Seconds();
        }

        [Test]
        public void test_saturated_cast()
        {
            new[] {int.MinValue, -1, 0, 1, int.MaxValue}.ToList().ForEach(i =>
            {
                Assert.AreEqual(i, ((long) i).SaturatedCast());
            });
            Assert.AreEqual(int.MaxValue, (int.MaxValue + 1L).SaturatedCast());
            Assert.AreEqual(int.MinValue, (int.MinValue - 1L).SaturatedCast());
            Assert.AreEqual(int.MaxValue, long.MaxValue.SaturatedCast());
            Assert.AreEqual(int.MinValue, long.MinValue.SaturatedCast());
        }
    }
}
