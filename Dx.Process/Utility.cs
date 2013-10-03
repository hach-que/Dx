using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Process4.Task
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
            delegateCtor = null;
            delegateVariable = null;

            // Store the body, method and module we are working on.
            MethodBody body = processor.Body;
            MethodDefinition method = processor.Body.Method;
            ModuleDefinition module = processor.Body.Method.Module;

            // Get type references to various builtin types.
            TypeReference type_MulticastDelegate = module.Import(typeof(MulticastDelegate));
            TypeReference type_Void = module.Import(typeof(void));
            TypeReference type_Object = module.Import(typeof(object));
            TypeReference type_IntPtr = module.Import(typeof(IntPtr));
            TypeReference type_AsyncCallback = module.Import(typeof(AsyncCallback));
            TypeReference type_IAsyncResult = module.Import(typeof(IAsyncResult));

            // Create a new TypeDefinition for the delegate.
            TypeDefinition delegateType = new TypeDefinition(
                "",
                method.Name + "__DistributedDelegate" + body.Method.DeclaringType.NestedTypes.Count,
                TypeAttributes.Sealed | TypeAttributes.NestedPublic,
                type_MulticastDelegate // Inherit from MulticastDelegate
                );
            foreach (GenericParameter gp in originalType.GenericParameters)
            {
                GenericParameter ngp = new GenericParameter(gp.Name, delegateType);
                ngp.Attributes = gp.Attributes;
                foreach (TypeReference gpc in gp.Constraints)
                    ngp.Constraints.Add(gpc);
                delegateType.GenericParameters.Add(ngp);
            }
            foreach (GenericParameter gp in sourceMethod.GenericParameters)
            {
                GenericParameter ngp = new GenericParameter(gp.Name, delegateType);
                ngp.Attributes = gp.Attributes;
                foreach (TypeReference gpc in gp.Constraints)
                    ngp.Constraints.Add(gpc);
                delegateType.GenericParameters.Add(ngp);
            }
            Utility.AddAttribute(delegateType, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), module);

            // Add the constructor to the delegate type.
            MethodDefinition ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.CompilerControlled |
                    MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                    MethodAttributes.RTSpecialName,
                type_Void
                );
            ctor.Parameters.Add(new ParameterDefinition(
                "object",
                ParameterAttributes.None,
                type_Object
                ));
            ctor.Parameters.Add(new ParameterDefinition(
                "method",
                ParameterAttributes.None,
                type_IntPtr
                ));
            ctor.Body = null;
            ctor.IsRuntime = true;
            ctor.ImplAttributes = MethodImplAttributes.CodeTypeMask;

            // Add the Invoke method to the delegate type.
            TypeReference retType = sourceMethod.ReturnType;
            if (sourceMethod.ReturnType is GenericInstanceType)
                retType = Utility.RewriteGenericReferencesToType(originalType, sourceMethod.ReturnType as GenericInstanceType);
            else if (sourceMethod.ReturnType is GenericParameter)
                retType = new GenericParameter(
                                (sourceMethod.ReturnType as GenericParameter).Type == GenericParameterType.Type ?
                                (sourceMethod.ReturnType as GenericParameter).Position :
                                (sourceMethod.ReturnType as GenericParameter).Position + originalType.GenericParameters.Count,
                                GenericParameterType.Type,
                                sourceType.Module);
            MethodDefinition invoke = new MethodDefinition(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                    MethodAttributes.Virtual,
                retType
            );
            foreach (ParameterDefinition p in sourceMethod.Parameters)
            {
                // Add the parameters that are accepted by the source
                // method to the delegate.
                TypeReference pType = p.ParameterType;
                if (p.ParameterType is GenericParameter)
                    pType = new GenericParameter(
                                    (p.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                                    (p.ParameterType as GenericParameter).Position :
                                    (p.ParameterType as GenericParameter).Position + originalType.GenericParameters.Count,
                                    GenericParameterType.Type,
                                    sourceMethod.Module);
                invoke.Parameters.Add(new ParameterDefinition(
                    p.Name,
                    p.Attributes,
                    pType
                    ));
            }
            invoke.Body = null;
            invoke.IsRuntime = true;
            invoke.ImplAttributes = MethodImplAttributes.CodeTypeMask;

            // Add the BeginInvoke method to the delegate type.
            MethodDefinition begininvoke = new MethodDefinition(
                "BeginInvoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                    MethodAttributes.Virtual,
                type_IAsyncResult
                );
            foreach (ParameterDefinition p in sourceMethod.Parameters)
            {
                // Add the parameters that are accepted by the source
                // method to the delegate.
                TypeReference pType = p.ParameterType;
                if (p.ParameterType is GenericParameter)
                    pType = new GenericParameter(
                                    (p.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                                    (p.ParameterType as GenericParameter).Position :
                                    (p.ParameterType as GenericParameter).Position + originalType.GenericParameters.Count,
                                    GenericParameterType.Type,
                                    sourceMethod.Module);
                begininvoke.Parameters.Add(new ParameterDefinition(
                    p.Name,
                    p.Attributes,
                    pType
                    ));
            }
            begininvoke.Parameters.Add(new ParameterDefinition(
                "callback",
                ParameterAttributes.None,
                type_AsyncCallback
                ));
            begininvoke.Parameters.Add(new ParameterDefinition(
                "object",
                ParameterAttributes.None,
                type_Object
                ));
            begininvoke.Body = null;
            begininvoke.IsRuntime = true;
            begininvoke.ImplAttributes = MethodImplAttributes.CodeTypeMask;

            // Add the EndInvoke method to the delegate type.
            MethodDefinition endinvoke = new MethodDefinition(
                "EndInvoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                    MethodAttributes.Virtual,
                retType
                );
            endinvoke.Parameters.Add(new ParameterDefinition(
                "result",
                ParameterAttributes.None,
                type_IAsyncResult
                ));
            endinvoke.Body = null;
            endinvoke.IsRuntime = true;
            endinvoke.ImplAttributes = MethodImplAttributes.CodeTypeMask;

            // Add all of the defined methods to the delegate type.
            delegateType.Methods.Add(ctor);
            delegateType.Methods.Add(invoke);
            delegateType.Methods.Add(begininvoke);
            delegateType.Methods.Add(endinvoke);

            // Add the type to the module.
            sourceType.NestedTypes.Add(delegateType);

            // Define the new variable for storing the delegate.
            //TypeReference delegateTypeReference = new TypeReference(delegateType.Namespace, delegateType.Name, module, null);
            //delegateTypeReference.DeclaringType = sourceType;
            delegateVariable = new VariableDefinition("d", type_MulticastDelegate);

            if (sourceType.GenericParameters.Count == 0)
                delegateCtor = ctor;
            else
            {
                // We need to make a generic version of the type and get the constructor from that.
                GenericInstanceType git = new GenericInstanceType(delegateType);
                foreach (GenericParameter gp in sourceType.GenericParameters)
                    git.GenericParameters.Add(new GenericParameter(gp.Position, GenericParameterType.Type, gp.Module)); // "!!" + gp.Name, git
                delegateCtor = new MethodReference(".ctor", type_Void, git);
                delegateCtor.Parameters.Add(new ParameterDefinition("object", ParameterAttributes.None, type_Object));
                delegateCtor.Parameters.Add(new ParameterDefinition("method", ParameterAttributes.None, type_IntPtr));
                delegateCtor.HasThis = true;
            }

            // Create the generic instance type.
            /*GenericInstanceType gdg = new GenericInstanceType(dg);
            foreach (GenericParameter gp in idc.GenericParameters)
            {
                gdg.GenericParameters.Add(new GenericParameter("!0", gdg));
                gdg.GenericArguments.Add(gdg.GenericParameters[0]);
            }*/

            // Add as a local variable to the method.
            body.Variables.Insert(0, delegateVariable);

            return delegateType;
        }

        /// <summary>
        /// Adds the specified attribute to the specified type.
        /// </summary>
        /// <param name="type">The type to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        public static void AddAttribute(TypeDefinition type, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            type.CustomAttributes.Add(new CustomAttribute(rf));
        }

        /// <summary>
        /// Adds the specified attribute to the specified field.
        /// </summary>
        /// <param name="field">The field to add the attribute to.</param>
        /// <param name="attribute">The attribute to add.</param>
        /// <param name="module">The module the field is defined in.</param>
        public static void AddAttribute(FieldDefinition field, Type attribute, ModuleDefinition module)
        {
            TypeDefinition tr = module.Import(attribute).Resolve();
            MethodDefinition mr = tr.Methods.First(value => value.IsConstructor);
            MethodReference rf = module.Import(mr);
            field.CustomAttributes.Add(new CustomAttribute(rf));
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
        public static void AddAutoProperty(TypeDefinition t, string name, Type type)
        {
            // Create the field definition.
            FieldDefinition fd = new FieldDefinition("<" + name + ">k__BackingField", FieldAttributes.Private, t.Module.Import(type));
            Utility.AddAttribute(fd, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), t.Module);

            // Define the getter and setter methods.
            MethodAttributes attribs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName
                                        | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            MethodDefinition mgd = new MethodDefinition("get_" + name, attribs, t.Module.Import(type));
            MethodDefinition msd = new MethodDefinition("set_" + name, attribs, t.Module.Import(typeof(void)));
            mgd.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            mgd.Body.Variables.Add(new VariableDefinition(t.Module.Import(type)));
            mgd.Body.InitLocals = true;
            msd.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            msd.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, t.Module.Import(type)));
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
            PropertyDefinition pd = new PropertyDefinition(name, PropertyAttributes.None, t.Module.Import(type));
            pd.HasThis = true;
            pd.GetMethod = mgd;
            pd.SetMethod = msd;

            // Now add all of the above to the type.
            t.Fields.Add(fd);
            t.Methods.Add(mgd);
            t.Methods.Add(msd);
            t.Properties.Add(pd);
        }

        /// <summary>
        /// Adds the deserialization constructor to the specified type.
        /// </summary>
        /// <param name="t">The type to add the deserialization constructor to.</param>
        public static void AddDeserializationConstructor(TypeDefinition t)
        {
            // Create the Process4.Providers.DpmEntrypoint::Deserialize method reference.
            MethodReference deserialize = new MethodReference("Deserialize", t.Module.Import(typeof(void)), t.Module.Import(typeof(Process4.Providers.DpmEntrypoint)));
            deserialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(object))));
            deserialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.SerializationInfo))));
            deserialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.StreamingContext))));

            // Define the method.
            MethodAttributes attribs = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            MethodDefinition ctor = new MethodDefinition(".ctor", attribs, t.Module.Import(typeof(void)));
            ctor.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.SerializationInfo))));
            ctor.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.StreamingContext))));

            // Define the method body.
            ILProcessor il = ctor.Body.GetILProcessor();
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Call, deserialize));
            il.Append(Instruction.Create(OpCodes.Ret));

            // Add the method.
            t.Methods.Add(ctor);
        }

        /// <summary>
        /// Adds the serialization method to the specified type.
        /// </summary>
        /// <param name="t">The type to add the serialization method to.</param>
        internal static void AddSerializationMethod(TypeDefinition t)
        {
            // Create the Process4.Providers.DpmEntrypoint::Serialize method reference.
            MethodReference serialize = new MethodReference("Serialize", t.Module.Import(typeof(void)), t.Module.Import(typeof(Process4.Providers.DpmEntrypoint)));
            serialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(object))));
            serialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.SerializationInfo))));
            serialize.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.StreamingContext))));

            // Define the method.
            MethodAttributes attribs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            MethodDefinition getobjdata = new MethodDefinition("GetObjectData", attribs, t.Module.Import(typeof(void)));
            getobjdata.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.SerializationInfo))));
            getobjdata.Parameters.Add(new ParameterDefinition(t.Module.Import(typeof(System.Runtime.Serialization.StreamingContext))));

            // Define the method body.
            ILProcessor il = getobjdata.Body.GetILProcessor();
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Call, serialize));
            il.Append(Instruction.Create(OpCodes.Ret));

            // Add the method.
            t.Methods.Add(getobjdata);
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
    }
}