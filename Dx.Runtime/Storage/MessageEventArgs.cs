using System;

namespace Dx.Runtime
{
    public class MessageEventArgs : EventArgs
    {
        private Message p_Message;
        private bool p_SendConfirmation = true;

        public MessageEventArgs(Message message)
        {
            this.p_Message = message;
        }

        public Message Message
        {
            get { return this.p_Message; }
        }

        public bool SendConfirmation
        {
            get { return this.p_SendConfirmation; }
            set { this.p_SendConfirmation = value; }
        }
    }
}

