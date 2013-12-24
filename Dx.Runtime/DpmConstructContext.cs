// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DpmConstructContext.cs" company="Redpoint Software">
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
//   The distributed construct context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The distributed construct context.  This maintains a construction context which is used
    /// when the "new" operator is called on types when executing inside a distributed object.
    /// </summary>
    public static class DpmConstructContext
    {
        #region Static Fields

        /// <summary>
        /// This thread static variable holds the current local node that is being used
        /// to store objects.  We need this information when the constructor of a
        /// distributed object calls back into Dx so that we can store it in the LocalNode's
        /// data storage.  All methods in distributed objects are instrumented so that
        /// any calls to new are immediately preceded with a call to assign the local
        /// node context here.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.MaintainabilityRules", 
            "SA1401:FieldsMustBePrivate", 
            Justification = "Reviewed. Suppression is OK here.")]
        [ThreadStatic]
        public static ILocalNode LocalNodeContext;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Used by instrumented code to assign the local node context before new calls.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        public static void AssignNodeContext(object obj)
        {
            var transparent = obj as ITransparent;

            if (transparent == null)
            {
                throw new InvalidOperationException();
            }

            if (transparent.Node == null)
            {
                throw new InvalidOperationException("Node not set against distributed object.");
            }

            LocalNodeContext = transparent.Node;
        }

        #endregion
    }
}