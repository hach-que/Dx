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
                var testA = (GenericType<string, int, short>)
                    new Distributed<GenericType<string, int, short>>(network.NodeA, "test");
                testA.Test();
                Assert.Equal(testA.Return("hello"), "hello");
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
                var testA = (GenericTypeAndMethod<string, int>)
                    new Distributed<GenericTypeAndMethod<string, int>>(network.NodeA, "test");
                Assert.Equal(testA.Return<short, uint, ushort>("hello", 1, 2, 3, 4), "hello");
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
                var testA = (GenericMethod)new Distributed<GenericMethod>(network.NodeA, "test");
                Assert.Equal(testA.Return("hello", 5), "hello");
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void SynchronisedClassBehavesCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create object and synchronise it on node A.
                var testA = new SynchronisedTest();
                testA.X = 3;
                testA.Y = 4;
                testA.Z = 5;
                network.NodeA.Synchronise(testA, "test", true);
                Assert.Equal(3, testA.X);
                Assert.Equal(4, testA.Y);
                Assert.Equal(5, testA.Z);
                
                // Create object and synchronise it on node B.
                var testB = new SynchronisedTest();
                Assert.Equal(0, testB.X);
                Assert.Equal(0, testB.Y);
                Assert.Equal(0, testB.Z);
                network.NodeB.Synchronise(testB, "test", false);
                Assert.Equal(3, testB.X);
                Assert.Equal(4, testB.Y);
                Assert.Equal(5, testB.Z);
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void SynchronisedClassDoesNotUpdateUntilSynchronised()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create object and synchronise it on node A.
                var testA = new SynchronisedTest();
                testA.X = 3;
                testA.Y = 4;
                testA.Z = 5;
                network.NodeA.Synchronise(testA, "test", true);
                Assert.Equal(3, testA.X);
                Assert.Equal(4, testA.Y);
                Assert.Equal(5, testA.Z);
                
                // Create object and synchronise it on node B.
                var testB = new SynchronisedTest();
                Assert.Equal(0, testB.X);
                Assert.Equal(0, testB.Y);
                Assert.Equal(0, testB.Z);
                network.NodeB.Synchronise(testB, "test", false);
                
                // Now update A, we should not see the changes reflected in B yet.
                testA.X = 6;
                testA.Y = 7;
                testA.Z = 8;
                Assert.Equal(3, testB.X);
                Assert.Equal(4, testB.Y);
                Assert.Equal(5, testB.Z);
                
                // Now synchronise A, we should still not see the changes reflected in B yet.
                network.NodeA.Synchronise(testA, "test", true);
                Assert.Equal(3, testB.X);
                Assert.Equal(4, testB.Y);
                Assert.Equal(5, testB.Z);
                
                // Now synchronise B and we should see the changes.
                network.NodeB.Synchronise(testB, "test", false);
                Assert.Equal(6, testB.X);
                Assert.Equal(7, testB.Y);
                Assert.Equal(8, testB.Z);
            }
            finally
            {
                network.NodeA.Leave();
                network.NodeB.Leave();
            }

            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void SynchronisedClassCanChangeAuthority()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create object and synchronise it on node A.
                var testA = new SynchronisedTest();
                testA.X = 3;
                testA.Y = 4;
                testA.Z = 5;
                network.NodeA.Synchronise(testA, "test", true);
                Assert.Equal(3, testA.X);
                Assert.Equal(4, testA.Y);
                Assert.Equal(5, testA.Z);
                
                // Create object and synchronise it on node B.
                var testB = new SynchronisedTest();
                Assert.Equal(0, testB.X);
                Assert.Equal(0, testB.Y);
                Assert.Equal(0, testB.Z);
                network.NodeB.Synchronise(testB, "test", false);
                Assert.Equal(3, testB.X);
                Assert.Equal(4, testB.Y);
                Assert.Equal(5, testB.Z);
                
                // Now set the values on B.
                testB.X = 6;
                testB.Y = 7;
                testB.Z = 8;
                Assert.Equal(3, testA.X);
                Assert.Equal(4, testA.Y);
                Assert.Equal(5, testA.Z);
                
                // Synchronise B authoritively.
                network.NodeB.Synchronise(testB, "test", true);
                Assert.Equal(3, testA.X);
                Assert.Equal(4, testA.Y);
                Assert.Equal(5, testA.Z);
                
                // Synchronise A non-authoritively.
                network.NodeA.Synchronise(testA, "test", false);
                Assert.Equal(6, testA.X);
                Assert.Equal(7, testA.Y);
                Assert.Equal(8, testA.Z);
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

