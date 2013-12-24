// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DpmEntrypoint.cs" company="Redpoint Software">
//   The MIT License (MIT)
//   
//   Copyright (c) 2013 James Rhodes
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// <summary>
//   Provides entry points into the library when methods or property getters / setters
//   are invoked.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dx.Runtime
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Provides entry points into the library when methods or property getters / setters
    /// are invoked.
    /// </summary>
    public static class DpmEntrypoint
    {
        #region Public Methods and Operators

        /// <summary>
        /// Called when an object is being constructed.  This is instrumented as the first
        /// operation to occur in the object's constructors so that this contextual information
        /// can be set up before any properties or other methods are called.
        /// </summary>
        /// <param name="obj">
        /// The object that is being constructed.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the object is being constructed outside of a distributed context.  This
        /// usually means that you are doing "new SomeType", where SomeType is a distributed type
        /// within code that is not distributed.
        /// </exception>
        public static void Construct(object obj)
        {
            if (!(obj is ITransparent))
            {
                throw new InvalidOperationException(
                    "A call to Construct can only occur on a post processed, distributed object.");
            }

            var transparentObj = (ITransparent)obj;

            // Check to see if we've already got a NetworkName; if we have
            // then we're a named distributed instance that doesn't need the
            // autoid assigned.
            if (transparentObj.NetworkName != null && transparentObj.Node != null)
            {
                return;
            }

            // We need to use some sort of thread static variable; when the post-processor
            // wraps methods, it also needs to update them so that calls to new are adjusted
            // so the thread static variable gets set to the object's current node.  Then we
            // pull the information out here to reassign it.
            // If there's no node context in the thread static variable, then that means someone
            // is new'ing up a distributed object from outside a distributed scope, and they
            // haven't used the Distributed<> class to create it.
            if (DpmConstructContext.LocalNodeContext == null)
            {
                throw new InvalidOperationException(
                    "Unable to determine current construction context for distributed object. "
                    + "You can only new distributed objects directly from inside other "
                    + "distributed objects.  For construction of a distributed object "
                    + "from a local context, use the Distributed<> class.");
            }

            var node = DpmConstructContext.LocalNodeContext;

            // Allocate a randomly generated NetworkName.
            transparentObj.Node = DpmConstructContext.LocalNodeContext;
            transparentObj.NetworkName = "autoid-" + ID.NewRandom();

            // Store the object in the Dht.
            node.Store(transparentObj.NetworkName, obj);

            // Reset the context automatically.
            DpmConstructContext.LocalNodeContext = null;
        }

        /// <summary>
        /// Called when a get property operation is occurring.
        /// </summary>
        /// <param name="d">
        /// The delegate to call if this operation should occur locally.
        /// </param>
        /// <param name="args">
        /// The arguments to the operation.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        public static object GetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
            {
                return InvokeDynamic(d, args);
            }

            // Get the network name of the object.
            var objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the get_ prefix and Distributed suffix.
            var propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the get property.
            return node.GetProperty(objectName, propertyName);
        }

        /// <summary>
        /// Called when a invocation operation is occurring.
        /// </summary>
        /// <param name="d">
        /// The delegate to call if this operation should occur locally.
        /// </param>
        /// <param name="args">
        /// The arguments to the operation.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        public static object Invoke(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
            {
                return InvokeDynamic(d, args);
            }

            // Get the network name of the object and the name of the method.
            var objectName = (d.Target as ITransparent).NetworkName;
            var methodName = d.Method.Name;

            // Get our local node and invoke the method.
            var o = node.Invoke(objectName, methodName, d.Method.GetGenericArguments(), args);
            return o;
        }

        /// <summary>
        /// A method to invoke a delegate locally.
        /// </summary>
        /// <param name="d">
        /// The delegate to invoke.
        /// </param>
        /// <param name="args">
        /// The arguments for the call.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        public static object InvokeDynamic(Delegate d, object[] args)
        {
            return InvokeDynamic(d.GetType().DeclaringType, d.Method, d.Target, d.Method.GetGenericArguments(), args);
        }

        /// <summary>
        /// A method to invoke a delegate locally.
        /// </summary>
        /// <param name="dt">
        /// The type of the target that the invocation will occur on.
        /// </param>
        /// <param name="mi">
        /// The method reference for the method that will be invoked.
        /// </param>
        /// <param name="target">
        /// The target that the invocation will occur on.
        /// </param>
        /// <param name="targs">
        /// The type arguments for the call.
        /// </param>
        /// <param name="args">
        /// The arguments for the call.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="targs"/> is null.
        /// </exception>
        public static object InvokeDynamic(Type dt, MethodInfo mi, object target, Type[] targs, object[] args)
        {
            if (targs == null || targs.Any(x => x == null))
            {
                throw new ArgumentNullException("targs");
            }

            foreach (var nt in dt.GetNestedTypes(BindingFlags.NonPublic))
            {
                if (mi.Name.IndexOf("__") == -1)
                {
                    continue;
                }

                if (nt.FullName.Contains("+" + mi.Name.Substring(0, mi.Name.IndexOf("__")) + "__InvokeDirect"))
                {
                    return InvokeDynamicBase(nt, mi, target, targs, args);
                }
            }

            // Fall back to slow invocation.
            if (mi.IsGenericMethod)
            {
                mi = mi.MakeGenericMethod(targs);
            }

            return mi.Invoke(target, args);
        }

        /// <summary>
        /// Called when an set property operation is occurring.
        /// </summary>
        /// <param name="d">
        /// The delegate to call if this operation should occur locally.
        /// </param>
        /// <param name="args">
        /// The arguments to the add event operation.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        public static object SetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
            {
                return InvokeDynamic(d, args);
            }

            // Get the network name of the object.
            var objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the set_ prefix and Distributed suffix.
            var propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the set property.
            node.SetProperty(objectName, propertyName, args[0]);

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// A method to invoke a delegate locally.
        /// </summary>
        /// <param name="dt">
        /// The type of the target that the invocation will occur on.
        /// </param>
        /// <param name="mi">
        /// The method reference for the method that will be invoked.
        /// </param>
        /// <param name="target">
        /// The target that the invocation will occur on.
        /// </param>
        /// <param name="targs">
        /// The type arguments for the call.
        /// </param>
        /// <param name="args">
        /// The arguments for the call.
        /// </param>
        /// <returns>
        /// The resulting <see cref="object"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there was a unresolved generic parameter.
        /// </exception>
        private static object InvokeDynamicBase(Type dt, MethodInfo mi, object target, Type[] targs, object[] args)
        {
            var tparams = mi.DeclaringType.GetGenericArguments().Concat(targs).ToArray();
            foreach (var t in tparams)
            {
                if (t.IsGenericParameter)
                {
                    throw new InvalidOperationException("Generic parameter not resolved before invocation of method.");
                }
            }

            if (dt.ContainsGenericParameters)
            {
                dt = dt.MakeGenericType(tparams);
            }

            if (mi.ContainsGenericParameters)
            {
                mi = mi.MakeGenericMethod(targs);
            }

            var di = dt.GetConstructor(Type.EmptyTypes).Invoke(null) as IDirectInvoke;
            var o = di.Invoke(mi, target, args);
            return o;
        }

        #endregion
    }
}