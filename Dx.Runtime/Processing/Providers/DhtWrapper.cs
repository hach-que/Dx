using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using Data4;
using System.Net;
using Process4.Remoting;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Process4.Attributes;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Threading;
using Data4.Inspector;

namespace Process4.Providers
{
    internal class DhtWrapper : IStorageProvider, IContactProvider
    {
        private Dht p_Dht = null;
        private LocalNode m_Node = null;
        private Dictionary<ID, object> p_CachedEntries = new Dictionary<ID, object>();
#if INSPECTOR
        private Thread m_InspectorThread = null;
        private InspectorForm m_Inspector = null;
#endif

        internal DhtWrapper(LocalNode node)
        {
            // Store our reference to our node so we can use the IProcessorProvider
            // later on.
            this.m_Node = node;

            // Start inspector.
#if INSPECTOR
            this.m_InspectorThread = new Thread(() =>
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(true);
                    this.m_Inspector = new InspectorForm(this);
                    System.Windows.Forms.Application.Run(this.m_Inspector);
                }) { IsBackground = true };
            this.m_InspectorThread.Start();
#endif
        }

        /// <summary>
        /// Starts the DHT.
        /// </summary>
        public void Start()
        {
            // Creates our new DHT node with the specified ID and endpoint.
            this.p_Dht = new Dht(this.m_Node.ID, new IPEndPoint(this.m_Node.Network.IPAddress, this.m_Node.Network.MessagingPort));

            // Register the message listener on the DHT which we will use to
            // detect InvokeMessage, SetPropertyMessage and GetPropertyMessage.
            this.p_Dht.OnReceived += new EventHandler<MessageEventArgs>(p_Dht_OnReceived);

            // Register the entry provider for when we are operating in StoreOnDemand mode.
            if (this.m_Node.Caching == Caching.StoreOnDemand)
                this.p_Dht.OnEntriesRequested += new EventHandler<EntriesRequestedEventArgs>(p_Dht_OnEntriesRequested);
        }

        /// <summary>
        /// Stops the DHT.
        /// </summary>
        public void Stop()
        {
            this.p_Dht.Close();
        }

        internal void InspectorLog(string message, string type)
        {
#if INSPECTOR
            if (this.m_Inspector != null)
                this.m_Inspector.Log(message, type);
#endif
        }

        /// <summary>
        /// This event is raised when the DHT has received a message and we should
        /// check to see if it is any of the types we want to handle.
        /// </summary>
        void p_Dht_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is FetchConfirmationMessage && this.m_Node.Caching == Caching.StoreOnDemand)
            {
                // We want to store a copy of everything we have fetched in our dictionary.
                foreach (Entry ee in (e.Message as FetchConfirmationMessage).Values)
                {
                    try
                    {
                        lock (this.p_CachedEntries)
                        {
                            this.p_CachedEntries.Add(ee.Key, ee.Value);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // The network is immutable, so any entries with the same ID must also
                        // have the same value and hence we don't care if they can't be added.
                    }
                }
            }

            if (e.Message is InvokeMessage ||
                e.Message is SetPropertyMessage ||
                e.Message is GetPropertyMessage ||
                e.Message is AddEventMessage ||
                e.Message is RemoveEventMessage ||
                e.Message is InvokeEventMessage)
            {
                // Tell the DHT to not automatically send a confirmation message (we
                // will do this ourselves as we need to attach data to it).
                e.SendConfirmation = false;

                // Handle each type of message.
                if (e.Message is InvokeMessage)
                {
                    // Tell the node to invoke the method.
                    InvokeMessage i = (e.Message as InvokeMessage);
                    object r = this.m_Node.Invoke(i.ObjectID, i.ObjectMethod, i.TypeArguments, i.Arguments);
                    InvokeConfirmationMessage icm = new InvokeConfirmationMessage(this.p_Dht, i, r);
                    try
                    {
                        icm.Send();
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                            ex.SocketErrorCode == SocketError.ConnectionReset ||
                            ex.SocketErrorCode == SocketError.TimedOut)
                            this.m_Node.Contacts.Remove(icm.Target);
                        else
                            throw;
                    }
                }
                else if (e.Message is SetPropertyMessage)
                {
                    SetPropertyMessage i = (e.Message as SetPropertyMessage);

                    // Check to see what network architecture we are using.  We need to see
                    // if we are a client in the server-client architecture receiving this
                    // message, and if so, handle it differently.
                    if (this.m_Node.Architecture == Architecture.ServerClient &&
                        this.m_Node.Caching == Caching.PushOnChange &&
                        !this.m_Node.IsServer)
                    {
                        // Get a reference to the cached object.
                        ITransparent obj = this.FetchCached(i.ObjectID) as ITransparent;

                        // Get our cached copy of the object.
                        if (obj == null)
                        {
                            // We don't yet have a cached copy of this object.  Request a complete
                            // copy of the data that the server has.
                            obj = this.Fetch(i.ObjectID) as ITransparent;
                            if (obj == null) return;
                            try
                            {
                                this.InspectorLog("Fetched complete copy of '" + i.ObjectID + "' for initial caching.", "updated_cache");
                                lock (this.p_CachedEntries)
                                {
                                    this.p_CachedEntries.Add(ID.NewHash(i.ObjectID), obj);
                                }
                            }
                            catch (ArgumentException) { }

                            // We don't need to call the setter since the cached value we just got
                            // will contain the new value anyway.
                        }
                        else
                        {
                            // Invoke the setter.
                            this.InspectorLog("Updating cached property '" + i.ObjectProperty + "' for '" + i.ObjectID + "'.", "updated_cache");
                            MethodInfo mi = obj.GetType().GetMethod("set_" + i.ObjectProperty + "__Distributed0", BindingFlagsCombined.All);
                            if (mi == null)
                                throw new MissingMethodException(obj.GetType().FullName, "set_" + i.ObjectProperty + "__Distributed0");
                            DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { i.NewValue });
                            //mi.Invoke(obj, new object[] { i.NewValue });
                        }

                        // The server doesn't care whether we got the message.
                        e.SendConfirmation = false;
                    }
                    // We also need to handle StoreOnDemand mode.
                    else if (this.m_Node.Caching == Caching.StoreOnDemand)
                    {
                        // Get a reference to the cached object.
                        ITransparent obj = this.FetchCached(i.ObjectID) as ITransparent;

                        // Get our cached copy of the object.
                        if (obj == null)
                        {
                            // We don't yet have a cached copy of this object.  Request a complete
                            // copy of the data that the server has.
                            obj = this.Fetch(i.ObjectID) as ITransparent;
                            if (obj == null) return;
                            try
                            {
                                lock (this.p_CachedEntries)
                                {
                                    this.p_CachedEntries.Add(ID.NewHash(i.ObjectID), obj);
                                }
                            }
                            catch (ArgumentException) { }

                            // We don't need to call the setter since the cached value we just got
                            // will contain the new value anyway.
                        }
                        else
                        {
                            // Check to see if we are in StoreOnDemand mode.
                            if (this.m_Node.Caching == Caching.StoreOnDemand)
                                throw new InvalidOperationException("Unable to set properties on immutable classes (caching is StoreOnDemand).");
                        }

                        // The server doesn't care whether we got the message.
                        e.SendConfirmation = false;
                    }
                    else
                    {
                        // Tell the node to set the property.
                        this.m_Node.SetProperty(i.ObjectID, i.ObjectProperty, i.NewValue);

                        // We get the DHT to send a generic confirmation message for this.
                        e.SendConfirmation = true;
                    }
                }
                else if (e.Message is GetPropertyMessage)
                {
                    // Tell the node to get the property.
                    GetPropertyMessage i = (e.Message as GetPropertyMessage);
                    object r = this.m_Node.GetProperty(i.ObjectID, i.ObjectProperty);
                    GetPropertyConfirmationMessage gpcm = new GetPropertyConfirmationMessage(this.p_Dht, i, r);
                    try
                    {
                        gpcm.Send();
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                            ex.SocketErrorCode == SocketError.ConnectionReset ||
                            ex.SocketErrorCode == SocketError.TimedOut)
                            this.m_Node.Contacts.Remove(gpcm.Target);
                        else
                            throw;
                    }
                }
                else if (e.Message is AddEventMessage)
                {
                    // Tell the node to add the event.
                    AddEventMessage i = (e.Message as AddEventMessage);
                    this.m_Node.AddEvent(i.EventTransport);

                    // We get the DHT to send a generic confirmation message for this.
                    e.SendConfirmation = true;
                }
                else if (e.Message is RemoveEventMessage)
                {
                    // Tell the node to remove the event.
                    RemoveEventMessage i = (e.Message as RemoveEventMessage);
                    this.m_Node.RemoveEvent(i.EventTransport);

                    // We get the DHT to send a generic confirmation message for this.
                    e.SendConfirmation = true;
                }
                else if (e.Message is InvokeEventMessage)
                {
                    // Tell the node to invoke the event.
                    InvokeEventMessage i = (e.Message as InvokeEventMessage);
                    this.m_Node.InvokeEvent(i.EventTransport, i.EventSender, i.EventArgs);

                    // We get the DHT to send a generic confirmation message for this.
                    e.SendConfirmation = true;
                }
            }
        }

        /// <summary>
        /// Handles providing any cached entries to the DHT when operating in StoreOnDemand
        /// mode (since we need to pass along entries which we didn't originally own).
        /// </summary>
        void p_Dht_OnEntriesRequested(object sender, EntriesRequestedEventArgs e)
        {
            lock (this.p_CachedEntries)
            {
                foreach (KeyValuePair<ID, object> kv in this.p_CachedEntries)
                    e.Entries.Add(kv.Key, kv.Value);
            }
        }

        #region IContactProvider Members

        public bool StorageStartRequired
        {
            get { return true; }
        }

        public void Add(Contact contact)
        {
            this.InspectorLog("Computer at endpoint " + contact.EndPoint + " has joined the network.", "contact");
            this.p_Dht.Contacts.Add(contact);
        }

        public void Remove(Contact contact)
        {
            this.InspectorLog("Computer at endpoint " + contact.EndPoint + " has left / disconnected from the network.", "contact");
            this.p_Dht.Contacts.Remove(contact);
        }

        public void Clear()
        {
            this.InspectorLog("All known contacts were cleared.", "contact");
            this.p_Dht.Contacts.Clear();
        }

        #endregion

        #region IEnumerable<Contact> Members

        public IEnumerator<Contact> GetEnumerator()
        {
            return this.p_Dht.Contacts.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this.p_Dht.Contacts as System.Collections.IEnumerable).GetEnumerator();
        }

        #endregion

        #region IStorageProvider Members

        public void SetProperty(string id, string property, object value)
        {
            if (this.m_Node.Architecture == Architecture.PeerToPeer)
            {
                // Fetch the owner of the specified entry.
                Contact owner = this.FetchOwner(id);
                if (owner == null)
                {
                    this.InspectorLog("Object '" + id + "' has vanished from the network.", "object_vanished");
                    throw new ObjectVanishedException(id);
                }
                else
                    this.InspectorLog("Retrieved owner of object '" + id + "' from network.", "retrieved_success");

                // Check to see if we own the property.
                if (owner.Identifier == this.m_Node.ID)
                {
                    // Invoke what would have been the delegate passed to DpmEntrypoint::SetProperty directly.
                    ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
                    if (obj == null)
                    {
                        this.InspectorLog("Object '" + id + "' has vanished from the network.", "object_vanished");
                        throw new ObjectVanishedException(id);
                    }
                    else
                        this.InspectorLog("Retrieved transparent proxy to object '" + id + "' from network.", "retrieved_success");

                    // Check to see if we are in StoreOnDemand mode.
                    if (this.m_Node.Caching == Caching.StoreOnDemand && obj.IsImmutablyPushed)
                        throw new InvalidOperationException("Unable to set properties on immutable classes (caching is StoreOnDemand).");

                    MethodInfo mi = obj.GetType().GetMethod("set_" + property + "__Distributed0", BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "set_" + property + "__Distributed0");
                    DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { value });
                    //mi.Invoke(obj, new object[] { value });

                    // Now also synchronise the object with the DHT.
                    if (obj.GetType().GetMethod("set_" + property, BindingFlagsCombined.All).GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() != 0)
                        LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);
                }
                else
                {
                    // Check to see if we are in StoreOnDemand mode.
                    if (this.m_Node.Caching == Caching.StoreOnDemand)
                        throw new InvalidOperationException("Unable to set properties on immutable classes (caching is StoreOnDemand).");

                    // Invoke the property setter remotely.
                    RemoteNode rnode = new RemoteNode(owner);
                    rnode.SetProperty(id, property, value);
                }
            }
            else if (this.m_Node.Architecture == Architecture.ServerClient)
            {
                // Only the server is permitted to set properties.
                if (!this.m_Node.IsServer)
                    throw new MemberAccessException("Only servers may set the '" + property + "' property.");

                // Check to see if we are in StoreOnDemand mode.
                if (this.m_Node.Caching == Caching.StoreOnDemand)
                    throw new InvalidOperationException("Unable to set properties on immutable classes (caching is StoreOnDemand).");

                // Get the object directly from the owned entries (this is much
                // faster than asking the entire network).
                ITransparent obj = this.FetchLocal(id) as ITransparent;
                if (obj == null)
                {
                    this.InspectorLog("Object '" + id + "' has vanished from the local machine.", "object_vanished");
                    throw new ObjectVanishedException(id);
                }
                
                // Invoke the setter.
                MethodInfo mi = obj.GetType().GetMethod("set_" + property + "__Distributed0", BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, "set_" + property + "__Distributed0");
                DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { value });
                //mi.Invoke(obj, new object[] { value });

                // If this is an auto-generated property we need to synchronise the DHT and
                // then tell all of the clients the new value.
                if (obj.GetType().GetMethod("set_" + property, BindingFlagsCombined.All).GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() != 0)
                {
                    // Synchronise the object with the DHT.
                    LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);

                    // Check to see if we need to alert all of the clients of the new
                    // value.
                    if (this.m_Node.Caching == Caching.PushOnChange)
                    {
                        // Send a SetProperty message to every client in the network.
                        foreach (Contact c in this.m_Node.Contacts.ToArray())
                        {
                            if (c.Identifier != this.m_Node.ID)
                            {
                                try
                                {
                                    if (id.StartsWith("autoid-"))
                                        this.InspectorLog("Sending updated property '" + property + "' for automatic object to remote contact at endpoint " + c.EndPoint + ".", "sent");
                                    else
                                        this.InspectorLog("Sending updated property '" + property + "' for named object '" + id + "' to remote contact at endpoint " + c.EndPoint + ".", "sent");
                                    SetPropertyMessage spm = new SetPropertyMessage(this.Dht, c, id, property, value);
                                    spm.Send();
                                }
                                catch (SocketException ex)
                                {
                                    if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                        ex.SocketErrorCode == SocketError.ConnectionReset ||
                                        ex.SocketErrorCode == SocketError.TimedOut)
                                        this.m_Node.Contacts.Remove(c);
                                }
                            }
                        }
                    }
                }
            }
            else
                throw new NotSupportedException("Unsupported network architecture detected.");
        }

        public object GetProperty(string id, string property)
        {
            if (this.m_Node.Architecture == Architecture.PeerToPeer)
            {
                // Fetch the owner of the specified entry.
                Contact owner = this.FetchOwner(id);
                if (owner == null) throw new ObjectVanishedException(id);

                // Check to see if we own the property.
                if (owner.Identifier == this.m_Node.ID || this.m_Node.Caching == Caching.StoreOnDemand)
                {
                    // Invoke what would have been the delegate passed to DpmEntrypoint::GetProperty directly.
                    ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
                    if (obj == null) throw new ObjectVanishedException(id);

                    MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                    object r = DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { });
                    //object r = mi.Invoke(obj, new object[] { });
                    return r;
                }
                else
                {
                    // Invoke the property getter remotely.
                    RemoteNode rnode = new RemoteNode(owner);
                    object r = rnode.GetProperty(id, property);
                    return r;
                }
            }
            else if (this.m_Node.Architecture == Architecture.ServerClient)
            {
                // The server will always call the getter directly.
                if (this.m_Node.IsServer)
                {
                    // Get the object directly from the owned entries (this is much
                    // faster than asking the entire network).
                    ITransparent obj = this.FetchLocal(id) as ITransparent;
                    if (obj == null) throw new ObjectVanishedException(id);

                    // Invoke the getter.
                    MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                    object r = DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { });
                    //object r = mi.Invoke(obj, new object[] { });
                    return r;
                }
                else
                {
                    // If we are using PushOnChange then the client can also use the
                    // getter directly (since the field will have the correct cached
                    // value in it).
                    if (this.m_Node.Caching == Caching.PushOnChange)
                    {
                        // Get the object directly from the owned entries (this is much
                        // faster than asking the entire network).
                        ITransparent obj = this.FetchCached(id) as ITransparent;
                        if (obj == null)
                        {
                            obj = this.Fetch(id) as ITransparent;
                            if (obj == null) throw new ObjectVanishedException(id);
                            lock (this.p_CachedEntries)
                            {
                                this.p_CachedEntries.Add(ID.NewHash(id), obj);
                            }
                        } 

                        // Invoke the getter.
                        MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlagsCombined.All);
                        if (mi == null)
                            throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                        object r = DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { });
                        //object r = mi.Invoke(obj, new object[] { });
                        return r;
                    }
                    else if (this.m_Node.Caching == Caching.PullOnDemand)
                    {
                        // Fetch the entry that would be returned by Storage.Fetch (since we
                        // need the contact information).
                        Contact owner = this.FetchOwner(id);
                        if (owner == null) throw new ObjectVanishedException(id);

                        // Invoke the property getter remotely.
                        RemoteNode rnode = new RemoteNode(owner);
                        object r = rnode.GetProperty(id, property);
                        return r;
                    }
                    else
                        throw new NotSupportedException("Unsupported caching mode detected.");
                }
            }
            else
                throw new NotSupportedException("Unsupported network architecture detected.");
        }

        public void Store(string id, object o)
        {
            if (this.FetchLocal(id) == null)
            {
                if (id.StartsWith("autoid-"))
                    this.InspectorLog("A new automatic object was stored in the network.", "object_new");
                else
                    this.InspectorLog("A new named object with ID '" + id + "' was stored in the network.", "object_new");
            }
            this.Dht.UpdateOrPut(ID.NewHash(id), o);
        }

        public object Fetch(string id)
        {
            Entry e = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
            if (e == null) return null;
            return e.Value;
        }

        public object FetchLocal(string idh)
        {
            ID id = ID.NewHash(idh);
            Entry e = this.Dht.OwnedEntries.ToArray().Where(value => value.Key == id).DefaultIfEmpty(null).First();
            if (e == null) return null;
            return e.Value;
        }

        public object FetchCached(string idh)
        {
            ID id = ID.NewHash(idh);
            lock (this.p_CachedEntries)
            {
                foreach (KeyValuePair<ID, object> kv in this.p_CachedEntries)
                    if (kv.Key == id)
                        return kv.Value;
            }
            return null;
        }

        public Contact FetchOwner(string id)
        {
            Entry e = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
            if (e == null) return null;
            return e.Owner;
        }

        #endregion

        /// <summary>
        /// Internal access to the underlying Dht.
        /// </summary>
        internal Dht Dht
        {
            get { return this.p_Dht; }
        }
    }
}
