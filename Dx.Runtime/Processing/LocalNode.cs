using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    internal class LocalNode : ILocalNode, ISerializable
    {
        internal bool m_Fake = false;
        private readonly SynchronisationEngine m_SyncEngine = new SynchronisationEngine();
    
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
        /// Gets or sets the profiler endpoint.  If you set this value, Hit() will
        /// be called whenever an operation occurs against the local node.
        /// </summary>
        /// <value>The profiler endpoint.</value>
        public INetworkProfilerEndpoint ProfilerEndpoint { get; set; }

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
                if (this.m_Fake)
                    throw new InvalidOperationException("Object graph has not been deserialized correctly.");
                
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
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
                
            this.Join(new ID(network, network, network, network));
        }

        /// <summary>
        /// Joins the specified network.
        /// </summary>
        /// <param name="network">The network ID.</param>
        public void Join(ID network)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
                
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
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
                
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
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            if (this.ProfilerEndpoint != null)
                this.ProfilerEndpoint.Hit(NetworkProfilerEndpointType.SetProperty, id, property);
            this.Storage.SetProperty(id, property, value);
        }

        public object GetProperty(string id, string property)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            if (this.ProfilerEndpoint != null)
                this.ProfilerEndpoint.Hit(NetworkProfilerEndpointType.GetProperty, id, property);
            return this.Storage.GetProperty(id, property);
        }

        public void AddEvent(EventTransport transport)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            if (this.ProfilerEndpoint != null)
                this.ProfilerEndpoint.Hit(NetworkProfilerEndpointType.AddEvent, transport.SourceObjectNetworkName, transport.SourceEventName);
            this.Processor.AddEvent(transport);
        }

        public void RemoveEvent(EventTransport transport)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            if (this.ProfilerEndpoint != null)
                this.ProfilerEndpoint.Hit(NetworkProfilerEndpointType.RemoveEvent, transport.SourceObjectNetworkName, transport.SourceEventName);
            this.Processor.RemoveEvent(transport);
        }

        public void InvokeEvent(EventTransport transport, object sender, EventArgs e)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            this.Processor.InvokeEvent(transport, sender, e);
        }

        public object Invoke(string id, string method, Type[] targs, object[] args)
        {
            if (this.m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");
            if (this.ProfilerEndpoint != null)
                this.ProfilerEndpoint.Hit(NetworkProfilerEndpointType.Invoke, id, method);
            return this.Processor.Invoke(id, method, targs, args);
        }

        #endregion
        
        #region Synchronisation
        
        public void Synchronise(object target, string name, bool authoritive)
        {
            var sync = target as ISynchronised;
            if (sync == null)
                throw new InvalidOperationException("The object of type " + target.GetType() + " is not a synchronised object.");
            var store = sync.GetSynchronisationStore(this, name);
            this.m_SyncEngine.Apply(sync, store, authoritive);
        }
        
        #endregion
        
        #region Fake Serialization
        
        protected LocalNode(SerializationInfo info, StreamingContext context)
        {
            this.m_Fake = true;
        }
  
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
        
        #endregion
    }
}
