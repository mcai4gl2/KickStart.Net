using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void test_left()
        {
            var left = Either<int, int>.Left(1);
            Assert.AreEqual(1, left.LeftOr(0));
            Assert.AreEqual(1, left.LeftOr(() => 0));
            Assert.AreEqual(0, left.RightOr(0));
            Assert.AreEqual(0, left.RightOr(() => 0));
        }

        [Test]
        public void test_right()
        {
            var right = Either<int, int>.Right(0);
            Assert.AreEqual(1, right.LeftOr(1));
            Assert.AreEqual(1, right.LeftOr(() => 1));
            Assert.AreEqual(0, right.RightOr(1));
            Assert.AreEqual(0, right.RightOr(() => 1));
        }

        [Test]
        public void test_map_left()
        {
            var left = Either<int, int>.Left(1);
            left = left.MapLeft(l => l + 1);
            Assert.IsTrue(2 == left.Left());
            Assert.IsTrue(0 == left.Right());
            var right = Either<int, int>.Right(1);
            right = right.MapLeft(r => r - 1);
            Assert.IsFalse(right.Left().IsPresent);
            Assert.IsTrue(1 == right.Right());
        }

        [Test]
        public void test_map_right()
        {
            
        }
    }
}
