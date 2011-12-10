using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examples.ServerClient
{
    public class ChildWorld : World
    {
        /// <summary>
        /// Because ChildWorld inherits from World, the post-processor will
        /// also make this a distributed class (even without the attribute).
        /// </summary>
        public void TestMethod()
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
