namespace Dx.Runtime
{
    class FetchResultMessageHandler : IMessageHandler
    {
        private readonly IMessageSideChannel m_MessageSideChannel;

        private readonly IObjectStorage m_ObjectLookup;

        private readonly IObjectWithTypeSerializer m_ObjectWithTypeSerializer;

        public FetchResultMessageHandler(
            IMessageSideChannel messageSideChannel,
            IObjectStorage objectLookup,
            IObjectWithTypeSerializer objectWithTypeSerializer)
        {
            this.m_MessageSideChannel = messageSideChannel;
            this.m_ObjectLookup = objectLookup;
            this.m_ObjectWithTypeSerializer = objectWithTypeSerializer;
        }

        public int GetMessageType()
        {
            return MessageType.FetchResult;
        }

        public void Handle(Message message)
        {
            // Empty arrays deserialize to a null value.
            if (message.FetchResult == null)
            {
                message.FetchResult = new SerializedEntry[0];
            }

            foreach (var entry in message.FetchResult)
            {
                this.m_ObjectLookup.Put(new LiveEntry
                {
                    Key = entry.Key,
                    Owner = entry.Owner,
                    Value = this.m_ObjectWithTypeSerializer.Deserialize(entry.Value)
                });
            }

            this.m_MessageSideChannel.Put(message);
        }
    }
}
