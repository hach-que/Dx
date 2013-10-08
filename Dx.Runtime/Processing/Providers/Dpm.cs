using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dx.Runtime
{
    internal class Dpm : IProcessorProvider
    {
        private ILocalNode m_Node = null;

        /// <summary>
        /// A list of "agreed references"; that is IDs set for external or
        /// non-distributed objects.  Used by the event system.
        /// </summary>
        public Dictionary<ID, object> AgreedReferences
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new Distributed Processing Module associated with the
        /// specified node.
        /// </summary>
        /// <param name="node">The local node to associate with.</param>
        public Dpm(ILocalNode node)
        {            
            this.m_Node = node;
            this.AgreedReferences = new Dictionary<ID, object>();
        }

        /// <summary>
        /// Starts the DPM.
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Stops the DPM.
        /// </summary>
        public void Stop()
        {
        }

        #region IProcessorProvider Members

        public void AddEvent(EventTransport transport)
        {
            // Fetch the entry that would be returned by Storage.Fetch (since we
            // need the contact information).
            Contact owner = this.m_Node.Storage.FetchOwner(transport.SourceObjectNetworkName);
            if (owner == null)
            {
                throw new ObjectVanishedException(transport.SourceObjectNetworkName);
            }
            
            // Check to see if we own the property.
            if (owner.Identifier == this.m_Node.ID)
            {
                // Get object that the event handler resides on.
                ITransparent obj = this.m_Node.Storage.Fetch(transport.SourceObjectNetworkName) as ITransparent;
                if (obj == null)
                {
                    throw new ObjectVanishedException(transport.SourceObjectNetworkName);
                }
                
                // Get a reference to the event adder.
                MethodInfo mi = obj.GetType().GetMethod("add_" + transport.SourceEventName + "__Distributed0", BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, transport.SourceEventName);

                // Create an EventHandler that will automatically remote the event callback
                // across the network to the node that originally registered it.
                EventHandler handler = transport.CreateRemotedDelegate();

                // Invoke the event adder.
                DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { handler });

                // Now also synchronise the object with the DHT.
                if (obj.GetType().GetMethod("add_" + transport.SourceEventName, BindingFlagsCombined.All).GetMethodImplementationFlags() == MethodImplAttributes.Synchronized)
                    this.m_Node.Storage.Store(obj.NetworkName, obj);
            }
            else
            {
                // Invoke the event adder remotely.
                RemoteNode rnode = new RemoteNode(this.m_Node, owner);
                rnode.AddEvent(transport);
            }
        }

        public void RemoveEvent(EventTransport transport)
        {
            // Fetch the entry that would be returned by Storage.Fetch (since we
            // need the contact information).
            Contact owner = this.m_Node.Storage.FetchOwner(transport.SourceObjectNetworkName);
            if (owner == null)
            {
                throw new ObjectVanishedException(transport.SourceObjectNetworkName);
            }
            
            // Check to see if we own the property.
            if (owner.Identifier == this.m_Node.ID)
            {
                // Get object that the event handler resides on.
                ITransparent obj = this.m_Node.Storage.Fetch(transport.SourceObjectNetworkName) as ITransparent;
                if (obj == null)
                {
                    throw new ObjectVanishedException(transport.SourceObjectNetworkName);
                }
                
                // Get a reference to the event adder.
                MethodInfo mi = obj.GetType().GetMethod("remove_" + transport.SourceEventName + "__Distributed0", BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, transport.SourceEventName);

                // Create an EventHandler that will automatically remote the event callback
                // across the network to the node that originally registered it.
                EventHandler handler = transport.CreateRemotedDelegate();

                // Invoke the event adder.
                DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { handler });

                // Now also synchronise the object with the DHT.
                if (obj.GetType().GetMethod("remove_" + transport.SourceEventName, BindingFlagsCombined.All).GetMethodImplementationFlags() == MethodImplAttributes.Synchronized)
                    this.m_Node.Storage.Store(obj.NetworkName, obj);
            }
            else
            {
                // Invoke the event adder remotely.
                RemoteNode rnode = new RemoteNode(this.m_Node, owner);
                rnode.RemoveEvent(transport);
            }
        }

        public void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            // Get the object based on the agreed reference.
            KeyValuePair<ID, object> kv = this.AgreedReferences.FirstOrDefault(value => value.Key == transport.ListenerAgreedReference);
            object obj = (object.ReferenceEquals(kv, null)) ? null : kv.Value;
            if (obj == null)
            {
                throw new ObjectVanishedException(transport.SourceObjectNetworkName);
            }
            
            // Invoke the target method.
            MethodInfo mi = obj.GetType().GetMethod(transport.ListenerMethod, BindingFlagsCombined.All);
            if (mi == null)
                throw new MissingMethodException(obj.GetType().FullName, transport.ListenerMethod);
            DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { sender, e });
        }

        public object Invoke(string id, string method, Type[] targs, object[] args)
        {
            ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
            if (obj == null)
            {
                throw new ObjectVanishedException(id);
            }
            
            if (this.m_Node.Architecture == Architecture.PeerToPeer)
            {
                // In peer-to-peer modes, methods are always invoked locally.
                MethodInfo mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, method);
                return DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, targs, args);
            }
            else if (this.m_Node.Architecture == Architecture.ServerClient)
            {
                if (this.m_Node.IsServer)
                {
                    // The server is always permitted to call methods.
                    MethodInfo mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    return DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, targs, args);
                }
                else
                {
                    // We must see if the client is permitted to call the specified method.
                    MethodInfo mi = obj.GetType().GetMethod(method.Substring(0, method.IndexOf("__Distributed0")), BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    if (mi.GetCustomAttributes(typeof(ClientIgnorableAttribute), false).Count() != 0)
                        return null;
                    if (mi.GetCustomAttributes(typeof(ClientCallableAttribute), false).Count() == 0)
                        throw new MemberAccessException("The method '" + method + "' is not accessible to client machines.");

                    // If we get to here, then we're permitted to call the method, but we still need
                    // to remote it to the server.
                    Contact owner = this.m_Node.Storage.FetchOwner(id);
                    if (owner == null) throw new ObjectVanishedException(id);
                    RemoteNode rnode = new RemoteNode(this.m_Node, owner);
                    object r = rnode.Invoke(id, method, targs, args);
                    return r;
                }
            }
            else
                throw new NotSupportedException("Unsupported network architecture detected.");
        }

        #endregion
    }
}
