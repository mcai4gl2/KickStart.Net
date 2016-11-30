using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class ObjectsTests
    {
        [Test]
        public void safe_equals_handles_null()
        {
            Assert.IsTrue(Objects.SafeEquals<int?>(null, null));
            Assert.IsFalse(Objects.SafeEquals<int?>(null, 1));
            Assert.IsFalse(Objects.SafeEquals<int?>(1, null));
        }

        [Test]
        public void safe_equals_handles_struct()
        {
            Assert.IsTrue(Objects.SafeEquals(1, 1));
        }
    }
}
