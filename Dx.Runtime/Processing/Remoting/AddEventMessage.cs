using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class AddEventMessage : DirectMessage, ISerializable
    {
        private EventTransport p_EventTransport = null;

        public AddEventMessage(Dht dht, Contact target, EventTransport transport)
            : base(dht, target, null)
        {
            this.p_EventTransport = transport;
        }

        public AddEventMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_EventTransport = info.GetValue("evinvoke.transport", typeof(EventTransport)) as EventTransport;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("evinvoke.transport", this.p_EventTransport, typeof(EventTransport));
        }
        
        public override bool SendBasicConfirmation
        {
            get { return true; }
        }

        /// <summary>
        /// Clones the fetch message.
        /// </summary>
        protected override Message Clone()
        {
            AddEventMessage fm = new AddEventMessage(Dht, this.Target, this.EventTransport);
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
