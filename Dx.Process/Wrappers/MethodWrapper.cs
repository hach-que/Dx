//-----------------------------------------------------------------------
// <copyright file="MethodWrapper.cs" company="Redpoint Software">
// The MIT License (MIT)
//
// Copyright (c) 2013 James Rhodes
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------
namespace Dx.Process
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Dx.Runtime;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    /// <summary>
    /// Wraps a method for distributed behaviour.
    /// </summary>
    internal class MethodWrapper : IWrapper
    {
        /// <summary>
        /// The method to be wrapped.
        /// </summary>
        private readonly MethodDefinition m_Method;

        /// <summary>
        /// The type on which the method to be wrapped is defined.
        /// </summary>
        private readonly TypeDefinition m_Type;

        /// <summary>
        /// The module containing the method to be wrapped.
        /// </summary>
        private readonly ModuleDefinition m_Module;

        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Process4.Task.Wrappers.MethodWrapper"/> class.
        /// </summary>
        /// <param name="method">The method to wrap.</param>
        public MethodWrapper(MethodDefinition method)
        {
            this.m_Method = method;
            this.m_Type = method.DeclaringType;
            this.m_Module = method.Module;
            this.m_TraceSource = new TraceSource("MethodWrapper");
        }

        /// <summary>
        /// Wraps the method.
        /// </summary>
        public void Wrap()
        {
            if (this.m_Method.CustomAttributes.Any(c => c.AttributeType.Name == "LocalAttribute"))
                return;
            this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Modifying {0} for distributed processing", this.m_Method.Name);

            // Generate the direct invocation class.
            TypeDefinition idc = this.GenerateDirectInvokeClass();

            // Get a list of existing instructions.
            Collection<Instruction> instructions = this.m_Method.Body.Instructions;

            // Get a reference to the context setting method.
            var assignNodeContext = new MethodReference("AssignNodeContext", this.m_Type.Module.Import(typeof(void)), this.m_Type.Module.Import(typeof(DpmConstructContext)));
            assignNodeContext.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object))));

            // Create a new Action delegate using those instructions.
            TypeReference mdr = this.m_Method.ReturnType;
            MethodDefinition md = new MethodDefinition(this.m_Method.Name + "__Distributed0", MethodAttributes.Private, mdr);
            md.Body = new MethodBody(md);
            md.Body.InitLocals = true;
            md.Body.Instructions.Clear();
            foreach (Instruction ii in instructions)
            {
                if (ii.OpCode == OpCodes.Newobj)
                {
                    md.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    md.Body.Instructions.Add(Instruction.Create(OpCodes.Call, assignNodeContext));
                }

                md.Body.Instructions.Add(ii);
            }

            foreach (VariableDefinition l in this.m_Method.Body.Variables)
            {
                md.Body.Variables.Add(l);
            }

            foreach (ExceptionHandler ex in this.m_Method.Body.ExceptionHandlers)
            {
                md.Body.ExceptionHandlers.Add(ex);
            }

            foreach (ParameterDefinition p in this.m_Method.Parameters)
            {
                md.Parameters.Add(p);
            }

            foreach (GenericParameter gp in this.m_Method.GenericParameters)
            {
                GenericParameter gpn = new GenericParameter(gp.Name, md);
                gpn.Attributes = gp.Attributes;
                foreach (TypeReference tr in gp.Constraints)
                {
                    gpn.Constraints.Add(tr);
                }

                md.GenericParameters.Add(gpn);
            }

            this.m_Type.Methods.Add(md);
            Utility.AddAttribute(md, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), this.m_Module);

            // Get the ILProcessor and create variables to store the delegate variable
            // and delegate constructor.
            ILProcessor il = this.m_Method.Body.GetILProcessor();
            VariableDefinition vd;
            MethodReference ct;

            // Clear the existing instructions and local variables.
            this.m_Method.Body.ExceptionHandlers.Clear();
            this.m_Method.Body.Instructions.Clear();
            this.m_Method.Body.Variables.Clear();

            // Generate the IL for the delegate definition and fill the vd and ct
            // variables.
            TypeDefinition dg = Utility.EmitDelegate(il, idc, md, this.m_Type, out vd, out ct);

            // Implement the Invoke method in the DirectInvoke class.
            this.ImplementDirectInvokeClass(idc, dg, md.Parameters, md.ReturnType);

            // Create the Process4.Providers.DpmEntrypoint::GetProperty method reference.
            MethodReference getproperty = new MethodReference("GetProperty", this.m_Type.Module.Import(typeof(object)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            getproperty.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Delegate))));
            getproperty.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object[]))));

            // Create the Process4.Providers.DpmEntrypoint::SetProperty method reference.
            MethodReference setproperty = new MethodReference("SetProperty", this.m_Type.Module.Import(typeof(object)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            setproperty.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Delegate))));
            setproperty.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object[]))));

            // Create the Process4.Providers.DpmEntrypoint::Invoke method reference.
            MethodReference invoke = new MethodReference("Invoke", this.m_Type.Module.Import(typeof(object)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            invoke.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Delegate))));
            invoke.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object[]))));

            // Create the Process4.Providers.DpmEntrypoint::AddEvent method reference.
            MethodReference addevent = new MethodReference("AddEvent", this.m_Type.Module.Import(typeof(object)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            addevent.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Delegate))));
            addevent.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object[]))));

            // Create the Process4.Providers.DpmEntrypoint::RemoveEvent method reference.
            MethodReference removeevent = new MethodReference("RemoveEvent", this.m_Type.Module.Import(typeof(object)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            removeevent.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Delegate))));
            removeevent.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object[]))));

            // Generate the local variables like:
            // * 0 - MultitypeDelegate
            // * 1 - object[]
            // * 2 - {return type} (if not void)
            VariableDefinition v_0 = vd;
            VariableDefinition v_1 = new VariableDefinition(this.m_Type.Module.Import(typeof(object[])));
            VariableDefinition v_2 = new VariableDefinition(md.ReturnType);

            // Add the variables to the local variable list (delegate is already added).
            this.m_Method.Body.Variables.Add(v_1);
            if (v_2.VariableType.Name.ToLower() != "void")
            {
                this.m_Method.Body.Variables.Add(v_2);
            }

            // Force local variables to be initalized.
            this.m_Method.Body.InitLocals = true;
            this.m_Method.Body.MaxStackSize = 10;

            // Create statement processor for method.
            StatementProcessor processor = new StatementProcessor(il);

            // Make a generic version of the delegate method.
            TypeDefinition btd = this.m_Type;
            MethodDefinition bmd = md;
            MethodReference bmr = btd.Module.Import(bmd);
            GenericInstanceType bti = null;
            if (this.m_Type.HasGenericParameters)
            {
                bti = (GenericInstanceType)Utility.MakeGenericType(this.m_Type, this.m_Type.GenericParameters.ToArray());
                bmr = Utility.MakeGeneric(bmr, bti.GenericArguments.ToArray());
            }

            if (this.m_Method.HasGenericParameters)
            {
                GenericInstanceMethod gim = new GenericInstanceMethod(bmr);
                foreach (GenericParameter gp in bmr.GenericParameters)
                {
                    gim.GenericArguments.Add(gp);
                }

                bmr = gim;
            }

            foreach (var gp in this.m_Type.GenericParameters)
            {
                ((GenericInstanceType)ct.DeclaringType).GenericArguments.Add(gp);
            }

            foreach (var gp in this.m_Method.GenericParameters)
            {
                ((GenericInstanceType)ct.DeclaringType).GenericArguments.Add(gp);
            }

            // Initialize the delegate.
            processor.Add(new InitDelegateStatement(ct, bmr, v_0));

            // Initialize the array.
            if (this.m_Method.IsSetter)
            {
                il.Append(Instruction.Create(OpCodes.Ldloc_0));
            }

            processor.Add(
                new InitArrayStatement(
                    this.m_Module.Import(typeof(object)),
                    (sbyte)md.Parameters.Count,
                    v_1,
                    new Action<ILProcessor, int>((p, i) =>
                    {
                        // Get a reference to the parameter being passed to the method.
                        ParameterDefinition pd = this.m_Method.Parameters[i];
                        if (i > this.m_Method.Parameters.Count - 1)
                        {
                            return;
                        }

                        // Create IL to copy the value from the parameter directly
                        // into the array, boxing the value if needed.
                        p.Append(Instruction.Create(OpCodes.Ldloc, v_1));
                        p.Append(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)i));
                        p.Append(Instruction.Create(OpCodes.Ldarg_S, pd));
                        if (pd.ParameterType.IsValueType || pd.ParameterType.IsGenericParameter)
                        {
                            p.Append(Instruction.Create(OpCodes.Box, pd.ParameterType));
                        }

                        p.Append(Instruction.Create(OpCodes.Stelem_Ref));
                    })));

            // Call the delegate.
            if (this.m_Method.IsSetter)
            {
                processor.Add(new CallStatement(setproperty, new VariableDefinition[] { v_1 }, this.m_Method.ReturnType, v_2));
            }
            else if (this.m_Method.IsGetter)
            {
                processor.Add(new CallStatement(getproperty, new VariableDefinition[] { v_0, v_1 }, this.m_Method.ReturnType, v_2));
            }
            else if (this.m_Method.IsAddOn)
            {
                processor.Add(new CallStatement(addevent, new VariableDefinition[] { v_0, v_1 }, this.m_Method.ReturnType, v_2));
            }
            else if (this.m_Method.IsRemoveOn)
            {
                processor.Add(new CallStatement(removeevent, new VariableDefinition[] { v_0, v_1 }, this.m_Method.ReturnType, v_2));
            }
            else
            {
                processor.Add(new CallStatement(invoke, new VariableDefinition[] { v_0, v_1 }, this.m_Method.ReturnType, v_2));
            }
        }

        /// <summary>
        /// Generates the direct invocation class which allows methods to be called
        /// without exceptions being wrapped in TargetInvocationException.
        /// </summary>
        /// <returns>The direct invocation class.</returns>
        private TypeDefinition GenerateDirectInvokeClass()
        {
            // Determine the generic appendix.
            var genericAppendix = string.Empty;
            if (this.m_Method.GenericParameters.Count > 0)
            {
                genericAppendix = "`" + this.m_Method.GenericParameters.Count;
            }

            // Create a new type.
            var idc = new TypeDefinition(
                string.Empty,
                this.m_Method.Name + "__InvokeDirect" + this.m_Type.NestedTypes.Count + genericAppendix,
                TypeAttributes.Public | TypeAttributes.NestedPublic | TypeAttributes.BeforeFieldInit);
            idc.BaseType = this.m_Module.Import(typeof(object));
            idc.DeclaringType = this.m_Type;
            foreach (var gp in this.m_Type.GenericParameters)
            {
                var gpn = new GenericParameter(gp.Name, idc);
                gpn.Attributes = gp.Attributes;
                foreach (var tr in gp.Constraints)
                {
                    if (tr is GenericInstanceType)
                    {
                        gpn.Constraints.Add(Utility.RewriteGenericReferencesToType(this.m_Type, tr as GenericInstanceType));
                    }
                    else
                    {
                        gpn.Constraints.Add(tr);
                    }
                }

                idc.GenericParameters.Add(gpn);
            }

            foreach (var gp in this.m_Method.GenericParameters)
            {
                var gpn = new GenericParameter(gp.Name, idc);
                gpn.Attributes = gp.Attributes;
                foreach (var tr in gp.Constraints)
                {
                    if (tr is GenericInstanceType)
                    {
                        gpn.Constraints.Add(Utility.RewriteGenericReferencesToType(this.m_Type, tr as GenericInstanceType));
                    }
                    else
                    {
                        gpn.Constraints.Add(tr);
                    }
                }

                idc.GenericParameters.Add(gpn);
            }

            this.m_Type.NestedTypes.Add(idc);

            // Add the IDirectInvoke interface.
            idc.Interfaces.Add(this.m_Module.Import(typeof(IDirectInvoke)));

            // Create the System.Object::.ctor method reference.
            var objctor = new MethodReference(".ctor", this.m_Type.Module.Import(typeof(void)), this.m_Type.Module.Import(typeof(object)));
            objctor.HasThis = true;

            // Add the constructor.
            var ctorAttributes = MethodAttributes.Public;
            ctorAttributes |= MethodAttributes.CompilerControlled;
            ctorAttributes |= MethodAttributes.SpecialName;
            ctorAttributes |= MethodAttributes.HideBySig;
            ctorAttributes |= MethodAttributes.RTSpecialName;
            var ctor = new MethodDefinition(
                ".ctor",
                ctorAttributes,
                this.m_Module.Import(typeof(void)));
            var p = ctor.Body.GetILProcessor();
            p.Append(Instruction.Create(OpCodes.Ldarg_0));
            p.Append(Instruction.Create(OpCodes.Call, objctor));
            p.Append(Instruction.Create(OpCodes.Ret));
            idc.Methods.Add(ctor);

            return idc;
        }

        /// <summary>
        /// Implements the direct invocation class.
        /// </summary>
        /// <param name="idc">Idc sdf.</param>
        /// <param name="dg">Dg sdf .</param>
        /// <param name="ps">Pssd fsdf.</param>
        /// <param name="tret">Tret sdf.</param>
        private void ImplementDirectInvokeClass(TypeDefinition idc, TypeDefinition dg, Collection<ParameterDefinition> ps, TypeReference tret)
        {
            // Create the generic instance type.
            GenericInstanceType gdg = new GenericInstanceType(dg);
            int gi = 0;
            foreach (GenericParameter gp in idc.GenericParameters)
            {
                gdg.GenericParameters.Add(new GenericParameter(gp.Name, gdg));
                gdg.GenericArguments.Add(gdg.GenericParameters[gi]);
                gi++;
            }

            TypeReference ret = tret;
            if (ret is GenericInstanceType)
            {
                ret = Utility.RewriteGenericReferencesToType(this.m_Type, ret as GenericInstanceType);
            }
            else if (ret is GenericParameter)
            {
                ret = new GenericParameter(
                    (tret as GenericParameter).Type == GenericParameterType.Type ?
                    (tret as GenericParameter).Position :
                    (tret as GenericParameter).Position + this.m_Type.GenericParameters.Count,
                    GenericParameterType.Type,
                    idc.Module);
            }

            // Pick the normal delegate if there were no generic type parameters.
            TypeReference rdg = gdg.GenericArguments.Count > 0 ? gdg : (TypeReference)dg;

            // Add the parameters and variables.
            MethodDefinition invoke = new MethodDefinition(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                this.m_Module.Import(typeof(object)));
            invoke.Parameters.Add(new ParameterDefinition("method", ParameterAttributes.None, this.m_Module.Import(typeof(System.Reflection.MethodInfo))));
            invoke.Parameters.Add(new ParameterDefinition("instance", ParameterAttributes.None, this.m_Module.Import(typeof(object))));
            invoke.Parameters.Add(new ParameterDefinition("parameters", ParameterAttributes.None, this.m_Module.Import(typeof(object[]))));
            invoke.Body.Variables.Add(new VariableDefinition("d", rdg));
            invoke.Body.Variables.Add(new VariableDefinition("ret", this.m_Module.Import(typeof(object))));

            // Get the ILProcessor and create variables to store the delegate variable.
            ILProcessor il = invoke.Body.GetILProcessor();
            VariableDefinition v_0 = invoke.Body.Variables[0];
            VariableDefinition v_1 = invoke.Body.Variables.Count == 1 ? null : invoke.Body.Variables[1];

            // Create the System.Type::GetTypeFromHandle method reference.
            MethodReference gettypefromhandle = new MethodReference("GetTypeFromHandle", this.m_Type.Module.Import(typeof(Type)), this.m_Type.Module.Import(typeof(Type)));
            gettypefromhandle.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(RuntimeTypeHandle))));

            // Create the System.Delegate::CreateDelegate method reference.
            MethodReference createdelegate = new MethodReference("CreateDelegate", this.m_Type.Module.Import(typeof(Delegate)), this.m_Type.Module.Import(typeof(Delegate)));
            createdelegate.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(Type))));
            createdelegate.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object))));
            createdelegate.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(System.Reflection.MethodInfo))));

            // Create the dg::Invoke method reference.
            MethodReference invokedelegate = new MethodReference("Invoke", this.m_Module.Import(ret), rdg);
            foreach (ParameterDefinition pd in ps)
            {
                var propType = pd.ParameterType;
                if (pd.ParameterType is GenericParameter)
                {
                    propType = new GenericParameter(
                        (pd.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                        (pd.ParameterType as GenericParameter).Position :
                        (pd.ParameterType as GenericParameter).Position + this.m_Type.GenericParameters.Count,
                        GenericParameterType.Type,
                        this.m_Module);
                }

                invokedelegate.Parameters.Add(new ParameterDefinition(
                    pd.Name,
                    pd.Attributes,
                    propType));
            }

            invokedelegate.HasThis = true;

            // Force local variables to be initalized.
            invoke.Body.InitLocals = true;

            // Get a type instance reference from the delegate type.
            il.Append(Instruction.Create(OpCodes.Nop));
            il.Append(Instruction.Create(OpCodes.Ldtoken, rdg));
            il.Append(Instruction.Create(OpCodes.Call, gettypefromhandle));

            // Get a delegate and then cast it.
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Call, createdelegate));
            il.Append(Instruction.Create(OpCodes.Castclass, rdg));
            il.Append(Instruction.Create(OpCodes.Stloc, v_0));

            // Load the delegate.
            il.Append(Instruction.Create(OpCodes.Ldloc, v_0));

            // Load the arguments.
            int i = 0;
            foreach (ParameterDefinition pd in ps)
            {
                var propType = pd.ParameterType;
                if (pd.ParameterType is GenericParameter)
                {
                    propType = new GenericParameter(
                        (pd.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                        (pd.ParameterType as GenericParameter).Position :
                        (pd.ParameterType as GenericParameter).Position + this.m_Type.GenericParameters.Count,
                        GenericParameterType.Type,
                        this.m_Module);
                }

                il.Append(Instruction.Create(OpCodes.Ldarg_3));
                il.Append(Instruction.Create(OpCodes.Ldc_I4, i));
                il.Append(Instruction.Create(OpCodes.Ldelem_Ref));
                if (propType.IsValueType || propType.IsGenericParameter)
                {
                    il.Append(Instruction.Create(OpCodes.Unbox_Any, propType));
                }
                else
                {
                    il.Append(Instruction.Create(OpCodes.Castclass, propType));
                }

                i += 1;
            }

            // Call the delegate's Invoke method.
            il.Append(Instruction.Create(OpCodes.Callvirt, invokedelegate));
            if (ret.FullName == this.m_Module.Import(typeof(void)).FullName)
            {
                il.Append(Instruction.Create(OpCodes.Ldnull));
            }
            else if (ret.IsValueType || ret.IsGenericParameter)
            {
                il.Append(Instruction.Create(OpCodes.Box, ret));
            }

            var ii_stloc = Instruction.Create(OpCodes.Stloc, v_1);
            var ii_ldloc = Instruction.Create(OpCodes.Ldloc, v_1);
            il.Append(ii_stloc);
            il.Append(ii_ldloc);
            il.Append(Instruction.Create(OpCodes.Ret));
            il.InsertAfter(ii_stloc, Instruction.Create(OpCodes.Br_S, ii_ldloc));

            idc.Methods.Insert(0, invoke);
        }
    }
}
