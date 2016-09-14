using System;
using KickStart.Net.Extensions;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class EwmaTests
    {
        private static readonly double Epsilon = 0.000001;

        [Test]
        public void test_one_min_ewma()
        {
            var ewma = Ewma.OneMinuteEwma();

            ewma.Update(3);
            ewma.Tick();
            Assert.AreEqual(0.6, ewma.GetRate(TimeUnits.Seconds));

            ewma.ElapseMinute();
            Assert.AreEqual(0.22072766, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.08120117, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.02987224, ewma.GetRate(TimeUnits.Seconds), Epsilon);


        }
    }

    static class EwmaExtensions
    {
        public static void ElapseMinute(this Ewma ewma)
        {
            foreach (var _ in 12.Range())
                ewma.Tick();
        }
    }
}
