using System;
using System.IO;
using KickStart.Net.Extensions;

namespace KickStart.Net.Metrics
{
    public class ConsoleReporter : IReporter
    {
        private const int ConsoleWidth = 80;

        private readonly MetricRegistry _registry;
        private readonly TextWriter _outputWriter;
        private readonly IClock _clock;
        private readonly IMetricFilter _filter;

        public ConsoleReporter(MetricRegistry registry, TextWriter outputWriter)
            : this(registry, outputWriter, Clocks.Default, MetricFilters.All)
        {
        }

        public ConsoleReporter(MetricRegistry registry, TextWriter outputWriter, IClock clock)
            : this(registry, outputWriter, clock, MetricFilters.All)
        {
        }

        public ConsoleReporter(MetricRegistry registry, TextWriter outputWriter, IClock clock, IMetricFilter filter)
        {
            _registry = registry;
            _outputWriter = outputWriter;
            _clock = clock;
            _filter = filter;
        }

        public void Report()
        {
            var datetime = new DateTime(_clock.Tick);
            PrintWithBanner(datetime.ToString(), '=');
            _outputWriter.WriteLine();

            PrintWithBanner("-- Counters", '-');
            foreach (var counter in _registry.Counters(_filter))
            {
                _outputWriter.WriteLine(counter.Key);
                PrintCounter(counter.Value);
                _outputWriter.WriteLine();
            }

            PrintWithBanner("-- Histograms", '-');
            foreach (var histogram in _registry.Histograms(_filter))
            {
                _outputWriter.WriteLine(histogram.Key);
                PrintHistogram(histogram.Value);
                _outputWriter.WriteLine();
            }

            PrintWithBanner("-- Meters", '-');
            foreach (var meter in _registry.Meters(_filter))
            {
                _outputWriter.WriteLine(meter.Key);
                PrintMeter(meter.Value);
                _outputWriter.WriteLine();
            }

            PrintWithBanner("-- Timers", '-');
            foreach (var timer in _registry.Timers(_filter))
            {
                _outputWriter.WriteLine(timer.Key);
                PrintTimer(timer.Value);
                _outputWriter.WriteLine();
            }
        }

        private void PrintCounter(Counter counter)
        {
            _outputWriter.WriteLine($"             count = {counter.Count}");
        }

        private void PrintHistogram(Histogram histogram)
        {
            _outputWriter.WriteLine($"             count = {histogram.Count}");
            var snapshot = histogram.GetSnapshot();
            _outputWriter.WriteLine($"               min = {snapshot.GetMin()}");
            _outputWriter.WriteLine($"               max = {snapshot.GetMax()}");
            _outputWriter.WriteLine($"              mean = {snapshot.GetMean()}");
            _outputWriter.WriteLine($"            stddev = {snapshot.GetStdDev()}");
            _outputWriter.WriteLine($"            median = {snapshot.GetMedian()}");
            _outputWriter.WriteLine($"               75% = {snapshot.Get75thPercentile()}");
            _outputWriter.WriteLine($"               95% = {snapshot.Get95thPercentile()}");
            _outputWriter.WriteLine($"               98% = {snapshot.Get98thPercentile()}");
            _outputWriter.WriteLine($"               99% = {snapshot.Get99thPercentile()}");
            _outputWriter.WriteLine($"             99.9% = {snapshot.Get999thPercentile()}");
        }

        private void PrintMeter(Meter meter)
        {
            _outputWriter.WriteLine($"             count = {meter.Count}");
            _outputWriter.WriteLine($"              mean = {meter.MeanRate}");
            _outputWriter.WriteLine($"          1-minute = {meter.OneMinuteRate}");
            _outputWriter.WriteLine($"          5-minute = {meter.FiveMinutesRate}");
            _outputWriter.WriteLine($"         15-minute = {meter.FifteenMinutesRate}");
        }

        private void PrintTimer(Timer timer)
        {
            var snapshot = timer.GetSnapshot();
            _outputWriter.WriteLine($"             count = {timer.Count}");
            _outputWriter.WriteLine($"              mean = {timer.MeanRate}");
            _outputWriter.WriteLine($"          1-minute = {timer.OneMinuteRate}");
            _outputWriter.WriteLine($"          5-minute = {timer.FiveMinutesRate}");
            _outputWriter.WriteLine($"         15-minute = {timer.FifteenMinutesRate}");

            _outputWriter.WriteLine($"               min = {snapshot.GetMin()}");
            _outputWriter.WriteLine($"               max = {snapshot.GetMax()}");
            _outputWriter.WriteLine($"              mean = {snapshot.GetMean()}");
            _outputWriter.WriteLine($"            stddev = {snapshot.GetStdDev()}");
            _outputWriter.WriteLine($"            median = {snapshot.GetMedian()}");
            _outputWriter.WriteLine($"               75% = {snapshot.Get75thPercentile()}");
            _outputWriter.WriteLine($"               95% = {snapshot.Get95thPercentile()}");
            _outputWriter.WriteLine($"               98% = {snapshot.Get98thPercentile()}");
            _outputWriter.WriteLine($"               99% = {snapshot.Get99thPercentile()}");
            _outputWriter.WriteLine($"             99.9% = {snapshot.Get999thPercentile()}");
        }

        private void PrintWithBanner(string s, char c)
        {
            _outputWriter.Write(s);
            _outputWriter.Write(' ');
            foreach (var i in (ConsoleWidth - s.Length - 1).Range())
                _outputWriter.Write(c);
            _outputWriter.WriteLine();
        }
    }
}
