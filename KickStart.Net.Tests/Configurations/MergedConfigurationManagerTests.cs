using System.Collections.Generic;
using System.Linq;
using KickStart.Net.Collections;
using KickStart.Net.Configurations;
using NUnit.Framework;

namespace KickStart.Net.Tests.Configurations
{
    [TestFixture]
    public class MergedConfigurationManagerTests
    {
        private IConfigurationManager _configurationManager;

        [SetUp]
        public void SetUp()
        {
            var firstConfigurationManager = new InMemoryConfigurationManager("First", new Dictionary<string, string>
            {
                {"TestKeyString1", "TestValue1"},
                {"TestKeyBool1", "true"},
                {"TestKeyBoolNonParsable1", "test1"},
                {"TestKeyInt1", "5001"},
                {"TestSame", "123" }
            });
            var secondConfigurationManager = new InMemoryConfigurationManager("Second", new Dictionary<string, string>
            {
                {"TestKeyString2", "TestValue2"},
                {"TestKeyBool2", "false"},
                {"TestKeyBoolNonParsable2", "test2"},
                {"TestKeyInt2", "5002"},
                {"TestSame", "234" }
            });
            _configurationManager = new MergedConfigurationManager(firstConfigurationManager, secondConfigurationManager);
        }

        [Test]
        public void test_name()
        {
            Assert.AreEqual("First.Second", _configurationManager.Name);
        }

        [Test]
        public void test_get_value_or_default_and_key_exists()
        {
            Assert.AreEqual("TestValue1", _configurationManager.GetOrDefault<string>("TestKeyString1"));
            Assert.AreEqual(true, _configurationManager.GetOrDefault<bool>("TestKeyBool1"));
            Assert.AreEqual(5001, _configurationManager.GetOrDefault<int>("TestKeyInt1"));
            Assert.AreEqual("TestValue2", _configurationManager.GetOrDefault<string>("TestKeyString2"));
            Assert.AreEqual(false, _configurationManager.GetOrDefault<bool>("TestKeyBool2"));
            Assert.AreEqual(5002, _configurationManager.GetOrDefault<int>("TestKeyInt2"));
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
            Assert.AreEqual(new List<string> { "TestValue1" }, _configurationManager.GetAll<string>("TestKeyString1"));
            Assert.AreEqual(new List<string> { "TestValue2" }, _configurationManager.GetAll<string>("TestKeyString2"));
            Assert.AreEqual(new List<bool> { true }, _configurationManager.GetAll<bool>("TestKeyBool1"));
            Assert.AreEqual(Lists<string>.EmptyList, _configurationManager.GetAll<string>("TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_or_default_with_environment()
        {
            Assert.AreEqual("TestValue1", _configurationManager.GetOrDefault<string>("testEnvironment", "TestKeyString1"));
            Assert.AreEqual(true, _configurationManager.GetOrDefault<bool>("testEnvironment", "TestKeyBool1"));
            Assert.AreEqual(5001, _configurationManager.GetOrDefault<int>("testEnvironment", "TestKeyInt1"));
            Assert.AreEqual("123", _configurationManager.GetOrDefault<string>("TestSame"));
        }

        [Test]
        public void test_get_all_with_environment()
        {
            Assert.AreEqual(new List<string> { "TestValue1" }, _configurationManager.GetAll<string>("testEnvironment", "TestKeyString1"));
            Assert.AreEqual(new List<bool> { true }, _configurationManager.GetAll<bool>("testEnvironment", "TestKeyBool1"));
            Assert.AreEqual(Lists<string>.EmptyList, _configurationManager.GetAll<string>("testEnvironment", "TestKeyStringNonExists1"));
            Assert.AreEqual(new List<string> {"123"}, _configurationManager.GetAll<string>("testEnvironment", "TestSame"));
        }

        [Test]
        public void test_get_config_or_default()
        {
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestKeyString1",
                Value = "TestValue1"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyString1"));
            Assert.AreEqual(new Configuration
            {
                Source = "Second",
                Key = "TestKeyBool2",
                Value = "false"
            }, _configurationManager.GetConfigurationOrDefault("TestKeyBool2"));
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestSame",
                Value = "123"
            }, _configurationManager.GetConfigurationOrDefault("TestSame"));
            Assert.AreEqual(null, _configurationManager.GetConfigurationOrDefault("TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_config_or_default_with_environment()
        {
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestKeyString1",
                Environment = "TestEnvironment",
                Value = "TestValue1"
            }, _configurationManager.GetConfigurationOrDefault("TestEnvironment", "TestKeyString1"));
            Assert.AreEqual(new Configuration
            {
                Source = "Second",
                Key = "TestKeyBool2",
                Environment = "TestEnvironment",
                Value = "false"
            }, _configurationManager.GetConfigurationOrDefault("TestEnvironment", "TestKeyBool2"));
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestSame",
                Environment = "TestEnvironment",
                Value = "123"
            }, _configurationManager.GetConfigurationOrDefault("TestEnvironment", "TestSame"));
            Assert.AreEqual(null, _configurationManager.GetConfigurationOrDefault("TestEnvironment", "TestKeyStringNonExists"));
        }

        [Test]
        public void test_get_all_configurations_with_key()
        {
            var configs = _configurationManager.GetAllConfigurations("testEnvironment", "TestKeyString1").ToList();
            Assert.AreEqual(1, configs.Count);
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestKeyString1",
                Environment = "testEnvironment",
                Value = "TestValue1"
            }, configs.First());
            configs = _configurationManager.GetAllConfigurations("TestKeyString2").ToList();
            Assert.AreEqual(1, configs.Count);
            Assert.AreEqual(new Configuration
            {
                Source = "Second",
                Key = "TestKeyString2",
                Value = "TestValue2"
            }, configs.First());

            configs = _configurationManager.GetAllConfigurations("TestKeyNonExist").ToList();
            Assert.AreEqual(0, configs.Count);
            configs = _configurationManager.GetAllConfigurations("testEnvironment", "TestKeyNonExist").ToList();
            Assert.AreEqual(0, configs.Count);

            configs = _configurationManager.GetAllConfigurations("TestSame").ToList();
            Assert.AreEqual(1, configs.Count);
            Assert.AreEqual(new Configuration
            {
                Source = "First",
                Key = "TestSame",
                Value = "123"
            }, configs.First());
        }

        [Test]
        public void test_get_all_configurations()
        {
            Assert.AreEqual(9, _configurationManager.GetAllConfigurations().Count());
            Assert.IsTrue(_configurationManager.GetAllConfigurations().Any(c => c.Key == "TestSame" && c.Value == "123"));
            Assert.AreEqual(9, _configurationManager.GetAllConfigurationsForEnvironment("TestEnvironment").Count());
            Assert.IsTrue(_configurationManager.GetAllConfigurations().Any(c => c.Key == "TestSame" && c.Value == "123"));
        }
    }
}
