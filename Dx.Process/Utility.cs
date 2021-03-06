using System;
using System.Linq;
using Dx.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using ProtoBuf;

namespace Dx.Process
{
    internal static class Utility
    {
        /// <summary>
        /// Emits the definition of a delegate into the assembly based on the definition of a local method.
        /// </summary>
        /// <remarks>Based on code from http://markmail.org/thread/rqbgst2kqgdv33uy#query:related%3Arqbgst2kqgdv33uy+page:1+mid:rqbgst2kqgdv33uy+state:results. </remarks>
        /// <param name="processor">The IL processor on which to define the delegate in.</param>
        /// <param name="sourceMethod">The local method on which to base the delegate.</param>
        /// <param name="delegateVariable">The delegate variable to define (in the local method).</param>
        /// <param name="delegateCtor">The delegate constructor that will be used to create new instances of the delegate.</param>
        public static TypeDefinition EmitDelegate(ILProcessor processor, TypeDefinition sourceType, MethodDefinition sourceMethod, TypeReference originalType, out VariableDefinition delegateVariable, out MethodReference delegateCtor)
        {
            var delegateEmitter = new DefaultDelegateEmitter();
            var definition = delegateEmitter.EmitDelegate(processor.Body.Method, out delegateCtor);
            
            var type_MulticastDelegate = processor.Body.Method.Module.Import(typeof(MulticastDelegate));
            delegateVariable = new VariableDefinition("d", type_MulticastDelegate);
            processor.Body.Variables.Insert(0, delegateVariable);
            
            return definition;
        }

        /// <summary>
        /// Adds the specified attribute to the specified type.
        /// </summary>
        /// <param name="type">The type to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        public static CustomAttribute AddAttribute(TypeDefinition type, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            var ca = new CustomAttribute(rf);
            type.CustomAttributes.Add(ca);
            return ca;
        }

        /// <summary>
        /// Adds the specified attribute to the specified type.
        /// </summary>
        /// <param name="prop">The type to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        public static CustomAttribute AddAttribute(PropertyDefinition prop, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            var customAttribute = new CustomAttribute(rf);
            prop.CustomAttributes.Add(customAttribute);
            return customAttribute;
        }

        /// <summary>
        /// Adds the specified attribute to the specified field.
        /// </summary>
        /// <param name="field">The field to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        /// <param name="module">The module the field is defined in.</param>
        public static CustomAttribute AddAttribute(FieldDefinition field, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            var customAttribute = new CustomAttribute(rf);
            field.CustomAttributes.Add(customAttribute);
            return customAttribute;
        }

        /// <summary>
        /// Adds the specified attribute to the specified method.
        /// </summary>
        /// <param name="field">The method to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        /// <param name="module">The module the field is defined in.</param>
        public static void AddAttribute(MethodDefinition method, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            method.CustomAttributes.Add(new CustomAttribute(rf));
        }

        /// <summary>
        /// Adds an automatic property to the specified class with the specified property name and type.
        /// </summary>
        public static void AddAutoProperty(TypeDefinition t, string name, Type type, int protoMember = 0)
        {
            AddAutoProperty(t, name, t.Module.Import(type), protoMember);
        }
        
        /// <summary>
        /// Adds an automatic property to the specified class with the specified property name and type.
        /// </summary>
        public static void AddAutoProperty(TypeDefinition t, string name, TypeReference typeRef, int protoMember = 0)
        {
            // Create the field definition.
            FieldDefinition fd = new FieldDefinition("<" + name + ">k__BackingField", FieldAttributes.Private, typeRef);
            Utility.AddAttribute(fd, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), t.Module);

            // Define the getter and setter methods.
            MethodAttributes attribs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName
                                        | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            MethodDefinition mgd = new MethodDefinition("get_" + name, attribs, typeRef);
            MethodDefinition msd = new MethodDefinition("set_" + name, attribs, t.Module.Import(typeof(void)));
            mgd.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            mgd.Body.Variables.Add(new VariableDefinition(typeRef));
            mgd.Body.InitLocals = true;
            msd.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            msd.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, typeRef));
            Utility.AddAttribute(mgd, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), t.Module);
            Utility.AddAttribute(msd, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), t.Module);

            // Add the IL to the getter method.
            ILProcessor mgdi = mgd.Body.GetILProcessor();
            mgdi.Append(Instruction.Create(OpCodes.Ldarg_0));
            mgdi.Append(Instruction.Create(OpCodes.Ldfld, fd));
            mgdi.Append(Instruction.Create(OpCodes.Stloc_0));
            Instruction ldloc = Instruction.Create(OpCodes.Ldloc_0); mgdi.Append(ldloc);
            mgdi.InsertBefore(ldloc, Instruction.Create(OpCodes.Br_S, ldloc));
            mgdi.Append(Instruction.Create(OpCodes.Ret));

            // Add the IL to the setter method.
            ILProcessor msdi = msd.Body.GetILProcessor();
            msdi.Append(Instruction.Create(OpCodes.Ldarg_0));
            msdi.Append(Instruction.Create(OpCodes.Ldarg_1));
            msdi.Append(Instruction.Create(OpCodes.Stfld, fd));
            msdi.Append(Instruction.Create(OpCodes.Ret));

            // Create the property definition.
            PropertyDefinition pd = new PropertyDefinition(name, PropertyAttributes.None, typeRef);
            pd.HasThis = true;
            pd.GetMethod = mgd;
            pd.SetMethod = msd;

            // Now add all of the above to the type.
            t.Fields.Add(fd);
            t.Methods.Add(mgd);
            t.Methods.Add(msd);
            t.Properties.Add(pd);

            if (protoMember != 0)
            {
                Utility.AddProtoMemberAttribute(pd, protoMember);
            }
        }

        /// <summary>
        /// Fixes a potential generic argument so that it's owner is changed (this method does not
        /// adjust types or offsets of generic parameters).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newOwner"></param>
        /// <returns></returns>
        public static TypeReference FixPotentialGenericArgument(TypeReference type)
        {
            if (type is GenericParameter)
            {
                GenericParameter gp = type as GenericParameter;
                return new GenericParameter(gp.Position, gp.Type, type.Module);
            }
            else if (type is GenericInstanceType)
            {
                GenericInstanceType git = type as GenericInstanceType;
                GenericInstanceType ngit = new GenericInstanceType(git.ElementType);
                foreach (GenericParameter gp in git.GenericParameters)
                    ngit.GenericParameters.Add(new GenericParameter(gp.Name, ngit));
                foreach (TypeReference tr in git.GenericArguments)
                    ngit.GenericArguments.Add(Utility.FixPotentialGenericArgument(tr));
                return ngit;
            }
            else
                return type;
        }

        /// <summary>
        /// Updates a generic instance's references to method generic parameters such that they
        /// refer to a type generic parameter instead (with the offset determined by the number
        /// of generic parameters in the original type). 
        /// </summary>
        /// <param name="original">The original type that contains the type-level parameters.</param>
        /// <param name="git">The generic instance type to update.</param>
        /// <returns></returns>
        public static TypeReference RewriteGenericReferencesToType(TypeReference original, GenericInstanceType git)
        {
            GenericInstanceType ngit = new GenericInstanceType(git.ElementType);
            foreach (GenericParameter gp in git.GenericParameters)
                ngit.GenericParameters.Add(new GenericParameter(gp.Name, ngit));
            foreach (TypeReference tr in git.GenericArguments)
            {
                if (tr is GenericParameter)
                {
                    // We fix this reference since it'll be a method generic
                    // instead of the type generic.
                    GenericParameter ngp = new GenericParameter(
                        (tr as GenericParameter).Type == GenericParameterType.Type ?
                        (tr as GenericParameter).Position :
                        (tr as GenericParameter).Position + original.GenericParameters.Count,
                        GenericParameterType.Type,
                        original.Module);
                    ngit.GenericArguments.Add(ngp);
                }
                else if (tr is GenericInstanceType)
                {
                    // This is another generic instance which may further contain
                    // more generic parameters that need to be fixed.
                    ngit.GenericArguments.Add(Utility.RewriteGenericReferencesToType(original, tr as GenericInstanceType));
                }
                else
                    // Standard type that can be simply added.
                    ngit.GenericArguments.Add(tr);
            }
            return ngit;
        }

        public static TypeReference MakeGenericType(TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                DeclaringType = Utility.MakeGenericType(self.DeclaringType, arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        public static bool HasAttribute(TypeDefinition type, string name)
        {
            while (type != null)
            {
                if (HasAttributeSpecific(type, name))
                    return true;
                if (type.BaseType != null)
                    type = type.BaseType.Resolve();
                else
                    type = null;
            }
            return false;
        }

        public static bool HasAttribute(Collection<CustomAttribute> attributes, string name)
        {
            foreach (CustomAttribute ca in attributes)
            {
                if (AttributeMatches(ca.AttributeType, name))
                    return true;
            }
            return false;
        }

        public static bool HasAttributeSpecific(TypeDefinition type, string name)
        {
            foreach (CustomAttribute ca in type.CustomAttributes)
            {
                if (AttributeMatches(ca.AttributeType, name))
                    return true;
            }
            return false;
        }

        public static bool AttributeMatches(TypeReference type, string name)
        {
            if (type.Name == name)
                return true;
            else if (type.Name == "Attribute")
                return (name == "Attribute");
            else
                return AttributeMatches(type.Resolve().BaseType, name);
        }

        public static void AddProtoMemberAttribute(PropertyDefinition property, int i)
        {
            var customAttribute = Utility.AddAttribute(property, typeof(ProtoMemberAttribute), property.DeclaringType.Module);
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(property.DeclaringType.Module.TypeSystem.Int32, i));
        }

        public static void AddProtoMemberAttribute(FieldDefinition field, int i)
        {
            var customAttribute = Utility.AddAttribute(field, typeof(ProtoMemberAttribute), field.DeclaringType.Module);
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(field.DeclaringType.Module.TypeSystem.Int32, i));
        }
    }
}
