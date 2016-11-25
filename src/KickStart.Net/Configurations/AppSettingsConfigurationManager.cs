#if !NET_CORE
using System;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Collections;

namespace KickStart.Net.Configurations
{
    public class AppSettingsConfigurationManager : IConfigurationManager
    {
        public AppSettingsConfigurationManager()
            : this("app.config")
        {
            
        }

        public AppSettingsConfigurationManager(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public T GetOrDefault<T>(string key)
        {
            try
            {
                if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                    return default(T);
                var value = ConfigurationManager.AppSettings[key];
                var typeConverter = TypeDescriptor.GetConverter(typeof (T));
                return (T)typeConverter.ConvertFromString(value);
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
                return new List<T> {GetOrDefault<T>(key)};
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
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return default(IConfiguration);
            return new Configuration
            {
                Source = Name,
                Key = key,
                Value = ConfigurationManager.AppSettings[key]
            };
        }

        public IConfiguration GetConfigurationOrDefault(string environment, string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return default(IConfiguration);
            return new Configuration
            {
                Source = Name,
                Key = key,
                Environment = environment,
                Value = ConfigurationManager.AppSettings[key]
            };
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
            return ConfigurationManager.AppSettings.AllKeys.Select(key => new Configuration
            {
                Source = Name,
                Key = key,
                Value = ConfigurationManager.AppSettings[key]
            }).Cast<IConfiguration>();
        }

        public IEnumerable<IConfiguration> GetAllConfigurationsForEnvironment(string environment)
        {
            return ConfigurationManager.AppSettings.AllKeys.Select(key => new Configuration
            {
                Source = Name,
                Key = key,
                Environment = environment,
                Value = ConfigurationManager.AppSettings[key]
            }).Cast<IConfiguration>();
        }
    }
}
#endif