using System;
using System.Collections.Generic;
using System.Linq;
using Process4;
using Process4.Collections;
using Process4.Attributes;
using Process4.Remoting;
using System.Threading;

namespace Examples.ServerClient
{
    [Distributed(Architecture.ServerClient, Caching.PushOnChange)]
    class Program
    {
        public static void Main(string[] args)
        {
            // Create a vector and manual vector both of the same.
            Vector v = new Vector(2, 4, 5);
            ManualVector mv = new ManualVector(1, 2, 3);
            Console.WriteLine("Vector " + v);
            Console.WriteLine("ManualVector " + mv);

            // Set up distributed node.
            LocalNode node = new LocalNode();
            node.Join();

            // Get the world.
            World world = new Distributed<World>("world");
            
            // Create a thread to run our main loop in (so we can still
            // monitor when the users want to quit).
            Thread thread;

            // Determine if we are running as a client or a server.
            if (node.IsServer)
            {
                // Running as a server.
                Console.WriteLine("Running as server.");
                thread = new Thread(Program.RunServer);
            }
            else
            {
                // Running as a client.
                Console.WriteLine("Running as client.");
                thread = new Thread(Program.RunClient);
            }

            // Wait until the user wants to quit.
            thread.Start(world);
            Console.WriteLine("Hit enter to quit at any time.");
            Console.ReadKey(true);
            thread.Abort();

            // Quit.
            node.Leave();
            return;
        }

        /// <summary>
        /// The server's main loop.
        /// </summary>
        public static void RunServer(object obj)
        {
            // Cast the world.
            World world = obj as World;
            
            // Create a random number generator for the
            // population generator.
            Random rand = new Random();

            // Update the world population every second.
            while (true)
            {
                world.Population += rand.Next(10000) - 5000;
                Console.WriteLine("Population is now " + world.Population + ".");
                Thread.Sleep(1000);
            }
        }

        public static void RunClient(object obj)
        {
            // Cast the world.
            World world = obj as World;
            
            // Register the event handler (method 1).
            world.PopulationChanged += (sender, e) =>
                {
                    Console.WriteLine("Population has changed to " + world.Population + " (via event handler).");
                };

            // Test the Ping method on the world.
            world.Ping();

            // Loop forever reading the population property (method 2).
            // This doesn't throttle the network since the property
            // is cached on clients due to the PushOnChange caching
            // mechanism (don't do something like this on other caching
            // modes).
            int old = world.Population;
            while (true)
            {
                if (world.Population != old)
                {
                    Console.WriteLine("Population has changed to " + world.Population + " (via loop).");
                    old = world.Population;
                }
                Thread.Sleep(100);
            }
        }
    }

}
