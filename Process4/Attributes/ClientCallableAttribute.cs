using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Attributes
{
    /// <summary>
    /// Used in master-slave networks when all properties and methods are
    /// private by default.  Specify this attribute on methods to indicate
    /// that slaves are permitted to call it.  There is no way to indicate
    /// that slaves are permitted to access or set properties (do this
    /// via methods which have checks to ensure that they're setting values
    /// correctly).
    /// </summary>
    public class ClientCallableAttribute : Attribute
    {
    }
}
