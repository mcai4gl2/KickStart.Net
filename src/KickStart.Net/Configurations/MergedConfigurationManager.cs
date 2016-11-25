using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Extensions;

namespace KickStart.Net.Configurations
{
    public class MergedConfigurationManager : IConfigurationManager
    {
        private readonly IConfigurationManager _one;
        private readonly IConfigurationManager _other;
        private readonly IConfigurationEqualityComparer _comparer;

        public MergedConfigurationManager(IConfigurationManager one, IConfigurationManager other)
        {
            _one = one;
            _other = other;
            Name = Configurations.Name(_one.Name, _other.Name);
            _comparer = new IConfigurationEqualityComparer();
        }

        public string Name { get; }

        public T GetOrDefault<T>(string key)
        {
            var first = _one.GetOrDefault<T>(key);
            if (Objects.SafeEquals(first, default(T)))
            {
                return _other.GetOrDefault<T>(key);
            }
            return first;
        }

        public IEnumerable<T> GetAll<T>(string key)
        {
            var first = _one.GetAll<T>(key).ToList();
            if (first.Count == 0)
                return _other.GetAll<T>(key);
            return first;
        }

        public T GetOrDefault<T>(string environment, string key)
        {
            var first = _one.GetOrDefault<T>(environment, key);
            if (Objects.SafeEquals(first, default(T)))
            {
                return _other.GetOrDefault<T>(environment, key);
            }
            return first;
        }

        public IEnumerable<T> GetAll<T>(string environment, string key)
        {
            var first = _one.GetAll<T>(environment, key).ToList();
            if (first.Count == 0)
                return _other.GetAll<T>(environment, key);
            return first;
        }

        public IConfiguration GetConfigurationOrDefault(string key)
        {
            var first = _one.GetConfigurationOrDefault(key);
            if (Objects.SafeEquals(first, default(IConfiguration)))
                return _other.GetConfigurationOrDefault(key);
            return first;
        }

        public IConfiguration GetConfigurationOrDefault(string environment, string key)
        {
            var first = _one.GetConfigurationOrDefault(environment, key);
            if (Objects.SafeEquals(first, default(IConfiguration)))
                return _other.GetConfigurationOrDefault(environment, key);
            return first;
        }

        public IEnumerable<IConfiguration> GetAllConfigurations(string key)
        {
            var first = _one.GetAllConfigurations(key).ToList();
            if (first.Count == 0)
                return _other.GetAllConfigurations(key);
            return first;
        }

        public IEnumerable<IConfiguration> GetAllConfigurations(string environment, string key)
        {
            var first = _one.GetAllConfigurations(environment, key).ToList();
            if (first.Count == 0)
                return _other.GetAllConfigurations(environment, key);
            return first;
        }

        public IEnumerable<IConfiguration> GetAllConfigurations()
        {
            var first = _one.GetAllConfigurations().ToList();
            var second = _other.GetAllConfigurations().ToList();
            var result = first.Union(second).Distinct(_comparer);
            return result.Override(first, config => new {config.Key, config.Environment});
        }

        public IEnumerable<IConfiguration> GetAllConfigurationsForEnvironment(string environment)
        {
            var first = _one.GetAllConfigurationsForEnvironment(environment).ToList();
            var second = _other.GetAllConfigurationsForEnvironment(environment).ToList();
            var result = first.Union(second).Distinct(_comparer);
            return result.Override(first, config => new { config.Key });
        }

        class IConfigurationEqualityComparer : IEqualityComparer<IConfiguration>
        {
            public bool Equals(IConfiguration x, IConfiguration y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return Objects.SafeEquals(x.Key, x.Key) &&
                       Objects.SafeEquals(x.Environment, y.Environment);
            }

            public int GetHashCode(IConfiguration obj)
            {
                if (obj == null) return 0;
                return obj.Key.GetHashCode();
            }
        }
    }
}
