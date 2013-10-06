using Mono.Cecil;

namespace Dx.Process
{
    public interface IWrapperFactory
    {
        SynchronisationWrapper CreateSynchronisationWrapper(TypeDefinition type);
    }
}

