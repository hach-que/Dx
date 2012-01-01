using Process4.Attributes;
using Process4.Interfaces;
using Process4.Providers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace Examples.ServerClient
{
    [Distributed, Processed]
    [Serializable]
    public class Genericstest<A, B> : ITransparent, ISerializable
        where A : new()
        where B : ITest
    {
        private class Method0__InvokeDirect0 : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate void Method0__DistributedDelegate1();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method0__InvokeDirect0.Method0__DistributedDelegate1 method0__DistributedDelegate = (Genericstest<A, B>.Method0__InvokeDirect0.Method0__DistributedDelegate1)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method0__InvokeDirect0.Method0__DistributedDelegate1), instance, method);
                method0__DistributedDelegate();
                return null;
            }
        }
        private class Method1__InvokeDirect1 : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate A Method1__DistributedDelegate2();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method1__InvokeDirect1.Method1__DistributedDelegate2 method1__DistributedDelegate = (Genericstest<A, B>.Method1__InvokeDirect1.Method1__DistributedDelegate2)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method1__InvokeDirect1.Method1__DistributedDelegate2), instance, method);
                return method1__DistributedDelegate();
            }
        }
        private class Method2__InvokeDirect2 : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate B Method2__DistributedDelegate3();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method2__InvokeDirect2.Method2__DistributedDelegate3 method2__DistributedDelegate = (Genericstest<A, B>.Method2__InvokeDirect2.Method2__DistributedDelegate3)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method2__InvokeDirect2.Method2__DistributedDelegate3), instance, method);
                return method2__DistributedDelegate();
            }
        }
        private class Method3__InvokeDirect3<C> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate C Method3__DistributedDelegate4();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method3__InvokeDirect3<C>.Method3__DistributedDelegate4 method3__DistributedDelegate = (Genericstest<A, B>.Method3__InvokeDirect3<C>.Method3__DistributedDelegate4)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method3__InvokeDirect3<C>.Method3__DistributedDelegate4), instance, method);
                return method3__DistributedDelegate.Invoke();
            }
        }
        private class Method4__InvokeDirect4<C> : IDirectInvoke where C : new()
        {
            [CompilerGenerated]
            public delegate C Method4__DistributedDelegate5();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method4__InvokeDirect4<C>.Method4__DistributedDelegate5 method4__DistributedDelegate = (Genericstest<A, B>.Method4__InvokeDirect4<C>.Method4__DistributedDelegate5)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method4__InvokeDirect4<C>.Method4__DistributedDelegate5), instance, method);
                return method4__DistributedDelegate.Invoke();
            }
        }
        private class Method5__InvokeDirect5<C> : IDirectInvoke where C : class
        {
            [CompilerGenerated]
            public delegate C Method5__DistributedDelegate6();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method5__InvokeDirect5<C>.Method5__DistributedDelegate6 method5__DistributedDelegate = (Genericstest<A, B>.Method5__InvokeDirect5<C>.Method5__DistributedDelegate6)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method5__InvokeDirect5<C>.Method5__DistributedDelegate6), instance, method);
                return method5__DistributedDelegate.Invoke();
            }
        }
        private class Method6__InvokeDirect6<C> : IDirectInvoke where C : struct
        {
            [CompilerGenerated]
            public delegate C Method6__DistributedDelegate7();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method6__InvokeDirect6<C>.Method6__DistributedDelegate7 method6__DistributedDelegate = (Genericstest<A, B>.Method6__InvokeDirect6<C>.Method6__DistributedDelegate7)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method6__InvokeDirect6<C>.Method6__DistributedDelegate7), instance, method);
                return method6__DistributedDelegate.Invoke();
            }
        }
        private class Method7__InvokeDirect7<C> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate C Method7__DistributedDelegate8(C c);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method7__InvokeDirect7<C>.Method7__DistributedDelegate8 method7__DistributedDelegate = (Genericstest<A, B>.Method7__InvokeDirect7<C>.Method7__DistributedDelegate8)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method7__InvokeDirect7<C>.Method7__DistributedDelegate8), instance, method);
                return method7__DistributedDelegate.Invoke((C)parameters[0]);
            }
        }
        private class Method8__InvokeDirect8<C, D> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate C Method8__DistributedDelegate9();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method8__InvokeDirect8<C, D>.Method8__DistributedDelegate9 method8__DistributedDelegate = (Genericstest<A, B>.Method8__InvokeDirect8<C, D>.Method8__DistributedDelegate9)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method8__InvokeDirect8<C, D>.Method8__DistributedDelegate9), instance, method);
                return method8__DistributedDelegate.Invoke();
            }
        }
        private class Method9__InvokeDirect9<C, D> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate D Method9__DistributedDelegate10();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method9__InvokeDirect9<C, D>.Method9__DistributedDelegate10 method9__DistributedDelegate = (Genericstest<A, B>.Method9__InvokeDirect9<C, D>.Method9__DistributedDelegate10)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method9__InvokeDirect9<C, D>.Method9__DistributedDelegate10), instance, method);
                return method9__DistributedDelegate.Invoke();
            }
        }
        private class Method10__InvokeDirect10<C, D> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate C Method10__DistributedDelegate11(C c);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method10__InvokeDirect10<C, D>.Method10__DistributedDelegate11 method10__DistributedDelegate = (Genericstest<A, B>.Method10__InvokeDirect10<C, D>.Method10__DistributedDelegate11)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method10__InvokeDirect10<C, D>.Method10__DistributedDelegate11), instance, method);
                return method10__DistributedDelegate.Invoke((C)parameters[0]);
            }
        }
        private class Method11__InvokeDirect11<C, D> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate D Method11__DistributedDelegate12(D d);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method11__InvokeDirect11<C, D>.Method11__DistributedDelegate12 method11__DistributedDelegate = (Genericstest<A, B>.Method11__InvokeDirect11<C, D>.Method11__DistributedDelegate12)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method11__InvokeDirect11<C, D>.Method11__DistributedDelegate12), instance, method);
                return method11__DistributedDelegate.Invoke((D)parameters[0]);
            }
        }
        private class Method12__InvokeDirect12<C, D> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate C Method12__DistributedDelegate13(C c, D d);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method12__InvokeDirect12<C, D>.Method12__DistributedDelegate13 method12__DistributedDelegate = (Genericstest<A, B>.Method12__InvokeDirect12<C, D>.Method12__DistributedDelegate13)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method12__InvokeDirect12<C, D>.Method12__DistributedDelegate13), instance, method);
                return method12__DistributedDelegate.Invoke((C)parameters[0], (D)parameters[1]);
            }
        }
        private class Method13__InvokeDirect13<C> : IDirectInvoke where C : new()
        {
            [CompilerGenerated]
            public delegate string Method13__DistributedDelegate14(B b);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method13__InvokeDirect13<C>.Method13__DistributedDelegate14 method13__DistributedDelegate = (Genericstest<A, B>.Method13__InvokeDirect13<C>.Method13__DistributedDelegate14)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method13__InvokeDirect13<C>.Method13__DistributedDelegate14), instance, method);
                return method13__DistributedDelegate((B)parameters[0]);
            }
        }
        private class Method15__InvokeDirect14 : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate List<A> Method15__DistributedDelegate15();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method15__InvokeDirect14.Method15__DistributedDelegate15 method15__DistributedDelegate = (Genericstest<A, B>.Method15__InvokeDirect14.Method15__DistributedDelegate15)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method15__InvokeDirect14.Method15__DistributedDelegate15), instance, method);
                return method15__DistributedDelegate();
            }
        }
        private class Method16__InvokeDirect15<C> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate List<C> Method16__DistributedDelegate16();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method16__InvokeDirect15<C>.Method16__DistributedDelegate16 method16__DistributedDelegate = (Genericstest<A, B>.Method16__InvokeDirect15<C>.Method16__DistributedDelegate16)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method16__InvokeDirect15<C>.Method16__DistributedDelegate16), instance, method);
                return method16__DistributedDelegate();
            }
        }
        private class Method17__InvokeDirect16<C> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate Dictionary<A, C> Method17__DistributedDelegate17();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                Genericstest<A, B>.Method17__InvokeDirect16<C>.Method17__DistributedDelegate17 method17__DistributedDelegate = (Genericstest<A, B>.Method17__InvokeDirect16<C>.Method17__DistributedDelegate17)Delegate.CreateDelegate(typeof(Genericstest<A, B>.Method17__InvokeDirect16<C>.Method17__DistributedDelegate17), instance, method);
                return method17__DistributedDelegate();
            }
        }
        public string NetworkName
        {
            get;
            set;
        }
        public void Method0()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method0__InvokeDirect0.Method0__DistributedDelegate1(this.Method0__Distributed0);
            object[] args = new object[0];
            DpmEntrypoint.Invoke(d, args);
        }
        public A Method1()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method1__InvokeDirect1.Method1__DistributedDelegate2(this.Method1__Distributed0);
            object[] args = new object[0];
            return (A)DpmEntrypoint.Invoke(d, args);
        }
        public B Method2()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method2__InvokeDirect2.Method2__DistributedDelegate3(this.Method2__Distributed0);
            object[] args = new object[0];
            return (B)DpmEntrypoint.Invoke(d, args);
        }
        public C Method3<C>()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method3__InvokeDirect3<C>.Method3__DistributedDelegate4(this.Method3__Distributed0<C>);
            object[] args = new object[0];
            return (C)DpmEntrypoint.Invoke(d, args);
        }
        public C Method4<C>() where C : new()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method4__InvokeDirect4<C>.Method4__DistributedDelegate5(this.Method4__Distributed0<C>);
            object[] args = new object[0];
            return (C)DpmEntrypoint.Invoke(d, args);
        }
        public C Method5<C>() where C : class
        {
            MulticastDelegate d = new Genericstest<A, B>.Method5__InvokeDirect5<C>.Method5__DistributedDelegate6(this.Method5__Distributed0<C>);
            object[] args = new object[0];
            return (C)DpmEntrypoint.Invoke(d, args);
        }
        public C Method6<C>() where C : struct
        {
            MulticastDelegate d = new Genericstest<A, B>.Method6__InvokeDirect6<C>.Method6__DistributedDelegate7(this.Method6__Distributed0<C>);
            object[] args = new object[0];
            return (C)DpmEntrypoint.Invoke(d, args);
        }
        public C Method7<C>(C c)
        {
            MulticastDelegate d = new Genericstest<A, B>.Method7__InvokeDirect7<C>.Method7__DistributedDelegate8(this.Method7__Distributed0<C>);
            return (C)DpmEntrypoint.Invoke(d, new object[]
			{
				c
			});
        }
        public C Method8<C, D>()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method8__InvokeDirect8<C, D>.Method8__DistributedDelegate9(this.Method8__Distributed0<C, D>);
            object[] args = new object[0];
            return (C)DpmEntrypoint.Invoke(d, args);
        }
        public D Method9<C, D>()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method9__InvokeDirect9<C, D>.Method9__DistributedDelegate10(this.Method9__Distributed0<C, D>);
            object[] args = new object[0];
            return (D)DpmEntrypoint.Invoke(d, args);
        }
        public C Method10<C, D>(C c)
        {
            MulticastDelegate d = new Genericstest<A, B>.Method10__InvokeDirect10<C, D>.Method10__DistributedDelegate11(this.Method10__Distributed0<C, D>);
            return (C)DpmEntrypoint.Invoke(d, new object[]
			{
				c
			});
        }
        public D Method11<C, D>(D d)
        {
            MulticastDelegate d2 = new Genericstest<A, B>.Method11__InvokeDirect11<C, D>.Method11__DistributedDelegate12(this.Method11__Distributed0<C, D>);
            return (D)DpmEntrypoint.Invoke(d2, new object[]
			{
				d
			});
        }
        public C Method12<C, D>(C c, D d)
        {
            MulticastDelegate d2 = new Genericstest<A, B>.Method12__InvokeDirect12<C, D>.Method12__DistributedDelegate13(this.Method12__Distributed0<C, D>);
            return (C)DpmEntrypoint.Invoke(d2, new object[]
			{
				c, 
				d
			});
        }
        public string Method13<C>(B b) where C : new()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method13__InvokeDirect13<C>.Method13__DistributedDelegate14(this.Method13__Distributed0<C>);
            return DpmEntrypoint.Invoke(d, new object[]
			{
				b
			}) as string;
        }
        public List<A> Method15()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method15__InvokeDirect14.Method15__DistributedDelegate15(this.Method15__Distributed0);
            object[] args = new object[0];
            return DpmEntrypoint.Invoke(d, args) as List<A>;
        }
        public List<C> Method16<C>()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method16__InvokeDirect15<C>.Method16__DistributedDelegate16(this.Method16__Distributed0<C>);
            object[] args = new object[0];
            return DpmEntrypoint.Invoke(d, args) as List<C>;
        }
        public Dictionary<A, C> Method17<C>()
        {
            MulticastDelegate d = new Genericstest<A, B>.Method17__InvokeDirect16<C>.Method17__DistributedDelegate17(this.Method17__Distributed0<C>);
            object[] args = new object[0];
            return DpmEntrypoint.Invoke(d, args) as Dictionary<A, C>;
        }
        public Genericstest()
		{
			DpmEntrypoint.Construct(this);
		}
        [CompilerGenerated]
        private void Method0__Distributed0()
        {
        }
        [CompilerGenerated]
        private A Method1__Distributed0()
        {
            A a = default(A);
            A arg_21_0;
            if (a != null)
            {
                a = default(A);
                arg_21_0 = a;
            }
            else
            {
                arg_21_0 = Activator.CreateInstance<A>();
            }
            return arg_21_0;
        }
        [CompilerGenerated]
        private B Method2__Distributed0()
        {
            return default(B);
        }
        [CompilerGenerated]
        private C Method3__Distributed0<C>()
        {
            return default(C);
        }
        [CompilerGenerated]
        private C Method4__Distributed0<C>() where C : new()
        {
            C c = default(C);
            C arg_21_0;
            if (c != null)
            {
                c = default(C);
                arg_21_0 = c;
            }
            else
            {
                arg_21_0 = Activator.CreateInstance<C>();
            }
            return arg_21_0;
        }
        [CompilerGenerated]
        private C Method5__Distributed0<C>() where C : class
        {
            return default(C);
        }
        [CompilerGenerated]
        private C Method6__Distributed0<C>() where C : struct
        {
            return default(C);
        }
        [CompilerGenerated]
        private C Method7__Distributed0<C>(C c)
        {
            return c;
        }
        [CompilerGenerated]
        private C Method8__Distributed0<C, D>()
        {
            return default(C);
        }
        [CompilerGenerated]
        private D Method9__Distributed0<C, D>()
        {
            return default(D);
        }
        [CompilerGenerated]
        private C Method10__Distributed0<C, D>(C c)
        {
            return c;
        }
        [CompilerGenerated]
        private D Method11__Distributed0<C, D>(D d)
        {
            return d;
        }
        [CompilerGenerated]
        private C Method12__Distributed0<C, D>(C c, D d)
        {
            return c;
        }
        [CompilerGenerated]
        private string Method13__Distributed0<C>(B b) where C : new()
        {
            A a = default(A);
            if (a != null)
            {
                a = default(A);
            }
            else
            {
                Activator.CreateInstance<A>();
            }
            C c = default(C);
            if (c != null)
            {
                c = default(C);
            }
            else
            {
                Activator.CreateInstance<C>();
            }
            a = default(A);
            A arg_65_0;
            if (a != null)
            {
                a = default(A);
                arg_65_0 = a;
            }
            else
            {
                arg_65_0 = Activator.CreateInstance<A>();
            }
            object arg_9E_0 = arg_65_0;
            object arg_9E_1 = b.AsString();
            c = default(C);
            C arg_99_0;
            if (c != null)
            {
                c = default(C);
                arg_99_0 = c;
            }
            else
            {
                arg_99_0 = Activator.CreateInstance<C>();
            }
            return (string)arg_9E_0 + arg_9E_1 + arg_99_0;
        }
        [CompilerGenerated]
        private List<A> Method15__Distributed0()
        {
            return new List<A>();
        }
        [CompilerGenerated]
        private List<C> Method16__Distributed0<C>()
        {
            return new List<C>();
        }
        [CompilerGenerated]
        private Dictionary<A, C> Method17__Distributed0<C>()
        {
            return new Dictionary<A, C>();
        }
        protected Genericstest(SerializationInfo info, StreamingContext context)
        {
            DpmEntrypoint.Deserialize(this, info, context);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            DpmEntrypoint.Serialize(this, info, context);
        }
    }
}
