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

namespace Dx.Runtime
{
    public class Dht
    {
        private readonly Contact p_Self;
        private readonly List<Contact> p_Contacts = new List<Contact>();
        private readonly IFormatter p_Formatter;
        private readonly TcpListener m_TcpListener;
        private readonly Thread m_TcpThread;
        private bool p_ShowDebug;
        private readonly List<Entry> p_OwnedEntries = new List<Entry>();
        private readonly object p_OwnedEntriesLock = new object();
        private readonly object p_YieldingLock = new object();
        private bool m_Stopping;
        private readonly List<MessageWithExpiry> m_ExpectingConfirmation = new List<MessageWithExpiry>();
        private readonly object m_ExpectingConfirmationLock = new object();
        private EventHandler<MessageEventArgs> m_CurrentOnReceived;

        // TODO: This is 60 seconds for the moment to account for large parameters or
        // result data.  When the data is being sent or received, this counts towards
        // the timeout, even though in reality we should be aware that we've started
        // to receive a response from a node and therefore just have to wait until it
        // completes.
        public const int TIMEOUT = 60;

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
                        if (!this.m_TcpListener.Pending())
                        {
                            if (this.m_Stopping)
                            {
                                this.m_TcpListener.Stop();
                                return;
                            }
                            Thread.Sleep(0);
                            continue;
                        }
                        
                        var client = this.m_TcpListener.AcceptTcpClient();
                        new Thread(() =>
                        {
                            using (client)
                            {
                                while (true)
                                {
                                    try
                                    {
                                        int received = 0;
                                        int total = sizeof(Int32);
                                        byte[] size = new byte[sizeof(Int32)];
                                        while (received < total)
                                            received += client.Client.Receive(size, received, sizeof(Int32) - received, SocketFlags.None);
                                        if (BitConverter.ToInt32(size, 0) == 0)
                                            throw new InvalidDataException("Length of received message must be greater than 0.");
                                        received = 0;
                                        total = BitConverter.ToInt32(size, 0);
                                        byte[] result = new byte[total];
                                        while (received < total)
                                            received += client.Client.Receive(result, received, total - received, SocketFlags.None);
                                        this.LogI(LogType.DEBUG, "Received - (from " + client.Client.RemoteEndPoint + ")");
                                        Thread handler = new Thread(a =>
                                            {
                                                this.OnReceive(((ThreadInformation)a).Endpoint, ((ThreadInformation)a).Result);
                                            });
                                        handler.Name = "Message Handling Thread";
                                        handler.IsBackground = true;
                                        handler.Start(new ThreadInformation { Endpoint = client.Client.RemoteEndPoint as IPEndPoint, Result = result.Clone() as byte[] });
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
        
        public void SetOnReceivedHandler(EventHandler<MessageEventArgs> handler)
        {
            this.m_CurrentOnReceived = handler;
        }

        private struct ThreadInformation
        {
            public IPEndPoint Endpoint;
            public byte[] Result;
        }
        
        private class MessageWithExpiry
        {
            public Message Message;
            public DateTime Expiry;
        }
        
        public void AwaitForConfirmation(Message message)
        {
            lock (this.m_ExpectingConfirmationLock)
            {
                this.m_ExpectingConfirmation.Add(new MessageWithExpiry
                {
                    Message = message,
                    Expiry = DateTime.Now.AddSeconds(TIMEOUT)
                });
            }
        }

        /// <summary>
        /// Updates an existing entry in the DHT, or adds a new entry if it does not
        /// exist.
        /// </summary>
        public void UpdateOrPut(ID key, object value)
        {
            lock (this.p_OwnedEntriesLock)
            {
                Entry e = this.p_OwnedEntries.FirstOrDefault(v => v.Key == key);
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
                fm.Send(fm.Target);
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
            {
                if (!take.DeadOnArrival)
                    yield return take;
            }
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
                this.LogI(Dht.LogType.DEBUG, "          Message - " + message);

                // Check waiting confirmation messages.
                if (message is ConfirmationMessage)
                {
                    var toRemove = new List<MessageWithExpiry>();
                    lock (this.m_ExpectingConfirmationLock)
                    {
                        foreach (var confirm in this.m_ExpectingConfirmation)
                        {
                            if (confirm.Message.Identifier == message.Identifier)
                            {
                                toRemove.Add(confirm);
                                confirm.Message.OnConfirm(this, e);
                            }
                            else if (DateTime.Now > confirm.Expiry)
                            {
                                toRemove.Add(confirm);
                            }
                        }
                        foreach (var @remove in toRemove)
                        {
                            this.m_ExpectingConfirmation.Remove(@remove);
                        }
                    }
                }

                // Fire OnReceived event.
                if (this.m_CurrentOnReceived != null)
                    this.m_CurrentOnReceived(this, e);

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
                        fcm.Send(fcm.Target);
                    }
                    else if (e.Message.SendBasicConfirmation && !(e.Message is ConfirmationMessage))
                    {
                        // Send confirmation message.
                        ConfirmationMessage cm = new ConfirmationMessage(this, message, "");
                        cm.Send(cm.Target);
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
            this.m_Stopping = true;
            while (this.m_TcpThread.IsAlive)
            {
                Console.WriteLine("Waiting for listening thread to close...");
                Thread.Sleep(0);
            }
            
            // Wait until all contact connections have also closed.
            foreach (var contact in this.p_Contacts)
            {
                while (ContactPool.ConnectionIsOpen(contact.EndPoint))
                {
                    ContactPool.AttemptToCloseConnection(contact.EndPoint);
                    Console.WriteLine("Waiting for connection to " + contact.Identifier + " to close...");
                    Thread.Sleep(0);
                }
            }
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
            string tid = Thread.CurrentThread.ManagedThreadId.ToString();
            switch (type)
            {
                case LogType.ERROR:
                    Console.WriteLine("ERROR  : " + tid + " : " + id + " " + msg);
                    break;
                case LogType.WARNING:
                    Console.WriteLine("WARNING: " + tid + " : " + id + " " + msg);
                    break;
                case LogType.INFO:
                    Console.WriteLine("INFO   : " + tid + " : " + id + " " + msg);
                    break;
                case LogType.DEBUG:
                    if (this.p_ShowDebug)
                        Console.WriteLine("DEBUG  : " + tid + " : " + id + " " + msg);
                    break;
                default:
                    Console.WriteLine("UNKNOWN: " + tid + " : " + id + " " + msg);
                    break;
            }
        }

        public static void LogS(LogType type, string msg)
        {
            string tid = Thread.CurrentThread.ManagedThreadId.ToString();
            switch (type)
            {
                case LogType.ERROR:
                    Console.WriteLine("ERROR  : " + tid + " : " + msg);
                    break;
                case LogType.WARNING:
                    Console.WriteLine("WARNING: " + tid + " : " + msg);
                    break;
                case LogType.INFO:
                    Console.WriteLine("INFO   : " + tid + " : " + msg);
                    break;
                case LogType.DEBUG:
                    Console.WriteLine("DEBUG  : " + tid + " : " + msg);
                    break;
                default:
                    Console.WriteLine("UNKNOWN: " + tid + " : " + msg);
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

