using System.Linq;
using Dx.Runtime.Tests.Data;
using Xunit;

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

        [Fact]
        public void GenericTypeBehavesCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create on node A.
                new Distributed<GenericType<string, int, short>>(network.NodeA, "test");

                // Retrieve and use on node B.
                var testB = (GenericType<string, int, short>)
                    new Distributed<GenericType<string, int, short>>(network.NodeB, "test");
                testB.Test();
                Assert.Equal(testB.Return("hello"), "hello");
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void GenericTypeAndMethodBehavesCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create on node A.
                new Distributed<GenericTypeAndMethod<string, int>>(network.NodeA, "test");

                // Retrieve and use on node B.
                var testB = (GenericTypeAndMethod<string, int>)
                    new Distributed<GenericTypeAndMethod<string, int>>(network.NodeB, "test");
                Assert.Equal(testB.Return<short, uint, ushort>("hello", 1, 2, 3, 4), "hello");
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void GenericMethodBehavesCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create on node A.
                new Distributed<GenericMethod>(network.NodeA, "test");

                // Retrieve and use on node B.
                var testB = (GenericMethod)new Distributed<GenericMethod>(network.NodeB, "test");
                Assert.Equal(testB.Return("hello", 5), "hello");
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }
    }
}

