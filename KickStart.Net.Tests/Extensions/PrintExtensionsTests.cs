using System.Collections.Generic;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
    public class PrintExtensionsTests
    {
        [Test]
        public void test_print_list()
        {
            var inputs = new List<int>() {1, 2, 3, 4, 5};
            inputs.P(3);
        }

        [Test]
        public void test_print_dict()
        {
            var inputs = new Dictionary<int, int>() {{1, 2}, {2, 3}, {3, 4}, {4, 5}};
            inputs.P(3);
        }
    }
}
