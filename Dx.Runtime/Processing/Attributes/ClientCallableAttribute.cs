using System;

namespace Dx.Runtime
{
    /// <summary>
    /// Used in master-slave or server-client networks when all properties and
    /// methods are private by default.  Specify this attribute on methods to
    /// indicate that slaves are permitted to call it.  Slaves can always get
    /// properties, but there is no way to indicate that slaves are permitted
    /// to set properties (do this via methods which have checks to ensure that
    /// they're setting values correctly).
    /// </summary>
    public class ClientCallableAttribute : Attribute
    {
    }
}
