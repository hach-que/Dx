using Mono.Cecil;

namespace Dx.Process
{
    public interface IMethodBuilder
    {
        MethodDefinition CreateConstructor(TypeDefinition type, params TypeReference[] parameters);
        MethodDefinition CreateNonvirtual(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters);
        MethodDefinition CreateVirtual(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters);
        MethodDefinition CreateOverride(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters);
    }
}

