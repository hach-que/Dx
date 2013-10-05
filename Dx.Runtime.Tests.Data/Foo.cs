namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class Foo
    {
        public string TestValue { get; set; }
        public Bar MyBar { get; set; }
    
        public Bar ConstructBar()
        {
            return new Bar();
        }
    }
}

