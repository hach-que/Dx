using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Dx.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dx.Process
{
    public class SynchronisationWrapper : IWrapper
    {
        private readonly ITypeBuilder m_TypeBuilder;
        
        private readonly IMethodBuilder m_MethodBuilder;
    
        /// <summary>
        /// The type on which synchronisation may be applied.
        /// </summary>
        private readonly TypeDefinition m_Type;

        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;
    
        public SynchronisationWrapper(
            ITypeBuilder typeBuilder,
            IMethodBuilder methodBuilder,
            TypeDefinition type)
        {
            this.m_TypeBuilder = typeBuilder;
            this.m_MethodBuilder = methodBuilder;
            this.m_Type = type;
            this.m_TraceSource = new TraceSource("SynchronisationWrapper");
        }
        
        public void Wrap()
        {
            var synchronisedProperties = new List<PropertyDefinition>();
            foreach (var property in this.m_Type.Properties)
            {
                if (Utility.HasAttribute(property.CustomAttributes, "SynchronisedAttribute"))
                {
                    synchronisedProperties.Add(property);
                }
            }
            
            if (synchronisedProperties.Count == 0)
                return;
                
            this.m_TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "Detected synchronisation attribute on one or more properties of {0}",
                this.m_Type.FullName);
                
            // Mark as processed.
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Adding ProcessedAttribute to {0}", this.m_Type.FullName);
            Utility.AddAttribute(this.m_Type, typeof(ProcessedAttribute), this.m_Type.Module);
            
            // Get a reference to the synchronisation store's constructor.
            var synchronisationStoreCtor = new MethodReference(
                ".ctor",
                this.m_Type.Module.Import(typeof(void)),
                this.m_Type.Module.Import(typeof(SynchronisationStore)));
            synchronisationStoreCtor.HasThis = true;
            
            // Generate nested class that inherits from SynchronisationStore.
            var nestedClass = this.m_TypeBuilder.CreateNestedClass(
                this.m_Type,
                string.Empty,
                this.m_Type.Name + "_SynchronisationStore",
                this.m_Type.Module.Import(typeof(SynchronisationStore)));
            
            // Add distributed attribute onto the nested class.
            nestedClass.CustomAttributes.Add(new CustomAttribute(
                new MethodReference(
                    ".ctor",
                    this.m_Type.Module.Import(typeof(void)),
                    this.m_Type.Module.Import(typeof(DistributedAttribute))) { HasThis = true }));
                
            // Create copies of all synchronised properties in the synchronisation store.
            foreach (var prop in synchronisedProperties)
                Utility.AddAutoProperty(nestedClass, prop.Name, prop.PropertyType);
            
            // Create default constructor.
            var ctor = this.m_MethodBuilder.CreateConstructor(nestedClass);
            var ctorIL = ctor.Body.GetILProcessor();
            ctorIL.Append(Instruction.Create(OpCodes.Ldarg_0));
            ctorIL.Append(Instruction.Create(OpCodes.Call, synchronisationStoreCtor));
            ctorIL.Append(Instruction.Create(OpCodes.Ret));
            
            // Create overrides for GetNames and GetTypes.
            var getNames = this.m_MethodBuilder.CreateOverride(
                nestedClass,
                "GetNames",
                nestedClass.Module.Import(typeof(string[])));
            var getTypes = this.m_MethodBuilder.CreateOverride(
                nestedClass,
                "GetTypes",
                nestedClass.Module.Import(typeof(Type[])));
            
            // Mark the methods as local so the distributed processor doesn't
            // intercept them at all.
            getNames.CustomAttributes.Add(new CustomAttribute(new MethodReference(
                ".ctor",
                this.m_Type.Module.Import(typeof(void)),
                this.m_Type.Module.Import(typeof(LocalAttribute))) { HasThis = true }));
            getTypes.CustomAttributes.Add(new CustomAttribute(new MethodReference(
                ".ctor",
                this.m_Type.Module.Import(typeof(void)),
                this.m_Type.Module.Import(typeof(LocalAttribute))) { HasThis = true }));
            
            // Emit opcodes to produce GetNames.
            getNames.Body.Variables.Add(new VariableDefinition(
                this.m_Type.Module.Import(typeof(string[]))));
            var getNamesIL = getNames.Body.GetILProcessor();
            getNamesIL.Append(Instruction.Create(OpCodes.Ldc_I4, synchronisedProperties.Count));
            getNamesIL.Append(Instruction.Create(OpCodes.Newarr, this.m_Type.Module.Import(typeof(string))));
            for (var i = 0; i < synchronisedProperties.Count; i++)
            {
                getNamesIL.Append(Instruction.Create(OpCodes.Dup));
                getNamesIL.Append(Instruction.Create(OpCodes.Ldc_I4, i));
                getNamesIL.Append(Instruction.Create(OpCodes.Ldstr, synchronisedProperties[i].Name));
                getNamesIL.Append(Instruction.Create(OpCodes.Stelem_Ref));
            }
            getNamesIL.Append(Instruction.Create(OpCodes.Stloc_0));
            getNamesIL.Append(Instruction.Create(OpCodes.Ldloc_0));
            getNamesIL.Append(Instruction.Create(OpCodes.Ret));
            
            // Get a reference to the GetTypeFromHandle method, which is used for typeof().
            var getTypeFromHandle = new MethodReference(
                "GetTypeFromHandle",
                this.m_Type.Module.Import(typeof(Type)),
                this.m_Type.Module.Import(typeof(Type)));
            getTypeFromHandle.Parameters.Add(new ParameterDefinition(
                this.m_Type.Module.Import(typeof(RuntimeTypeHandle))));
            
            // Emit opcodes to produce GetTypes.
            getTypes.Body.Variables.Add(new VariableDefinition(
                this.m_Type.Module.Import(typeof(Type[]))));
            var getTypesIL = getTypes.Body.GetILProcessor();
            getTypesIL.Append(Instruction.Create(OpCodes.Ldc_I4, synchronisedProperties.Count));
            getTypesIL.Append(Instruction.Create(OpCodes.Newarr, this.m_Type.Module.Import(typeof(Type))));
            for (var i = 0; i < synchronisedProperties.Count; i++)
            {
                getTypesIL.Append(Instruction.Create(OpCodes.Dup));
                getTypesIL.Append(Instruction.Create(OpCodes.Ldc_I4, i));
                getTypesIL.Append(Instruction.Create(
                    OpCodes.Ldtoken,
                    this.m_Type.Module.Import(synchronisedProperties[i].PropertyType)));
                getTypesIL.Append(Instruction.Create(OpCodes.Call, getTypeFromHandle));
                getTypesIL.Append(Instruction.Create(OpCodes.Stelem_Ref));
            }
            getTypesIL.Append(Instruction.Create(OpCodes.Stloc_0));
            getTypesIL.Append(Instruction.Create(OpCodes.Ldloc_0));
            getTypesIL.Append(Instruction.Create(OpCodes.Ret));
            
            // Create the field on the original type to hold an instance of
            // the nested class.
            var field = new FieldDefinition(
                "<>_SynchronisationField",
                FieldAttributes.Private,
                nestedClass);
            field.CustomAttributes.Add(new CustomAttribute(new MethodReference(
                ".ctor",
                this.m_Type.Module.Import(typeof(void)),
                this.m_Type.Module.Import(typeof(CompilerGeneratedAttribute))) { HasThis = true }));
            this.m_Type.Fields.Add(field);
            
            // Implement the ISynchronised interface on the original class.
            var syncMethod = new MethodDefinition(
                "GetSynchronisationStore",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual,
                this.m_Type.Module.Import(typeof(SynchronisationStore)));
            syncMethod.Parameters.Add(new ParameterDefinition(
                this.m_Type.Module.Import(typeof(ILocalNode))));
            syncMethod.Parameters.Add(new ParameterDefinition(
                this.m_Type.Module.Import(typeof(string))));
            this.m_Type.Methods.Add(syncMethod);
            
            // Get a reference to Distributed<NestedClass> and it's constructor.
            var distributedClass = this.m_Type.Module.Import(typeof(Distributed<>));
            var distributedNestedClass = new GenericInstanceType(distributedClass);
            distributedNestedClass.GenericArguments.Add(nestedClass);
            var distributedNestedCtor = new MethodReference(
                ".ctor",
                this.m_Type.Module.Import(typeof(void)),
                distributedNestedClass) { HasThis = true };
            distributedNestedCtor.Parameters.Add(new ParameterDefinition(
                this.m_Type.Module.Import(typeof(ILocalNode))));
            distributedNestedCtor.Parameters.Add(new ParameterDefinition(
                this.m_Type.Module.Import(typeof(string))));
            
            // Get a reference to the cast to convert it to back to it's original object.
            var distributedNestedCast = new MethodReference(
                "op_Implicit",
                distributedClass.GenericParameters[0],
                distributedNestedClass);
            distributedNestedCast.HasThis = false;
            var distributedNestedClassParam2 = new GenericInstanceType(this.m_Type.Module.Import(typeof(Distributed<>)));
            distributedNestedClassParam2.GenericArguments.Add(distributedClass.GenericParameters[0]);
            distributedNestedCast.Parameters.Add(new ParameterDefinition(distributedNestedClassParam2));
            
            // Implement the GetSynchronisationStore method.
            syncMethod.Body.Variables.Add(new VariableDefinition(nestedClass));
            var il = syncMethod.Body.GetILProcessor();
            
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            var beforeCheck = Instruction.Create(OpCodes.Ldfld, field);
            il.Append(beforeCheck);
            // Brtrue to be inserted here
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Newobj, distributedNestedCtor));
            il.Append(Instruction.Create(OpCodes.Call, distributedNestedCast));
            il.Append(Instruction.Create(OpCodes.Stfld, field));
            var skip = Instruction.Create(OpCodes.Ldarg_0);
            il.Append(skip);
            il.Append(Instruction.Create(OpCodes.Ldfld, field));
            il.Append(Instruction.Create(OpCodes.Ret));
                        
            // Insert brtrue.
            il.InsertAfter(
                beforeCheck,
                Instruction.Create(OpCodes.Brtrue, skip));
            
            // Add interface.
            this.m_Type.Interfaces.Add(this.m_Type.Module.Import(typeof(ISynchronised)));
            
            // Finally, apply distributed processing to the nested class!
            this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Starting processing of {0}", nestedClass.Name);
            new TypeWrapper(nestedClass).Wrap();
            this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Finished processing of {0}", nestedClass.Name);
        }
    }
}

