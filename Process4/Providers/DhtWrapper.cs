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

namespace Process4.Providers
{
    internal class DhtWrapper : IStorageProvider, IContactProvider
    {
        private Dht p_Dht = null;
        private LocalNode m_Node = null;
        private List<Entry> p_CachedEntries = new List<Entry>();

        internal DhtWrapper(LocalNode node)
        {
            // Store our reference to our node so we can use the IProcessorProvider
            // later on.
            this.m_Node = node;
        }

        /// <summary>
        /// Starts the DHT.
        /// </summary>
        public void Start()
        {
            // Creates our new DHT node with the specified ID and endpoint.
            this.p_Dht = new Dht(this.m_Node.ID, new IPEndPoint(this.m_Node.Network.IPAddress, 18000));

            // Register the message listener on the DHT which we will use to
            // detect InvokeMessage, SetPropertyMessage and GetPropertyMessage.
            this.p_Dht.OnReceived += new EventHandler<MessageEventArgs>(m_Dht_OnReceived);
        }

        /// <summary>
        /// Stops the DHT.
        /// </summary>
        public void Stop()
        {
            this.p_Dht.Close();
        }

        /// <summary>
        /// This event is raised when the DHT has received a message and we should
        /// check to see if it is any of the types we want to handle.
        /// </summary>
        void m_Dht_OnReceived(object sender, MessageEventArgs e)
        {
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
                    object r = this.m_Node.Invoke(i.ObjectID, i.ObjectMethod, i.Arguments);
                    InvokeConfirmationMessage icm = new InvokeConfirmationMessage(this.p_Dht, i, r);
                    icm.Send();
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
                        // Get our cached copy of the object.
                        IEnumerable<Entry> results = this.p_CachedEntries.Where(value => value.Key == ID.NewHash(i.ObjectID));
                        if (results.Count() == 0)
                        {
                            // We don't yet have a cached copy of this object.  Request a complete
                            // copy of the data that the server has.
                            Entry entry = this.Dht.Get(ID.NewHash(i.ObjectID)).DefaultIfEmpty(null).First();
                            if (entry == null) return;
                            this.p_CachedEntries.Add(entry);

                            // We don't need to call the setter since the cached value we just got
                            // will contain the new value anyway.
                        }
                        else
                        {
                            // Call the setter on the cached object.
                            ITransparent obj = this.FetchCached(i.ObjectID) as ITransparent;

                            // Invoke the setter.
                            MethodInfo mi = obj.GetType().GetMethod("set_" + i.ObjectProperty + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (mi == null)
                                throw new MissingMethodException(obj.GetType().FullName, "set_" + i.ObjectProperty + "__Distributed0");
                            mi.Invoke(obj, new object[] { i.NewValue });
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
                    gpcm.Send();
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

        #region IContactProvider Members

        public bool StorageStartRequired
        {
            get { return true; }
        }

        public void Add(Contact contact)
        {
            this.p_Dht.Contacts.Add(contact);
        }

        public void Remove(Contact contact)
        {
            this.p_Dht.Contacts.Remove(contact);
        }

        public void Clear()
        {
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
                // Fetch the entry that would be returned by Storage.Fetch (since we
                // need the contact information).
                Entry o = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (o == null) throw new ObjectVanishedException(id);

                // Check to see if we own the property.
                if (o.Owner.Identifier == this.m_Node.ID)
                {
                    // Invoke what would have been the delegate passed to DpmEntrypoint::SetProperty directly.
                    ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
                    if (obj == null) throw new ObjectVanishedException(id);

                    MethodInfo mi = obj.GetType().GetMethod("set_" + property + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "set_" + property + "__Distributed0");
                    mi.Invoke(obj, new object[] { value });

                    // Now also synchronise the object with the DHT.
                    if (obj.GetType().GetMethod("set_" + property, BindingFlags.NonPublic | BindingFlags.Instance) != null)
                        LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);
                }
                else
                {
                    // Invoke the property setter remotely.
                    RemoteNode rnode = new RemoteNode(o.Owner);
                    rnode.SetProperty(id, property, value);
                }
            }
            else if (this.m_Node.Architecture == Architecture.ServerClient)
            {
                // Only the server is permitted to set properties.
                if (!this.m_Node.IsServer)
                    throw new MemberAccessException("Only servers may set the '" + property + "' property.");

                // Get the object directly from the owned entries (this is much
                // faster than asking the entire network).
                ITransparent obj = this.FetchLocal(id) as ITransparent;
                if (obj == null) throw new ObjectVanishedException(id);
                
                // Invoke the setter.
                MethodInfo mi = obj.GetType().GetMethod("set_" + property + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, "set_" + property + "__Distributed0");
                mi.Invoke(obj, new object[] { value });

                // Now also synchronise the object with the DHT if it's an auto-generated property.
                if (obj.GetType().GetMethod("set_" + property, BindingFlags.NonPublic | BindingFlags.Instance) != null)
                    LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);

                // Check to see if we need to alert all of the clients of the new
                // value.
                if (this.m_Node.Caching == Caching.PushOnChange)
                {
                    // Send a SetProperty message to every client in the network.
                    foreach (Contact c in this.m_Node.Contacts)
                    {
                        if (c.Identifier != this.m_Node.ID)
                        {
                            SetPropertyMessage spm = new SetPropertyMessage(this.Dht, c, id, property, value);
                            spm.Send();
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
                // Fetch the entry that would be returned by Storage.Fetch (since we
                // need the contact information).
                Entry o = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (o == null) throw new ObjectVanishedException(id);

                // Check to see if we own the property.
                if (o.Owner.Identifier == this.m_Node.ID)
                {
                    // Invoke what would have been the delegate passed to DpmEntrypoint::SetProperty directly.
                    ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
                    if (obj == null) throw new ObjectVanishedException(id);

                    MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                    object r = mi.Invoke(obj, new object[] { });
                    return r;
                }
                else
                {
                    // Invoke the property getter remotely.
                    RemoteNode rnode = new RemoteNode(o.Owner);
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
                    MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                    object r = mi.Invoke(obj, new object[] { });
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
                        ITransparent obj = this.FetchLocal(id) as ITransparent;
                        if (obj == null) throw new ObjectVanishedException(id);

                        // Invoke the getter.
                        MethodInfo mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mi == null)
                            throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
                        object r = mi.Invoke(obj, new object[] { });
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
            using (MemoryStream stream = new MemoryStream())
            {
                StreamingContext old = this.Dht.Formatter.Context;
                this.Dht.Formatter.Context = new StreamingContext(this.Dht.Formatter.Context.State, new SerializationData { Storage = this, Root = o });
                this.Dht.Formatter.Serialize(stream, o);
                this.Dht.Formatter.Context = old;
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, bytes.Length);
                this.Dht.Remove(ID.NewHash(id));
                this.Dht.Put(ID.NewHash(id), Convert.ToBase64String(bytes));
            }
        }

        public object Fetch(string id)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Entry e = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (e == null) return null;
                byte[] b = Convert.FromBase64String(e.Value);
                stream.Write(b, 0, b.Length);
                stream.Position = 0;
                StreamingContext old = this.Dht.Formatter.Context;
                this.Dht.Formatter.Context = new StreamingContext(this.Dht.Formatter.Context.State, new SerializationData { Storage = this, Entry = e });
                object r = this.Dht.Formatter.Deserialize(stream);
                this.Dht.Formatter.Context = old;
                return r;
            }
        }

        public object FetchLocal(string id)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Entry e = this.Dht.OwnedEntries.Where(value => value.Key == ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (e == null) return null;
                byte[] b = Convert.FromBase64String(e.Value);
                stream.Write(b, 0, b.Length);
                stream.Position = 0;
                StreamingContext old = this.Dht.Formatter.Context;
                this.Dht.Formatter.Context = new StreamingContext(this.Dht.Formatter.Context.State, new SerializationData { Storage = this, Entry = e });
                object r = this.Dht.Formatter.Deserialize(stream);
                this.Dht.Formatter.Context = old;
                return r;
            }
        }

        public object FetchCached(string id)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Entry e = this.p_CachedEntries.Where(value => value.Key == ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (e == null) return null;
                byte[] b = Convert.FromBase64String(e.Value);
                stream.Write(b, 0, b.Length);
                stream.Position = 0;
                StreamingContext old = this.Dht.Formatter.Context;
                this.Dht.Formatter.Context = new StreamingContext(this.Dht.Formatter.Context.State, new SerializationData { Storage = this, Entry = e });
                object r = this.Dht.Formatter.Deserialize(stream);
                this.Dht.Formatter.Context = old;
                return r;
            }
        }

        public Contact FetchOwner(string id)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Entry e = this.Dht.Get(ID.NewHash(id)).DefaultIfEmpty(null).First();
                if (e == null) return null;
                return e.Owner;
            }
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
