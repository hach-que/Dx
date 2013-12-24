using System.Collections.Generic;
using System.Linq;
using System;

namespace Dx.Runtime
{
    public class DefaultObjectStorage : IObjectStorage
    {
        private readonly List<LiveEntry> m_Entries = new List<LiveEntry>();

        public IEnumerable<LiveEntry> Find(ID key)
        {
            lock (this.m_Entries)
            {
                return this.m_Entries.Where(x => x.Key == key).ToArray();
            }
        }

        public void UpdateOrPut(LiveEntry liveEntry)
        {
            lock (this.m_Entries)
            {
                var existing = this.m_Entries.Where(x => x.Key == liveEntry.Key).ToArray();
                this.m_Entries.Add(liveEntry);
                foreach (var e in existing)
                    this.m_Entries.Remove(e);
            }
        }

        public void Put(LiveEntry liveEntry)
        {
            if (liveEntry == null)
            {
                throw new ArgumentNullException("serializedEntry");
            }

            lock (this.m_Entries)
            {
                this.m_Entries.Add(liveEntry);
            }
        }

        public void Remove(ID key)
        {
            lock (this.m_Entries)
            {
                this.m_Entries.RemoveAll(x => x.Key == key);
            }
        }
    }
}
