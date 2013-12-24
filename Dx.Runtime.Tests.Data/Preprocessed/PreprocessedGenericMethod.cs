using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace Dx.Runtime.Tests.Data
{
    [Distributed, Processed]
    public class PreprocessedGenericMethod : ITransparent
    {
        private class Return__InvokeDirect0<T, T2> : IDirectInvoke
        {
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                PreprocessedGenericMethod.Return__DistributedDelegate1<T, T2> return__DistributedDelegate = (PreprocessedGenericMethod.Return__DistributedDelegate1<T, T2>)Delegate.CreateDelegate(typeof(PreprocessedGenericMethod.Return__DistributedDelegate1<T, T2>), instance, method);
                return return__DistributedDelegate((T)((object)parameters[0]), (T2)((object)parameters[1]));
            }
        }
        [CompilerGenerated]
        public delegate T Return__DistributedDelegate1<T, T2>(T val, T2 second);
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
        public T Return<T, T2>(T val, T2 second)
        {
            MulticastDelegate d = new PreprocessedGenericMethod.Return__DistributedDelegate1<T, T2>(this.Return__Distributed0<T, T2>);
            return (T)((object)DpmEntrypoint.Invoke(d, new object[]
            {
                val,
                second
            }));
        }
        public PreprocessedGenericMethod()
        {
            DpmEntrypoint.Construct(this);
        }
        [CompilerGenerated]
        private T Return__Distributed0<T, T2>(T val, T2 second)
        {
            return val;
        }
    }
}
