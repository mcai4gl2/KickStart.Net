using System.Collections.Generic;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class DictionaryExtensionsTests
    {
        private readonly Dictionary<int, int> _emptyDict = new Dictionary<int, int>();

        [Test]
        public void test_remove_range_with_params()
        {
            var dict = new Dictionary<int, int> {{1, 2}, {3, 4}, {5, 6}};
            dict.RemoveRange(5);
            Assert.AreEqual(new Dictionary<int, int> {{1, 2}, {3, 4}}, dict);
            dict.RemoveRange(1, 3);
            Assert.AreEqual(_emptyDict, dict);
        }

        [Test]
        public void test_remove_range_with_list()
        {
            var dict = new Dictionary<int, int> { { 1, 2 }, { 3, 4 }, { 5, 6 } };
            dict.RemoveRange(new List<int> {5});
            Assert.AreEqual(new Dictionary<int, int> { { 1, 2 }, { 3, 4 } }, dict);
            dict.RemoveRange(new List<int> {1, 3});
            Assert.AreEqual(_emptyDict, dict);
        }

        [Test]
        public void test_remove_by_value()
        {
            var dict = new Dictionary<int, int> {{1, 2}, {2, 2}, {3, 3}, {4, 2}};
            dict.RemoveByValue(2);
            Assert.AreEqual(new Dictionary<int, int> { {3,3} }, dict);
        }

        [Test]
        public void test_get_or_default()
        {
            var dict = new Dictionary<int, int>();
            Assert.AreEqual(5, dict.GetOrDefault(1, 5));
        }
    }
}
