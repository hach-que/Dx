using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Process4.Providers;
using Process4.Attributes;
using System.Runtime.Serialization;
using Process4.Interfaces;
using System.Reflection;

namespace Examples.ServerClient
{
    [Distributed, Processed]
    [Serializable]
    public sealed class SealedTest_Compare : ITransparent, ISerializable
    {
        private class Test__InvokeDirect0<T> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate void Test__DistributedDelegate1();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                SealedTest_Compare.Test__InvokeDirect0<T>.Test__DistributedDelegate1 test__DistributedDelegate = (SealedTest_Compare.Test__InvokeDirect0<T>.Test__DistributedDelegate1)Delegate.CreateDelegate(typeof(SealedTest_Compare.Test__InvokeDirect0<T>.Test__DistributedDelegate1), instance, method);
                test__DistributedDelegate();
                return null;
            }
        }
        private class TestPlain__InvokeDirect1<T> : IDirectInvoke
        {
            [CompilerGenerated]
            public delegate T TestPlain__DistributedDelegate2();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                SealedTest_Compare.TestPlain__InvokeDirect1<T>.TestPlain__DistributedDelegate2 testPlain__DistributedDelegate = (SealedTest_Compare.TestPlain__InvokeDirect1<T>.TestPlain__DistributedDelegate2)Delegate.CreateDelegate(typeof(SealedTest_Compare.TestPlain__InvokeDirect1<T>.TestPlain__DistributedDelegate2), instance, method);
                return testPlain__DistributedDelegate();
            }
        }
        private class Test2__InvokeDirect2<T> : IDirectInvoke where T : new()
        {
            [CompilerGenerated]
            public delegate T Test2__DistributedDelegate3();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                SealedTest_Compare.Test2__InvokeDirect2<T>.Test2__DistributedDelegate3 test2__DistributedDelegate = (SealedTest_Compare.Test2__InvokeDirect2<T>.Test2__DistributedDelegate3)Delegate.CreateDelegate(typeof(SealedTest_Compare.Test2__InvokeDirect2<T>.Test2__DistributedDelegate3), instance, method);
                return test2__DistributedDelegate();
            }
        }
        private class Test3__InvokeDirect3<T> : IDirectInvoke where T : ITest, new()
        {
            [CompilerGenerated]
            public delegate T Test3__DistributedDelegate4();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                SealedTest_Compare.Test3__InvokeDirect3<T>.Test3__DistributedDelegate4 test3__DistributedDelegate = (SealedTest_Compare.Test3__InvokeDirect3<T>.Test3__DistributedDelegate4)Delegate.CreateDelegate(typeof(SealedTest_Compare.Test3__InvokeDirect3<T>.Test3__DistributedDelegate4), instance, method);
                return test3__DistributedDelegate();
            }
        }
        public string NetworkName
        {
            get;
            set;
        }
        public void Test<T>()
        {
            MulticastDelegate d = new SealedTest_Compare.Test__InvokeDirect0<T>.Test__DistributedDelegate1(this.Test__Distributed0<T>);
            object[] args = new object[0];
            DpmEntrypoint.Invoke(d, args);
        }
        public T TestPlain<T>()
        {
            MulticastDelegate d = new SealedTest_Compare.TestPlain__InvokeDirect1<T>.TestPlain__DistributedDelegate2(this.TestPlain__Distributed0<T>);
            object[] args = new object[0];
            return (T)DpmEntrypoint.Invoke(d, args);
        }
        public T Test2<T>() where T : new()
        {
            MulticastDelegate d = new SealedTest_Compare.Test2__InvokeDirect2<T>.Test2__DistributedDelegate3(this.Test2__Distributed0<T>);
            object[] args = new object[0];
            return (T)DpmEntrypoint.Invoke(d, args);
        }
        public T Test3<T>() where T : ITest, new()
        {
            MulticastDelegate d = new SealedTest_Compare.Test3__InvokeDirect3<T>.Test3__DistributedDelegate4(this.Test3__Distributed0<T>);
            object[] args = new object[0];
            return (T)DpmEntrypoint.Invoke(d, args);
        }
        public SealedTest_Compare()
		{
			DpmEntrypoint.Construct(this);
		}
        [CompilerGenerated]
        private void Test__Distributed0<T>()
        {
            T t = default(T);
        }
        [CompilerGenerated]
        private T TestPlain__Distributed0<T>()
        {
            return default(T);
        }
        [CompilerGenerated]
        private T Test2__Distributed0<T>() where T : new()
        {
            T t = default(T);
            T arg_21_0;
            if (t != null)
            {
                t = default(T);
                arg_21_0 = t;
            }
            else
            {
                arg_21_0 = Activator.CreateInstance<T>();
            }
            return arg_21_0;
        }
        [CompilerGenerated]
        private T Test3__Distributed0<T>() where T : ITest, new()
        {
            T t = default(T);
            T arg_21_0;
            if (t != null)
            {
                t = default(T);
                arg_21_0 = t;
            }
            else
            {
                arg_21_0 = Activator.CreateInstance<T>();
            }
            return arg_21_0;
        }
        protected SealedTest_Compare(SerializationInfo info, StreamingContext context)
        {
            DpmEntrypoint.Deserialize(this, info, context);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            DpmEntrypoint.Serialize(this, info, context);
        }
    }
}
