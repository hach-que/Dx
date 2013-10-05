namespace Dx.Runtime.Tests.Data
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