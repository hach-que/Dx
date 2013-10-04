namespace Dx.Runtime.Tests
{
    [Distributed]
    public class InterceptNewInstructionTest
    {
        public object PerformConstruction()
        {
            return new object();
        }
    }
}

