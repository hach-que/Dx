using System;

namespace Dx.Runtime
{
    public class MessageEventArgs : EventArgs
    {
        private Message p_Message;

        public MessageEventArgs(Message message)
        {
            this.p_Message = message;
        }

        public Message Message
        {
            get { return this.p_Message; }
        }
    }
}

