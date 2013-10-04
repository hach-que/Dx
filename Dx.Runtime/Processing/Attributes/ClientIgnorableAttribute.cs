using System;

namespace Dx.Runtime
{
    /// <summary>
    /// Used in master-slave networks when all properties and methods are
    /// private by default.  Specify this attribute on methods to indicate
    /// that when slaves call this method it should be ignored, with the
    /// default value returned.
    /// </summary>
    public class ClientIgnorableAttribute : Attribute
    {
    }
}
