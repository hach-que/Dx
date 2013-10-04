using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class RemoveEventMessage : DirectMessage, ISerializable
    {
        private EventTransport p_EventTransport = null;

        public RemoveEventMessage(Dht dht, Contact target, EventTransport transport)
            : base(dht, target, null)
        {
            this.p_EventTransport = transport;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public RemoveEventMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_EventTransport = info.GetValue("evinvoke.transport", typeof(EventTransport)) as EventTransport;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("evinvoke.transport", this.p_EventTransport, typeof(EventTransport));
        }

        /// <summary>
        /// Sends the remove event message to it's recipient.
        /// </summary>
        public new RemoveEventMessage Send()
        {
            return base.Send(this.Target) as RemoveEventMessage;
        }

        /// <summary>
        /// This event is raised when the DHT has received a message and we need to
        /// parse it to see if it's relevant (i.e. confirmation).
        /// </summary>
        private void OnConfirm(object sender, MessageEventArgs e)
        {
            if (!this.Sent)
                return;

            e.SendConfirmation = true;

            // The DHT will send our confirmation message for us as we do not
            // need to return any additional information.
        }

        /// <summary>
        /// Clones the fetch message.
        /// </summary>
        protected override Message Clone()
        {
            RemoveEventMessage fm = new RemoveEventMessage(Dht, this.Target, this.EventTransport);
            return fm;
        }

        /// <summary>
        /// The event transport information.
        /// </summary>
        public EventTransport EventTransport
        {
            get { return this.p_EventTransport; }
        }
    }
}
