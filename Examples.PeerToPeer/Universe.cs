using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.PeerToPeer
{
    [Distributed]
    public class Universe
    {
        /// <summary>
        /// The universe's counter.
        /// </summary>
        public int Counter { get; private set; }

        /// <summary>
        /// The inventory of the universe.
        /// </summary>
        public Inventory Inventory { get; private set; }

        public Universe()
        {
            this.Inventory = new Inventory();
        }

        public void Enter()
        {
            this.Counter += 1;
        }

        public void Leave()
        {
            this.Counter -= 1;
        }
    }
}
