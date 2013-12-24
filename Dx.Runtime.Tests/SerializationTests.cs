using Xunit;
using System.IO;
using ProtoBuf;

namespace Dx.Runtime.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void TestEntrySerialization()
        {
            var message = new Message();
            message.FetchResult = new SerializedEntry[]
            {
                new SerializedEntry { Key = ID.NewHash("test") },
            };

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                stream.Seek(0, SeekOrigin.Begin);

                var results = Serializer.Deserialize<Message>(stream);
                Assert.NotNull(results.FetchResult);
                Assert.Equal(1, results.FetchResult.Length);
                Assert.Equal(ID.NewHash("test"), results.FetchResult[0].Key);
            }
        }
    }
}

