using System.Linq;

namespace Dx.Runtime
{
    public class FetchMessageHandler : IMessageHandler
    {
        private readonly IObjectStorage m_Lookup;

        private readonly IMessageConstructor m_MessageConstructor;

        private readonly IClientLookup m_ClientLookup;

        private readonly IObjectWithTypeSerializer m_ObjectWithTypeSerializer;

        public FetchMessageHandler(
            IObjectStorage lookup,
            IMessageConstructor messageConstructor,
            IClientLookup clientLookup,
            IObjectWithTypeSerializer objectWithTypeSerializer)
        {
            this.m_Lookup = lookup;
            this.m_MessageConstructor = messageConstructor;
            this.m_ClientLookup = clientLookup;
            this.m_ObjectWithTypeSerializer = objectWithTypeSerializer;
        }

        public int GetMessageType()
        {
            return MessageType.Fetch;
        }

        public void Handle(Message message)
        {
            var client = this.m_ClientLookup.Lookup(message.Sender.IPEndPoint);

            var results = this.m_Lookup.Find(message.FetchKey).ToArray();

            var serializedResults =
                results.Select(
                    x =>
                    new SerializedEntry
                    {
                        Key = x.Key,
                        Owner = x.Owner,
                        Value = this.m_ObjectWithTypeSerializer.Serialize(x.Value)
                    }).ToArray();

            var confirmationMessage = this.m_MessageConstructor.ConstructFetchResultMessage(
                message.FetchKey,
                serializedResults);

            client.Send(confirmationMessage);
        }
    }
}
