using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class SetPropertyMessage : DirectMessage, ISerializable
    {
        private string p_ObjectID = null;
        private string p_ObjectProperty = null;
        private object p_NewValue = null;

        public SetPropertyMessage(Dht dht, Contact target, string id, string property, object value) : base(dht, target, null)
        {
            this.p_ObjectID = id;
            this.p_ObjectProperty = property;
            this.p_NewValue = value;
        }

        public SetPropertyMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_ObjectID = info.GetValue("setproperty.objid", typeof(string)) as string;
            this.p_ObjectProperty = info.GetValue("setproperty.objproperty", typeof(string)) as string;
            this.p_NewValue = info.GetValue("setproperty.value", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("setproperty.objid", this.p_ObjectID, typeof(string));
            info.AddValue("setproperty.objproperty", this.p_ObjectProperty, typeof(string));
            info.AddValue("setproperty.value", this.p_NewValue, typeof(object));
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
            SetPropertyMessage fm = new SetPropertyMessage(Dht, this.Target, this.p_ObjectID, this.p_ObjectProperty, this.p_NewValue);
            return fm;
        }

        /// <summary>
        /// The requested object ID to invoke the method on.
        /// </summary>
        public string ObjectID
        {
            get { return this.p_ObjectID; }
        }

        /// <summary>
        /// The requested property to set.
        /// </summary>
        public string ObjectProperty
        {
            get { return this.p_ObjectProperty; }
        }

        /// <summary>
        /// The requested value for the property.
        /// </summary>
        public object NewValue
        {
            get { return this.p_NewValue; }
        }
    }
}
