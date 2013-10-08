namespace Dx.Runtime
{
    public class DefaultDxFactory : IDxFactory
    {
        public virtual ILocalNode CreateLocalNode(
            ID id,
            Caching caching = Caching.PullOnDemand,
            Architecture architecture = Architecture.PeerToPeer)
        {
            return new LocalNode(this, id, caching, architecture);
        }
        
        public virtual ILocalNode CreateLocalNode(
            Caching caching = Caching.PullOnDemand,
            Architecture architecture = Architecture.PeerToPeer)
        {
            return new LocalNode(this, null, caching, architecture);
        }
        
        public virtual INetworkProvider CreateNetworkProvider(ILocalNode localNode)
        {
            return new AutomaticUdpNetwork(localNode);
        }
        
        public virtual IProcessorProvider CreateProcessorProvider(ILocalNode localNode)
        {
            return new Dpm(localNode);
        }
        
        public virtual IStorageProvider CreateStorageProvider(ILocalNode localNode)
        {
            return new DhtWrapper(localNode);
        }
        
        public virtual IContactProvider CreateContactProvider(ILocalNode localNode)
        {
            return (DhtWrapper)(((LocalNode)localNode).Storage);
        }
    }
}

