// 
//  Copyright 2010  Trust4 Developers
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Data4
{
    [Serializable()]
    public class FetchMessage : DirectMessage, ISerializable
    {
        private ID p_Key = null;
        private List<Entry> p_Values = null;

        public event EventHandler ResultReceived;

        public FetchMessage(Dht dht, Contact target, ID key) : base(dht, target, "")
        {
            this.p_Key = key;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public FetchMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.p_Key = info.GetValue("fetch.key", typeof(ID)) as ID;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("fetch.key", this.p_Key, typeof(ID));
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
                this.p_Values = ( e.Message as FetchConfirmationMessage ).Values;

                // Now assign the owner of the values as the owner of the message.  We do this to
                // prevent people faking ownership by a more trusted user.
                foreach (Entry t in this.p_Values)
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
            FetchMessage fm = new FetchMessage(Dht, this.Target, this.p_Key);
            return fm;
        }

        /// <summary>
        /// The requested key.
        /// </summary>
        public ID Key
        {
            get { return this.p_Key; }
        }

        /// <summary>
        /// The returned values.  Only valid when Received is true.
        /// </summary>
        public List<Entry> Values
        {
            get { return this.p_Values; }
        }
    }
}
