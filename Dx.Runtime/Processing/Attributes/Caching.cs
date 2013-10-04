namespace Dx.Runtime
{
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
    }
}

