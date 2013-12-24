using System;
using System.Collections;

namespace Dx.Runtime
{
    public static class GraphWalker
    {
        public static void Apply(object result, ILocalNode localNode)
        {
            if (result != null)
            {
                if (result is ITransparent)
                    (result as ITransparent).Node = localNode;
                else if (result.GetType().IsArray)
                {
                    if (typeof(ITransparent).IsAssignableFrom(result.GetType().GetElementType()))
                    {
                        foreach (var elem in (IEnumerable)result)
                        {
                            if (elem != null)
                                (elem as ITransparent).Node = localNode;
                        }
                    }
                    else if (!result.GetType().GetElementType().IsValueType && result.GetType() != typeof(string))
                        throw new InvalidOperationException("Unable to assign local node to result data for " + result.GetType().GetElementType().FullName);
                }
                else if (!result.GetType().IsValueType && result.GetType() != typeof(string))
                    throw new InvalidOperationException("Unable to assign local node to result data for " + result.GetType().FullName);
            }
        }
    }
}

