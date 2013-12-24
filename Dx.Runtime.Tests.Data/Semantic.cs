namespace Dx.Runtime.Tests.Data
{
    [Distributed]
    public class Semantic
    {
        [Local]
        public string Value { get; set; }

        public string GetException()
        {
            return Value;
        }

        [ClientIgnorable]
        public string GetIgnore()
        {
            return Value;
        }

        [ClientCallable]
        public string GetValue()
        {
            return Value;
        }
    }
}
