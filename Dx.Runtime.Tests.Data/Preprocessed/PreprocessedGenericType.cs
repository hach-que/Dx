using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace Dx.Runtime.Tests.Data
{
    [Distributed, Processed]
    public class PreprocessedGenericType<T1, T2, T3> : ITransparent
    {
        private class Test__InvokeDirect0 : IDirectInvoke
        {
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                PreprocessedGenericType<T1, T2, T3>.Test__DistributedDelegate1 test__DistributedDelegate = (PreprocessedGenericType<T1, T2, T3>.Test__DistributedDelegate1)Delegate.CreateDelegate(typeof(PreprocessedGenericType<T1, T2, T3>.Test__DistributedDelegate1), instance, method);
                return test__DistributedDelegate();
            }
        }
        [CompilerGenerated]
        public delegate bool Test__DistributedDelegate1();
        private class Return__InvokeDirect2 : IDirectInvoke
        {
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                PreprocessedGenericType<T1, T2, T3>.Return__DistributedDelegate3 return__DistributedDelegate = (PreprocessedGenericType<T1, T2, T3>.Return__DistributedDelegate3)Delegate.CreateDelegate(typeof(PreprocessedGenericType<T1, T2, T3>.Return__DistributedDelegate3), instance, method);
                return return__DistributedDelegate((T1)((object)parameters[0]));
            }
        }
        [CompilerGenerated]
        public delegate T1 Return__DistributedDelegate3(T1 val);
        public string NetworkName
        {
            get;
            set;
        }
        public ILocalNode Node
        {
            get;
            set;
        }
        public bool Test()
        {
            MulticastDelegate d = new PreprocessedGenericType<T1, T2, T3>.Test__DistributedDelegate1(this.Test__Distributed0);
            object[] args = new object[0];
            return (bool)DpmEntrypoint.Invoke(d, args);
        }
        public T1 Return(T1 val)
        {
            MulticastDelegate d = new PreprocessedGenericType<T1, T2, T3>.Return__DistributedDelegate3(this.Return__Distributed0);
            return (T1)((object)DpmEntrypoint.Invoke(d, new object[]
            {
                val
            }));
        }
        public PreprocessedGenericType()
        {
            DpmEntrypoint.Construct(this);
        }
        [CompilerGenerated]
        private bool Test__Distributed0()
        {
            return true;
        }
        [CompilerGenerated]
        private T1 Return__Distributed0(T1 val)
        {
            return val;
        }
    }
}
