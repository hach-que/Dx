namespace Dx.Runtime.Tests
{
    using System.Net;
    using Xunit;

    public class ContactTests
    {
        [Fact]
        public void TestEquality()
        {
            var a = new Contact { IPAddress = IPAddress.Loopback, Port = 8000 };
            var b = new Contact { IPAddress = IPAddress.Loopback, Port = 8000 };
            
            Assert.Equal(a, b);
        }
        
        [Fact]
        public void TestInequality()
        {
            var a = new Contact { IPAddress = IPAddress.Loopback, Port = 8000 };
            var b = new Contact { IPAddress = IPAddress.Loopback, Port = 8080 };
            
            Assert.NotEqual(a, b);
        }
    }
}

