using System;

namespace Dx.Runtime
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
