using System;
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
            var factory = new DefaultDxFactory();
            var node = factory.CreateLocalNode();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Distributed<InterceptNewInstructionTest>(node, "hello");
            });
        }
        
        [Fact]
        public void DoesNotThrowExceptionWhenDirectlyConstructingInsideDistributedContext()
        {
            var factory = new DefaultDxFactory();
            var node = factory.CreateLocalNode();
            node.Join(ID.NewRandom());
            
            Assert.DoesNotThrow(() =>
            {
                new Distributed<InterceptNewInstructionTest>(node, "hello");
            });
        }
        
        [Fact]
        public void DoesNotThrowExceptionWhenIndirectlyConstructingInsideDistributedContext()
        {
            var factory = new DefaultDxFactory();
            var node = factory.CreateLocalNode();
            node.Join(ID.NewRandom());
            
            Assert.DoesNotThrow(() =>
            {
                var foo = (Foo)new Distributed<Foo>(node, "foo");
                var bar = foo.ConstructBar();
            });
        }
        
        [Fact]
        public void BarReturnsCorrectString()
        {
            var factory = new DefaultDxFactory();
            var node = factory.CreateLocalNode();
            node.Join(ID.NewRandom());
            
            var foo = (Foo)new Distributed<Foo>(node, "foo");
            var bar = foo.ConstructBar();
            Assert.Equal("Hello, World!", bar.GetHelloWorldString());
        }
    }
}

