using System.Collections.Generic;
using System.Net;

namespace Dx.Runtime
{
    public class DefaultClientLookup : IClientLookup
    {
        private Dictionary<IPEndPoint, IClientHandler> m_Handlers = new Dictionary<IPEndPoint, IClientHandler>(); 

        public void Add(IPEndPoint endpoint, IClientHandler clientHandler)
        {
            this.m_Handlers.Add(endpoint, clientHandler);
        }

        public IClientHandler Lookup(IPEndPoint endpoint)
        {
            return this.m_Handlers[endpoint];
        }

        public IEnumerable<KeyValuePair<IPEndPoint, IClientHandler>> GetAll()
        {
            return this.m_Handlers;
        }

        public void Remove(IPEndPoint endpoint)
        {
            this.m_Handlers.Remove(endpoint);
        }
    }
}
