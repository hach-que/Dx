using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Process4.Interfaces
{
    public interface IDirectInvoke
    {
        object Invoke(MethodInfo method, object instance, object[] parameters);
    }
}
