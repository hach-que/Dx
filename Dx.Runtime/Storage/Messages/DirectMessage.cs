using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class DirectMessage : Message, ISerializable
    {
        public DirectMessage(Dht dht, Contact target, string data) : base(dht, data)
        {
            this.Target = target;
        }

        public DirectMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        
        public Contact Target { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Sends the direct message to it's recipient.
        /// </summary>
        public DirectMessage Send()
        {
            return base.Send(this.Target) as DirectMessage;
        }

        /// <summary>
        /// Clones the direct message.
        /// </summary>
        protected override Message Clone()
        {
            DirectMessage dm = new DirectMessage(Dht, this.Target, this.Data);
            return dm;
        }
    }
}

