using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;
using Process4;
using Process4.Collections;

namespace Examples.PeerToPeerSigned
{
    [Distributed(Architecture.PeerToPeer, Caching.StoreOnDemand)]
    class Program
    {
        static void Main(string[] args)
        {
            // Set up distributed node.
            LocalNode node = new LocalNode();
            node.Join();

            Console.WriteLine("Distributed peer-to-peer example with signing.");
            Console.WriteLine("Commands are 'add', 'get', 'test' and 'quit'.");
            bool running = true;
            while (running)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();
                string key, value;

                switch (cmd.Trim().ToLower())
                {
                    case "add":
                        // Ask the user to create a new distributed object.
                        Console.WriteLine("You are going to add a new entry to the network.");
                        Console.Write("Key: ");
                        key = Console.ReadLine().Trim().ToLower();
                        Console.Write("Value: ");
                        value = Console.ReadLine();

                        // Create a new instance of the distributed value.
                        Distributed<StringValue> str = new Distributed<StringValue>(key);
                        ((StringValue)str).Value = value;
                        str.PushImmutable();
                        Console.WriteLine("Immutable string object has been pushed to network.");
                        break;
                    case "get":
                        Console.WriteLine("You are going to get an entry from the network.");
                        Console.Write("Key: ");
                        key = Console.ReadLine().Trim().ToLower();

                        StringValue str2 = Distributed<StringValue>.PullImmutable(key);
                        Console.WriteLine("Value is '" + str2.Value + "'.");
                        break;
                    case "test":
                        Console.WriteLine("You are going to test immutability of an entry in the network.");
                        Console.Write("Key: ");
                        key = Console.ReadLine().Trim().ToLower();

                        StringValue str3 = Distributed<StringValue>.PullImmutable(key);
                        string expected = str3.Value;
                        try
                        {
                            str3.Value = "somethingelse";
                        }
                        catch (InvalidOperationException) { }
                        Console.WriteLine("Value is '" + str3.Value + "' (should be '" + expected + "').");
                        break;
                    case "quit":
                        Console.WriteLine("If there is another node in the network, you should be able to query for added values and get them back after restart.");
                        Console.WriteLine("Press any key to quit.");
                        Console.ReadLine();
                        running = false;
                        break;
                }
            }
            
            // Leave the network.
            node.Leave();
        }
    }
}
