namespace Dx.Runtime
{
    class InvokeResultMessageHandler : IMessageHandler
    {
        private readonly IMessageSideChannel m_MessageSideChannel;

        public InvokeResultMessageHandler(IMessageSideChannel messageSideChannel)
        {
            this.m_MessageSideChannel = messageSideChannel;
        }

        public int GetMessageType()
        {
            return MessageType.InvokeResult;
        }

        public void Handle(Message message)
        {
            this.m_MessageSideChannel.Put(message);
        }
    }
}
