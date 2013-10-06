using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Dx.Process
{
    public class DefaultTypeBuilder : ITypeBuilder
    {
        public TypeDefinition CreateClass(ModuleDefinition module, string @namespace, string name, TypeReference baseType = null)
        {
            if (baseType == null)
                baseType = module.Import(typeof(object));
            
            var newType = new TypeDefinition(
                @namespace,
                name,
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Public,
                baseType);
            module.Types.Add(newType);
            return newType;
        }
        
        public TypeDefinition CreateNestedClass(TypeDefinition parent, string @namespace, string name, TypeReference baseType = null)
        {
            if (baseType == null)
                baseType = parent.Module.Import(typeof(object));
            
            var newType = new TypeDefinition(
                @namespace,
                name,
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.NestedPublic,
                baseType);
            parent.NestedTypes.Add(newType);
            return newType;
        }

        public TypeDefinition CreateInterface(ModuleDefinition module, string @namespace, string name)
        {
            var newType = new TypeDefinition(
                @namespace,
                name,
                TypeAttributes.Interface | TypeAttributes.Public);
            module.Types.Add(newType);
            return newType;
        }
    }
}

