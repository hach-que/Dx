using System.Collections.Generic;

namespace Dx.Runtime
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> before, IEnumerable<T> after)
        {
            foreach (var item in before)
                yield return item;
            foreach (var item in after)
                yield return item;
        }
    }
}
