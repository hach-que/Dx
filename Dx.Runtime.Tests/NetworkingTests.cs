using Xunit;
using Dx.Runtime.Tests.Data;
using System.Linq;

namespace Dx.Runtime.Tests
{
    public class NetworkingTests
    {
        private struct TwoNodes
        {
            public ILocalNode NodeA;
            public ILocalNode NodeB;
        }
        
        private TwoNodes SetupNetwork()
        {
            var id = ID.NewRandom();
            var result = new TwoNodes();
            var factory = new InternalDxFactory();
            result.NodeA = factory.CreateLocalNode();
            factory.CreateNodeB = true;
            result.NodeB = factory.CreateLocalNode();
            result.NodeA.Join(id);
            result.NodeB.Join(id);
            return result;
        }
        
        private void AssertNoActiveConnections()
        {
            Assert.False(ContactPool.GetAllOpenClients().Any());
        }
        
        [Fact]
        public void NodesCanCommunicate()
        {
            this.AssertNoActiveConnections();
            
            var network = this.SetupNetwork();
            try
            {
                // Create a Foo from node A.
                var fooA = (Foo)new Distributed<Foo>(network.NodeA, "foo");
                fooA.TestValue = "My test value!";
                
                // Retrieve the Foo from node B.
                var fooB = (Foo)new Distributed<Foo>(network.NodeB, "foo");
                Assert.Equal("My test value!", fooB.TestValue);
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }
            
            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void DeserializationWorksAcrossPropertyChain()
        {
            this.AssertNoActiveConnections();
            
            var network = this.SetupNetwork();
            try
            {
                // Create a Baz from node A.
                var bazA = (Baz)new Distributed<Baz>(network.NodeA, "baz");
                bazA.SetupTestChain("Hello!");
                
                // Retrieve the Baz from node B.
                var bazB = (Baz)new Distributed<Baz>(network.NodeB, "baz");
                Assert.NotNull(bazB);
                Assert.NotNull(bazB.MyFoo);
                Assert.NotNull(bazB.MyFoo.MyBar);
                Assert.NotNull(bazB.MyFoo.MyBar.OtherString);
                Assert.Equal("Hello!", bazB.MyFoo.MyBar.OtherString);
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }
            
            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void LeavingNetworkDoesNotCrash()
        {
            this.AssertNoActiveConnections();
            
            var network = this.SetupNetwork();
            network.NodeA.Leave();
            network.NodeB.Leave();
            
            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void LeavingNetworkClosesAllNetworkConnections()
        {
            this.AssertNoActiveConnections();
            
            var network = this.SetupNetwork();
            network.NodeA.Leave();
            network.NodeB.Leave();
            
            this.AssertNoActiveConnections();
            
            var network2 = this.SetupNetwork();
            network2.NodeA.Leave();
            network2.NodeB.Leave();
            
            this.AssertNoActiveConnections();
        }
    }
}

