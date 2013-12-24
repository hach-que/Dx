using System;
using System.Net;
using Dx.Runtime.Tests.Data;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class SemanticTests
    {
        [Fact]
        public void InvocationIsCorrectForServer()
        {
            // Set up nodes.
            var first = new LocalNode(Architecture.ServerClient, Caching.PushOnChange) { IsServer = true };
            var second = new LocalNode(Architecture.ServerClient, Caching.PushOnChange);
            first.Bind(IPAddress.Loopback, 9004);
            second.Bind(IPAddress.Loopback, 9005);

            // Connect the second node to the first.
            second.GetService<IClientConnector>().Connect(IPAddress.Loopback, 9004);

            // Create the bar object in the first node.
            var barFirst = (Semantic)new Distributed<Semantic>(first, "semantic");

            // Retrieve it on the second node.
            var barSecond = (Semantic)new Distributed<Semantic>(second, "semantic");

            // Set the property.
            barFirst.Value = "Hello, World!";

            // If we try and call any of the methods from the server, they should
            // all work.
            Assert.Equal("Hello, World!", barFirst.GetException());
            Assert.Equal("Hello, World!", barFirst.GetIgnore());
            Assert.Equal("Hello, World!", barFirst.GetValue());

            // If we try and call barSecond.GetException, we should get an exception
            // because we are not a server.
            Assert.Throws<MemberAccessException>(() => barSecond.GetException());

            // If we try and call barSecond.GetIgnore, we should get a null value
            // because we are not a server.
            Assert.Equal(null, barSecond.GetIgnore());

            // If we try and call barSecond.GetValue, we should get "Hello, World!"
            Assert.Equal("Hello, World!", barSecond.GetValue());

            // Close nodes.
            first.Close();
            second.Close();
        }
    }
}
