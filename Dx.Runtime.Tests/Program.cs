namespace Dx.Runtime.Tests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var networkingTests = new NetworkingTests();
            networkingTests.SynchronisedClassBehavesCorrectly();
        }
    }
}

