using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable()]
    internal class InvokeEventMessage : DirectMessage, ISerializable
    {
        private EventTransport p_EventTransport = null;
        private object p_Sender = null;
        private EventArgs p_EventArgs = null;

        public InvokeEventMessage(Dht dht, Contact target, EventTransport transport, object sender, EventArgs e)
            : base(dht, target, null)
        {
            this.p_EventTransport = transport;
            this.p_Sender = sender;
            this.p_EventArgs = e;
        }

        public InvokeEventMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_EventTransport = info.GetValue("evinvoke.transport", typeof(EventTransport)) as EventTransport;
            this.p_Sender = info.GetValue("evinvoke.sender", typeof(object)) as object;
            this.p_EventArgs = info.GetValue("evinvoke.args", typeof(EventArgs)) as EventArgs;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("evinvoke.transport", this.p_EventTransport, typeof(EventTransport));
            if (this.p_Sender.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Count() > 0)
                info.AddValue("evinvoke.sender", this.p_Sender, typeof(object));
            else
                info.AddValue("evinvoke.sender", null, typeof(object));
            info.AddValue("evinvoke.args", this.p_EventArgs, typeof(EventArgs));
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
            InvokeEventMessage fm = new InvokeEventMessage(Dht, this.Target, this.EventTransport, this.EventSender, this.EventArgs);
            return fm;
        }

        /// <summary>
        /// The event transport data.
        /// </summary>
        public EventTransport EventTransport
        {
            get { return this.p_EventTransport; }
        }

        /// <summary>
        /// The object that sent the event, if the sending object supported
        /// serialization (otherwise null).
        /// </summary>
        public object EventSender
        {
            get { return this.p_Sender; }
        }

        /// <summary>
        /// The event arguments.
        /// </summary>
        public EventArgs EventArgs
        {
            get { return this.p_EventArgs; }
        }
    }
}
