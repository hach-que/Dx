//-----------------------------------------------------------------------
// <copyright file="IDelegateEmitter.cs" company="Redpoint Software">
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
    using Mono.Cecil;

    /// <summary>
    /// An interface which provides the ability to emit delegate definitions based
    /// on the signature of an existing method.
    /// </summary>
    public interface IDelegateEmitter
    {
        /// <summary>
        /// Emits a delegate that matches the signature of a method being processed.  The delegate
        /// definition is added to the same type that the method is declared on.
        /// </summary>
        /// <returns>The type definition of the delegate.</returns>
        /// <param name="method">The method on which to base the new delegate.</param>
        /// <param name="delegateCtor">A reference to the constructor that can be used to create an instance of the delegate.</param>
        TypeDefinition EmitDelegate(
            MethodDefinition method,
            out MethodReference delegateCtor);
    }
}
