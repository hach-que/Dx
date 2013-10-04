namespace Dx.Runtime
{
    public class DefaultDxFactory : IDxFactory
    {
        public ILocalNode CreateLocalNode(
            Caching caching = Caching.PullOnDemand,
            Architecture architecture = Architecture.PeerToPeer)
        {
            return new LocalNode(this, caching, architecture);
        }
        
        public INetworkProvider CreateNetworkProvider(ILocalNode localNode)
        {
            return new AutomaticUdpNetwork(localNode);
        }
        
        public IProcessorProvider CreateProcessorProvider(ILocalNode localNode)
        {
            return new Dpm(localNode);
        }
        
        public IStorageProvider CreateStorageProvider(ILocalNode localNode)
        {
            return new DhtWrapper(localNode);
        }
        
        public IContactProvider CreateContactProvider(ILocalNode localNode)
        {
            return (DhtWrapper)(((LocalNode)localNode).Storage);
        }
    }
}

