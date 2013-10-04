using System;
using System.Collections.Generic;

namespace Dx.Runtime
{
    public class EntriesRequestedEventArgs : EventArgs
    {
        private Dictionary<ID, object> p_Entries = new Dictionary<ID,object>();

        public Dictionary<ID, object> Entries
        {
            get { return this.p_Entries; }
        }
    }
}

