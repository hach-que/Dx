using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class GetPropertyConfirmationMessage : ConfirmationMessage, ISerializable
    {
        private object p_Result = null;

        public GetPropertyConfirmationMessage(Dht dht, Message original, object result) : base(dht, original, "")
        {
            this.p_Result = result;
        }

        public GetPropertyConfirmationMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_Result = info.GetValue("getproperty.result", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("getproperty.result", this.p_Result, typeof(object));
        }

        /// <summary>
        /// The result of the property getter.  Only valid when Received is true.
        /// </summary>
        public object Result
        {
            get { return this.p_Result; }
        }
    }
}
