using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Data4;

namespace Process4.Networks
{
    public class AutomaticUdpNetwork : INetworkProvider
    {
        private UdpClient m_Udp = null;

        /// <summary>
        /// The node this network provider is associated with.
        /// </summary>
        public LocalNode Node { get; private set; }

        /// <summary>
        /// The network identifier.
        /// </summary>
        public ID ID { get; private set; }

        /// <summary>
        /// The discovery port that the UDP network (INetworkProvider) is listening on.
        /// </summary>
        public int DiscoveryPort { get; private set; }

        /// <summary>
        /// The messaging port that the UDP network (IStorageProvider) is listening on.
        /// </summary>
        public int MessagingPort { get; private set; }

        /// <summary>
        /// Whether this is the first node to join the network.
        /// </summary>
        public bool IsFirst { get; private set; }

        /// <summary>
        /// The local, external IP address of the machine.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                // Force IPv4 since Windows is strange and returns multiple IPv6 addresses (including loopback).
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where((value) => value.AddressFamily == AddressFamily.InterNetwork).First();
            }
        }

        /// <summary>
        /// The broadcast endpoint to send packets to.
        /// </summary>
        private IPEndPoint BroadcastEndpoint
        {
            get
            {
                if (this.m_Udp.Client.AddressFamily == AddressFamily.InterNetwork)
                    return new IPEndPoint(IPAddress.Parse("255.255.255.255"), this.DiscoveryPort);
                else if (this.m_Udp.Client.AddressFamily == AddressFamily.InterNetworkV6)
                    return new IPEndPoint(IPAddress.Parse("ff02::1"), this.DiscoveryPort);
                else
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates a new automatic UDP network provider.  Updates the node's
        /// contact list as soon as new nodes arrive on the local area network.
        /// Automatically sets the discovery port to 18001 and messaging port
        /// to 18000.
        /// </summary>
        /// <param name="node">The node to associate with this network provider.</param>
        public AutomaticUdpNetwork(LocalNode node)
            : this(node, 18001, 18000)
        {
        }

        /// <summary>
        /// Creates a new automatic UDP network provider.  Updates the node's
        /// contact list as soon as new nodes arrive on the local area network.
        /// </summary>
        /// <param name="node">The node to associate with this network provider.</param>
        /// <param name="port">The discovery port to listen on.</param>
        /// <param name="port">The messaging port to forward onto contacts.</param>
        public AutomaticUdpNetwork(LocalNode node, int discoveryPort, int messagingPort)
        {
            this.Node = node;
            this.DiscoveryPort = discoveryPort;
            this.MessagingPort = messagingPort;
        }

        /// <summary>
        /// Joins the local network.
        /// </summary>
        public void Join(ID id)
        {
            this.ID = id;

            // Send out a broadcast UDP signal on the local area network to get
            // a list of peers.
            this.m_Udp = new UdpClient(new IPEndPoint(this.IPAddress, this.DiscoveryPort));
            if (this.m_Udp.Client.AddressFamily == AddressFamily.InterNetworkV6)
                this.m_Udp.JoinMulticastGroup(this.BroadcastEndpoint.Address);
            else
                this.m_Udp.EnableBroadcast = true;
            this.m_Udp.MulticastLoopback = false;
            byte[] data = Encoding.ASCII.GetBytes(
                "PROCESS4|" +
                this.ID.ToString() + "|" +
                this.Node.ID.ToString()
                );
            this.HandleUdp(this.m_Udp);
            this.m_Udp.Send(data, data.Length, this.BroadcastEndpoint);

            // The Join method is going to be used synchronously, but the Distributed<> generic won't be able
            // to function correctly whileever this node doesn't have peers.  So we just sleep here for half
            // a second to give nodes time to respond..
            Thread.Sleep(500);
            this.IsFirst = this.Node.Contacts.Count() == 0;
        }

        /// <summary>
        /// Leaves the network.
        /// </summary>
        public void Leave()
        {
            byte[] data = Encoding.ASCII.GetBytes(
                "PROCESS4LEAVE|" +
                this.ID.ToString()
                );
            this.m_Udp.Send(data, data.Length, this.BroadcastEndpoint);
            this.m_Udp.Close();
            this.Node.Contacts.Clear();
        }

        /// <summary>
        /// Handles the UDP listening loop which monitors when new clients appear
        /// and drop out from the local network.
        /// </summary>
        /// <param name="udp">The UDP client to use.</param>
        private void HandleUdp(UdpClient udp)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        // Get data from other nodes.
                        IPEndPoint other = new IPEndPoint(IPAddress.None, 0);
                        byte[] bytes = udp.Receive(ref other);
                        string s = Encoding.ASCII.GetString(bytes);

                        // Make sure it is a valid request.
                        if (s.StartsWith("PROCESS4|"))
                        {
                            // Handle the contact request.
                            string[] data = s.Split(new char[] { '|' });
                            if (data[1] == this.ID.ToString() && data[2] != this.Node.ID.ToString())
                            {
                                // Only respond if the network ID is the same and the request
                                // isn't from ourselves.
                                byte[] wbytes = Encoding.ASCII.GetBytes(
                                    "PROCESS4ENDPOINT|" +
                                    this.Node.ID.ToString()
                                    );
                                udp.Send(wbytes, wbytes.Length, other);

                                // Add the contact so that each node knows each other.
                                Data4.Contact contact = new Data4.Contact(
                                    Data4.ID.FromString(data[2]),
                                    new IPEndPoint(other.Address, this.MessagingPort)
                                    );
                                this.Node.Contacts.Add(contact);
                            }
                        }
                        else if (s.StartsWith("PROCESS4ENDPOINT|"))
                        {
                            // Handle the response to the contact request.
                            Data4.Contact contact = new Data4.Contact(
                                Data4.ID.FromString(s.Substring("PROCESS4ENDPOINT|".Length)),
                                new IPEndPoint(other.Address, this.MessagingPort)
                                );
                            if (contact.Identifier != this.Node.ID)
                                this.Node.Contacts.Add(contact);
                        }
                        else if (s.StartsWith("PROCESS4LEAVE|"))
                        {
                            // Handle the contact left request by removing them from our contacts.
                            Data4.Contact contact = new Data4.Contact(
                                Data4.ID.FromString(s.Substring("PROCESS4LEAVE|".Length)),
                                new IPEndPoint(other.Address, this.MessagingPort)
                                );
                            if (contact.Identifier != this.Node.ID)
                            {
                                Contact actual = this.Node.Contacts.Where(value => value.Identifier == contact.Identifier).FirstOrDefault();
                                if (actual != null)
                                    this.Node.Contacts.Remove(actual);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException || e is ObjectDisposedException)
                        return;
                    Console.WriteLine(e.ToString());
                }
            });
            t.Name = "Automatic Udp Network Thread";
            t.IsBackground = true;
            t.Start();
        }
    }
}
