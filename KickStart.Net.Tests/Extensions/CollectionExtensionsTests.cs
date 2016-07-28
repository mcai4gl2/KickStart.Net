using System.Collections.Generic;
using KickStart.Net.Extensions;
using NUnit.Framework;
using System;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class CollectionExtensionsTests
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

        [Test]
        public void can_convert_to_hashset()
        {
            var items = new[] { 1, 2, 3, 3 };
            var set = items.ToHashSet();
            
            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void can_AddRange_on_a_collection()
        {
            var items = new List<int> { 1, 2, 3 };
            var col = (ICollection<int>)items;
            col.AddRange(4, 5);
            
            Assert.AreEqual(5, items.Count);
            Assert.AreEqual(4, items[3]);
            Assert.AreEqual(5, items[4]);
        }

        [Test]
        public void can_SetRange_on_a_collection()
        {
            var testData1 = new TestData(1);
            var testData2 = new TestData(2);
            var testData3 = new TestData(3);
            var items = new List<TestData> { testData1, testData2, testData3 };
            var col = (ICollection<TestData>)items;

            var testData4 = new TestData(4);
            var testData1Again = new TestData(1);
            col.SetRange(testData4, testData1Again);
            
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(testData4, items[2], "testData4 added at end, then testData1 removed which moves it's position down one");
            Assert.AreEqual(testData1Again, items[3], "testData1 removed, then added at the end");
        }

        [Test]
        public void can_SetRange_on_a_list()
        {
            var testData1 = new TestData(1);
            var testData2 = new TestData(2);
            var testData3 = new TestData(3);
            var items = new List<TestData> { testData1, testData2, testData3 };

            var testData4 = new TestData(4);
            var testData1Again = new TestData(1);
            items.SetRange(testData4, testData1Again);
            
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(testData1Again, items[0], "testData1 replaced perserving it's position");
            Assert.AreEqual(testData4, items[3], "testData4 added to then end");
        }

        class TestData : IEquatable<TestData>
        {
            public readonly int Value;

            public TestData(int value)
            {
                Value = value;
            }

            public override bool Equals(object obj) => Equals(obj as TestData);

            public bool Equals(TestData other)
            {
                if (other == null) return false;
                return Value == other.Value;
            }

            public override int GetHashCode() => Value;
        }
    }
}
