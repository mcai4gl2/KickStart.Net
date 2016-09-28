using KickStart.Net.Collections;
using NUnit.Framework;

namespace KickStart.Net.Tests.Collections
{
    [TestFixture]
    public class CollectionsTests
    {
        [Test]
        public void test_empty_constant()
        {
            Assert.AreSame(Lists<int>.EmptyList, Lists<int>.EmptyList);
            Assert.AreNotSame(Lists<int?>.EmptyList, Lists<int>.EmptyList);
            Assert.AreSame(Lists<int>.EmptyLinkedList, Lists<int>.EmptyLinkedList);
            Assert.AreNotSame(Lists<int?>.EmptyLinkedList, Lists<int>.EmptyLinkedList);
            Assert.AreSame(Dictionaries<int, int>.EmptyDictionary, Dictionaries<int, int>.EmptyDictionary);
        }
    }
}
