using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class ConfirmationMessage : DirectMessage, ISerializable
    {
        public ConfirmationMessage(Dht dht, Message original, string data) : base(dht, original.Source, data)
        {
            this.Identifier = original.Identifier;
        }

        public ConfirmationMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}

