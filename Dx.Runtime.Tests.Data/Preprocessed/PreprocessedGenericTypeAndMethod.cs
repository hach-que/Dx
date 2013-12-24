using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace Dx.Runtime.Tests.Data
{
    [Distributed, Processed]
    public class PreprocessedGenericTypeAndMethod<T1, T2> : ITransparent
    {
        private class Return__InvokeDirect0<T3, T4, T5> : IDirectInvoke
        {
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                PreprocessedGenericTypeAndMethod<T1, T2>.Return__DistributedDelegate1<T3, T4, T5> return__DistributedDelegate = (PreprocessedGenericTypeAndMethod<T1, T2>.Return__DistributedDelegate1<T3, T4, T5>)Delegate.CreateDelegate(typeof(PreprocessedGenericTypeAndMethod<T1, T2>.Return__DistributedDelegate1<T3, T4, T5>), instance, method);
                return return__DistributedDelegate((T1)((object)parameters[0]), (T2)((object)parameters[1]), (T3)((object)parameters[2]), (T4)((object)parameters[3]), (T5)((object)parameters[4]));
            }
        }
        [CompilerGenerated]
        public delegate T1 Return__DistributedDelegate1<T3, T4, T5>(T1 val, T2 val2, T3 val3, T4 val4, T5 val5);
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
        public T1 Return<T3, T4, T5>(T1 val, T2 val2, T3 val3, T4 val4, T5 val5)
        {
            MulticastDelegate d = new PreprocessedGenericTypeAndMethod<T1, T2>.Return__DistributedDelegate1<T3, T4, T5>(this.Return__Distributed0<T3, T4, T5>);
            return (T1)((object)DpmEntrypoint.Invoke(d, new object[]
            {
                val,
                val2,
                val3,
                val4,
                val5
            }));
        }
        public PreprocessedGenericTypeAndMethod()
        {
            DpmEntrypoint.Construct(this);
        }
        [CompilerGenerated]
        private T1 Return__Distributed0<T3, T4, T5>(T1 val, T2 val2, T3 val3, T4 val4, T5 val5)
        {
            return val;
        }
    }
}
