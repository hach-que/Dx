using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Dx.Runtime
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
                    if (ContactPool.m_Connections[endpoint].Client == null ||
                        !ContactPool.m_Connections[endpoint].Connected)
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
        
        public static bool ConnectionIsOpen(IPEndPoint endpoint)
        {
            lock (ContactPool.m_ConnectionLock)
            {
                return ContactPool.m_Connections.ContainsKey(endpoint) &&
                       ContactPool.m_Connections[endpoint].Client != null &&
                       ContactPool.m_Connections[endpoint].Connected;
            }
        }
        
        public static void AttemptToCloseConnection(IPEndPoint endpoint)
        {
            lock (ContactPool.m_ConnectionLock)
            {
                if (ContactPool.m_Connections.ContainsKey(endpoint) &&
                    ContactPool.m_Connections[endpoint].Client != null &&
                    ContactPool.m_Connections[endpoint].Connected)
                {
                    ContactPool.m_Connections[endpoint].Close();
                    if (ContactPool.m_Connections[endpoint].Client == null)
                        ContactPool.m_Connections.Remove(endpoint);
                }
            }
        }
        
        public static IEnumerable<TcpClient> GetAllOpenClients()
        {
            lock (ContactPool.m_ConnectionLock)
            {
                foreach (var kv in ContactPool.m_Connections.ToDictionary(x => x.Key, x => x.Value))
                {
                    if (kv.Value.Client == null)
                    {
                        ContactPool.m_Connections.Remove(kv.Key);
                        continue;
                    }
                    if (kv.Value.Connected)
                        yield return kv.Value;
                }
            }
        }
    }
}
