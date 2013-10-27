using System.Net;

namespace Dx.Runtime
{
    public interface INetworkProvider
    {
        /// <summary>
        /// The node this network provider is associated with.
        /// </summary>
        ILocalNode Node { get; }

        /// <summary>
        /// The network identifier.
        /// </summary>
        /// <remarks>The implementation of this property should be read-only, to be set by the class internally when Join is called.</remarks>
        ID ID { get; }

        /// <summary>
        /// The messaging port that the TCP network (IStorageProvider) is listening on.
        /// </summary>
        int MessagingPort { get; }

        /// <summary>
        /// The IP address that processing and storage services should bind on.
        /// </summary>
        IPAddress IPAddress { get; }

        /// <summary>
        /// Whether this node was the first node to join the network.  This should return
        /// the same value regardless of new nodes joining the network.
        /// </summary>
        bool IsFirst { get; }

        /// <summary>
        /// Joins the network with the specified ID.
        /// </summary>
        /// <param name="id">The network ID.</param>
        void Join(ID id);

        /// <summary>
        /// Leaves the network.
        /// </summary>
        void Leave();
    }
}
