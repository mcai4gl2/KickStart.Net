using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class ToStringHelperTests
    {
        [Test]
        public void test_constructor_instance()
        {
            Assert.AreEqual("ToStringHelperTests{}", Objects.ToStringHelper(this).ToString());
        }

        [Test]
        public void test_one_field()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add("field1", 42)
                .ToString();
            Assert.AreEqual("TestClass{field1=42}", actual);
        }

        [Test]
        public void test_null_field()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add("field1", null)
                .ToString();
            Assert.AreEqual("TestClass{field1=null}", actual);
        }

        [Test]
        public void test_two_strings_twice()
        {
            var helper = Objects.ToStringHelper(new TestClass())
                .Add("field1", null);
            var actual = helper.ToString();
            Assert.AreEqual("TestClass{field1=null}", actual);
            Assert.AreEqual(actual, helper.ToString());
        }

        [Test]
        public void test_add_only_value()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add(42)
                .ToString();
            Assert.AreEqual("TestClass{42}", actual);
        }

        [Test]
        public void test_add_only_null_value()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add(null)
                .ToString();
            Assert.AreEqual("TestClass{null}", actual);
        }

        [Test]
        public void test_omit_null_values()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add(null)
                .OmitNullValues()
                .ToString();
            Assert.AreEqual("TestClass{}", actual);
        }

        [Test]
        public void test_omit_null_values_with_multiple_values()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .Add("field1", 42)
                .Add(1)
                .Add("field2", 2)
                .Add("field3", null)
                .Add(null)
                .OmitNullValues()
                .ToString();
            Assert.AreEqual("TestClass{field1=42, 1, field2=2}", actual);
        }

        [Test]
        public void test_omit_null_values_multiple_times()
        {
            var actual = Objects.ToStringHelper(new TestClass())
                .OmitNullValues()
                .Add("field1", 42)
                .Add(1)
                .Add("field2", 2)
                .Add("field3", null)
                .Add(null)
                .OmitNullValues()
                .ToString();
            Assert.AreEqual("TestClass{field1=42, 1, field2=2}", actual);
        }


        class TestClass { }
    }
}
