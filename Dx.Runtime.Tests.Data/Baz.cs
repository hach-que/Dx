namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class Baz
    {
        public Foo MyFoo { get; set; }
        
        public void SetupTestChain(string data)
        {
            this.MyFoo = new Foo();
            this.MyFoo.MyBar = new Bar();
            this.MyFoo.MyBar.OtherString = data;
        }
    }
}

