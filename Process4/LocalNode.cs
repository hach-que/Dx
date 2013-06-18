using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Process4.Interfaces;
using System.Runtime.InteropServices;
using Data4;
using Process4.Providers;
using Process4.Networks;
using Process4.Remoting;
using Process4.Attributes;

namespace Process4
{
    public class LocalNode : Node
    {
        private Assembly m_Target = null;
        private ID m_DefaultNetworkID = null;

        /// <summary>
        /// The singleton instance of LocalNode.
        /// </summary>
        public static LocalNode Singleton = null;

        /// <summary>
        /// The provider which gives this node it's network representation.
        /// </summary>
        public INetworkProvider Network { get; set; }

        /// <summary>
        /// The provider which gives this node it's set of contacts.
        /// </summary>
        public IContactProvider Contacts { get; set; }

        /// <summary>
        /// The storage provider which stores data in the network on this node's behalf.
        /// </summary>
        public IStorageProvider Storage { get; set; }

        /// <summary>
        /// The processing provider which invokes methods on this node's behalf.
        /// </summary>
        public IProcessorProvider Processor { get; set; }

        /// <summary>
        /// The network architecture.
        /// </summary>
        public Architecture Architecture { get; set; }

        /// <summary>
        /// The caching mode currently in use.
        /// </summary>
        public Caching Caching { get; set; }

        /// <summary>
        /// Creates a new reference to the local processing node, targetting the entry
        /// assembly.
        /// </summary>
        public LocalNode() : this(Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// Creates a new reference to the local processing node, targeting the specified
        /// assembly.
        /// </summary>
        /// <param name="target">The assembly to target.</param>
        public LocalNode(Assembly target)
        {
            if (LocalNode.Singleton != null)
                throw new InvalidOperationException("You can only initialize one LocalNode per application instance.");
            else
                LocalNode.Singleton = this;

            // Set our target and generate a unique ID.
            this.ID = ID.NewRandom();
            this.m_Target = target;

            // Get the assembly Guid.
            object[] o = this.m_Target.GetCustomAttributes(typeof(GuidAttribute), false);
            Guid g = Guid.Empty;
            if (o.Length > 0)
                g = new Guid((o[0] as GuidAttribute).Value);
            this.m_DefaultNetworkID = new ID(g, g, g, g);

            // Get the architecture and caching modes of the program.
            o = this.m_Target.EntryPoint.DeclaringType.GetCustomAttributes(typeof(DistributedAttribute), false);
            if (o.Length == 1)
            {
                this.Architecture = (o[0] as DistributedAttribute).Architecture;
                this.Caching = (o[0] as DistributedAttribute).Caching;
            }
            else
            {
                LocalNode.Singleton = null;
                throw new InvalidOperationException("The entry point class of the program must contain a Distributed attribute in order to initialize an instance of LocalNode.");
            }

            // Set defaults for properties.
            this.Network = new AutomaticUdpNetwork(this);
            this.Storage = new DhtWrapper(this);
            this.Contacts = (this.Storage as DhtWrapper); // Cast because the DHT provides contacts as well.
            this.Processor = new Dpm(this);
        }

        #region Functionality for Server-Client Mode

        /// <summary>
        /// Returns whether this DPM is the server node in the network.  Only applicable
        /// to applications running in server-client mode.
        /// </summary>
        public bool IsServer
        {
            get
            {
                if (this.Architecture == Architecture.ServerClient)
                    return this.Network.IsFirst;
                else
                    throw new InvalidOperationException("The IsServer property is only supported on server-client architectures.");
            }
        }

        #endregion

        #region Functionality for Master-Slave Mode

#if false

        /// <summary>
        /// Returns whether this DPM is a slave node in the network.  Only applicable
        /// to applications running in master-slave mode.
        /// </summary>
        public bool IsSlave
        {
            get
            {
                if (this.Architecture == Architecture.MasterSlave)
                    return !this.Network.IsFirst;
                else
                    throw new InvalidOperationException("The IsSlave property is only supported on master-slave architectures.");
            }
        }

        /// <summary>
        /// Cycles forever waiting for requests from the master node.  Returns when the
        /// master exits.
        /// </summary>
        public void Cycle()
        {
            if (this.Architecture == Architecture.MasterSlave)
                throw new InvalidOperationException("The Cycle method is only supported on master-slave architectures.");
            if (!this.IsSlave)
                return; // Prevent slave cycling if the node isn't actually a slave.
        }

#endif

        #endregion

        #region Functionality for Peer-to-Peer Mode

        // There is no specified peer-to-peer functionality
        // required.

        #endregion

        #region General Network Management

        /// <summary>
        /// Joins the network based on the assembly GUID.
        /// </summary>
        public void Join()
        {
            this.Join(this.m_DefaultNetworkID);
        }

        /// <summary>
        /// Joins the specified network.
        /// </summary>
        /// <param name="network">The network ID.</param>
        public void Join(Guid network)
        {
            this.Join(new ID(network, network, network, network));
        }

        /// <summary>
        /// Joins the specified network.
        /// </summary>
        /// <param name="network">The network ID.</param>
        public void Join(ID network)
        {
            if (this.Contacts.StorageStartRequired)
            {
                this.Storage.Start();
                this.Network.Join(network);
            }
            else
            {
                this.Network.Join(network);
                this.Storage.Start();
            }
            this.Processor.Start();
        }

        /// <summary>
        /// Leaves the network and closes the node.
        /// </summary>
        public void Leave()
        {
            this.Processor.Stop();
            if (this.Contacts.StorageStartRequired)
            {
                this.Network.Leave();
                this.Storage.Stop();
            }
            else
            {
                this.Storage.Stop();
                this.Network.Leave();
            }
        }

        #endregion

        #region Node Members

        internal override void SetProperty(string id, string property, object value)
        {
            this.Storage.SetProperty(id, property, value);
        }

        internal override object GetProperty(string id, string property)
        {
            return this.Storage.GetProperty(id, property);
        }

        internal override void AddEvent(EventTransport transport)
        {
            this.Processor.AddEvent(transport);
        }

        internal override void RemoveEvent(EventTransport transport)
        {
            this.Processor.RemoveEvent(transport);
        }

        internal override void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            this.Processor.InvokeEvent(transport, sender, e);
        }

        internal override object Invoke(string id, string method, object[] args)
        {
            return this.Processor.Invoke(id, method, args);
        }

        internal override DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback)
        {
            return this.Processor.InvokeAsync(id, method, args, callback);
        }

        #endregion
    }
}
