namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class Bar
    {
        public string OtherString { get; set; }
    
        public string GetHelloWorldString()
        {
            return "Hello, World!";
        }
    }
}

