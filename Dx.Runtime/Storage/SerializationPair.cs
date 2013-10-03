using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data4
{
    public class SerializationData
    {
        public Dht Dht { get; set; }
        /* Data4 doesn't know about IStorageProvider, so we allow Process4 to store a reference to it here */
        public object Storage { get; set; }
        public object Root { get; set; }
        public Entry Entry { get; set; }
        public bool IsMessage { get; set; }
    }
}
