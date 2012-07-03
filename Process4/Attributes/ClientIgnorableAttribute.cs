using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Attributes
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
