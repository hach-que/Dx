// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalNode.cs" company="Redpoint Software">
//   The MIT License (MIT)
//   
//   Copyright (c) 2013 James Rhodes
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// <summary>
//   The local node; an implementation of <see cref="ILocalNode"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;

    using Ninject;
    using Ninject.Extensions.Factory;

    /// <summary>
    /// The local node; an implementation of <see cref="ILocalNode"/>.
    /// </summary>
    /// <seealso cref="ILocalNode"/>
    public class LocalNode : ILocalNode
    {
        #region Fields

        /// <summary>
        /// The architecture of the network.
        /// </summary>
        private readonly Architecture m_Architecture;

        /// <summary>
        /// The caching mode of the network.
        /// </summary>
        private readonly Caching m_Caching;

        /// <summary>
        /// The dependency injection kernel.
        /// </summary>
        private readonly IKernel m_Kernel;

        /// <summary>
        /// Whether the local node is bound to the network.
        /// </summary>
        private bool m_Bound;

        /// <summary>
        /// The connection handler for incoming connections.
        /// </summary>
        private IConnectionHandler m_ConnectionHandler;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalNode"/> class.
        /// </summary>
        public LocalNode()
            : this(Architecture.PeerToPeer, Caching.PullOnDemand)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalNode"/> class.
        /// </summary>
        /// <param name="architecture">
        /// The architecture of the network.
        /// </param>
        /// <param name="caching">
        /// The caching mode of the network.
        /// </param>
        public LocalNode(Architecture architecture, Caching caching)
            : this(architecture, caching, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalNode"/> class.
        /// </summary>
        /// <param name="architecture">
        /// The architecture of the network.
        /// </param>
        /// <param name="caching">
        /// The caching mode of the network.
        /// </param>
        /// <param name="rebinding">
        /// A delegate specifying any rebinding to occur after the inversion of control kernel has been created.
        /// </param>
        public LocalNode(Architecture architecture, Caching caching, Action<IKernel> rebinding)
        {
            this.m_Architecture = architecture;
            this.m_Caching = caching;

            this.m_Kernel = new StandardKernel();

            this.m_Kernel.Bind<ILocalNode>().ToMethod(x => this);

            this.m_Kernel.Bind<IObjectWithTypeSerializer>().To<DefaultObjectWithTypeSerializer>();
            this.m_Kernel.Bind<IObjectLookup>().To<DefaultObjectLookup>();
            this.m_Kernel.Bind<IClientHandlerFactory>().ToFactory();
            this.m_Kernel.Bind<IMessageConstructor>().To<DefaultMessageConstructor>();
            this.m_Kernel.Bind<IMessageIO>().To<DefaultMessageIO>();
            this.m_Kernel.Bind<IMessageHandler>().To<FetchMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<FetchConfirmationMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<InvokeMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<InvokeResultMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<SetPropertyMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<GetPropertyMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<GetPropertyResultMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<ConnectionPingMessageHandler>();
            this.m_Kernel.Bind<IMessageHandler>().To<ConnectionPongMessageHandler>();
            this.m_Kernel.Bind<SynchronisationEngine>().ToSelf();
            this.m_Kernel.Bind<IClientConnector>().To<DefaultClientConnector>();

            this.m_Kernel.Bind<IConnectionHandler>().To<DefaultConnectionHandler>().InSingletonScope();
            this.m_Kernel.Bind<IClientLookup>().To<DefaultClientLookup>().InSingletonScope();
            this.m_Kernel.Bind<IObjectStorage>().To<DefaultObjectStorage>().InSingletonScope();
            this.m_Kernel.Bind<IMessageSideChannel>().To<DefaultMessageSideChannel>().InSingletonScope();

            if (rebinding != null)
            {
                rebinding(this.m_Kernel);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the architecture of the network.
        /// </summary>
        public Architecture Architecture
        {
            get
            {
                return this.m_Architecture;
            }
        }

        /// <summary>
        /// Gets the caching mode of the network.
        /// </summary>
        public Caching Caching
        {
            get
            {
                return this.m_Caching;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current node is a server node (in the server-client architecture).
        /// </summary>
        public bool IsServer { get; set; }

        /// <summary>
        /// Gets the contact that represents this node.
        /// </summary>
        public Contact Self { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Binds this node to the specified address and port, waiting for inbound connections.
        /// </summary>
        /// <param name="address">
        /// The address to bind to.
        /// </param>
        /// <param name="port">
        /// The port to bind to.
        /// </param>
        public void Bind(IPAddress address, int port)
        {
            this.Self = new Contact { IPAddress = address, Port = port };

            this.GetService<IClientLookup>().Add(new IPEndPoint(address, port), this.GetService<SelfClientHandler>());

            this.m_ConnectionHandler = this.GetService<IConnectionHandler>();
            this.m_ConnectionHandler.Start(address, port);

            this.m_Bound = true;
        }

        /// <summary>
        /// Closes the node, terminating all connections.
        /// </summary>
        public void Close()
        {
            this.AssertBound();

            this.m_ConnectionHandler.Stop();

            foreach (var handler in this.GetService<IClientLookup>().GetAll().Select(x => x.Value))
            {
                handler.Stop();
            }

            this.Self = null;

            // TODO: Clean up kernel state.
        }

        /// <summary>
        /// Fetch an object from storage.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the object.
        /// </param>
        /// <returns>
        /// The object, or null if none was found.
        /// </returns>
        public object Fetch(string id)
        {
            this.AssertBound();

            var objectLookup = this.GetService<IObjectLookup>();
            var objectWithTypeSerializer = this.GetService<IObjectWithTypeSerializer>();

            var entry = objectLookup.GetFirst(ID.NewHash(id), 60000);
            if (entry == null)
            {
                return null;
            }

            return entry.Value;
        }

        /// <summary>
        /// Gets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>
        /// GetProperty("abc", new string[] { "Something", "PropertyToGet" });
        /// </code>
        /// would get the value of "PropertyToGet", on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="property">
        /// The property to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public object GetProperty(string id, string property)
        {
            this.AssertBound();

            var messageConstructor = this.GetService<IMessageConstructor>();
            var messageSideChannel = this.GetService<IMessageSideChannel>();
            var objectWithTypeSerializer = this.GetService<IObjectWithTypeSerializer>();

            var entry = this.GetHandlerAndObjectByID(id);
            var obj = entry.Value;

            if (this.Architecture == Architecture.PeerToPeer && entry.Owner != this.Self)
            {
                // We need to request the value from the node that owns it.
                var getMessage = messageConstructor.ConstructGetPropertyMessage(ID.NewHash(id), property);
                entry.ClientHandler.Send(getMessage);

                var message =
                    messageSideChannel.WaitUntil(
                        x => x.Type == MessageType.GetPropertyResult && x.GetPropertyMessageID == getMessage.ID, 
                        50000000);

                if (message == null)
                {
                    throw new InvalidOperationException("No response");
                }

                return objectWithTypeSerializer.Deserialize(message.GetPropertyResult);
            }

            if (this.Architecture == Architecture.ServerClient)
            {
                if (!this.IsServer && this.Caching != Caching.PushOnChange)
                {
                    // We are the client, but we need to request the value
                    // from the server.
                    var getMessage = messageConstructor.ConstructGetPropertyMessage(ID.NewHash(id), property);
                    entry.ClientHandler.Send(getMessage);

                    var message =
                        messageSideChannel.WaitUntil(
                            x => x.Type == MessageType.GetPropertyResult && x.InvokeMessageID == getMessage.ID, 
                            5000);

                    if (message == null)
                    {
                        throw new InvalidOperationException("No response");
                    }

                    return objectWithTypeSerializer.Deserialize(message.GetPropertyResult);
                }
            }

            var mi = obj.GetType().GetMethod("get_" + property + "__Distributed0", BindingFlagsCombined.All);
            if (mi == null)
            {
                throw new MissingMethodException(obj.GetType().FullName, "get_" + property + "__Distributed0");
            }

            return DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new object[] { });
        }

        /// <summary>
        /// Retrieves a service associated with this node.  Dx uses an inversion of control library internally
        /// so that components can be bound together.  You can use this method to locate a given service
        /// and retrieve the concrete implementation that it is bound to.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to retrieve.
        /// </typeparam>
        /// <returns>
        /// The concrete instance of <see cref="T"/>.
        /// </returns>
        public T GetService<T>()
        {
            return this.m_Kernel.Get<T>();
        }

        /// <summary>
        /// Retrieves all services associated with this node.  Dx uses an inversion of control library internally
        /// so that components can be bound together.  You can use this method to locate given services
        /// and retrieve the concrete implementations that it is bound to.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to retrieve.
        /// </typeparam>
        /// <returns>
        /// The concrete instances of <see cref="T"/>.
        /// </returns>
        public IEnumerable<T> GetServices<T>()
        {
            return this.m_Kernel.GetAll<T>();
        }

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>
        /// Invoke("abc", "MethodToCall", new object[] { });
        /// </code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="method">
        /// The method to be invoked.
        /// </param>
        /// <param name="targs">
        /// The type arguments to pass to the method.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the method.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public object Invoke(string id, string method, Type[] targs, object[] args)
        {
            this.AssertBound();

            var messageConstructor = this.GetService<IMessageConstructor>();
            var messageSideChannel = this.GetService<IMessageSideChannel>();
            var objectWithTypeSerializer = this.GetService<IObjectWithTypeSerializer>();

            var entry = this.GetHandlerAndObjectByID(id);
            var obj = entry.Value;

            if (this.Architecture == Architecture.PeerToPeer)
            {
                // In peer-to-peer modes, methods are always invoked locally.
                var mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                if (mi == null)
                {
                    throw new MissingMethodException(obj.GetType().FullName, method);
                }

                return DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, targs, args);
            }

            if (this.Architecture == Architecture.ServerClient)
            {
                if (this.IsServer)
                {
                    // The server is always permitted to call methods.
                    var mi = obj.GetType().GetMethod(method, BindingFlagsCombined.All);
                    if (mi == null)
                    {
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    }

                    return DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, targs, args);
                }
                else
                {
                    // We must see if the client is permitted to call the specified method.
                    var mi = obj.GetType()
                        .GetMethod(method.Substring(0, method.IndexOf("__Distributed0")), BindingFlagsCombined.All);
                    if (mi == null)
                    {
                        throw new MissingMethodException(obj.GetType().FullName, method);
                    }

                    if (mi.GetCustomAttributes(typeof(ClientIgnorableAttribute), false).Count() != 0)
                    {
                        return null;
                    }

                    if (!mi.GetCustomAttributes(typeof(ClientCallableAttribute), false).Any())
                    {
                        throw new MemberAccessException(
                            "The method '" + method + "' is not accessible to client machines.");
                    }

                    // If we get to here, then we're permitted to call the method, but we still need
                    // to remote it to the server.
                    var invokeMessage = messageConstructor.ConstructInvokeMessage(ID.NewHash(id), method, targs, args);
                    entry.ClientHandler.Send(invokeMessage);

                    var message =
                        messageSideChannel.WaitUntil(
                            x => x.Type == MessageType.InvokeResult && x.InvokeMessageID == invokeMessage.ID, 
                            5000);

                    if (message == null)
                    {
                        throw new InvalidOperationException("No response");
                    }

                    return objectWithTypeSerializer.Deserialize(message.InvokeResult);
                }
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>
        /// SetProperty("abc", "PropertyToSet", 5);
        /// </code>
        /// would set "PropertyToSet" to 5, on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="property">
        /// The property to store the value in.
        /// </param>
        /// <param name="value">
        /// The value to set the property to.
        /// </param>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public void SetProperty(string id, string property, object value)
        {
            this.AssertBound();

            var objectStorage = this.GetService<IObjectStorage>();
            var clientLookup = this.GetService<IClientLookup>();
            var messageConstructor = this.GetService<IMessageConstructor>();

            var entry = this.GetHandlerAndObjectByID(id);
            var obj = entry.Value;

            var mi = obj.GetType().GetMethod("set_" + property + "__Distributed0", BindingFlagsCombined.All);
            if (mi == null)
            {
                throw new MissingMethodException(obj.GetType().FullName, "set_" + property + "__Distributed0");
            }

            if (this.Architecture == Architecture.PeerToPeer)
            {
                if (entry.Owner == this.Self)
                {
                    DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new[] { value });
                    objectStorage.UpdateOrPut(new LiveEntry { Key = ID.NewHash(id), Value = obj, Owner = this.Self });
                    return;
                }

                var clientHandler = clientLookup.Lookup(entry.Owner.IPEndPoint);
                clientHandler.Send(messageConstructor.ConstructSetPropertyMessage(ID.NewHash(id), property, value));
                return;
            }

            if (this.Architecture == Architecture.ServerClient)
            {
                // If we are a client, we can't set properties.
                if (!this.IsServer)
                {
                    throw new MemberAccessException("Only servers may set the '" + property + "' property.");
                }

                DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new[] { value });
                objectStorage.UpdateOrPut(new LiveEntry { Key = ID.NewHash(id), Value = obj, Owner = this.Self });

                if (this.Caching == Caching.PushOnChange)
                {
                    // We need to push the new value out to clients.
                    foreach (var client in clientLookup.GetAll().Where(x => x.Key != this.Self.IPEndPoint))
                    {
                        client.Value.Send(
                            messageConstructor.ConstructSetPropertyMessage(ID.NewHash(id), property, value));
                    }
                }

                return;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Store an object into storage, with the current node as the owner.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the object.
        /// </param>
        /// <param name="data">
        /// The object itself.
        /// </param>
        public void Store(string id, object data)
        {
            this.AssertBound();

            var objectWithTypeSerializer = this.GetService<IObjectWithTypeSerializer>();
            var objectStorage = this.GetService<IObjectStorage>();

            objectStorage.Put(new LiveEntry { Key = ID.NewHash(id), Value = data, Owner = this.Self });
        }

        /// <summary>
        /// Applies values from the synchronisation store to the target
        /// object.  One or more properties of the target must have the
        /// [Synchronised] attribute for this function to do anything
        /// useful.
        /// </summary>
        /// <param name="target">
        /// The target to synchronise.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="authoritative">
        /// Whether the instance is considered to be the authoritative instance.
        /// This changes the direction of synchronisation.
        /// </param>
        public void Synchronise(object target, string name, bool authoritative)
        {
            var sync = target as ISynchronised;
            if (sync == null)
            {
                throw new InvalidOperationException(
                    "The object of type " + target.GetType() + " is not a synchronised object.");
            }

            var store = sync.GetSynchronisationStore(this, name);
            this.GetService<SynchronisationEngine>().Apply(sync, store, authoritative);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assert that the current node is bound to the network before continuing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void AssertBound()
        {
            if (!this.m_Bound)
            {
                throw new InvalidOperationException("You must bind the node before using it.");
            }
        }

        /// <summary>
        /// Retrieve the client handler, owner and live object based on the
        /// specified object's ID.
        /// </summary>
        /// <param name="id">
        /// The id of the object to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="ObjectEntry"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private ObjectEntry GetHandlerAndObjectByID(string id)
        {
            var objectLookup = this.GetService<IObjectLookup>();
            var clientLookup = this.GetService<IClientLookup>();
            var objectWithTypeSerializer = this.GetService<IObjectWithTypeSerializer>();

            var entry = objectLookup.GetFirst(ID.NewHash(id), 60000);
            if (entry == null)
            {
                throw new NullReferenceException();
            }

            var clientHandler = clientLookup.Lookup(entry.Owner.IPEndPoint);
            if (clientHandler == null)
            {
                throw new InvalidOperationException();
            }

            return new ObjectEntry { ClientHandler = clientHandler, Owner = entry.Owner, Value = entry.Value };
        }

        #endregion

        /// <summary>
        /// The object entry, representing both the live object, the owner of the object
        /// and the client handler through which the owner can be contacted.
        /// </summary>
        private struct ObjectEntry
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the client handler.
            /// </summary>
            public IClientHandler ClientHandler { get; set; }

            /// <summary>
            /// Gets or sets the owner.
            /// </summary>
            public Contact Owner { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public object Value { get; set; }

            #endregion
        }
    }
}