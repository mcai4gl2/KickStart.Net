using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KickStart.Net.Collections;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class DictionarySafeExtensionsTests
    {
        [Test]
        public void test_safe_contains_keys()
        {
            var dictionary = new Dictionary<int?, int> { {1, 2} };
            Assert.Throws<ArgumentNullException>(() =>
            {
                dictionary.ContainsKey(null);
            });
            Assert.IsFalse(dictionary.SafeContainsKey(null));
            Assert.IsTrue(dictionary.SafeContainsKey(1));
        }

        [Test]
        public void test_safe_remove()
        {
            var dictionary = new Dictionary<int?, int> { {1, 2} };
            Assert.Throws<ArgumentNullException>(() =>
            {
                dictionary.Remove(null);
            });
            Assert.IsFalse(dictionary.SafeRemove(null));
            Assert.IsTrue(dictionary.SafeRemove(1));
        }

        [Test]
        public void test_safe_get()
        {
            var dictionary = new Dictionary<int?, int> { {1, 2} };

            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = dictionary[null];
            });
            Assert.AreEqual(default(int), dictionary.SafeGet(null));

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var result = dictionary[2];
            });
            Assert.AreEqual(default(int), dictionary.SafeGet(2));

            Assert.AreEqual(2, dictionary.SafeGet(1));
        }

        [Test]
        public void test_safe_set()
        {
            var dictionary = new Dictionary<int?, int> { { 1, 2 } };

            Assert.Throws<ArgumentNullException>(() =>
            {
                dictionary[null] = 2;
            });
            dictionary.SafeSet(null, 1);
            Assert.AreEqual(1, dictionary.Count);

            IDictionary<int?, int> readonlyDictionary = new ReadOnlyDictionary<int?, int>(dictionary);
            Assert.Throws<NotSupportedException>(() =>
            {
                readonlyDictionary[1] = 3;
            });
            readonlyDictionary.SafeSet(1, 3);
            Assert.AreEqual(2, readonlyDictionary[1]);
            Assert.AreEqual(1, readonlyDictionary.Count);

            dictionary.SafeSet(1, 3);
            Assert.AreEqual(3, dictionary[1]);
        }

        [Test]
        public void test_safe_add()
        {
            var dictionary = new Dictionary<int?, int> {{1, 2}};

            Assert.Throws<ArgumentNullException>(() =>
            {
                dictionary.Add(null, 1);
            });
            dictionary.SafeAdd(null, 1);
            Assert.AreEqual(1, dictionary.Count);

            Assert.Throws<ArgumentException>(() =>
            {
                dictionary.Add(1, 3);
            });
            Assert.AreEqual(2, dictionary[1]);
            dictionary.SafeAdd(1, 3);
            Assert.AreEqual(2, dictionary[1]);

            IDictionary<int?, int> readonlyDictionary = new ReadOnlyDictionary<int?, int>(dictionary);
            Assert.Throws<NotSupportedException>(() =>
            {
                readonlyDictionary.Add(1, 3);
            });
            readonlyDictionary.SafeAdd(1, 3);
            Assert.AreEqual(2, readonlyDictionary[1]);
            Assert.AreEqual(1, readonlyDictionary.Count);

            dictionary.SafeAdd(2, 3);
            Assert.AreEqual(3, dictionary[2]);
            Assert.AreEqual(2, dictionary.Count);
        }

        [Test]
        public void test_safe_keys()
        {
            Dictionary<int, int> dict = null;
            Assert.AreSame(Lists<int>.EmptyList, dict.SafeKeys(Lists<int>.EmptyList));
            Assert.IsNull(dict.SafeKeys());
        }

        [Test]
        public void test_safe_values()
        {
            Dictionary<int, int> dict = null;
            Assert.AreSame(Lists<int>.EmptyList, dict.SafeValues(Lists<int>.EmptyList));
            Assert.IsNull(dict.SafeValues());
        }
    }
}
