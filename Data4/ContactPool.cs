using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Data4
{
    /// <summary>
    /// Maintains a TCP connection pool.
    /// </summary>
    public static class ContactPool
    {
        private static Dictionary<IPEndPoint, TcpClient> m_Connections = new Dictionary<IPEndPoint, TcpClient>();
        private static object m_ConnectionLock = new object();

        public static TcpClient GetTcpClient(IPEndPoint endpoint)
        {
            lock (ContactPool.m_ConnectionLock)
            {
                if (ContactPool.m_Connections.ContainsKey(endpoint))
                {
                    if (!ContactPool.m_Connections[endpoint].Connected)
                    {
                        ContactPool.m_Connections.Remove(endpoint);
                        return GetTcpClient(endpoint);
                    }
                    return ContactPool.m_Connections[endpoint];
                }
                else
                {
                    // Connect to target.
                    TcpClient client = new TcpClient();
                    client.Connect(endpoint);
                    client.SendTimeout = Dht.TIMEOUT;
                    client.ReceiveTimeout = Dht.TIMEOUT;
                    ContactPool.m_Connections.Add(endpoint, client);
                    return client;
                }
            }
        }
    }
}
