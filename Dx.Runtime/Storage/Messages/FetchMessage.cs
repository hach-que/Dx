using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class FetchMessage : DirectMessage, ISerializable
    {
        private ID m_Key;
        private List<Entry> m_Values;

        public event EventHandler ResultReceived;

        public FetchMessage(Dht dht, Contact target, ID key) : base(dht, target, "")
        {
            this.m_Key = key;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public FetchMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_Key = info.GetValue("fetch.key", typeof(ID)) as ID;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("fetch.key", this.m_Key, typeof(ID));
        }

        /// <summary>
        /// Sends the fetch message to it's recipient.
        /// </summary>
        public new FetchMessage Send()
        {
            return base.Send(this.Target) as FetchMessage;
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

            if (e.Message is FetchConfirmationMessage && e.Message.Identifier == this.Identifier)
            {
                this.m_Values = ( e.Message as FetchConfirmationMessage ).Values;

                // Now assign the owner of the values as the owner of the message.  We do this to
                // prevent people faking ownership by a more trusted user.
                foreach (Entry t in this.m_Values)
                    t.Owner = e.Message.Sender;

                this.Received = true;
                if (this.ResultReceived != null)
                    this.ResultReceived(this, new EventArgs());
            }
        }

        /// <summary>
        /// Clones the fetch message.
        /// </summary>
        protected override Message Clone()
        {
            FetchMessage fm = new FetchMessage(Dht, this.Target, this.m_Key);
            return fm;
        }

        /// <summary>
        /// The requested key.
        /// </summary>
        public ID Key
        {
            get { return this.m_Key; }
        }

        /// <summary>
        /// The returned values.  Only valid when Received is true.
        /// </summary>
        public List<Entry> Values
        {
            get { return this.m_Values; }
        }
    }
}
