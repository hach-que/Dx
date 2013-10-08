namespace Dx.Runtime
{
    public interface IDxFactory
    {    
        ILocalNode CreateLocalNode(
            Caching caching = Caching.PullOnDemand,
            Architecture architecture = Architecture.PeerToPeer);
            
        ILocalNode CreateLocalNode(
            ID id,
            Caching caching = Caching.PullOnDemand,
            Architecture architecture = Architecture.PeerToPeer);
        
        INetworkProvider CreateNetworkProvider(ILocalNode localNode);
        IProcessorProvider CreateProcessorProvider(ILocalNode localNode);
        IStorageProvider CreateStorageProvider(ILocalNode localNode);
        IContactProvider CreateContactProvider(ILocalNode localNode);
    }
}

