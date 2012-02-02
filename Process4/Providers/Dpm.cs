using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using System.Runtime.CompilerServices;
using System.Net;
using Data4;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;
using Process4.Collections;
using Process4.Remoting;
using Process4.Attributes;

namespace Process4.Providers
{
    internal class Dpm : IProcessorProvider
    {
        private LocalNode m_Node = null;

        /// <summary>
        /// The DHT in use by the associated node.
        /// </summary>
        private Dht Dht
        {
            get
            {
                if (this.m_Node.Storage is DhtWrapper)
                    return (this.m_Node.Storage as DhtWrapper).Dht;
                else
                    throw new InvalidOperationException("The default processing provider can only be used in conjunction with the default storage provider.");
            }
        }

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
        public Dpm(LocalNode node)
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
            if (owner == null) throw new ObjectVanishedException(transport.SourceObjectNetworkName);

            // Check to see if we own the property.
            if (owner.Identifier == this.m_Node.ID)
            {
                // Get object that the event handler resides on.
                ITransparent obj = this.m_Node.Storage.Fetch(transport.SourceObjectNetworkName) as ITransparent;
                if (obj == null) throw new ObjectVanishedException(transport.SourceObjectNetworkName);

                // Get a reference to the event adder.
                MethodInfo mi = obj.GetType().GetMethod("add_" + transport.SourceEventName + "__Distributed0", BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, transport.SourceEventName);

                // Create an EventHandler that will automatically remote the event callback
                // across the network to the node that originally registered it.
                EventHandler handler = transport.CreateRemotedDelegate();

                // Invoke the event adder.
                mi.Invoke(obj, new object[] { handler });

                // Now also synchronise the object with the DHT.
                if (obj.GetType().GetMethod("add_" + transport.SourceEventName, BindingFlagsCombined.All).GetMethodImplementationFlags() == MethodImplAttributes.Synchronized)
                    LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);
            }
            else
            {
                // Invoke the event adder remotely.
                RemoteNode rnode = new RemoteNode(owner);
                rnode.AddEvent(transport);
            }
        }

        public void RemoveEvent(EventTransport transport)
        {
            // Fetch the entry that would be returned by Storage.Fetch (since we
            // need the contact information).
            Contact owner = this.m_Node.Storage.FetchOwner(transport.SourceObjectNetworkName);
            if (owner == null) throw new ObjectVanishedException(transport.SourceObjectNetworkName);

            // Check to see if we own the property.
            if (owner.Identifier == this.m_Node.ID)
            {
                // Get object that the event handler resides on.
                ITransparent obj = this.m_Node.Storage.Fetch(transport.SourceObjectNetworkName) as ITransparent;
                if (obj == null) throw new ObjectVanishedException(transport.SourceObjectNetworkName);

                // Get a reference to the event adder.
                MethodInfo mi = obj.GetType().GetMethod("remove_" + transport.SourceEventName + "__Distributed0", BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, transport.SourceEventName);

                // Create an EventHandler that will automatically remote the event callback
                // across the network to the node that originally registered it.
                EventHandler handler = transport.CreateRemotedDelegate();

                // Invoke the event adder.
                mi.Invoke(obj, new object[] { handler });

                // Now also synchronise the object with the DHT.
                if (obj.GetType().GetMethod("remove_" + transport.SourceEventName, BindingFlagsCombined.All).GetMethodImplementationFlags() == MethodImplAttributes.Synchronized)
                    LocalNode.Singleton.Storage.Store(obj.NetworkName, obj);
            }
            else
            {
                // Invoke the event adder remotely.
                RemoteNode rnode = new RemoteNode(owner);
                rnode.RemoveEvent(transport);
            }
        }

        public void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            // Get the object based on the agreed reference.
            KeyValuePair<ID, object> kv = this.AgreedReferences.FirstOrDefault(value => value.Key == transport.ListenerAgreedReference);
            object obj = (object.ReferenceEquals(kv, null)) ? null : kv.Value;
            if (obj == null) throw new ObjectVanishedException(transport.SourceObjectNetworkName);

            // Invoke the target method.
            MethodInfo mi = obj.GetType().GetMethod(transport.ListenerMethod, BindingFlagsCombined.All);
            if (mi == null)
                throw new MissingMethodException(obj.GetType().FullName, transport.ListenerMethod);
            mi.Invoke(obj, new object[] { sender, e });
        }

        public object Invoke(string id, string method, object[] args)
        {
            ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
            if (obj == null) throw new ObjectVanishedException(id);

            if (this.m_Node.Architecture == Architecture.PeerToPeer)
            {
                // In peer-to-peer modes, methods are always invoked locally.
                MethodInfo mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, method);
                return mi.Invoke(obj, args);
            }
            else if (this.m_Node.Architecture == Architecture.ServerClient)
            {
                if (this.m_Node.IsServer)
                {
                    // The server is always permitted to call methods.
                    MethodInfo mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    return mi.Invoke(obj, args);
                }
                else
                {
                    // We must see if the client is permitted to call the specified method.
                    MethodInfo mi = obj.GetType().GetMethod(method.Substring(0, method.IndexOf("__Distributed0")), BindingFlagsCombined.All);
                    if (mi == null)
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    if (mi.GetCustomAttributes(typeof(ClientCallableAttribute), false).Count() == 0)
                        throw new MemberAccessException("The method '" + method + "' is not accessible to client machines.");

                    // If we get to here, then we're permitted to call the method, but we still need
                    // to remote it to the server.
                    Contact owner = this.m_Node.Storage.FetchOwner(id);
                    if (owner == null) throw new ObjectVanishedException(id);
                    RemoteNode rnode = new RemoteNode(owner);
                    object r = rnode.Invoke(id, method, args);
                    return r;
                }
            }
            else
                throw new NotSupportedException("Unsupported network architecture detected.");
        }

        public DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback)
        {
            if (this.m_Node.Architecture == Architecture.PeerToPeer)
            {
                DTask<object> task = new DTask<object>();
                ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
                if (obj == null) throw new ObjectVanishedException(id);

                MethodInfo mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                if (mi == null)
                    throw new MissingMethodException(obj.GetType().FullName, method);
                new Thread(() =>
                {
                    task.Value = mi.Invoke(obj, args);
                    task.Completed = true;
                    callback.DynamicInvoke(null);
                }).Start();
                return task;
            }
            else
                throw new NotSupportedException("Asynchronous invocation of methods is not supported in network architectures other than peer-to-peer.");
        }

        #endregion
    }

    /// <summary>
    /// Provides entry points into the library when methods or property getters / setters
    /// are invoked.
    /// </summary>
    public static class DpmEntrypoint
    {
        public static object InvokeDynamic(Delegate d, object[] args)
        {
            Type[] tparams = d.Method.DeclaringType.GetGenericArguments()
                                .Concat(d.Method.GetGenericArguments())
                                .ToArray();
            Type dt = d.GetType().DeclaringType;
            if (dt.ContainsGenericParameters)
                dt = dt.MakeGenericType(tparams);
            IDirectInvoke di = dt.GetConstructor(Type.EmptyTypes).Invoke(null) as IDirectInvoke;
            object o = di.Invoke(d.Method, d.Target, args);
            return o;
        }

        public static object SetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            if (LocalNode.Singleton == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the set_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the set property.
            LocalNode.Singleton.SetProperty(objectName, propertyName, args[0]);

            return null;
        }

        public static object GetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            if (LocalNode.Singleton == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the get_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the get property.
            return LocalNode.Singleton.GetProperty(objectName, propertyName);
        }

        public static object AddEvent(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            if (LocalNode.Singleton == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the add_ prefix and Distributed suffix.
            string eventName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);
            Delegate handler = args[0] as Delegate;
            ID agreedref = null;
            if (handler.Target != null)
                agreedref = EventTransport.GetAgreedReference(LocalNode.Singleton.Processor.AgreedReferences, handler.Target);

            // Construct the event transport information.
            EventTransport ev = new EventTransport
            {
                SourceObjectNetworkName = objectName,
                SourceEventName = eventName,
                SourceEventType = handler.Method.GetParameters()[1].ParameterType.FullName,
                ListenerAgreedReference = agreedref,
                ListenerNodeID = LocalNode.Singleton.ID,
                ListenerType = handler.Method.DeclaringType.FullName,
                ListenerMethod = handler.Method.Name
            };

            // Get our local node and invoke the add event.
            LocalNode.Singleton.AddEvent(ev);

            return null;
        }

        public static object RemoveEvent(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            if (LocalNode.Singleton == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the remove_ prefix and Distributed suffix.
            string eventName = d.Method.Name.Substring(7, d.Method.Name.LastIndexOf("__Distributed") - 7);
            Delegate handler = args[0] as Delegate;
            ID agreedref = null;
            if (handler.Target != null)
                agreedref = EventTransport.GetAgreedReference(LocalNode.Singleton.Processor.AgreedReferences, handler);

            // Construct the event transport information.
            EventTransport ev = new EventTransport
            {
                SourceObjectNetworkName = objectName,
                SourceEventName = eventName,
                SourceEventType = handler.Method.GetParameters()[1].ParameterType.FullName,
                ListenerAgreedReference = agreedref,
                ListenerNodeID = LocalNode.Singleton.ID,
                ListenerType = handler.Method.DeclaringType.FullName,
                ListenerMethod = handler.Method.Name
            };

            // Get our local node and invoke the remove event.
            LocalNode.Singleton.RemoveEvent(ev);

            return null;
        }

        public static object Invoke(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            if (LocalNode.Singleton == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;
            string methodName = d.Method.Name;

            // Get our local node and invoke the method.
            object o = LocalNode.Singleton.Invoke(objectName, methodName, args);
            return o;
        }

        public static void Construct(object obj)
        {
            // Skip if not networked.
            if (LocalNode.Singleton == null)
                return;

            // Check to see if we've already got a NetworkName; if we have
            // then we're a named distributed instance that doesn't need the
            // autoid assigned.
            if ((obj as ITransparent).NetworkName != null)
                return;

            // Allocate a randomly generated NetworkName.
            (obj as ITransparent).NetworkName = "autoid-" + ID.NewRandom().ToString();

            // Store the object in the Dht.
            LocalNode.Singleton.Storage.Store((obj as ITransparent).NetworkName, obj);
        }

        public static void Serialize(object obj, SerializationInfo info, StreamingContext context)
        {
            // Loop through all of the private, internal fields in the object and
            // serialize all of their values.
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlagsCombined.All))
            {
                if (fi.GetCustomAttributes(typeof(LocalAttribute), true).Length > 0)
                    continue;
                if (fi.FieldType.GetInterface("ITransparent") != null)
                {
                    // Just serialize the NetworkName instead of the whole object.
                    if (fi.GetValue(obj) != null)
                        info.AddValue(fi.Name, (fi.GetValue(obj) as ITransparent).NetworkName);
                    else
                        info.AddValue(fi.Name, null);
                }
                else
                {
                    // Serialize the value itself.
                    info.AddValue(fi.Name, fi.GetValue(obj), fi.FieldType);
                }
            }
        }

        public static void Deserialize(object obj, SerializationInfo info, StreamingContext context)
        {
            // Loop through all of the private, internal fields in the object and
            // set their values based on the deserialized information.
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlagsCombined.All))
            {
                if (fi.GetCustomAttributes(typeof(LocalAttribute), true).Length > 0)
                    continue;
                if (fi.FieldType.GetInterface("ITransparent") != null)
                {
                    // Deserialize the object based on the NetworkName.
                    if (info.GetValue(fi.Name, typeof(object)) != null)
                    {
                        object v = FormatterServices.GetUninitializedObject(fi.FieldType);
                        (v as ITransparent).NetworkName = info.GetString(fi.Name);
                        fi.SetValue(obj, v);
                    }
                    else
                        fi.SetValue(obj, null);
                }
                else
                {
                    // Deserialize the value itself.
                    fi.SetValue(obj, info.GetValue(fi.Name, fi.FieldType));
                }
            }
        }
    }
}
