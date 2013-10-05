
namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class GenericType<T1, T2, T3>
    {
        public bool Test()
        {
            return true;
        }

        public T1 Return(T1 val)
        {
            return val;
        }
    }
}

