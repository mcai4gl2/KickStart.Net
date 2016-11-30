using System;
using NUnit.Framework;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class TimeUnitTests
    {
        [Test]
        public void test_ticks()
        {
            var unit = TimeUnits.Ticks;
            Assert.AreEqual(1, unit.ToTicks(1));
            Assert.AreEqual(1, unit.ToMillis(10000));
            Assert.AreEqual(1, unit.ToSeconds(1000 * 10000));
            Assert.AreEqual(1, unit.ToMinutes(60 * 1000 * 10000));
            Assert.AreEqual(1, unit.ToHours(60L * 60 * 1000 * 10000));
            Assert.AreEqual(1, unit.ToDays(24 * 60L * 60 * 1000 * 10000));
            Assert.AreEqual(TimeSpan.FromTicks(1), unit.ToTimeSpan(1));
            Assert.AreEqual(10000, unit.Convert(1, TimeUnits.Milliseconds));
        }

        [Test]
        public void test_millis()
        {
            var unit = TimeUnits.Milliseconds;
            Assert.AreEqual(10000, unit.ToTicks(1));
            Assert.AreEqual(1, unit.ToMillis(1));
            Assert.AreEqual(1, unit.ToSeconds(1000));
            Assert.AreEqual(1, unit.ToMinutes(60 * 1000));
            Assert.AreEqual(1, unit.ToHours(60 * 60 * 1000));
            Assert.AreEqual(1, unit.ToDays(24 * 60 * 60 * 1000));
            Assert.AreEqual(TimeSpan.FromMilliseconds(1), unit.ToTimeSpan(1));
            Assert.AreEqual(1000, unit.Convert(1, TimeUnits.Seconds));
        }

        [Test]
        public void test_seconds()
        {
            var unit = TimeUnits.Seconds;
            Assert.AreEqual(1000 * 10000, unit.ToTicks(1));
            Assert.AreEqual(1000, unit.ToMillis(1));
            Assert.AreEqual(1, unit.ToSeconds(1));
            Assert.AreEqual(1, unit.ToMinutes(60));
            Assert.AreEqual(1, unit.ToHours(60 * 60));
            Assert.AreEqual(1, unit.ToDays(24 * 60 * 60));
            Assert.AreEqual(TimeSpan.FromSeconds(1), unit.ToTimeSpan(1));
            Assert.AreEqual(1, unit.Convert(1000, TimeUnits.Milliseconds));
        }

        [Test]
        public void test_minutes()
        {
            var unit = TimeUnits.Minutes;
            Assert.AreEqual(60 * 1000 * 10000, unit.ToTicks(1));
            Assert.AreEqual(60 * 1000, unit.ToMillis(1));
            Assert.AreEqual(60, unit.ToSeconds(1));
            Assert.AreEqual(1, unit.ToMinutes(1));
            Assert.AreEqual(1, unit.ToHours(60));
            Assert.AreEqual(1, unit.ToDays(24 * 60));
            Assert.AreEqual(TimeSpan.FromMinutes(1), unit.ToTimeSpan(1));
            Assert.AreEqual(1, unit.Convert(60, TimeUnits.Seconds));
        }

        [Test]
        public void test_hours()
        {
            var unit = TimeUnits.Hours;
            Assert.AreEqual(60L * 60 * 1000 * 10000, unit.ToTicks(1));
            Assert.AreEqual(60 * 60 * 1000, unit.ToMillis(1));
            Assert.AreEqual(60 * 60, unit.ToSeconds(1));
            Assert.AreEqual(60, unit.ToMinutes(1));
            Assert.AreEqual(1, unit.ToHours(1));
            Assert.AreEqual(1, unit.ToDays(24));
            Assert.AreEqual(TimeSpan.FromHours(1), unit.ToTimeSpan(1));
            Assert.AreEqual(1, unit.Convert(60 * 60, TimeUnits.Seconds));
        }

        [Test]
        public void test_days()
        {
            var unit = TimeUnits.Days;
            Assert.AreEqual(24L * 60 * 60 * 1000 * 10000, unit.ToTicks(1));
            Assert.AreEqual(24L * 60 * 60 * 1000, unit.ToMillis(1));
            Assert.AreEqual(24 * 60 * 60, unit.ToSeconds(1));
            Assert.AreEqual(24 * 60, unit.ToMinutes(1));
            Assert.AreEqual(24, unit.ToHours(1));
            Assert.AreEqual(1, unit.ToDays(1));
            Assert.AreEqual(TimeSpan.FromDays(1), unit.ToTimeSpan(1));
            Assert.AreEqual(1, unit.Convert(24, TimeUnits.Hours));
        }
    }
}
