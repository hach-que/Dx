
namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class GenericTypeAndMethod<T1, T2>
    {
        public T1 Return<T3, T4, T5>(T1 val, T2 val2, T3 val3, T4 val4, T5 val5)
        {
            return val;
        }
    }
}

