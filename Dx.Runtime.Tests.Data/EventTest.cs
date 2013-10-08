using System;

namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class EventTest
    {
        public event EventHandler Test;
        
        public void Fire()
        {
            if (this.Test != null)
                this.Test(this, new EventArgs());
        }
    }
}

