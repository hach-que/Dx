using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;
using Process4.Providers;
using System.Threading;

namespace Process4.Remoting
{
    internal class RemoteNode : Node
    {
        private Contact m_Target = null;
        private Dht m_LocalDht = (LocalNode.Singleton.Storage as DhtWrapper).Dht;

        /// <summary>
        /// Creates a new remote node which references the specified contact.
        /// </summary>
        /// <param name="target">The target contact.</param>
        internal RemoteNode(Contact target)
        {
            this.m_Target = target;
        }

        internal override void SetProperty(string id, string property, object value)
        {
            bool received = false;

            //while (!received)
            {
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
                while (!received && DateTime.Now.Subtract(start).Seconds < 10) Thread.Sleep(0);

                // We have nothing to return (but it was still important to wait
                // until confirmation).
                if (!received)
                    spm.ConfirmationReceived -= ev;
            }
        }

        internal override object GetProperty(string id, string property)
        {
            bool received = false;
            GetPropertyMessage gpm = null;

            //while (!received)
            {
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
                while (!received/* && DateTime.Now.Subtract(start).Seconds < 10*/) Thread.Sleep(0);

                if (!received)
                    gpm.ResultReceived -= ev;
            }

            // Return the result.
            return gpm.Result;
        }

        internal override object Invoke(string id, string method, object[] args)
        {
            bool received = false;
            InvokeMessage fm = null;

            //while (!received)
            {
                DateTime start = DateTime.Now;

                // Create the message.
                fm = new InvokeMessage(this.m_LocalDht, this.m_Target, id, method, args, false);

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
                while (!received && DateTime.Now.Subtract(start).Seconds < 10) Thread.Sleep(0);

                if (!received)
                    fm.ResultReceived -= ev;
            }

            // Return the result.
            return fm.Result;
        }

        internal override DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback)
        {
            DTask<object> task = new DTask<object>();

            // Create the message.
            InvokeMessage fm = new InvokeMessage(this.m_LocalDht, this.m_Target, id, method, args, true);

            // Register the event handler.
            fm.ResultReceived += (sender, e) =>
            {
                // Execute the callback.
                task.Value = fm.Result;
                task.Completed = true;
                callback.DynamicInvoke(null);
            };

            // Send the message.
            fm.Send();

            // Return the task.
            return task;
        }
    }
}
