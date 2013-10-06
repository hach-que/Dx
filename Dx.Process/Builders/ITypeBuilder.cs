using Mono.Cecil;
using System.Collections.Generic;

namespace Dx.Process
{
    public interface ITypeBuilder
    {
        TypeDefinition CreateClass(ModuleDefinition module, string @namespace, string name, TypeReference baseType = null);
        TypeDefinition CreateNestedClass(TypeDefinition parent, string @namespace, string name, TypeReference baseType = null);
        TypeDefinition CreateInterface(ModuleDefinition module, string @namespace, string name);
    }
}

