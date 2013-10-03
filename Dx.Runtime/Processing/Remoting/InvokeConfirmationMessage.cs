using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;
using System.Runtime.Serialization;

namespace Process4.Remoting
{
    [Serializable()]
    public class InvokeConfirmationMessage : ConfirmationMessage, ISerializable
    {
        private object p_Result = null;

        public InvokeConfirmationMessage(Dht dht, Message original, object result) : base(dht, original, "")
        {
            this.p_Result = result;
        }

        public InvokeConfirmationMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_Result = info.GetValue("invoke.result", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("invoke.result", this.p_Result, typeof(object));
        }

        /// <summary>
        /// The result of the function.  Only valid when Received is true.
        /// </summary>
        public object Result
        {
            get { return this.p_Result; }
        }
    }
}
