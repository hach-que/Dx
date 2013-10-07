using System;

namespace Dx.Runtime
{
    public enum NetworkProfilerEndpointType
    {
        Invoke,
        GetProperty,
        SetProperty,
        AddEvent,
        RemoveEvent
    }

    public interface INetworkProfilerEndpoint
    {
        /// <summary>
        /// Called when an operation has occurred inside the local node.
        /// </summary>
        /// <param name="type">The type of operation performed.</param>
        /// <param name="id">The identifier for the object the operation was performed on.</param>
        /// <param name="target">The name of the target of the operation (e.g. method or property name).</param>
        void Hit(NetworkProfilerEndpointType type, string id, string target);
    }
}

