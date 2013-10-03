using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;
using System.Runtime.Serialization;

namespace Process4.Remoting
{
    [Serializable()]
    public class GetPropertyMessage : DirectMessage, ISerializable
    {
        private string p_ObjectID = null;
        private string p_ObjectProperty = null;
        private object p_Result = null;

        public event EventHandler ResultReceived;

        public GetPropertyMessage(Dht dht, Contact target, string id, string property) : base(dht, target, null)
        {
            this.p_ObjectID = id;
            this.p_ObjectProperty = property;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public GetPropertyMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_ObjectID = info.GetValue("getproperty.objid", typeof(string)) as string;
            this.p_ObjectProperty = info.GetValue("getproperty.objproperty", typeof(string)) as string;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("getproperty.objid", this.p_ObjectID, typeof(string));
            info.AddValue("getproperty.objproperty", this.p_ObjectProperty, typeof(string));
        }

        /// <summary>
        /// Sends the invoke message to it's recipient.
        /// </summary>
        public new GetPropertyMessage Send()
        {
            return base.Send(this.Target) as GetPropertyMessage;
        }

        /// <summary>
        /// This event is raised when the DHT has received a message and we need to
        /// parse it to see if it's relevant (i.e. confirmation).
        /// </summary>
        private void OnConfirm(object sender, MessageEventArgs e)
        {
            if (!this.Sent)
                return;

            e.SendConfirmation = false;

            if (e.Message is GetPropertyConfirmationMessage && e.Message.Identifier == this.Identifier)
            {
                this.Received = true;
                this.p_Result = (e.Message as GetPropertyConfirmationMessage).Result;
                if (this.ResultReceived != null)
                    this.ResultReceived(this, new EventArgs());
            }
        }

        /// <summary>
        /// Clones the fetch message.
        /// </summary>
        protected override Message Clone()
        {
            GetPropertyMessage fm = new GetPropertyMessage(Dht, this.Target, this.p_ObjectID, this.p_ObjectProperty);
            return fm;
        }

        /// <summary>
        /// The requested object ID to retrieve the property on.
        /// </summary>
        public string ObjectID
        {
            get { return this.p_ObjectID; }
        }

        /// <summary>
        /// The requested property to get.
        /// </summary>
        public string ObjectProperty
        {
            get { return this.p_ObjectProperty; }
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
