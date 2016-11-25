using System;
using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class OptionalTests
    {
        [Test]
        public void test_empty()
        {
            var op = Optional<string>.Empty();
            Assert.IsFalse(op.IsPresent);
        }

        [Test]
        public void test_of()
        {
            Assert.AreEqual("test", Optional<string>.Of("test").Value);
        }

        [Test]
        public void test_of_null()
        {
            Assert.Throws<InvalidOperationException>(() => { Optional<string>.Of(null); });
        }

        [Test]
        public void test_of_nullable()
        {
            var op = Optional<string>.OfNullable("test");
            Assert.AreEqual("test", op.Value);
        }

        [Test]
        public void test_of_nullable_null()
        {
            var op = Optional<string>.OfNullable(null);
            Assert.AreEqual(Optional<string>.Empty(), op);
        }

        [Test]
        public void test_is_present()
        {
            Assert.IsFalse(Optional<string>.Empty().IsPresent);
            Assert.IsTrue(Optional<string>.Of("test").IsPresent);
        }

        [Test]
        public void test_get_when_empty()
        {
            Assert.Throws<InvalidOperationException>(() => { var result = Optional<string>.Empty().Value; });
        }

        [Test]
        public void test_or()
        {
            Assert.AreEqual("test", Optional<string>.Of("test").Or("other"));
            Assert.AreEqual("other", Optional<string>.Empty().Or("other"));
        }

        [Test]
        public void test_or_optional()
        {
            Assert.AreEqual(Optional<string>.Of("test"), Optional<string>.Of("test").Or(Optional<string>.Empty()));
            Assert.AreEqual(Optional<string>.Of("test"), Optional<string>.Empty().Or(Optional<string>.Of("test")));
        }

        [Test]
        public void test_or_null()
        {
            Assert.AreEqual("test", Optional<string>.Of("test").OrNull());
            Assert.IsNull(Optional<string>.Empty().OrNull());
        }

        [Test]
        public void test_equals_and_hashcode_on_empty()
        {
            Assert.AreEqual(Optional<string>.Empty(), Optional<string>.Empty());
            Assert.AreEqual(Optional<string>.Empty().GetHashCode(), Optional<string>.Empty().GetHashCode());
            Assert.AreEqual(0, Optional<string>.Empty().GetHashCode());
        }

        [Test]
        public void test_equals_and_hashcode_on_non_empty()
        {
            Assert.AreEqual(Optional<string>.Of("test"), Optional<string>.Of("test"));
            Assert.AreEqual(Optional<string>.Of("test"), Optional<string>.Of("test-".TrimEnd('-'))); // This makes sure "Test" is a different string
            var op = Optional<string>.Of("a");
            Assert.IsTrue(op.Equals(op));
            Assert.IsFalse(op.Equals(new object()));
            Assert.IsFalse(Optional<string>.Of("test").Equals(Optional<string>.Of("test2")));
        }

        [Test]
        public void test_to_string()
        {
            Assert.AreEqual("Optional[Test]", Optional<string>.Of("Test").ToString());
            Assert.AreEqual("Optional.Empty", Optional<string>.Empty().ToString());
        }
    }
}
