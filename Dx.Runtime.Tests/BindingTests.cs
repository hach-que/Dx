using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class BindingTests
    {
        [Fact]
        public void CanCreateNode()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new LocalNode();
        }

        [Fact]
        public void CanBindNode()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 9000);
            node.Close();
        }

        [Fact]
        public void NodeBindsAndClosesCleanly()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 9001);
            node.Close();

            var second = new LocalNode();
            second.Bind(IPAddress.Loopback, 9001);
            second.Close();
        }

        [Fact]
        public void TwoNodesCanRunInTheSameProcessOnDifferentPorts()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 9002);

            var second = new LocalNode();
            second.Bind(IPAddress.Loopback, 9003);

            node.Close();
            second.Close();
        }

        [Fact]
        public void TwoNodesCanNotRunInTheSameProcessOnTheSamePort()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 9004);

            var second = new LocalNode();
            Assert.Throws<SocketException>(() => second.Bind(IPAddress.Loopback, 9004));

            node.Close();
        }
    }
}
