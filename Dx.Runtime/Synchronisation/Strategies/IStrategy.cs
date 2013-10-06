namespace Dx.Runtime
{
    public interface IStrategy
    {
        void Apply(ISynchronised synchronised, SynchronisationStore store, string name);
    }
}

