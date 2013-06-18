using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Attributes
{
    public class DistributedAttribute : Attribute
    {
        public Architecture Architecture { get; private set; }
        public Caching Caching { get; private set; }

        /// <summary>
        /// Specifies that the class should be distributed within a network.
        /// </summary>
        public DistributedAttribute()
        {
        }

        /// <summary>
        /// Specifies the type of distributed program that should be used.  If this is
        /// not declared on the Program class, the program will not be distributed.  Uses
        /// the PullOnDemand caching mode.
        /// </summary>
        /// <param name="network">The type of network architecture.</param>
        public DistributedAttribute(Architecture network)
        {
            this.Architecture = network;
            this.Caching = Caching.PullOnDemand;
        }

        /// <summary>
        /// Specifies the type of distributed program that should be used.  If this is
        /// not declared on the Program class, the program will not be distributed.
        /// </summary>
        /// <param name="network">The type of network architecture.</param>
        /// <param name="cache">The type of caching to use.</param>
        public DistributedAttribute(Architecture network, Caching cache)
        {
            this.Architecture = network;
            this.Caching = cache;
        }
    }

    public enum Architecture
    {
        /// <summary>
        /// Specifies that the program should execute in master-slave mode.  In this mode, the first program
        /// that starts is designated as a master, and all other programs execute as slaves.  The program's
        /// entry point is only executed once regardless of the number nodes in the system.  Also see the
        /// ServerClient architecture for a similar but reversed model.
        /// </summary>
        //MasterSlave,

        /// <summary>
        /// Specifies that the program should execute in peer-to-peer mode.  In this mode, all programs are
        /// equal and execute their entry points regardless of whether they are the first node in the network.
        /// </summary>
        PeerToPeer,

        /// <summary>
        /// Specifies that the first node in the network should act a server managing all data.  Further connections
        /// to the network will act as slaves, except that the can only call methods with the ClientCallable
        /// attribute applied.  This mode is similar to MasterSlave except that the instruction roles are reversed;
        /// rather than the server telling clients what to do (MasterSlave), clients tell the server what they want
        /// to do (ServerClient).
        /// </summary>
        ServerClient,
    }

    public enum Caching
    {
        /// <summary>
        /// The data is fetched from the node than owns it when the property is accessed or set.  The node accessing
        /// the property waits until the operation completes.
        /// </summary>
        PullOnDemand,

        /// <summary>
        /// When the server changes data that it owns, it pushes the information to the clients in the network who cache
        /// the value indefinitely.  Only valid in the ServerClient architecture.
        /// </summary>
        PushOnChange,
        
        /// <summary>
        /// The data is permanently stored in the local cache forever when initially retrieved.  Only appropriate for
        /// networks where all distributed objects are immutable (such as where peer-to-peer instances are signed with
        /// cryptography).
        /// </summary>
        StoreOnDemand,

        /// <summary>
        /// The data is periodically fetched from the node that owns it.  The data is fetched in the background.  When
        /// a node sets a property, it acts the same as PullOnDemand (the node must wait for the operation to complete).
        /// </summary>
        //PullOnTimeout,

        /// <summary>
        /// The data is fetched from the node that owns it when the property is accessed.  The last known value is
        /// immediately returned and the access operation completes in the background.  When a node sets a property,
        /// it acts the same as PullOnDemand (the node must wait for the operation to complete).
        /// </summary>
        //RequestOnDemand,
    }
}
