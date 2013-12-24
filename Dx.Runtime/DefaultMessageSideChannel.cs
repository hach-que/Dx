using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dx.Runtime
{
    public class DefaultMessageSideChannel : IMessageSideChannel
    {
        private readonly List<Message> m_Messages = new List<Message>(); 

        public Message WaitUntil(Func<Message, bool> predicate, int timeout)
        {
            var start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeout)
            {
                lock (this.m_Messages)
                {
                    var result = this.m_Messages.FirstOrDefault(predicate);
                    if (result != null)
                    {
                        this.m_Messages.Remove(result);
                        return result;
                    }
                }

                Thread.Sleep(0);
            }

            return null;
        }

        public bool Has(Func<Message, bool> predicate)
        {
            lock (this.m_Messages)
            {
                return this.m_Messages.FirstOrDefault(predicate) != null;
            }
        }

        public void Put(Message message)
        {
            lock (this.m_Messages)
            {
                this.m_Messages.Add(message);
            }
        }
    }
}
