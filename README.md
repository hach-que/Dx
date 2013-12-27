Distributed Extensions for .NET
=====================================

Version 3 Released
-------------

Version 3 has just been released.  This involves a completely rewritten runtime library, which now
uses protocol buffers instead of .NET serialization.

The main points are:

  * Network messaging is now much more reliable.
  * All messages are provided with a checksum to ensure integrity.
  * .NET serialization has been completely replaced with protocol buffers.
  * Network discovery is no longer coupled to the library; it is up to the developer to specify the remote nodes to connect to.
  * Support for distributed events (`events with += and -= `) has been dropped due to non-usage.
  * Various internals have been decoupled and seperated, providing a much better architecture for future improvements.
  * Public APIs are now documented for most classes.

Example Usage
-----------------

Automatically distribute your code with a single attribute, like so:

```csharp
namespace Application.PeerToPeer
{
    class Program
    {
        public static void Main(string[] args)
        {
			if (args.Length == 0)
			{
				Console.WriteLine("usage: Program.exe <local ip address> [<remote ip address>]");
			}

            // Create distributed node and listen on port 9000 for connections.
            var node = new LocalNode();
			node.Bind(IPAddress.Parse(args[0]), 9000);
            
			// If an argument has been supplied on the command line, connect
			// to that remote node as well.
			if (args.Length >= 2)
			{
				node.GetService<IClientConnector>().Connect(IPAddress.Parse(args[1]), 9000);
			}
 
            // Get the universe and increase it's counter.
            Universe universe = new Distributed<Universe>(node, "universe");
            universe.Enter();
 
            // Output the counter.
            Console.WriteLine("The universe counter is now at: " + universe.Counter);
 
            // Wait until the user hits enter.
            Console.WriteLine("Hit enter to quit at any time.");
            Console.ReadKey(true);
            universe.Leave();
 
            // Quit.
            node.Close();
            return;
        }
    }
 
    [Distributed]
    public class Universe
    {
        /// <summary>
        /// The universe's counter.
        /// </summary>
        public int Counter { get; private set; }
 
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
```
