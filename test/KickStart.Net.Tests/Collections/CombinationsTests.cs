using System.Linq;
using KickStart.Net.Collections;
using NUnit.Framework;

namespace KickStart.Net.Tests.Collections
{
    [TestFixture]
    public class CombinationsTests
    {
        [Test]
        public void test_combination_generation()
        {
            var combinations = new Combinations<int>(new[] {1, 2, 3}, new[] {2, 3, 4}).ToList();
            Assert.AreEqual(9, combinations.Count);
            Assert.AreEqual(new[] { 1, 2 }, combinations[0]);
            Assert.AreEqual(new[] { 1, 3 }, combinations[1]);
            Assert.AreEqual(new[] { 1, 4 }, combinations[2]);

            Assert.AreEqual(new[] { 2, 2 }, combinations[3]);
            Assert.AreEqual(new[] { 2, 3 }, combinations[4]);
            Assert.AreEqual(new[] { 2, 4 }, combinations[5]);

            Assert.AreEqual(new[] { 3, 2 }, combinations[6]);
            Assert.AreEqual(new[] { 3, 3 }, combinations[7]);
            Assert.AreEqual(new[] { 3, 4 }, combinations[8]);
        }

        [Test]
        public void test_empty_combination_generation()
        {
            var combinations = new Combinations<int>(new int[] {});
            Assert.AreEqual(0, combinations.Count());
            combinations = new Combinations<int>(new int[] {}, new int[] {});
            Assert.AreEqual(0, combinations.Count());
        }

        [Test]
        public void test_combination_with_one_axis()
        {
            var combinations = new Combinations<int>(new [] {1,2,3}).ToList();
            Assert.AreEqual(3, combinations.Count);
            Assert.AreEqual(new[] { 1 }, combinations[0]);
            Assert.AreEqual(new[] { 2 }, combinations[1]);
            Assert.AreEqual(new[] { 3 }, combinations[2]);
        }

        [Test]
        public void test_combination_with_one_empty_axis()
        {
            var combinations = new Combinations<int>(new[] { 1, 2, 3 }, new int[0]).ToList();
            Assert.AreEqual(3, combinations.Count);
            Assert.AreEqual(new[] { 1, 0 }, combinations[0]);
            Assert.AreEqual(new[] { 2, 0 }, combinations[1]);
            Assert.AreEqual(new[] { 3, 0 }, combinations[2]);
        }
    }
}
