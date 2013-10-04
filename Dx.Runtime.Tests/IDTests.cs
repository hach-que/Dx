using System;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class IDTests
    {
        [Fact]
        public void TestEquality()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var c = Guid.NewGuid();
            var d = Guid.NewGuid();
            
            var id1 = new ID(a, b, c, d);
            var id2 = new ID(a, b, c, d);
            
            Assert.Equal(id1, id2);
        }
        
        [Fact]
        public void TestInequality()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var c = Guid.NewGuid();
            var d = Guid.NewGuid();
            
            var id1 = new ID(a, b, c, d);
            var id2 = new ID(a, b, c, c);
            
            Assert.NotEqual(id1, id2);
        }
    }
}

