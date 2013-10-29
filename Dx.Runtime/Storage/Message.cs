using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable()]
    public abstract class Message : ISerializable
    {
        private Dht p_Dht = null;
        private Contact p_Source = null;
        private Contact p_Sender = null;
        private List<Contact> p_Seen = new List<Contact>();
        private string p_Data = null;
        private static object m_SendingLock = new object();

        private ID m_Identifier = null;
        private bool p_Sent = false;
        private bool p_Received = false;

        /// <summary>
        /// An event that indicates confirmation has been received for the message.
        /// </summary>
        public event EventHandler<MessageEventArgs> ConfirmationReceived;

        public Message(Dht dht, string data)
        {
            this.Dht = dht;
            this.p_Source = this.Dht.Self;
            this.p_Data = data;
        }

        public Message(SerializationInfo info, StreamingContext context)
        {
            this.m_Identifier = info.GetValue("message.id", typeof(ID)) as ID;
            this.p_Source = info.GetValue("message.source", typeof(Contact)) as Contact;
            this.p_Seen = info.GetValue("message.seen", typeof(List<Contact>)) as List<Contact>;
            this.p_Data = info.GetString("message.data");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("message.id", this.m_Identifier, typeof(ID));
            info.AddValue("message.source", this.p_Source, typeof(Contact));
            info.AddValue("message.seen", this.p_Seen, typeof(List<Contact>));
            info.AddValue("message.data", this.p_Data);
        }

        /// <summary>
        /// Sends the message to it's target through the peer-to-peer network.  Returns a duplicate of
        /// this message, but without the send-specific information such as unique identifier and event
        /// handler attached.  The duplicated message should be used when sending a message to multiple
        /// receipients.
        /// </summary>
        /// <param name="target">The target to send the message to.</param>
        /// <returns>A new message that duplicates the properties of the one being sent.</returns>
        public void Send(Contact target)
        {
            if (this.Dht == null)
                throw new InvalidOperationException("The message could not be sent because there is no DHT associated with the message.");

            if (this.p_Sent)
                throw new InvalidOperationException("Messages can not be resent with Sent().  Use the duplicated message object from Sent() to resend a message.");
            this.p_Sent = true;

            // Wait for confirmation if we need it.
            if (this.ExpectsConfirmation)
            {
                this.Dht.AwaitForConfirmation(this);
            }
            
            // TODO: Give this more randomization to ensure a higher degree of uniqueness.
            if (this.m_Identifier == null)
                this.m_Identifier = ID.NewRandom();

            // Send the message to the target.
            using (MemoryStream buffer = new MemoryStream())
            {
                using (MemoryStream writer = new MemoryStream())
                {
                    this.Dht.LogI(Dht.LogType.DEBUG, "Sending -");
                    this.Dht.LogI(Dht.LogType.DEBUG, "          Message - " + this.ToString());
                    this.Dht.LogI(Dht.LogType.DEBUG, "          Target - " + target.ToString());
                    StreamingContext old = this.Dht.Formatter.Context;
                    this.Dht.Formatter.Context = new StreamingContext(this.Dht.Formatter.Context.State, new SerializationData { Dht = this.Dht, IsMessage = true });
                    this.Dht.Formatter.Serialize(writer, this);
                    this.Dht.Formatter.Context = old;
                    if (writer.Length >= Int32.MaxValue)
                        throw new InvalidDataException("Message contents too large to send over network!");
                    buffer.Write(BitConverter.GetBytes(writer.Length), 0, sizeof(Int32));
                    buffer.Write(writer.GetBuffer(), 0, (Int32)writer.Length);
                    lock (Message.m_SendingLock)
                    {
                        TcpClient tcp = ContactPool.GetTcpClient(target.EndPoint);
                        int sent = 0;
                        int total = (int)buffer.Length;
                        while (sent < total)
                            sent += tcp.Client.Send(buffer.GetBuffer(), sent, total - sent, SocketFlags.None);
                        this.Dht.LogI(Dht.LogType.DEBUG, sent + " total bytes sent.");
                    }
                }
            }
        }

        /// <summary>
        /// Clones the current message without providing any send information in the returned message.
        /// </summary>
        protected abstract Message Clone();

        /// <summary>
        /// This event is raised when the Dht receives a message.  It is used by the
        /// Message class to detect confirmation replies.
        /// </summary>
        public virtual void OnConfirm(object sender, MessageEventArgs e)
        {
            if (!this.p_Sent)
                return;

            if (e.Message is ConfirmationMessage && e.Message.m_Identifier == this.m_Identifier)
            {
                this.p_Received = true;
                if (this.ConfirmationReceived != null)
                    this.ConfirmationReceived(this, e);
            }
        }

        /// <summary>
        /// Returns whether or not this message has already seen the specified contact.
        /// </summary>
        /// <param name="c">The contact to test.</param>
        /// <returns>Whether or not this message has already seen the specified contact.</returns>
        public bool SeenBy(Contact c)
        {
            return ( this.p_Seen.Contains(c) || this.p_Source == c );
        }

        /// <summary>
        /// The DHT associated with this message.  A DHT must be associated to use the
        /// Send() and Received() functions.
        /// </summary>
        public Dht Dht
        {
            get { return this.p_Dht; }
            set { this.p_Dht = value; }
        }
        
        /// <summary>
        /// Whether to send a basic "ConfirmationMessage" when this message type is received.  This
        /// should only be set to true when messages are sending responses by themselves (e.g.
        /// InvokeMessage sends back InvokeConfirmationMessage manually, so it would leave this
        /// property as false).
        /// </summary>
        public virtual bool SendBasicConfirmation
        {
            get { return false; }
        }
        
        /// <summary>
        /// Whether this message type expects to receive confirmation at some point in time.  When
        /// messages do not send basic confirmation, but instead send confirmation manually, this
        /// property should be overridden to return true.
        /// </summary>
        public virtual bool ExpectsConfirmation
        {
            get { return this.SendBasicConfirmation; }
        }

        /// <summary>
        /// Whether the message has been sent with Send() and hence can not be resent.
        /// </summary>
        public bool Sent
        {
            get { return this.p_Sent; }
        }

        /// <summary>
        /// Whether the message was received by the recipient specified as the argument
        /// to Send().
        /// </summary>
        public bool Received
        {
            get { return this.p_Received; }
            protected set { this.p_Received = value; }
        }

        /// <summary>
        /// The source of this message.
        /// </summary>
        public Contact Source
        {
            get { return this.p_Source; }
        }

        /// <summary>
        /// The sender of this message (i.e. the node that is was directly received from).
        /// </summary>
        public Contact Sender
        {
            get { return this.p_Sender; }
            set
            {
                if (this.p_Sender == null)
                    this.p_Sender = value;
                else
                    throw new ArgumentException("Can not change sender of Message once assigned.");
            }
        }

        /// <summary>
        /// The nodes that this message has passed through, excluding the creator and
        /// the current node.
        /// </summary>
        public ReadOnlyCollection<Contact> Seen
        {
            get { return this.p_Seen.AsReadOnly(); }
        }

        /// <summary>
        /// The data associated with this message.
        /// </summary>
        public string Data
        {
            get { return this.p_Data; }
        }

        /// <summary>
        /// The identifier used to pair up confirmation requests with the original message.
        /// </summary>
        public ID Identifier
        {
            get { return this.m_Identifier; }
            protected set { this.m_Identifier = value; }
        }

        public override string ToString()
        {
            return string.Format("[Message: Type={0}, Dht={1}, Source={2}, Seen={3}, Data={4}]", this.GetType().FullName, this.Dht, this.Source, this.Seen, this.Data);
        }
    }
}

