namespace Dx.Runtime.Tests.Data
{
    public class SynchronisedSimpleFieldTest
    {
        [Synchronised]
        private int Private;
        
        [Synchronised]
        protected int Protected;
        
        [Synchronised]
        public int Public;
        
        public int GetPrivate() { return this.Private; }
        public void SetPrivate(int value) { this.Private = value; }
        public int GetProtected() { return this.Protected; }
        public void SetProtected(int value) { this.Protected = value; }
    }
}

