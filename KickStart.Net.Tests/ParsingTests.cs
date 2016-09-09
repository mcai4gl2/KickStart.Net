using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KickStart.Net;

namespace KickStart.Net.Tests
{
    [TestFixture]
    public class ParsingTests
    {
        [Test]
        public void Parse_returns_parsed_value()
        {
            Assert.AreEqual(1, "1".Parse<int>(int.TryParse));
        }

        [Test]
        public void Parse_returns_default_value_if_text_cannot_be_parsed()
        {
            Assert.AreEqual(0, "a1".Parse<int>(int.TryParse));
        }

        [Test]
        public void Parse_returns_used_specified_default_value_if_text_cannot_be_parsed()
        {
            Assert.AreEqual(-1, "a1".Parse(int.TryParse, -1));
        }

        [Test]
        public void Choose_returns_valid_values()
        {
            var parsed = new[] { "1" }.Parse<int>(int.TryParse);
            Assert.AreEqual(1, parsed.Single());
        }

        [Test]
        public void Choose_skips_invalid_values_returning_only_values_that_can_be_parsed()
        {
            var parsed = new[] { "bad-one", "1", "" }.Parse<int>(int.TryParse);
            Assert.AreEqual(1, parsed.Single());
        }

        [Test]
        public void Choose_returns_all_valid_values()
        {
            var parsed = new[] { "1", "2" }.Parse<int>(int.TryParse);
            Assert.AreEqual(2, parsed.Count());
            Assert.AreEqual(1, parsed.First());
            Assert.AreEqual(2, parsed.Skip(1).First());
        }

    }
}
