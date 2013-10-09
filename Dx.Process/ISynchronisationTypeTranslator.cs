using Mono.Cecil;

namespace Dx.Process
{
    /// <summary>
    /// Converts a given type to it's equivalent distributed type.  Synchronisation
    /// implements backing properties on a nested class and marks them for
    /// distributed processing; thus some types (such as lists and stacks) have to
    /// translated to their distributed equivalents.
    /// </summary>
    public interface ISynchronisationTypeTranslator
    {
        TypeReference GetDistributedType(TypeReference original);
    }
}

