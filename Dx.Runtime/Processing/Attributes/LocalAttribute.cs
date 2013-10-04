using System;

namespace Dx.Runtime
{
    /// <summary>
    /// Specifies that this property, method or field always resides locally
    /// on the machine and is never shared between computers.  Useful for
    /// server-client architectures since it allows you to include non-distributed
    /// data inside distributed classes.
    /// </summary>
    public class LocalAttribute : Attribute
    {
    }
}
