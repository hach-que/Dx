using System.Net;
using Dx.Runtime.Tests.Data;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class InvokeTests
    {
        [Fact]
        public void InvokeOnSameNode()
        {
            // Set up nodes.
            var first = new LocalNode();
            first.Bind(IPAddress.Loopback, 9002);

            // Create the bar object in the first node.
            var barFirst = (Bar)new Distributed<Bar>(first, "bar");

            // Assert that the second bar returns the right value.
            Assert.Equal("Hello, World!", barFirst.GetHelloWorldString());

            // Close nodes.
            first.Close();
        }

        [Fact]
        public void InvokeAcrossNodes()
        {
            // Set up nodes.
            var first = new LocalNode();
            var second = new LocalNode();
            first.Bind(IPAddress.Loopback, 9004);
            second.Bind(IPAddress.Loopback, 9005);

            // Create the bar object in the first node.
            new Distributed<Bar>(first, "bar");

            // Retrieve it on the second node.
            var barSecond = (Bar)new Distributed<Bar>(second, "bar");

            // Assert that the second bar returns the right value.
            Assert.Equal("Hello, World!", barSecond.GetHelloWorldString());

            // Close nodes.
            first.Close();
            second.Close();
        }

    }
}
