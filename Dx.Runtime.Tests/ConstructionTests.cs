using System;
using System.Net;
using Dx.Runtime.Tests.Data;
using Xunit;

#pragma warning disable 0219

namespace Dx.Runtime.Tests
{
    public class ConstructionTests
    {
        [Fact]
        public void ThrowsExceptionWhenConstructingOutsideDistributedContext()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new InterceptNewInstructionTest();
            });
        }
        
        [Fact]
        public void ThrowsExceptionWhenNetworkIsNotJoined()
        {
            var node = new LocalNode();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Distributed<InterceptNewInstructionTest>(node, "hello");
            });
        }
        
        [Fact]
        public void DoesNotThrowExceptionWhenDirectlyConstructingInsideDistributedContext()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 11001);
            
            Assert.DoesNotThrow(() =>
            {
                new Distributed<InterceptNewInstructionTest>(node, "hello");
            });
        }
        
        [Fact]
        public void DoesNotThrowExceptionWhenIndirectlyConstructingInsideDistributedContext()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 11002);
            
            Assert.DoesNotThrow(() =>
            {
                var foo = (Foo)new Distributed<Foo>(node, "foo");
                var bar = foo.ConstructBar();
            });
        }
        
        [Fact]
        public void BarReturnsCorrectString()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 11003);
            
            var foo = (Foo)new Distributed<Foo>(node, "foo");
            var bar = foo.ConstructBar();
            Assert.Equal("Hello, World!", bar.GetHelloWorldString());
        }
    }
}

