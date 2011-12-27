using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.ServerClient
{
    [Distributed]
    public sealed class SealedTest
    {
        public void Test<T>()
        {
            T t = default(T);
            return;
        }

        public T TestPlain<T>()
        {
            return default(T);
        }

        public T Test2<T>() where T : new()
        {
            return new T();
        }

        public T Test3<T>() where T : ITest, new()
        {
            return new T();
        }
    }

    public interface ITest
    {
    }

    public class Something : ITest
    {
    }
}
