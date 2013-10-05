using System;

namespace Dx.Runtime
{
    public interface ILocalNode : INode
    {
        /// <summary>
        /// The provider which gives this node it's network representation.
        /// </summary>
        INetworkProvider Network { get; set; }

        /// <summary>
        /// The provider which gives this node it's set of contacts.
        /// </summary>
        IContactProvider Contacts { get; set; }

        /// <summary>
        /// The storage provider which stores data in the network on this node's behalf.
        /// </summary>
        IStorageProvider Storage { get; set; }

        /// <summary>
        /// The processing provider which invokes methods on this node's behalf.
        /// </summary>
        IProcessorProvider Processor { get; set; }

        /// <summary>
        /// The network architecture.
        /// </summary>
        Architecture Architecture { get; set; }

        /// <summary>
        /// The caching mode currently in use.
        /// </summary>
        Caching Caching { get; set; }
        
        /// <summary>
        /// Returns whether this DPM is the server node in the network.  Only applicable
        /// to applications running in server-client mode.
        /// </summary>
        bool IsServer { get; }
        
        /// <summary>
        /// Joins the specified network.
        /// </summary>
        /// <param name="network">The network ID.</param>
        void Join(Guid network);

        /// <summary>
        /// Joins the specified network.
        /// </summary>
        /// <param name="network">The network ID.</param>
        void Join(ID network);

        /// <summary>
        /// Leaves the network and closes the node.
        /// </summary>
        void Leave();
        
        /// <summary>
        /// Applies values from the synchronisation store to the target
        /// object.  One or more properties of the target must have the
        /// [Synchronised] attribute for this function to do anything
        /// useful.
        /// </summary>
        /// <param name="target">The target to synchronise.</param>
        void Synchronise(object target);
    }
}

