using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class StreamExtensionsTests
    {
        [Test]
        public void test_to_stream_from_byte_array()
        {
            var input = "Input test data";
            var bytes = Encoding.UTF8.GetBytes(input);
            var stream = bytes.ToStream();
            Assert.AreEqual(input, stream.StreamToString());
        }

        [Test]
        public void test_to_stream_from_string()
        {
            var input = "Input test data";
            var stream = input.ToStream();
            Assert.AreEqual(input, stream.StreamToString());
        }
    }
}
