using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.PeerToPeer
{
    [Distributed]
    public class World
    {
        private int m_Population { get; set; }

        /// <summary>
        /// The population of the world.
        /// </summary>
        public int Population
        {
            get
            {
                return this.m_Population;
            }
            set
            {
                this.m_Population = value;
                if (this.PopulationChanged != null)
                    this.PopulationChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// An event that is raised when the population property changes.
        /// </summary>
        public event EventHandler PopulationChanged;

        public event EventHandler SomeMagicHandler
        {
            add
            {
                Console.WriteLine("Magic handler has been assigned.");
                this.PopulationChanged += value;
            }
            remove
            {
                Console.WriteLine("Magic handler has been deassigned.");
                this.PopulationChanged -= value;
            }
        }

        public World()
        {
            this.Population = 6000000;
        }

        /// <summary>
        /// Pings the server; causes the server to write a message to the
        /// console when called.
        /// </summary>
        [ClientCallable]
        public void Ping()
        {
            Console.WriteLine("The server has been pinged!");
        }
    }
}
