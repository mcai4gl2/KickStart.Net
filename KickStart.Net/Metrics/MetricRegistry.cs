using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Extensions;

namespace KickStart.Net.Metrics
{
    public class MetricRegistry : IMetricSet
    {
        public static string Name(string name, params string[] names)
        {
            return ".".JoinSkipNulls(name, ".".JoinSkipNulls(names));
        }

        public static string Name(Type type, params string[] names)
        {
            return Name(type.Name, names);
        }

        private readonly ConcurrentDictionary<string, IMetric> _metrics;

        public MetricRegistry()
        {
            _metrics = new ConcurrentDictionary<string, IMetric>();   
        }

        public T Register<T>(string name, T metric) where T : IMetric
        {
            if (metric is IMetricSet)
                RegisterAll(name, (IMetricSet) metric);
            else
            {
                _metrics.AddOrUpdate(name, metric, (key, old) => old);
            }
            return metric;
        }

        public void RegisterAll(IMetricSet metrics)
        {
            RegisterAll(null, metrics);
        }

        public bool Remove(string name)
        {
            IMetric removed;
            return _metrics.TryRemove(name, out removed);
        }

        public void RemoveMatching(IMetricFilter filter)
        {
            var keysToRemove = _metrics.Where(entry => filter.Matches(entry.Key, entry.Value))
                .Select(entry => entry.Key);
            foreach (var key in keysToRemove)
                Remove(key);
        }

        private void RegisterAll(string prefix, IMetricSet metrics)
        {
            foreach (var entry in metrics.GetMetrics())
            {
                if (entry.Value is IMetricSet)
                {
                    RegisterAll(Name(prefix, entry.Key), (IMetricSet) entry.Value);
                }
                else
                {
                    Register(Name(prefix, entry.Key), entry.Value);
                }
            }
        }

        private T GetOrAdd<T>(string name, IMetricBuilder<T> builder) where T : IMetric
        {
            return (T)_metrics.GetOrAdd(name, key => Register(name, builder.New()));
        }

        public T GetOrAdd<T>(string name, IMetric metric) where T : IMetric
        {
            return (T) _metrics.GetOrAdd(name, metric);
        }

        public IReadOnlyDictionary<string, IMetric> GetMetrics()
        {
            return _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Counter Counter(string name) => GetOrAdd(name, MetricBuilders.Counters);
        public Meter Meter(string name) => GetOrAdd(name, MetricBuilders.Meters);
        public Histogram Histogram(string name) => GetOrAdd(name, MetricBuilders.Histograms);
        public Timer Timer(string name) => GetOrAdd(name, MetricBuilders.Timers);

        public Dictionary<string, Counter> Counters()
        {
            return Counters(MetricFilters.All);
        }

        public Dictionary<string, Counter> Counters(IMetricFilter filter)
        {
            return Metrics<Counter>(filter);
        }

        public Dictionary<string, Meter> Meters()
        {
            return Metrics<Meter>(MetricFilters.All);
        }

        public Dictionary<string, Meter> Meters(IMetricFilter filter)
        {
            return Metrics<Meter>(filter);
        }

        public Dictionary<string, Histogram> Histograms()
        {
            return Metrics<Histogram>(MetricFilters.All);
        }

        public Dictionary<string, Histogram> Histograms(IMetricFilter filter)
        {
            return Metrics<Histogram>(filter);
        }

        public Dictionary<string, Timer> Timers()
        {
            return Metrics<Timer>(MetricFilters.All);
        }

        public Dictionary<string, Timer> Timers(IMetricFilter filter)
        {
            return Metrics<Timer>(filter);
        }

        private Dictionary<string, T> Metrics<T>(IMetricFilter filter) where T : IMetric
        {
            return _metrics.Where(kvp => filter.Matches(kvp.Key, kvp.Value) && kvp.Value is T)
                           .ToDictionary(kvp => kvp.Key, kvp => (T)kvp.Value);
        } 
    }

    interface IMetricBuilder<T> where T : IMetric
    {
        T New();
        bool Is(IMetric metric);
    }

    static class MetricBuilders
    {
        public static readonly IMetricBuilder<Counter> Counters = new CounterMetricBuilder(); 
        public static readonly IMetricBuilder<Meter> Meters = new MeterMetricBuilder(); 
        public static readonly IMetricBuilder<Histogram> Histograms = new HistogramMetricBuilder(); 
        public static readonly IMetricBuilder<Timer> Timers = new TimerMetricBuilder(); 

        class CounterMetricBuilder : IMetricBuilder<Counter>
        {
            public Counter New()
            {
                return new Counter();
            }

            public bool Is(IMetric metric)
            {
                return metric is Counter;
            }
        }

        class MeterMetricBuilder : IMetricBuilder<Meter>
        {
            public Meter New()
            {
                return new Meter();
            }

            public bool Is(IMetric metric)
            {
                return metric is Meter;
            }
        }

        class HistogramMetricBuilder : IMetricBuilder<Histogram>
        {
            public Histogram New()
            {
                return new Histogram(new ExponentiallyDecayingReservoir());
            }

            public bool Is(IMetric metric)
            {
                return metric is Histogram;
            }
        }

        class TimerMetricBuilder : IMetricBuilder<Timer>
        {
            public Timer New()
            {
                return new Timer();
            }

            public bool Is(IMetric metric)
            {
                return metric is Timer;
            }
        }
    }
}
