using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KickStart.Net.Collections;
using KickStart.Net.Extensions;

namespace KickStart.Net.Configurations
{
    public class InMemoryConfigurationManager : IConfigurationManager
    {
        private readonly Dictionary<string, string> _configurations; 

        public InMemoryConfigurationManager(Dictionary<string, string> configurations)
            : this("InMemory", configurations)
        {

        }

        public InMemoryConfigurationManager(string name, Dictionary<string, string> configurations)
        {
            Name = name;
            _configurations = configurations;
        }

        public string Name { get; }

        public T GetOrDefault<T>(string key)
        {
            try
            {
                if (!_configurations.SafeContainsKey(key))
                    return default(T);
                var value = _configurations.SafeGet(key);
                var typeConverter = TypeDescriptor.GetConverter(typeof (T));
                return (T) typeConverter.ConvertFromString(value);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public IEnumerable<T> GetAll<T>(string key)
        {
            var config = GetOrDefault<T>(key);
            if (!Objects.SafeEquals(config, default(T)))
                return new List<T> { GetOrDefault<T>(key) };
            return Lists<T>.EmptyList;
        }

        public T GetOrDefault<T>(string environment, string key)
        {
            return GetOrDefault<T>(key);
        }

        public IEnumerable<T> GetAll<T>(string environment, string key)
        {
            return GetAll<T>(key);
        }

        public IConfiguration GetConfigurationOrDefault(string key)
        {
            if (_configurations.SafeContainsKey(key))
                return new Configuration
                {
                    Source = Name,
                    Key = key,
                    Value = _configurations.SafeGet(key)
                };
            return default(IConfiguration);
        }

        public IConfiguration GetConfigurationOrDefault(string environment, string key)
        {
            if (_configurations.SafeContainsKey(key))
                return new Configuration
                {
                    Source = Name,
                    Key = key,
                    Environment = environment,
                    Value = _configurations.SafeGet(key)
                };
            return default(IConfiguration);
        }

        public IEnumerable<IConfiguration> GetAllConfigurations(string key)
        {
            var config = GetConfigurationOrDefault(key);
            if (!Objects.SafeEquals(config, default(IConfiguration)))
                return new List<IConfiguration> { GetConfigurationOrDefault(key) };
            return Lists<IConfiguration>.EmptyList;
        }

        public IEnumerable<IConfiguration> GetAllConfigurations(string environment, string key)
        {
            var config = GetConfigurationOrDefault(environment, key);
            if (!Objects.SafeEquals(config, default(IConfiguration)))
                return new List<IConfiguration> { config };
            return Lists<IConfiguration>.EmptyList;
        }

        public IEnumerable<IConfiguration> GetAllConfigurations()
        {
            return _configurations.Keys.Select(key => new Configuration
            {
                Source = Name,
                Key = key,
                Value = _configurations[key]
            }).Cast<IConfiguration>();
        }

        public IEnumerable<IConfiguration> GetAllConfigurationsForEnvironment(string environment)
        {
            return _configurations.Keys.Select(key => new Configuration
            {
                Source = Name,
                Key = key,
                Environment = environment,
                Value = _configurations[key]
            }).Cast<IConfiguration>();
        }
    }
}
