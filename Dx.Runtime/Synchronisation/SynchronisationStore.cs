using System;

namespace Dx.Runtime
{
    [Distributed]
    public class SynchronisationStore
    {
        public virtual string[] GetNames()
        {
            return new string[0];
        }
        
        public virtual Type[] GetTypes()
        {
            return new Type[0];
        }
        
        public virtual bool[] GetIsFields()
        {
            return new bool[0];
        }
    }
}

