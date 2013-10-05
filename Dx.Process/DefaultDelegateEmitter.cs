using System;
using Mono.Cecil;

namespace Dx.Process
{
    public class DefaultDelegateEmitter : IDelegateEmitter
    {
        /// <summary>
        /// Emits a delegate that matches the signature of a method being processed.  The delegate
        /// definition is added to the same type that the method is declared on.
        /// </summary>
        /// <returns>The type definition of the delegate.</returns>
        /// <param name="method">The method on which to base the new delegate.</param>
        /// <param name="delegateCtor">A reference to the constructor that can be used to create an instance of the delegate.</param>
        public TypeDefinition EmitDelegate(
            MethodDefinition method,
            out MethodReference delegateCtor)
        {
            delegateCtor = null;

            // Get type references to various builtin types.
            var type_MulticastDelegate = method.Module.Import(typeof(MulticastDelegate));
            var type_Void = method.Module.Import(typeof(void));
            var type_Object = method.Module.Import(typeof(object));
            var type_IntPtr = method.Module.Import(typeof(IntPtr));
            var type_AsyncCallback = method.Module.Import(typeof(AsyncCallback));
            var type_IAsyncResult = method.Module.Import(typeof(IAsyncResult));

            // Create a new TypeDefinition for the delegate.
            var suffix = method.GenericParameters.Count == 0 ?
                string.Empty :
                "`" + method.GenericParameters.Count;
            var delegateType = new TypeDefinition(
                string.Empty,
                method.Name + "__DistributedDelegate" + method.DeclaringType.NestedTypes.Count + suffix,
                TypeAttributes.Sealed | TypeAttributes.NestedPublic,
                type_MulticastDelegate);
            foreach (var gp in method.DeclaringType.GenericParameters)
            {
                var ngp = new GenericParameter(gp.Name, delegateType);
                ngp.Attributes = gp.Attributes;
                foreach (TypeReference gpc in gp.Constraints)
                    ngp.Constraints.Add(gpc);
                delegateType.GenericParameters.Add(ngp);
            }
            foreach (var gp in method.GenericParameters)
            {
                var ngp = new GenericParameter(gp.Name, delegateType);
                ngp.Attributes = gp.Attributes;
                foreach (TypeReference gpc in gp.Constraints)
                    ngp.Constraints.Add(gpc);
                delegateType.GenericParameters.Add(ngp);
            }

            Utility.AddAttribute(
                delegateType,
                typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),
                method.Module);

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
            TypeReference retType = method.ReturnType;
            if (method.ReturnType is GenericInstanceType)
                retType = Utility.RewriteGenericReferencesToType(method.DeclaringType, method.ReturnType as GenericInstanceType);
            else if (method.ReturnType is GenericParameter)
                retType = new GenericParameter(
                                (method.ReturnType as GenericParameter).Type == GenericParameterType.Type ?
                                (method.ReturnType as GenericParameter).Position :
                                (method.ReturnType as GenericParameter).Position + method.DeclaringType.GenericParameters.Count,
                                GenericParameterType.Type,
                                method.Module);
            MethodDefinition invoke = new MethodDefinition(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                    MethodAttributes.Virtual,
                retType
            );
            foreach (ParameterDefinition p in method.Parameters)
            {
                // Add the parameters that are accepted by the source
                // method to the delegate.
                TypeReference pType = p.ParameterType;
                if (p.ParameterType is GenericParameter)
                    pType = new GenericParameter(
                                    (p.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                                    (p.ParameterType as GenericParameter).Position :
                                    (p.ParameterType as GenericParameter).Position + method.DeclaringType.GenericParameters.Count,
                                    GenericParameterType.Type,
                                    method.Module);
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
            foreach (ParameterDefinition p in method.Parameters)
            {
                // Add the parameters that are accepted by the source
                // method to the delegate.
                TypeReference pType = p.ParameterType;
                if (p.ParameterType is GenericParameter)
                    pType = new GenericParameter(
                                    (p.ParameterType as GenericParameter).Type == GenericParameterType.Type ?
                                    (p.ParameterType as GenericParameter).Position :
                                    (p.ParameterType as GenericParameter).Position + method.DeclaringType.GenericParameters.Count,
                                    GenericParameterType.Type,
                                    method.Module);
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
            method.DeclaringType.NestedTypes.Add(delegateType);

            // Check if either the method or the declaring type has type parameters.  Even though the
            // code in the else doesn't do anything with generic parameters on the method itself (it
            // only adds parameters for the generic type), the code calling this method will add generic
            // arguments of the method onto the declaring type of the constructor.  Unfortunately, if
            // we just assign the resulting constructor to 'ctor', then the declaring type isn't a
            // 'GenericInstanceType', so the calling code can't add the arguments in.
            if (method.DeclaringType.GenericParameters.Count == 0 &&
                method.GenericParameters.Count == 0)
                delegateCtor = ctor;
            else
            {
                // We need to make a generic version of the type and get the constructor from that.
                GenericInstanceType git = new GenericInstanceType(delegateType);
                foreach (GenericParameter gp in method.DeclaringType.GenericParameters)
                    git.GenericParameters.Add(new GenericParameter(gp.Position, GenericParameterType.Type, gp.Module)); // "!!" + gp.Name, git
                delegateCtor = new MethodReference(".ctor", type_Void, git);
                delegateCtor.Parameters.Add(new ParameterDefinition("object", ParameterAttributes.None, type_Object));
                delegateCtor.Parameters.Add(new ParameterDefinition("method", ParameterAttributes.None, type_IntPtr));
                delegateCtor.HasThis = true;
            }

            return delegateType;
        }
    }
}

