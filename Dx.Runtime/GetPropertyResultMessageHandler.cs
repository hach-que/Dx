﻿namespace Dx.Runtime
{
    class GetPropertyResultMessageHandler : IMessageHandler
    {
        private readonly IMessageSideChannel m_MessageSideChannel;

        public GetPropertyResultMessageHandler(IMessageSideChannel messageSideChannel)
        {
            this.m_MessageSideChannel = messageSideChannel;
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
