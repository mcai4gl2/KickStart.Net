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
        public void test_one_min_ewma_with_value_of_three()
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

            ewma.ElapseMinute();
            Assert.AreEqual(0.01098938, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00404277, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00148725, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00054713, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00020128, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00007405, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00002724, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00001002, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00000369, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00000136, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00000050, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.00000018, ewma.GetRate(TimeUnits.Seconds), Epsilon);
        }

        [Test]
        public void test_five_min_ewma_with_value_of_three()
        {
            var ewma = Ewma.FiveMinuteEwma();

            ewma.Update(3);
            ewma.Tick();
            Assert.AreEqual(0.6, ewma.GetRate(TimeUnits.Seconds));

            ewma.ElapseMinute();
            Assert.AreEqual(0.49123845, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.40219203, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.32928698, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.26959738, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.22072766, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.18071653, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.14795818, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.12113791, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.09917933, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.08120117, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.06648190, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.05443077, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.04456415, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.03648604, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.02987224, ewma.GetRate(TimeUnits.Seconds), Epsilon);
        }

        [Test]
        public void test_fifteen_min_ewma_with_value_of_three()
        {
            var ewma = Ewma.FifteenMintueEwma();

            ewma.Update(3);
            ewma.Tick();
            Assert.AreEqual(0.6, ewma.GetRate(TimeUnits.Seconds));

            ewma.ElapseMinute();
            Assert.AreEqual(0.56130419, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.52510399, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.49123845, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.45955700, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.42991879, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.40219203, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.37625345, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.35198773, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.32928698, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.30805027, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.28818318, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.26959738, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.25221023, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.23594443, ewma.GetRate(TimeUnits.Seconds), Epsilon);

            ewma.ElapseMinute();
            Assert.AreEqual(0.22072766, ewma.GetRate(TimeUnits.Seconds), Epsilon);
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
