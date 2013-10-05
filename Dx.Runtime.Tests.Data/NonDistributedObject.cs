using System;

namespace Dx.Runtime.Tests.Data
{
    public class NonDistributedObject : BaseType
    {
        public NonDistributedObject() : base(new OtherType())
        {
        }
    }
}

