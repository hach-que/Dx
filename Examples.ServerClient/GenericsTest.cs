using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.ServerClient
{
    [Distributed]
    public class GenericsTest<A, B>
        where A : new()
        where B : ITest
    {
        public void Method0()
        {
        }

        public A Method1()
        {
            return new A();
        }

        public B Method2()
        {
            return default(B);
        }

        public C Method3<C>()
        {
            return default(C);
        }

        public C Method4<C>() where C : new()
        {
            return new C();
        }

        public C Method5<C>() where C : class
        {
            return null;
        }

        public C Method6<C>() where C : struct
        {
            return default(C);
        }

        public C Method7<C>(C c)
        {
            return c;
        }

        public C Method8<C, D>()
        {
            return default(C);
        }

        public D Method9<C, D>()
        {
            return default(D);
        }

        public C Method10<C, D>(C c)
        {
            return c;
        }

        public D Method11<C, D>(D d)
        {
            return d;
        }

        public C Method12<C, D>(C c, D d)
        {
            return c;
        }

        public string Method13<C>(B b) where C : new()
        {
            A a = new A();
            C c = new C();
            return (new A()) + (b.AsString()) + (new C());
        }

        /*public E Method14<C, D, E, F>(E e, F f, D d, C c)
            where E : class, new()
            where F : ITest
            where C : IEnumerable<D>
            where D : IComparable
        {
            foreach (D dd in c)
                if (dd.Equals(d))
                    return null;
            Console.WriteLine(f.AsString());
            return new E();
        }*/

        public List<A> Method15()
        {
            return new List<A>();
        }

        public List<C> Method16<C>()
        {
            return new List<C>();
        }

        public Dictionary<A, C> Method17<C>()
        {
            return new Dictionary<A, C>();
        }
    }

    public class IntTest : ITest
    {
        private int m_Int = 0;

        public IntTest(int it)
        {
            this.m_Int = it;
        }

        public string AsString()
        {
            return this.m_Int.ToString();
        }
    }
}
