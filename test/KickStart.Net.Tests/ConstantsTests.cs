using System;
using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class ConstantsTests
    {
        [Test]
        public void test_yesterday()
        {
            Assert.AreEqual(new DateTime(2016, 9, 4), Constants.Yesterday(new DateTime(2016, 9, 5, 13, 0, 0)));
        }

        [Test]
        public void test_tomorrow()
        {
            Assert.AreEqual(new DateTime(2016, 9, 6), Constants.Tomorrow(new DateTime(2016, 9, 5, 13, 0, 0)));
        }

        [Test]
        public void test_this_sunday()
        {
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 5, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 6, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 7, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 8, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 9, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 10, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 11), Constants.ThisSunday(new DateTime(2016, 9, 11, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 18), Constants.ThisSunday(new DateTime(2016, 9, 12, 13, 12, 11)));
        }

        [Test]
        public void test_this_saturday()
        {
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 5, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 6, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 7, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 8, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 9, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 10, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 10), Constants.ThisSaturday(new DateTime(2016, 9, 11, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 17), Constants.ThisSaturday(new DateTime(2016, 9, 12, 13, 12, 11)));
        }

        [Test]
        public void test_this_friday()
        {
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 5, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 6, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 7, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 8, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 9, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 10, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 9), Constants.ThisFriday(new DateTime(2016, 9, 11, 13, 12, 11)));
            Assert.AreEqual(new DateTime(2016, 9, 16), Constants.ThisFriday(new DateTime(2016, 9, 12, 13, 12, 11)));
        }

        [Test]
        public void test_last_day_of_this_year()
        {
            Assert.AreEqual(new DateTime(2016, 12, 31), Constants.LastDayOfThisYear(new DateTime(2016, 12, 31)));
            Assert.AreEqual(new DateTime(2016, 12, 31), Constants.LastDayOfThisYear(new DateTime(2016, 12, 30)));
            Assert.AreEqual(new DateTime(2017, 12, 31), Constants.LastDayOfThisYear(new DateTime(2017, 1, 1)));
        }

        [Test]
        public void test_last_day_of_this_month()
        {
            Assert.AreEqual(new DateTime(2016, 1, 31), Constants.LastDayOfThisMonth(new DateTime(2016, 1, 1)));
            Assert.AreEqual(new DateTime(2016, 2, 29), Constants.LastDayOfThisMonth(new DateTime(2016, 2, 29)));
            Assert.AreEqual(new DateTime(2016, 3, 31), Constants.LastDayOfThisMonth(new DateTime(2016, 3, 30)));
        }

        [Test]
        public void test_first_day_of_next_month()
        {
            Assert.AreEqual(new DateTime(2016, 1, 1), Constants.FirstDayOfNextMonth(new DateTime(2015, 12, 31)));
            Assert.AreEqual(new DateTime(2016, 3, 1), Constants.FirstDayOfNextMonth(new DateTime(2016, 2, 28)));
        }
    }
}
