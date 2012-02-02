using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using Process4.Attributes;

namespace Examples.PeerToPeerSigned
{
    [Distributed]
    public class StringValue : IImmutable
    {
        public string Value { get; set; }

        #region IImmutable Members

        public bool AcceptKey(string key)
        {
            // We accept any form of storage key.  In properly signed peer-to-peer networks you would
            // want to ensure that the key forms some kind of signature hash of the actual object data
            // so that the local DHT won't cache invalid information.
            return true;
        }

        #endregion
    }
}
