using System;

namespace Dx.Runtime
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
}
