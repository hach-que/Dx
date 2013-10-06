using System;

namespace Dx.Runtime
{
    public class AssignmentStrategy : IStrategy
    {
        public void Apply(ISynchronised synchronised, SynchronisationStore store, string name)
        {
           // if (store.Data == null)
           //     store.Data = this;
        }
    }
}

