using System.Reflection;

namespace Dx.Runtime
{
    public interface IDirectInvoke
    {
        object Invoke(MethodInfo method, object instance, object[] parameters);
    }
}
