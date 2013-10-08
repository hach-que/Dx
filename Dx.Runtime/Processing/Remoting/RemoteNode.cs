using System;
using System.Threading;
using System.Collections;

namespace Dx.Runtime
{
    internal class RemoteNode : INode
    {
        private Contact m_Target;
        private ILocalNode m_LocalNode;
        private Dht m_LocalDht;
        
        public ID ID { get; set; }

        /// <summary>
        /// Creates a new remote node which references the specified contact.
        /// </summary>
        /// <param name="target">The target contact.</param>
        internal RemoteNode(ILocalNode localNode, Contact target)
        {
            if (target == null)
                throw new ArgumentException("The remote node's target can not be null.", "target");
            this.m_Target = target;
            this.m_LocalDht = (localNode.Storage as DhtWrapper).Dht;
            this.m_LocalNode = localNode;
        }

        public void SetProperty(string id, string property, object value)
        {
            bool received = false;

            DateTime start = DateTime.Now;

            // Create the message.
            SetPropertyMessage spm = new SetPropertyMessage(this.m_LocalDht, this.m_Target, id, property, value);

            // Register the event handler.
            EventHandler<MessageEventArgs> ev = null;
            ev = (sender, e) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                spm.ConfirmationReceived -= ev;
            };
            spm.ConfirmationReceived += ev;

            // Send the message.
            spm.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            // We have nothing to return (but it was still important to wait
            // until confirmation).
            if (!received)
                spm.ConfirmationReceived -= ev;
        }

        public object GetProperty(string id, string property)
        {
            bool received = false;
            GetPropertyMessage gpm = null;

            DateTime start = DateTime.Now;

            // Create the message.
            gpm = new GetPropertyMessage(this.m_LocalDht, this.m_Target, id, property);

            // Register the event handler.
            EventHandler ev = null;
            ev = (sender, e) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                gpm.ResultReceived -= ev;
            };
            gpm.ResultReceived += ev;

            // Send the message.
            gpm.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            if (!received)
                gpm.ResultReceived -= ev;

            // Assign the local node to the result.
            GraphWalker.Apply(gpm.Result, this.m_LocalNode);
            
            // Return the result.
            return gpm.Result;
        }

        public void AddEvent(EventTransport transport)
        {
            bool received = false;

            DateTime start = DateTime.Now;

            // Create the message.
            AddEventMessage aem = new AddEventMessage(this.m_LocalDht, this.m_Target, transport);

            // Register the event handler.
            EventHandler<MessageEventArgs> ev = null;
            ev = (sender, e) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                aem.ConfirmationReceived -= ev;
            };
            aem.ConfirmationReceived += ev;

            // Send the message.
            aem.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            // We have nothing to return (but it was still important to wait
            // until confirmation).
            if (!received)
                aem.ConfirmationReceived -= ev;
        }

        public void RemoveEvent(EventTransport transport)
        {
            bool received = false;

            DateTime start = DateTime.Now;

            // Create the message.
            RemoveEventMessage aem = new RemoveEventMessage(this.m_LocalDht, this.m_Target, transport);

            // Register the event handler.
            EventHandler<MessageEventArgs> ev = null;
            ev = (sender, e) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                aem.ConfirmationReceived -= ev;
            };
            aem.ConfirmationReceived += ev;

            // Send the message.
            aem.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            // We have nothing to return (but it was still important to wait
            // until confirmation).
            if (!received)
                aem.ConfirmationReceived -= ev;
        }

        public object Invoke(string id, string method, Type[] targs, object[] args)
        {
            bool received = false;
            InvokeMessage fm = null;

            DateTime start = DateTime.Now;

            // Create the message.
            fm = new InvokeMessage(this.m_LocalDht, this.m_Target, id, method, targs, args, false);

            // Register the event handler.
            EventHandler ev = null;
            ev = (sender, e) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                fm.ResultReceived -= ev;
            };
            fm.ResultReceived += ev;

            // Send the message.
            fm.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            if (!received)
                fm.ResultReceived -= ev;

            // Apply the local node to the result.
            GraphWalker.Apply(fm.Result, this.m_LocalNode);
            
            // Return the result.
            return fm.Result;
        }

        public void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            bool received = false;
            InvokeEventMessage fm = null;

            DateTime start = DateTime.Now;

            // Create the message.
            fm = new InvokeEventMessage(this.m_LocalDht, this.m_Target, transport, sender, e);

            // Register the event handler.
            EventHandler<MessageEventArgs> ev = null;
            ev = (s, ee) =>
            {
                // Mark as received so the thread will continue.
                received = true;
                fm.ConfirmationReceived -= ev;
            };
            fm.ConfirmationReceived += ev;

            // Send the message.
            fm.Send();

            // Wait until we have received it.
            while (!received && DateTime.Now.Subtract(start).TotalSeconds < Dht.TIMEOUT) Thread.Sleep(0);

            // If the request timed out, remove the contact.
            if (!received)
                this.m_LocalDht.Contacts.Remove(this.m_Target);

            // We have nothing to return (but it was still important to wait
            // until confirmation).
            if (!received)
                fm.ConfirmationReceived -= ev;
        }
    }
}
