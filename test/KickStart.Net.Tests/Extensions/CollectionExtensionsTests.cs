using System.Collections.Generic;
using KickStart.Net.Extensions;
using NUnit.Framework;
using System;
using System.Linq;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class CollectionExtensionsTests
    {
        private readonly Dictionary<int, int> _emptyDict = new Dictionary<int, int>();

        [Test]
        public void Override_without_any_overrides()
        {
            var input = new[] { 1, 2, 3 }.Select(v => new { Id = v, Value = v + 1 });
            var output = input.Override(i => i.Id);
            Assert.AreEqual(input, output);
        }

        [Test]
        public void Override_with_one_value()
        {
            var input = new[] {1, 2, 3}.Select(v => new {Id = v, Value = v + 1});
            var output = input.Override(i => i.Id, new {Id = 1, Value = 3});
            Assert.AreEqual(new[] {new {Id = 1, Value = 3},
                                   new {Id = 2, Value = 3},
                                   new {Id = 3, Value = 4}}, output);
        }

        [Test]
        public void test_index_by()
        {
            var input = new[] {1, 2, 3}.Select(v => new {Id = v, Value = v + 1});
            var byId = input.IndexBy(i => i.Id);
            Assert.AreEqual(new {Id = 1, Value = 2}, byId[1]);
            Assert.AreEqual(new {Id = 2, Value = 3}, byId[2]);
            Assert.AreEqual(new {Id = 3, Value = 4}, byId[3]);
        }

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

        [Test]
        public void can_create_delimited_string_using_default_delimiter()
        {
            var items = new[] { 1, 2, 3 };
            Assert.AreEqual("1,2,3", items.ToDelimitedString());
        }

        [Test]
        public void delimited_string_can_skip_nulls()
        {
            var items = new int?[] { 1, 2, 3, null, 4 };
            Assert.AreEqual("1,2,3,4", items.ToDelimitedStringSkipNulls());
        }

        [Test]
        public void can_create_delimited_string_using_custom_delimiter()
        {
            var items = new[] { 1, 2, 3 };
            Assert.AreEqual("1|2|3", items.ToDelimitedString("|"));
        }

        [TestCase("7ad", 0)]
        [TestCase("9a1", 0)]
        [TestCase("8bw", 1)]
        [TestCase("7cw", 2)]
        [TestCase("9dd", -1)]
        public void can_find_index_of_item_using_custom_delimiter(string find, int expectedIndex)
        {
            var items = new[] { "9a1", "9b2", "9c3", "9a4" };
            Assert.AreEqual(expectedIndex, items.IndexOf(find, new SecondCharEqualityComparer()));
        }

        [Test]
        public void can_split()
        {
            var items = new List<int>() {1, 2, 3, 4, 5};
            var result = items.Split(i => i > 3);
            Assert.AreEqual(new[] {4,5}, result.True);
            Assert.AreEqual(new[] {1,2,3}, result.False);
        }

        [Test]
        public void test_is_empty()
        {
            var items = new List<int>();
            Assert.IsTrue(items.IsEmpty());
            Assert.IsFalse(items.IsNotEmpty());
        }

        [Test]
        public void test_all_with_index()
        {
            var inputs = new List<int>() {0, 1, 2, 3, 4};
            Assert.IsTrue(inputs.All((v, i) => v == i));
        }

        [Test]
        public void test_any_with_index()
        {
            var inputs = new List<int> {0, 2, 3, 4, 5};
            Assert.IsTrue(inputs.Any((v, i) => v == i));
            Assert.IsFalse(inputs.All((v, i) => v == i));
        }

        class SecondCharEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => x[1] == y[1];
            public int GetHashCode(string obj) => obj[1].GetHashCode();
        }

        class TestData : IEquatable<TestData>
        {
            public readonly int Value;

            public TestData(int value)
            {
                Value = value;
            }

            public override bool Equals(object obj) => Equals(obj as TestData);
            public bool Equals(TestData other) => other != null && Value == other.Value;
            public override int GetHashCode() => Value;
        }
    }
}
