using Mono.Cecil;
using System;

namespace Dx.Process
{
    public class DefaultMethodBuilder : IMethodBuilder
    {
        public MethodDefinition CreateConstructor(TypeDefinition type, params TypeReference[] parameters)
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.CompilerControlled |
                    MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                    MethodAttributes.RTSpecialName,
                type.Module.Import(typeof(void)));
            ctor.Body.InitLocals = true;
            
            var i = 0;
            foreach (var p in parameters)
            {
                ctor.Parameters.Add(new ParameterDefinition(
                    "param" + (i++),
                    ParameterAttributes.None,
                    p));
            }
            
            type.Methods.Add(ctor);
            return ctor;
        }

        public MethodDefinition CreateNonvirtual(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters)
        {
            throw new NotImplementedException();
        }

        public MethodDefinition CreateVirtual(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters)
        {
            throw new NotImplementedException();
        }

        public MethodDefinition CreateOverride(TypeDefinition type, string name, TypeReference returnType, params TypeReference[] parameters)
        {
            var @override = new MethodDefinition(
                name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                returnType);
            @override.Body.InitLocals = true;
            
            var i = 0;
            foreach (var p in parameters)
            {
                @override.Parameters.Add(new ParameterDefinition(
                    "param" + (i++),
                    ParameterAttributes.None,
                    p));
            }
            
            type.Methods.Add(@override);
            return @override;
        }
    }
}

