using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Remoting
{
    public class ObjectVanishedException : NullReferenceException
    {
        /// <summary>
        /// The ID of the object that vanished from the network.
        /// </summary>
        public string ID { get; private set; }

        public ObjectVanishedException(string id)
            : base("The object with ID '" + id + "' has vanished from the network (likely due to a node leaving).")
        {
            this.ID = id;
        }

        public ObjectVanishedException(string id, Exception innerException)
            : base("The object with ID '" + id + "' has vanished from the network (likely due to a node leaving).", innerException)
        {
            this.ID = id;
        }
    }
}
