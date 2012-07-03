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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Collections.ObjectModel;

namespace Data4
{
    public class Dht
    {
        private Contact p_Self = null;
        private List<Contact> p_Contacts = new List<Contact>();
        private IFormatter p_Formatter = null;
        private TcpListener m_TcpListener = null;
        private Thread m_TcpThread = null;
        private bool p_ShowDebug = false;
        private List<Entry> p_OwnedEntries = new List<Entry>();
        private object p_OwnedEntriesLock = new object();
        private object p_YieldingLock = new object();

        public const int TIMEOUT = 5;

        public event EventHandler<MessageEventArgs> OnReceived;
        public event EventHandler<EntriesRequestedEventArgs> OnEntriesRequested;

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
            this.m_TcpListener = new TcpListener(endpoint);
            this.m_TcpThread = new Thread(delegate()
            {
                while (true)
                {
                    try
                    {
                        Socket client = this.m_TcpListener.AcceptSocket();
                        new Thread(() =>
                        {
                            while (true)
                            {
                                try
                                {
                                    int received = 0;
                                    int total = sizeof(Int32);
                                    byte[] size = new byte[sizeof(Int32)];
                                    while (received < total)
                                        received += client.Receive(size, received, sizeof(Int32) - received, SocketFlags.None);
                                    if (BitConverter.ToInt32(size, 0) == 0)
                                        throw new InvalidDataException("Length of received message must be greater than 0.");
                                    received = 0;
                                    total = BitConverter.ToInt32(size, 0);
                                    byte[] result = new byte[total];
                                    while (received < total)
                                        received += client.Receive(result, received, total - received, SocketFlags.None);
                                    this.LogI(LogType.DEBUG, "Received a message from " + client.RemoteEndPoint.ToString());
                                    Thread handler = new Thread(a =>
                                        {
                                            this.OnReceive(((ThreadInformation)a).Endpoint, ((ThreadInformation)a).Result);
                                        });
                                    handler.Name = "Message Handling Thread";
                                    handler.IsBackground = true;
                                    handler.Start(new ThreadInformation { Endpoint = client.RemoteEndPoint as IPEndPoint, Result = result.Clone() as byte[] });
                                }
                                catch (SocketException ex)
                                {
                                    if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                        ex.SocketErrorCode == SocketError.ConnectionReset ||
                                        ex.SocketErrorCode == SocketError.TimedOut)
                                        return; // Other host disconnected.
                                    else
                                        throw;
                                }
                                catch (Exception ex)
                                {
                                    if (ex is ThreadAbortException)
                                        return;
                                    Console.WriteLine(ex.ToString());
                                    throw;
                                }
                            }
                        }) { IsBackground = true }.Start();
                    }
                    catch (Exception e)
                    {
                        if (e is ThreadAbortException)
                            return;
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
            });
            this.m_TcpThread.Name = "Dht Receiving Thread";
            this.m_TcpThread.IsBackground = true;
            this.m_TcpListener.Start();
            this.m_TcpThread.Start();

            if (this.p_ShowDebug)
                Console.WriteLine("Dht node created with " + identifier.ToString() + " on " + endpoint.ToString());
        }

        private struct ThreadInformation
        {
            public IPEndPoint Endpoint;
            public byte[] Result;
        }

        /// <summary>
        /// Updates an existing entry in the DHT, or adds a new entry if it does not
        /// exist.
        /// </summary>
        public void UpdateOrPut(ID key, object value)
        {
            lock (this.p_OwnedEntriesLock)
            {
                Entry e = this.p_OwnedEntries.Where(v => v.Key == key).FirstOrDefault();
                if (e == null)
                    this.p_OwnedEntries.Add(new Entry(this, this.p_Self, key, value));
                else
                    e.Value = value;
            }
        }

        /// <summary>
        /// Adds a key-value entry to the DHT, storing the key-value pair on this node.
        /// </summary>
        public void Put(ID key, object value)
        {
            lock (this.p_OwnedEntriesLock)
            {
                this.p_OwnedEntries.Add(new Entry(this, this.p_Self, key, value));
            }
        }

        /// <summary>
        /// Remove a key-value pair from this node.
        /// </summary>
        public void Remove(ID key)
        {
            List<Entry> es = new List<Entry>();
            lock (this.p_OwnedEntriesLock)
            {
                foreach (Entry e in this.p_OwnedEntries)
                    if (e.Key == key)
                        es.Add(e);
                foreach (Entry e in es)
                    this.p_OwnedEntries.Remove(e);
            }
        }

        /// <summary>
        /// Retrieves values from the DHT as a yeilded enumerable.  You should
        /// only enumerate up to the number of entries you need (so if you only
        /// need the first entry, use the Linq .First() operation).
        /// </summary>
        public IEnumerable<Entry> Get(ID key)
        {
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
                     foreach (Entry ee in (sender as FetchMessage).Values)
                         entries.Add(ee);
                     respondants.Add((sender as FetchMessage).Target);
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
                DateTime start = DateTime.Now;

                // If there are still contacts left to respond and we have
                // no entries.
                while (entries.Count == 0 && respondants.Count < 1 && DateTime.Now.Subtract(start).Seconds < Dht.TIMEOUT)
                    Thread.Sleep(0);
            }

            // Take entries from the collection while
            // ever we can.
            Entry take;
            while (entries.TryTake(out take))
                yield return take;
        }

        /// <summary>
        /// Handles receiving data through the TcpClient.
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
                if (stream.Position != stream.Length)
                    throw new InvalidDataException("Not all transferred data was consumed during deserialization.");
                MessageEventArgs e = new MessageEventArgs(message);
                message.Dht = this;
                message.Sender = this.FindContactByEndPoint(endpoint);

                if (this.OnReceived != null)
                    this.OnReceived(this, e);

                // If the remote host that sent the message disappears when sending
                // the confirmation data then we don't really care, since we were only
                // ever sending confirmation anyway.
                try
                {
                    // TODO: Is there a better way to do this?
                    if (e.Message is FetchMessage)
                    {
                        // Handle the fetch request.
                        FetchConfirmationMessage fcm = new FetchConfirmationMessage(this, message, this.OnFetch(e.Message as FetchMessage));
                        fcm.Send();
                    }
                    else if (e.SendConfirmation && !(e.Message is ConfirmationMessage))
                    {
                        // Send confirmation message.
                        ConfirmationMessage cm = new ConfirmationMessage(this, message, "");
                        cm.Send();
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                        ex.SocketErrorCode == SocketError.ConnectionReset ||
                        ex.SocketErrorCode == SocketError.TimedOut)
                        return;
                    else
                        throw;
                }
            }
        }

        /// <summary>
        /// Handle a fetch request.
        /// </summary>
        private List<Entry> OnFetch(FetchMessage request)
        {
            List<Entry> entries = new List<Entry>();
            lock (this.p_OwnedEntriesLock)
            {
                foreach (Entry e in this.p_OwnedEntries)
                    if (e.Key == request.Key)
                        entries.Add(e);
            }
            if (this.OnEntriesRequested != null)
            {
                EntriesRequestedEventArgs erea = new EntriesRequestedEventArgs();
                this.OnEntriesRequested(this, erea);
                foreach (KeyValuePair<ID, object> kv in erea.Entries)
                    if (kv.Key == request.Key)
                        entries.Add(new Entry(this, this.Self, kv.Key, kv.Value));
            }
            return entries;
        }

        public bool Debug
        {
            get { return this.p_ShowDebug; }
            set { this.p_ShowDebug = value; }
        }

        public void Close()
        {
            this.m_TcpThread.Abort();
            this.m_TcpListener.Stop();
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

        public ReadOnlyCollection<Entry> OwnedEntries
        {
            get { return this.p_OwnedEntries.AsReadOnly(); }
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

