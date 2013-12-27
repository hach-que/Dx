namespace Dx.Runtime.Tests
{
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Dx.Runtime.Tests.Data;
    using Xunit;

    public class NetworkingTests
    {
        private struct TwoNodes
        {
            public ILocalNode NodeA;
            public ILocalNode NodeB;
        }

        private TwoNodes SetupNetwork()
        {
            var result = new TwoNodes();
            result.NodeA = new LocalNode();
            result.NodeB = new LocalNode();
            result.NodeA.Bind(IPAddress.Loopback, 12000);
            result.NodeB.Bind(IPAddress.Loopback, 12001);
            result.NodeB.GetService<IClientConnector>().Connect(IPAddress.Loopback, 12000);
            return result;
        }

        private void AssertNoActiveConnections()
        {
            var listener = new TcpListener(IPAddress.Loopback, 12000);
            Assert.DoesNotThrow(listener.Start);
            listener.Stop();

            listener = new TcpListener(IPAddress.Loopback, 12001);
            Assert.DoesNotThrow(listener.Start);
            listener.Stop();
        }
        
        [Fact]
        public void NodesKnowAboutEachOther()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                Assert.Equal(2, network.NodeA.GetService<IClientLookup>().GetAll().Count());
                Assert.Equal(2, network.NodeB.GetService<IClientLookup>().GetAll().Count());
            }
            finally
            {
                network.NodeA.Close();
                network.NodeB.Close();
            }

            this.AssertNoActiveConnections();
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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void LeavingNetworkDoesNotCrash()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            network.NodeA.Close();
            network.NodeB.Close();

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void LeavingNetworkClosesAllNetworkConnections()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            network.NodeA.Close();
            network.NodeB.Close();

            this.AssertNoActiveConnections();

            var network2 = this.SetupNetwork();
            network2.NodeA.Close();
            network2.NodeB.Close();

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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
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
                network.NodeA.Close();
                network.NodeB.Close();
            }

            this.AssertNoActiveConnections();
        }
        
        [Fact]
        public void DistributedListStoresItemsCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create list and store it on node A.
                var listA = (DList<string>)new Distributed<DList<string>>(network.NodeA, "list");
                listA.Add("hello");
                listA.Add("world");
                
                // Retrieve the list from node B.
                var listB = (DList<string>)new Distributed<DList<string>>(network.NodeB, "list");
                Assert.Equal(2, listB.Count);
                Assert.Contains("hello", listB);
                Assert.Contains("world", listB);
            }
            finally
            {
                network.NodeA.Close();
                network.NodeB.Close();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void SynchronisedFieldsBehaveCorrectly()
        {
            this.AssertNoActiveConnections();

            var network = this.SetupNetwork();
            try
            {
                // Create object and synchronise it on node A.
                var testA = new SynchronisedSimpleFieldTest();
                testA.Public = 3;
                testA.SetProtected(4);
                testA.SetPrivate(5);
                network.NodeA.Synchronise(testA, "test", true);
                Assert.Equal(3, testA.Public);
                Assert.Equal(4, testA.GetProtected());
                Assert.Equal(5, testA.GetPrivate());
                
                // Create object and synchronise it on node B.
                var testB = new SynchronisedSimpleFieldTest();
                Assert.Equal(0, testB.Public);
                Assert.Equal(0, testB.GetProtected());
                Assert.Equal(0, testB.GetPrivate());
                network.NodeB.Synchronise(testB, "test", false);
                Assert.Equal(3, testB.Public);
                Assert.Equal(4, testB.GetProtected());
                Assert.Equal(5, testB.GetPrivate());
            }
            finally
            {
                network.NodeA.Close();
                network.NodeB.Close();
            }

            this.AssertNoActiveConnections();
        }

        [Fact]
        public void NodeIsReusable()
        {
            this.AssertNoActiveConnections();

            var other = new LocalNode();
            var node = new LocalNode();

            try
            {
                other.Bind(IPAddress.Loopback, 12002);

                node.Bind(IPAddress.Loopback, 12000);
                node.GetService<IClientConnector>().Connect(IPAddress.Loopback, 12002);
                Assert.Equal(2, node.GetService<IClientLookup>().GetAll().Count());
                node.Close();
                node.Bind(IPAddress.Loopback, 12001);
                Assert.Equal(1, node.GetService<IClientLookup>().GetAll().Count());
            }
            finally
            {
                other.Close();
                node.Close();
            }

            this.AssertNoActiveConnections();
        }
    }
}

