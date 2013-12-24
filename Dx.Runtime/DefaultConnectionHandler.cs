using System;
using System.Net;
using System.Net.Sockets;

namespace Dx.Runtime
{
    public class DefaultConnectionHandler : IConnectionHandler
    {
        private readonly IClientHandlerFactory m_ClientHandlerFactory;

        private readonly IClientLookup m_ClientLookup;

        private TcpListener m_Listener;

        public DefaultConnectionHandler(IClientHandlerFactory clientHandlerFactory, IClientLookup clientLookup)
        {
            this.m_ClientHandlerFactory = clientHandlerFactory;
            this.m_ClientLookup = clientLookup;
        }

        public void Start(IPAddress address, int port)
        {
            this.m_Listener = new TcpListener(address, port);

            this.m_Listener.Start();
            
            this.m_Listener.BeginAcceptTcpClient(this.OnAcceptTcpClient, null);
        }

        public void Stop()
        {
            this.m_Listener.Stop();
            this.m_Listener = null;
        }

        private void OnAcceptTcpClient(IAsyncResult ar)
        {
            // If the listener is null, then Stop() has been called and we can't handle
            // anything anyway.
            if (this.m_Listener == null)
            {
                return;
            }

            // Retrieve the client.
            var client = this.m_Listener.EndAcceptTcpClient(ar);

            // Use the IClientHandlerFactory to create a new client handler.
            var handler = this.m_ClientHandlerFactory.CreateListeningClientHandler(client);

            // Store the client in the pool.
            this.m_ClientLookup.Add((IPEndPoint)client.Client.RemoteEndPoint, handler);

            // Start the handler.
            handler.Start();

            // Start listening for another client.
            this.m_Listener.BeginAcceptTcpClient(this.OnAcceptTcpClient, null);
        }
    }
}
