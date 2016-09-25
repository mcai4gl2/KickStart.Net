using System;
using KickStart.Net.Metrics;
using NUnit.Framework;

namespace KickStart.Net.Tests.Metrics
{
    [TestFixture]
    public class ConsoleReporterTests
    {
        [Test]
        public void can_report_counter()
        {
            var registry = new MetricRegistry();
            var counter = new Counter();
            counter.Increment(100);
            registry.GetOrAdd<Counter>("test.counter", counter);

            var reporter = new ConsoleReporter(registry, Console.Out);
            reporter.Report();
        }
    }
}
