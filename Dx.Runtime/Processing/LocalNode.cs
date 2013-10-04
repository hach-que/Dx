using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dx.Runtime
{
    internal class LocalNode : ILocalNode
    {
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
        
        public ID ID { get; set; }

        public LocalNode(IDxFactory factory, Caching caching, Architecture architecture)
        {
            // Generate a unique ID.
            this.ID = ID.NewRandom();

            // Assign caching and architecture.
            this.Caching = caching;
            this.Architecture = architecture;
            
            // Set defaults for properties.
            this.Network = factory.CreateNetworkProvider(this);
            this.Storage = factory.CreateStorageProvider(this);
            this.Contacts = factory.CreateContactProvider(this);
            this.Processor = factory.CreateProcessorProvider(this);
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

        #region Functionality for Peer-to-Peer Mode

        // There is no specified peer-to-peer functionality
        // required.

        #endregion

        #region General Network Management

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

        public void SetProperty(string id, string property, object value)
        {
            this.Storage.SetProperty(id, property, value);
        }

        public object GetProperty(string id, string property)
        {
            return this.Storage.GetProperty(id, property);
        }

        public void AddEvent(EventTransport transport)
        {
            this.Processor.AddEvent(transport);
        }

        public void RemoveEvent(EventTransport transport)
        {
            this.Processor.RemoveEvent(transport);
        }

        public void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            this.Processor.InvokeEvent(transport, sender, e);
        }

        public object Invoke(string id, string method, Type[] targs, object[] args)
        {
            return this.Processor.Invoke(id, method, targs, args);
        }

        #endregion
    }
}
