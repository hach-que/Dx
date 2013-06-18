using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;
using Process4.Providers;
using Process4.Interfaces;
using System.Reflection;

namespace Examples.ServerClient
{
    public class ManualVector
    {
        private class get_X__InvokeDirect0 : IDirectInvoke
        {
            public delegate double get_X__DistributedDelegate1();
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1 get_X__DistributedDelegate = (ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1)Delegate.CreateDelegate(typeof(ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1), instance, method);
                return get_X__DistributedDelegate();
            }
        }

        private class set_X__InvokeDirect1 : IDirectInvoke
        {
            public delegate void set_X__DistributedDelegate2(double value);
            public object Invoke(MethodInfo method, object instance, object[] parameters)
            {
                ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2 set_X__DistributedDelegate = (ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2)Delegate.CreateDelegate(typeof(ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2), instance, method);
                set_X__DistributedDelegate((double)parameters[0]);
                return null;
            }
        }

        private double X_backing;
        private double Y_backing;
        private double Z_backing;

        public double X
        {
            get
            {
                MulticastDelegate d = new ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1(this.get_X_backing);
                object[] args = new object[0];
                return (double)DpmEntrypoint.GetProperty(d, args);
            }
            private set
            {
                MulticastDelegate d = new ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2(this.set_X_backing);
                DpmEntrypoint.SetProperty(d, new object[]
	            {
		            value
	            });
            }
        }

        public double Y
        {
            get
            {
                MulticastDelegate d = new ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1(this.get_Y_backing);
                object[] args = new object[0];
                return (double)DpmEntrypoint.GetProperty(d, args);
            }
            private set
            {
                MulticastDelegate d = new ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2(this.set_Y_backing);
                DpmEntrypoint.SetProperty(d, new object[]
	            {
		            value
	            });
            }
        }

        public double Z
        {
            get
            {
                MulticastDelegate d = new ManualVector.get_X__InvokeDirect0.get_X__DistributedDelegate1(this.get_Z_backing);
                object[] args = new object[0];
                return (double)DpmEntrypoint.GetProperty(d, args);
            }
            private set
            {
                MulticastDelegate d = new ManualVector.set_X__InvokeDirect1.set_X__DistributedDelegate2(this.set_Z_backing);
                DpmEntrypoint.SetProperty(d, new object[]
	            {
		            value
	            });
            }
        }

        public ManualVector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        private double get_X_backing()
        {
            return this.X_backing;
        }

        private void set_X_backing(double value)
        {
            this.X_backing = value;
        }

        private double get_Y_backing()
        {
            return this.Y_backing;
        }

        private void set_Y_backing(double value)
        {
            this.Y_backing = value;
        }

        private double get_Z_backing()
        {
            return this.Z_backing;
        }

        private void set_Z_backing(double value)
        {
            this.Z_backing = value;
        }

        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + "," + this.Z + ")";
        }
    }
}
