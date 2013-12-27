namespace Dx.Runtime
{
    class SetPropertyConfirmationMessageHandler : IMessageHandler
    {
        private readonly IMessageSideChannel m_MessageSideChannel;

        public SetPropertyConfirmationMessageHandler(
            IMessageSideChannel messageSideChannel)
        {
            this.m_MessageSideChannel = messageSideChannel;
        }

        public int GetMessageType()
        {
            return MessageType.SetPropertyConfirmation;
        }

        public void Handle(Message message)
        {
            this.m_MessageSideChannel.Put(message);
        }
    }
}
