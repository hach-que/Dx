using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;
using System.Runtime.Serialization;

namespace Process4.Remoting
{
    [Serializable()]
    public class InvokeMessage : DirectMessage, ISerializable
    {
        private string p_ObjectID = null;
        private string p_ObjectMethod = null;
        private object[] p_Arguments = null;
        private Type[] p_TypeArguments = null;
        private object p_Result = null;
        private bool p_Asynchronous = false;

        public event EventHandler ResultReceived;

        public InvokeMessage(Dht dht, Contact target, string id, string method, Type[] targs, object[] args)
            : this(dht, target, id, method, targs, args, false)
        {
            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public InvokeMessage(Dht dht, Contact target, string id, string method, Type[] targs, object[] args, bool async)
            : base(dht, target, null)
        {
            this.p_ObjectID = id;
            this.p_ObjectMethod = method;
            this.p_Arguments = args;
            this.p_TypeArguments = targs;
            this.p_Asynchronous = async;

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public InvokeMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_ObjectID = info.GetValue("invoke.objid", typeof(string)) as string;
            this.p_ObjectMethod = info.GetValue("invoke.objmethod", typeof(string)) as string;
            this.p_Arguments = info.GetValue("invoke.arguments", typeof(object[])) as object[];
            this.p_TypeArguments = (info.GetValue("invoke.typearguments", typeof(string[])) as string[]).Select(Type.GetType).ToArray();
            this.p_Asynchronous = (bool)info.GetValue("invoke.asynchronous", typeof(bool));

            this.ConfirmationReceived += new EventHandler<MessageEventArgs>(this.OnConfirm);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("invoke.objid", this.p_ObjectID, typeof(string));
            info.AddValue("invoke.objmethod", this.p_ObjectMethod, typeof(string));
            info.AddValue("invoke.arguments", this.p_Arguments, typeof(object[]));
            info.AddValue("invoke.typearguments", this.p_TypeArguments.Select(x => x.AssemblyQualifiedName).ToArray(), typeof(string[]));
            info.AddValue("invoke.asynchronous", this.p_Asynchronous, typeof(bool));
        }

        /// <summary>
        /// Sends the invoke message to it's recipient.
        /// </summary>
        public new InvokeMessage Send()
        {
            return base.Send(this.Target) as InvokeMessage;
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

            if (e.Message is InvokeConfirmationMessage && e.Message.Identifier == this.Identifier)
            {
                this.Received = true;
                this.p_Result = ( e.Message as InvokeConfirmationMessage ).Result;
                if (this.ResultReceived != null)
                    this.ResultReceived(this, new EventArgs());
            }
        }

        /// <summary>
        /// Clones the fetch message.
        /// </summary>
        protected override Message Clone()
        {
            InvokeMessage fm = new InvokeMessage(Dht, this.Target, this.ObjectID, this.ObjectMethod, this.p_TypeArguments, this.p_Arguments, this.p_Asynchronous);
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
        /// The requested object fields / properties to navigate to access the method.
        /// </summary>
        public string ObjectMethod
        {
            get { return this.p_ObjectMethod; }
        }

        /// <summary>
        /// The requested arguments for the delegate.
        /// </summary>
        public object[] Arguments
        {
            get { return this.p_Arguments; }
        }

        /// <summary>
        /// The requested type arguments for the delegate.
        /// </summary>
        public Type[] TypeArguments
        {
            get { return this.p_TypeArguments; }
        }

        /// <summary>
        /// The result of the function.  Only valid when Received is true.
        /// </summary>
        public object Result
        {
            get { return this.p_Result; }
        }

        /// <summary>
        /// Whether this method should be invoked asynchronously.
        /// </summary>
        public bool Asynchronous
        {
            get { return this.p_Asynchronous; }
        }
    }
}
