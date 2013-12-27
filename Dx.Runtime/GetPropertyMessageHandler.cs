using System;
using System.Linq;

namespace Dx.Runtime
{
    public class GetPropertyMessageHandler : IMessageHandler
    {
        private readonly IMessageConstructor m_MessageConstructor;

        private readonly IClientLookup m_ClientLookup;

        private readonly IObjectStorage m_ObjectStorage;

        public GetPropertyMessageHandler(
            IMessageConstructor messageConstructor,
            IClientLookup clientLookup,
            IObjectStorage objectStorage)
        {
            this.m_MessageConstructor = messageConstructor;
            this.m_ClientLookup = clientLookup;
            this.m_ObjectStorage = objectStorage;
        }

        public int GetMessageType()
        {
            return MessageType.GetProperty;
        }

        public void Handle(Message message)
        {
            if (message.GetPropertyObjectID == null)
            {
                throw new ArgumentNullException("GetPropertyObjectID is null");
            }

            var entry = this.m_ObjectStorage.Find(message.GetPropertyObjectID).FirstOrDefault();

            if (entry == null)
            {
                throw new InvalidOperationException("Get property message received but we don't own the object!");
            }

            var obj = entry.Value;

            var mi = obj.GetType().GetMethod("get_" + message.GetPropertyPropertyName + "__Distributed0", BindingFlagsCombined.All);
            if (mi == null)
            {
                throw new MissingMethodException(
                    obj.GetType().FullName,
                    "get_" + message.GetPropertyPropertyName + "__Distributed0");
            }

            var value = DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { });

            var client = this.m_ClientLookup.Lookup(message.Sender.IPEndPoint);
            client.Send(this.m_MessageConstructor.ConstructGetPropertyResultMessage(message.ID, value));
        }
    }
}
