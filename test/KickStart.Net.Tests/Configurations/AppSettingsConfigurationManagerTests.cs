#if !NET_CORE
using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Collections;
using KickStart.Net.Configurations;
using NUnit.Framework;

namespace KickStart.Net.Tests.Configurations
{
    [TestFixture]
    public class AppSettingsConfigurationManagerTests
    {
        private IConfigurationManager _configurationManager;

        [SetUp]
        public void SetUp()
        {
            _configurationManager = new AppSettingsConfigurationManager();
        }

        [Test]
        public void test_default_name()
        {
            Assert.AreEqual("app.config", _configurationManager.Name);
        }

        [Test]
        public void test_get_value_or_default_and_key_exists()
        {
            Assert.AreEqual("TestValue", _configurationManager.GetOrDefault<string>("TestKeyString"));
            Assert.AreEqual(true, _configurationManager.GetOrDefault<bool>("TestKeyBool"));
            Assert.AreEqual(500, _configurationManager.GetOrDefault<int>("TestKeyInt"));
        }

        [Test]
        public void test_get_value_or_default_when_key_not_exists()
        {
            Assert.AreEqual(null, _configurationManager.GetOrDefault<string>("TestKeyStringNonExists"));
            Assert.AreEqual(false, _configurationManager.GetOrDefault<bool>("TestKeyBoolNonExists"));
            Assert.AreEqual(null, _configurationManager.GetOrDefault<int?>("TestKeyIntNonExists"));
        }

        [Test]
        public void test_non_parsable_config()
        {
            Assert.AreEqual(false, _configurationManager.GetOrDefault<bool>("TestKeyBoolNonParsable"));
            Assert.AreEqual(null, _configurationManager.GetOrDefault<bool?>("TestKeyBoolNonParsable"));
        }

        [Test]
        public void test_get_all()
        {
            Assert.AreEqual(new List<string>{"TestValue"}, _configurationManager.GetAll<string>("TestKeyString"));
            Assert.AreEqual(new List<bool> {true}, _configurationManager.GetAll<bool>("TestKeyBool"));
            Assert.AreEqual(Lists<string>.EmptyList, _configurationManager.GetAll<string>("TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_or_default_with_environment()
        {
            // Environment doesn't matter for the AppSettings config as app.config doesn't support multi environment
            Assert.AreEqual("TestValue", _configurationManager.GetOrDefault<string>("testEnvironment", "TestKeyString"));
            Assert.AreEqual(true, _configurationManager.GetOrDefault<bool>("testEnvironment", "TestKeyBool"));
            Assert.AreEqual(500, _configurationManager.GetOrDefault<int>("testEnvironment", "TestKeyInt"));
        }

        [Test]
        public void test_get_all_with_environment()
        {
            Assert.AreEqual(new List<string> { "TestValue" }, _configurationManager.GetAll<string>("testEnvironment", "TestKeyString"));
            Assert.AreEqual(new List<bool> { true }, _configurationManager.GetAll<bool>("testEnvironment", "TestKeyBool"));
            Assert.AreEqual(Lists<string>.EmptyList, _configurationManager.GetAll<string>("testEnvironment", "TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_config_or_default()
        {
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyString",
                Value = "TestValue"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyString"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyBool",
                Value = "true"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyBool"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyBoolNonParsable",
                Value = "test"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyBoolNonParsable"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyInt",
                Value = "500"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyInt"));
            Assert.AreEqual(null, _configurationManager.GetConfigurationOrDefault("TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_config_or_default_with_environment()
        {
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyString",
                Environment = "testEnvironment",
                Value = "TestValue"
            }, _configurationManager.GetConfigurationOrDefault("testEnvironment", "TestKeyString"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyBool",
                Environment = "testEnvironment",
                Value = "true"
            }, _configurationManager.GetConfigurationOrDefault("testEnvironment", "TestKeyBool"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyBoolNonParsable",
                Environment = "testEnvironment",
                Value = "test"
            }, _configurationManager.GetConfigurationOrDefault("testEnvironment", "TestKeyBoolNonParsable"));
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyInt",
                Environment = "testEnvironment",
                Value = "500"
            }, _configurationManager.GetConfigurationOrDefault("testEnvironment", "TestKeyInt"));
            Assert.AreEqual(null, _configurationManager.GetConfigurationOrDefault("testEnvironment", "TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_all_configurations_with_key()
        {
            var configs = _configurationManager.GetAllConfigurations("testEnvironment", "TestKeyString").ToList();
            Assert.AreEqual(1, configs.Count);
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyString",
                Environment = "testEnvironment",
                Value = "TestValue"
            }, configs.First());
            configs = _configurationManager.GetAllConfigurations("TestKeyString").ToList();
            Assert.AreEqual(1, configs.Count);
            Assert.AreEqual(new Configuration
            {
                Source = "app.config",
                Key = "TestKeyString",
                Value = "TestValue"
            }, configs.First());

            configs = _configurationManager.GetAllConfigurations("TestKeyNonExist").ToList();
            Assert.AreEqual(0, configs.Count);
            configs = _configurationManager.GetAllConfigurations("testEnvironment", "TestKeyNonExist").ToList();
            Assert.AreEqual(0, configs.Count);
        }

        [Test]
        public void test_get_all_configurations()
        {
            Assert.AreEqual(4, _configurationManager.GetAllConfigurations().Count());
            Assert.AreEqual(4, _configurationManager.GetAllConfigurationsForEnvironment("TestEnvironment").Count());
        }
    }
}
#endif