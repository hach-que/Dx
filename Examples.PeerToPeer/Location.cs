using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4;
using Process4.Attributes;

namespace Examples.PeerToPeer
{
    [Distributed]
    public class Location
    {
        public string Name { get; set; }

        public Location(string name)
        {
            this.Name = name;
        }
    }
}
