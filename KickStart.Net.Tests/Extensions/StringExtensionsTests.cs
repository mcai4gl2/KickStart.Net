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
    public class StringExtensionsTests
    {
        [Test]
        public void join_can_skip_nulls()
        {
            Assert.AreEqual("1,2,3,4", ",".JoinSkipNulls("", null, "1", "2", "", "3", "4"));
        }
    }
}
