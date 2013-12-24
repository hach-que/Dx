namespace Dx.Runtime.Tests.Data
{
    public class SynchronisedTest
    {
        [Synchronised]
        public float X { get; set; }
        
        [Synchronised]
        public float Y { get; set; }
        
        [Synchronised]
        public float Z { get; set; }
        
        public void Update(ILocalNode node)
        {
            node.Synchronise(this, "test", true);
        }
    }
}
