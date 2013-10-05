namespace Dx.Runtime.Tests
{
    public class InternalDxFactory : DefaultDxFactory
    {
        public bool CreateNodeB { get; set; }
    
        public override INetworkProvider CreateNetworkProvider(ILocalNode localNode)
        {
            return new InternalTestNetwork(localNode, !this.CreateNodeB);
        }
    }
}

