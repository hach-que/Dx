// 
//  Copyright 2010  Trust4 Developers
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Data4
{
    public class Dht
    {
        private Contact p_Self = null;
        private List<Contact> p_Contacts = new List<Contact>();
        private IFormatter p_Formatter = null;
        private UdpClient m_UdpClient = null;
        private Thread m_UdpThread = null;
        private bool p_ShowDebug = false;
        private List<Entry> p_OwnedEntries = new List<Entry>();
        private object p_YieldingLock = new object();
        //private ConcurrentBag<Entry> p_CachedEntries = new ConcurrentBag<Entry>();

        public event EventHandler<MessageEventArgs> OnReceived;

        public Dht(ID identifier, IPEndPoint endpoint)
            : this(identifier, endpoint, false)
        {
        }

        public Dht(ID identifier, IPEndPoint endpoint, bool debug)
        {
            this.p_Self = new Contact(identifier, endpoint);
            this.p_Formatter = new BinaryFormatter();
            this.p_ShowDebug = debug;

            // Start listening for events.
            IPEndPoint from = null;
            this.m_UdpClient = new UdpClient(endpoint);
            this.m_UdpThread = new Thread(delegate()
            {
                try
                {
                    while (true)
                    {
                        byte[] result = this.m_UdpClient.Receive(ref from);
                        this.LogI(LogType.DEBUG, "Received a message from " + from.ToString());
                        this.OnReceive(from, result);
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        return;
                    Console.WriteLine(e.ToString());
                }
            }
            );
            this.m_UdpThread.Name = "Dht Receiving Thread";
            //this.m_UdpThread.IsBackground = true;
            this.m_UdpThread.Start();

            if (this.p_ShowDebug)
                Console.WriteLine("Dht node created with " + identifier.ToString() + " on " + endpoint.ToString());
        }

        /// <summary>
        /// Adds a key-value entry to the DHT, storing the key-value pair on this node.
        /// </summary>
        public void Put(ID key, string value)
        {
            this.p_OwnedEntries.Add(new Entry(this, this.p_Self, key, value));
        }

        /// <summary>
        /// Remove a key-value pair from this node.
        /// </summary>
        public void Remove(ID key)
        {
            List<Entry> es = new List<Entry>();
            foreach (Entry e in this.p_OwnedEntries)
                if (e.Key == key)
                    es.Add(e);
            foreach (Entry e in es)
                this.p_OwnedEntries.Remove(e);
        }

        /// <summary>
        /// Retrieves values from the DHT as a yeilded enumerable.  You should
        /// only enumerate up to the number of entries you need (so if you only
        /// need the first entry, use the Linq .First() operation).
        /// </summary>
        public IEnumerable<Entry> Get(ID key)
        {
            // 
            // TODO: Make this return null instantly if all contacts have responded
            //       but there are still no results rather than waiting for one
            //       second!  It looks like it does this, but it doesn't actually
            //       work that way.
            //

            // Yield any entries that we already have
            // first.
            Entry[] el = this.p_OwnedEntries.ToArray();
            foreach (Entry e in el)
            {
                if (e.Key == key)
                    yield return e;
            }

            // Define the event handler.
            ConcurrentBag<Contact> respondants = new ConcurrentBag<Contact>();
            ConcurrentBag<Entry> entries = new ConcurrentBag<Entry>();
            EventHandler ev = null;
            ev = (sender, e) =>
                 {
                     respondants.Add((sender as FetchMessage).Target);
                     foreach (Entry ee in (sender as FetchMessage).Values)
                         entries.Add(ee);
                     (sender as FetchMessage).ResultReceived -= ev;
                 };

            // Send a fetch message out to everyone.
            foreach (Contact c in this.p_Contacts)
            {
                FetchMessage fm = new FetchMessage(this, c, key);
                fm.ResultReceived += ev;
                fm.Send();
            }

            // Wait a little bit for the first entry to
            // arrive if there isn't anything in the entries
            // list yet.
            if (this.p_Contacts.Count > 0)
            {
                // If there are still contacts left to respond and we have
                // no entries.
                while (entries.Count == 0 && respondants.Count < this.p_Contacts.Count)
                {
                    DateTime start = DateTime.Now;

                    while (entries.Count == 0 && respondants.Count < this.p_Contacts.Count && DateTime.Now.Subtract(start).Seconds < 1)
                        Thread.Sleep(0);

                    if (entries.Count == 0 && respondants.Count < this.p_Contacts.Count)
                    {
                        // Still nothing, request the data from the nodes again.
                        Contact[] cc = respondants.ToArray();
                        int totalsent = 0;
                        foreach (Contact c in this.p_Contacts)
                        {
                            // No idea why .Contains isn't working here...
                            bool isin = false;
                            foreach (Contact c2 in cc)
                                if (c == c2)
                                {
                                    isin = true;
                                    break;
                                }

                            if (!isin)
                            {
                                FetchMessage fm = new FetchMessage(this, c, key);
                                fm.ResultReceived += ev;
                                fm.Send();
                                totalsent += 1;
                            }
                        }

                        // Check to see if there's no more contacts to send to.
                        if (totalsent == 0)
                            break;
                    }
                }
            }

            // Take entries from the collection while
            // ever we can.
            Entry take;
            while (entries.TryTake(out take))
                yield return take;
        }

        /// <summary>
        /// Handles receiving data through the UdpClient.
        /// </summary>
        /// <param name="endpoint">The endpoint from which the message was received.</param>
        /// <param name="result">The data that was received.</param>
        private void OnReceive(IPEndPoint endpoint, byte[] result)
        {
            using (MemoryStream stream = new MemoryStream(result))
            {
                StreamingContext old = this.p_Formatter.Context;
                this.p_Formatter.Context = new StreamingContext(this.p_Formatter.Context.State, new SerializationData { Dht = this, IsMessage = true });
                Message message = this.p_Formatter.Deserialize(stream) as Message;
                this.p_Formatter.Context = old;
                MessageEventArgs e = new MessageEventArgs(message);
                message.Dht = this;
                message.Sender = this.FindContactByEndPoint(endpoint);

                if (this.OnReceived != null)
                    this.OnReceived(this, e);

                // TODO: Is there a better way to do this?
                if (e.Message is FetchMessage)
                {
                    // Handle the fetch request.
                    FetchConfirmationMessage fcm = new FetchConfirmationMessage(this, message, this.OnFetch(e.Message as FetchMessage));
                    fcm.Send();

                    // TODO: Make sure that the confirmation message is received.
                }
                else if (e.SendConfirmation && !( e.Message is ConfirmationMessage ))
                {
                    ConfirmationMessage cm = new ConfirmationMessage(this, message, "");
                    cm.Send();

                    // TODO: Make sure that the confirmation message is received.  Probably should
                    //       implement confirmation of confirmations in ConfirmationMessage class itself.
                }
            }
        }

        /// <summary>
        /// Handle a fetch request.
        /// </summary>
        private List<Entry> OnFetch(FetchMessage request)
        {
            List<Entry> entries = new List<Entry>();
            foreach (Entry e in this.p_OwnedEntries)
                if (e.Key == request.Key)
                    entries.Add(e);
            /*foreach (Entry e in this.p_CachedEntries)
                if (e.Key == request.Key)
                    entries.Add(e);*/
            return entries;
        }

        public bool Debug
        {
            get { return this.p_ShowDebug; }
            set { this.p_ShowDebug = value; }
        }

        public void Close()
        {
            this.m_UdpThread.Abort();
            this.m_UdpClient.Close();
        }

        public IFormatter Formatter
        {
            get { return this.p_Formatter; }
        }

        public Contact Self
        {
            get { return this.p_Self; }
        }

        public List<Contact> Contacts
        {
            get { return this.p_Contacts; }
        }

        public enum LogType
        {
            ERROR,
            WARNING,
            INFO,
            DEBUG
        }

        public void LogI(LogType type, string msg)
        {
            string id = (this.p_Self.EndPoint != null) ? this.p_Self.EndPoint.ToString() + " :" : "";
            switch (type)
            {
                case LogType.ERROR:
                    Console.WriteLine("ERROR  : " + id + " " + msg);
                    break;
                case LogType.WARNING:
                    Console.WriteLine("WARNING: " + id + " " + msg);
                    break;
                case LogType.INFO:
                    Console.WriteLine("INFO   : " + id + " " + msg);
                    break;
                case LogType.DEBUG:
                    if (this.p_ShowDebug)
                        Console.WriteLine("DEBUG  : " + id + " " + msg);
                    break;
                default:
                    Console.WriteLine("UNKNOWN: " + id + " " + msg);
                    break;
            }
        }

        public static void LogS(LogType type, string msg)
        {
            switch (type)
            {
                case LogType.ERROR:
                    Console.WriteLine("ERROR  : " + msg);
                    break;
                case LogType.WARNING:
                    Console.WriteLine("WARNING: " + msg);
                    break;
                case LogType.INFO:
                    Console.WriteLine("INFO   : " + msg);
                    break;
                case LogType.DEBUG:
                    Console.WriteLine("DEBUG  : " + msg);
                    break;
                default:
                    Console.WriteLine("UNKNOWN: " + msg);
                    break;
            }
        }

        /// <summary>
        /// Returns the contact in the Contacts list that has the specified endpoint, or
        /// null if there was no contact.
        /// </summary>
        public Contact FindContactByEndPoint(IPEndPoint endpoint)
        {
            foreach (Contact c in this.p_Contacts)
            {
                if (c.EndPoint.Address.ToString() == endpoint.Address.ToString())
                    return c;
            }

            return null;
        }
    }
}

