Distributed Extensions for .NET
=====================================

Automatically distribute your code with a single attribute, like so:

```csharp
namespace Application.PeerToPeer
{
    [Distributed(Mode.PeerToPeer)]
    class Program
    {
        public static void Main(string[] args)
        {
            // Set up distributed node.
            LocalNode node = new LocalNode();
            node.Join();
 
            // Get the universe and increase it's counter.
            Universe universe = new Distributed<Universe>("universe");
            universe.Enter();
 
            // Output the counter.
            Console.WriteLine("The universe counter is now at: " + universe.Counter);
 
            // Wait until the user hits enter.
            Console.WriteLine("Hit enter to quit at any time.");
            Console.ReadKey(true);
            universe.Leave();
 
            // Quit.
            node.Leave();
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

More information can be found at http://dxlibrary.net.  Licensed under MIT.
