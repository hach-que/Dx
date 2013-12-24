namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class GenericMethod
    {
        public T Return<T, T2>(T val, T2 second)
        {
            return val;
        }
    }
}

