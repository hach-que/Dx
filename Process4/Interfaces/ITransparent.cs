using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Interfaces
{
    /// <summary>
    /// Defines required information for distributed objects.  The post-processor
    /// will implement this interface for you.
    /// </summary>
    public interface ITransparent
    {
        string NetworkName { get; set; }
    }
}
