//-----------------------------------------------------------------------
// <copyright file="ConstructorWrapper.cs" company="Redpoint Software">
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
    using Dx.Runtime;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System.Diagnostics;

    internal class ConstructorWrapper : IWrapper
    {
        private readonly MethodDefinition m_Constructor = null;
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;
        
        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;

        /// <summary>
        /// Creates a new constructor wrapper which will wrap the specified constructor.
        /// </summary>
        /// <param name="method">The constructor to wrap.</param>
        public ConstructorWrapper(MethodDefinition constructor)
        {
            this.m_Constructor = constructor;
            this.m_Type = constructor.DeclaringType;
            this.m_Module = constructor.Module;
            this.m_TraceSource = new TraceSource("ConstructorWrapper");
        }

        /// <summary>
        /// Wraps the method.
        /// </summary>
        public void Wrap()
        {
            this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Modifying {0} for distributed processing", this.m_Constructor.Name);

            // Create the Process4.Providers.DpmEntrypoint::Construct method reference.
            var construct = new MethodReference("Construct", this.m_Type.Module.Import(typeof(void)), this.m_Type.Module.Import(typeof(DpmEntrypoint)));
            construct.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object))));

            // Prepend instructions.
            var il = this.m_Constructor.Body.GetILProcessor();
            il.InsertBefore(this.m_Constructor.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            il.InsertAfter(this.m_Constructor.Body.Instructions[0], Instruction.Create(OpCodes.Call, construct));
        }
    }
}
