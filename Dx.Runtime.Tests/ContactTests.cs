using System;
using System.Net;
using Xunit;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dx.Runtime.Tests
{
    public class ContactTests
    {
        [Fact]
        public void TestEquality()
        {
            var id = ID.NewRandom();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8000);
            var a = new Contact(id, endpoint);
            var b = new Contact(id, endpoint);
            
            Assert.Equal(a, b);
        }
        
        [Fact]
        public void TestInequality()
        {
            var id = ID.NewRandom();
            var endpoint1 = new IPEndPoint(IPAddress.Loopback, 8000);
            var endpoint2 = new IPEndPoint(IPAddress.Loopback, 8080);
            var a = new Contact(id, endpoint1);
            var b = new Contact(id, endpoint2);
            
            Assert.NotEqual(a, b);
        }
        
        [Fact]
        public void TestSerialization()
        {
            var id = ID.NewRandom();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8000);
            var contact = new Contact(id, endpoint);
            
            var memory = new MemoryStream();
            
            var formatter = new BinaryFormatter();
            formatter.Serialize(memory, contact);
            memory.Seek(0, SeekOrigin.Begin);
            var result = formatter.Deserialize(memory);
            
            Assert.IsType<Contact>(result);
            
            var resultContact = (Contact)result;
            
            Assert.Equal(contact.EndPoint, resultContact.EndPoint);
            Assert.Equal(contact.Identifier, resultContact.Identifier);
        }
    }
}

