namespace Dx.Runtime
{
    public class SerializationData
    {
        public Dht Dht { get; set; }
        public object Storage { get; set; }
        public object Root { get; set; }
        public Entry Entry { get; set; }
        public bool IsMessage { get; set; }
    }
}
