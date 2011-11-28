using System;
using System.Collections.Generic;
using System.Linq;
using Process4;
using Process4.Collections;
using Process4.Attributes;
using Process4.Remoting;

namespace Examples.PeerToPeer
{
    [Distributed(Architecture.PeerToPeer)]
    class Program
    {
        public static void Main(string[] args)
        {
            // Set up distributed node.
            LocalNode node = new LocalNode();
            node.Join();

            // Get the universe and increase it's counter.
            Universe universe = new Distributed<Universe>("universe");
            universe.Entered += (sender, e) =>
            {
                // Output the counter whenever a node enters the network.
                Console.WriteLine("The universe counter is now at: " + universe.Counter);
            };
            universe.Left += (sender, e) =>
            {
                // Output the counter whenever a node leaves the network.
                Console.WriteLine("The universe counter is now at: " + universe.Counter);
            };
            universe.Enter();

            // Add some random items.
            Program.AddRandomItems(universe);
            
            // Print a list of items in the universe.
            foreach (Item i in universe.Inventory)
            {
                try
                {
                    Console.WriteLine(" * " + i);
                }
                catch (ObjectVanishedException)
                {
                    Console.WriteLine(" * <vanished>");
                }
            }

            // Wait until the user hits enter.
            Console.WriteLine("Hit enter to quit at any time.");
            Console.ReadKey(true);
            universe.Leave();

            // Quit.
            node.Leave();
            return;
        }

        public static void AddRandomItems(Universe universe)
        {
            string[] items = new string[]
                {
                    "sword",
                    "ship",
                    "planet",
                    "laser",
                    "sun",
                    "star",
                    "wormhole",
                    "box",
                    "container",
                    "key",
                    "player",
                };
            Random r = new Random();
            int i1 = r.Next(items.Length);
            int i2 = r.Next(items.Length);

            universe.Inventory.Store(new Item(items[i1]));
            universe.Inventory.Store(new Item(items[i2]));
        }
    }

}
