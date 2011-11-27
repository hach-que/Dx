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
        public INetworkProvider Network { get; private set; }

        /// <summary>
        /// The provider which gives this node it's set of contacts.
        /// </summary>
        public IContactProvider Contacts { get; private set; }

        /// <summary>
        /// The storage provider which stores data in the network on this node's behalf.
        /// </summary>
        public IStorageProvider Storage { get; private set; }

        /// <summary>
        /// The processing provider which invokes methods on this node's behalf.
        /// </summary>
        public IProcessorProvider Processor { get; private set; }

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

            // Set defaults for properties.
            this.Network = new AutomaticUdpNetwork(this);
            this.Storage = new DhtWrapper(this);
            this.Contacts = (this.Storage as DhtWrapper); // Cast because the DHT provides contacts as well.
            this.Processor = new Dpm(this);

            // TODO: Use reflection to get the mode of the
            //       program.
        }

        #region Functionality for Master-Slave Mode

        /// <summary>
        /// Returns whether this DPM is a slave node in the network.  Only applicable
        /// to applications running in master-slave mode.
        /// </summary>
        public bool IsSlave
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Cycles forever waiting for requests from the master node.  Returns when the
        /// master exits.
        /// </summary>
        public void Cycle()
        {
            if (!this.IsSlave)
                return; // Prevent slave cycling if the node isn't actually a slave.
        }

        #endregion

        #region Functionality for Peer-to-Peer Mode

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
