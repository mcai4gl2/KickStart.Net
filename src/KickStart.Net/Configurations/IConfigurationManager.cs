using System;
using System.Collections.Generic;

namespace KickStart.Net.Configurations
{
    public interface IConfigurationManager
    {
        /// <summary>The name of the configuration manager</summary>
        string Name { get; }
        /// <summary>Get the configuration value by <paramref name="key"/>.</summary>
        /// <remarks>If there are multiple configurations, this method just returns the first one available.</remarks>
        T GetOrDefault<T>(string key);
        /// <summary>Get all configuration values by <paramref name="key"/>.</summary>
        IEnumerable<T> GetAll<T>(string key);
        /// <summary>Get the configuration value by <paramref name="environment"/> and <paramref name="key"/>.</summary>
        /// <remarks>If there are multiple configurations, this method just returns the first one available.</remarks>
        T GetOrDefault<T>(string environment, string key);
        /// <summary>Get all configuration values by <paramref name="environment"/> and <paramref name="key"/>.</summary>
        IEnumerable<T> GetAll<T>(string environment, string key);
        /// <summary>Get the configuration by <paramref name="key"/>.</summary>
        /// <remarks>If there are multiple configurations, this method just returns the first one available.</remarks>
        IConfiguration GetConfigurationOrDefault(string key);
        /// <summary>Get the configuration by <paramref name="environment"/> and <paramref name="key"/>.</summary>
        /// <remarks>If there are multiple configurations, this method just returns the first one available.</remarks>
        IConfiguration GetConfigurationOrDefault(string environment, string key);
        /// <summary>Get all configurations by <paramref name="key"/></summary>
        IEnumerable<IConfiguration> GetAllConfigurations(string key);
        /// <summary>Get all configurations by <paramref name="environment"/> and <paramref name="key"/>.</summary>
        IEnumerable<IConfiguration> GetAllConfigurations(string environment, string key);
        /// <summary>Get all configurations.</summary>
        IEnumerable<IConfiguration> GetAllConfigurations();
        /// <summary>Get all configurations by <paramref name="environment"/>.</summary>
        IEnumerable<IConfiguration> GetAllConfigurationsForEnvironment(string environment);
    }
}
