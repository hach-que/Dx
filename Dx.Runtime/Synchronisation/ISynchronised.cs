namespace Dx.Runtime
{
    public interface ISynchronised
    {
        SynchronisationStore GetSynchronisationStore(ILocalNode node, string name);
    }
}

