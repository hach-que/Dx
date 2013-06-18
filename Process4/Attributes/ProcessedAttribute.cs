using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Attributes
{
    /// <summary>
    /// An attribute which indicates that the specified class has
    /// been processed by the post-processor.  Do not add this
    /// attribute to your own classes.
    /// </summary>
    public class ProcessedAttribute : Attribute
    {
    }
}
