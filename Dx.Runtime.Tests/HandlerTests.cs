namespace Dx.Runtime.Tests
{
    using System.Linq;
    using System.Net;
    using Xunit;

    public class HandlerTests
    {
        [Fact]
        public void TestFetchHandler()
        {
            var node = new LocalNode();
            node.Bind(IPAddress.Loopback, 10001);

            try
            {
                var constructor = node.GetService<IMessageConstructor>();
                var serializer = node.GetService<IObjectWithTypeSerializer>();

                var storage = node.GetService<IObjectStorage>();
                storage.Put(new LiveEntry
                {
                    Key = ID.NewHash("hello"),
                    Owner = node.Self,
                    Value = 40
                });

                var fetchMessage = constructor.ConstructFetchMessage(ID.NewHash("hello"));
                fetchMessage.Sender = node.Self;

                var handler = node.GetService<FetchMessageHandler>();
                handler.Handle(fetchMessage);

                var sideChannel = node.GetService<IMessageSideChannel>();
                Assert.True(
                    sideChannel.Has(x => x.Type == MessageType.FetchResult),
                    "side channel does not report message");

                var result = sideChannel.WaitUntil(x => x.Type == MessageType.FetchResult, 100);

                Assert.Equal(1, result.FetchResult.Length);

                var value = serializer.Deserialize(result.FetchResult.First().Value);

                Assert.IsType<int>(value);
                Assert.Equal(40, (int)value);
            }
            finally
            {
                node.Close();
            }
        }
    }
}
