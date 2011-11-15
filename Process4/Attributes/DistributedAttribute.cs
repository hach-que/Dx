using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Attributes
{
    public class DistributedAttribute : Attribute
    {
        /// <summary>
        /// Specifies that the class should be distributed within a network.
        /// </summary>
        public DistributedAttribute()
        {
        }

        /// <summary>
        /// Specifies the type of distributed program that should be used.  If this is
        /// not declared on the Program class, the program will not be distributed.
        /// </summary>
        /// <param name="m">The type of distribution.</param>
        public DistributedAttribute(Mode m)
        {
        }
    }

    public enum Mode
    {
        Unknown,

        /// <summary>
        /// Specifies that the program should execute in master-slave mode.  In this mode, the first program
        /// that starts is designated as a master, and all other programs execute as slaves.  The program's
        /// entry point is only executed once regardless of the number nodes in the system.
        /// </summary>
        MasterSlave,

        /// <summary>
        /// Specifies that the program should execute in peer-to-peer mode.  In this mode, all programs are
        /// equal and execute their entry points regardless of whether they are the first node in the network.
        /// </summary>
        PeerToPeer
    }
}
