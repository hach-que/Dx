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
            // Test generics.
            Program.GenericsTest();

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

        private static void GenericsTest()
        {
            SealedTest st = new SealedTest();
            st.Test<int>();
            st.TestPlain<int>();
            st.Test2<Tile>();
            st.Test3<Something>();

            GenericsTest<Tile, IntTest> gt = new GenericsTest<Tile, IntTest>();
            // Delegate just needs the <A, B> parameters passed to it in it's definition
            // so that it matches the definition in Genericstest`2
            try { gt.Method0(); } catch (Exception e) { Console.WriteLine("Method0: " + e.Message); }
            try { gt.Method1(); } catch (Exception e) { Console.WriteLine("Method1: " + e.Message); }
            try { gt.Method2(); } catch (Exception e) { Console.WriteLine("Method2: " + e.Message); }
            /*try { */gt.Method3<string>();/* } catch (Exception e) { Console.WriteLine("Method3: " + e.Message); }*/
            try { gt.Method4<Tile>(); } catch (Exception e) { Console.WriteLine("Method4: " + e.Message); }
            try { gt.Method5<Tile>(); } catch (Exception e) { Console.WriteLine("Method5: " + e.Message); }
            try { gt.Method6<int>(); } catch (Exception e) { Console.WriteLine("Method6: " + e.Message); }
            try { gt.Method7<string>("blah"); } catch (Exception e) { Console.WriteLine("Method7: " + e.Message); }
            try { gt.Method8<string, int>(); } catch (Exception e) { Console.WriteLine("Method8: " + e.Message); }
            try { gt.Method9<string, int>(); } catch (Exception e) { Console.WriteLine("Method9: " + e.Message); }
            try { gt.Method10<Tile, int>(new Tile()); } catch (Exception e) { Console.WriteLine("Method10: " + e.Message); }
            try { gt.Method11<Tile, int>(5); } catch (Exception e) { Console.WriteLine("Method11: " + e.Message); }
            try { gt.Method12<Tile, int>(new Tile(), 5); } catch (Exception e) { Console.WriteLine("Method12: " + e.Message); }
            try { gt.Method13<Tile>(new IntTest(5)); } catch (Exception e) { Console.WriteLine("Method13: " + e.Message); }
            //try { gt.Method14<List<int>, int, Tile, Something>(new Tile(), new Something(), 7, new List<int> { 5, 6, 8 }); } catch (Exception e) { Console.WriteLine("Method14: " + e.Message); }
            try { gt.Method15(); } catch (Exception e) { Console.WriteLine("Method15: " + e.Message); }
            try { gt.Method16<string>(); } catch (Exception e) { Console.WriteLine("Method16: " + e.Message); }
            try { gt.Method17<double>(); } catch (Exception e) { Console.WriteLine("Method17: " + e.Message); }

            // 3 - 12 currently failing due to unknown method.
            // 14 currently failing due to invalid program format.

            // 13 succeeds because it returns a string value (not a generic parameter).
            // 15 - 17 succeeds because they return a generic instance (not a generic parameter).
            //
            // CONTINUE: PROBLEM DESCRIPTION
            // Source of problem is line 90 in Utility.cs.  DistributedDelegate is not getting a return type
            // that is valid when the return type is a generic parameter owned by the method (rather than the
            // type container).
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
