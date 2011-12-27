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

        public SealedTest_Compare()
		{
			DpmEntrypoint.Construct(this);
		}

        [CompilerGenerated]
        private void Test__Distributed0<T>()
        {
            T t = default(T);
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
