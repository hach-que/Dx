using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Dx.Runtime
{
    public class DefaultObjectLookup : IObjectLookup
    {
        private readonly ILocalNode m_LocalNode;

        private readonly IObjectStorage m_ObjectStorage;

        private readonly IClientLookup m_ClientLookup;

        private readonly IMessageConstructor m_MessageConstructor;

        private readonly IMessageSideChannel m_MessageSideChannel;

        public DefaultObjectLookup(
            ILocalNode localNode,
            IObjectStorage objectStorage,
            IClientLookup clientLookup,
            IMessageConstructor messageConstructor,
            IMessageSideChannel messageSideChannel)
        {
            this.m_LocalNode = localNode;
            this.m_ObjectStorage = objectStorage;
            this.m_ClientLookup = clientLookup;
            this.m_MessageConstructor = messageConstructor;
            this.m_MessageSideChannel = messageSideChannel;
        }

        public LiveEntry GetFirst(ID key, int timeout)
        {
            var cached = this.m_ObjectStorage.Find(key).FirstOrDefault();
            if (cached != null)
            {
                return cached;
            }

            var fetchMessage = this.m_MessageConstructor.ConstructFetchMessage(key);

            var waitFor = new List<IPEndPoint>();
            foreach (var kv in this.m_ClientLookup.GetAll())
            {
                if (object.Equals(kv.Key, this.m_LocalNode.Self.IPEndPoint))
                {
                    continue;
                }

                kv.Value.Send(fetchMessage);
                waitFor.Add(kv.Key);
            }

            var start = DateTime.Now;

            // Wait until all contacts have responded, the timeout is up or we have an entry.
            while (
                (DateTime.Now - start).TotalMilliseconds < timeout && 
                waitFor.Count > 0 && 
                this.m_ObjectStorage.Find(key).FirstOrDefault() == null)
            {
                foreach (var endpoint in waitFor.ToArray())
                {
                    var endpointCopy = endpoint;
                    if (this.m_MessageSideChannel.Has(x =>
                            x.Type == MessageType.FetchConfirmation &&
                            x.FetchKey == key && object.Equals(x.Sender.IPAddress, endpointCopy.Address)
                            && x.Sender.Port == endpointCopy.Port))
                    {
                        waitFor.Remove(endpoint);
                    }
                }

                Thread.Sleep(0);
            }

            return this.m_ObjectStorage.Find(key).FirstOrDefault();
        }
    }
}
