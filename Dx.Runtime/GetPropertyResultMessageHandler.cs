namespace Dx.Runtime
{
    class GetPropertyResultMessageHandler : IMessageHandler
    {
        private readonly IMessageSideChannel m_MessageSideChannel;

        private readonly IObjectStorage m_ObjectLookup;

        public GetPropertyResultMessageHandler(IMessageSideChannel messageSideChannel, IObjectStorage objectLookup)
        {
            this.m_MessageSideChannel = messageSideChannel;
            this.m_ObjectLookup = objectLookup;
        }

        public int GetMessageType()
        {
            return MessageType.GetPropertyResult;
        }

        public void Handle(Message message)
        {
            this.m_MessageSideChannel.Put(message);
        }
    }
}
