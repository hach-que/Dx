//-----------------------------------------------------------------------
// <copyright file="InitDelegateStatement.cs" company="Redpoint Software">
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
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Dx.Process
{
    internal class InitDelegateStatement : Statement
    {
        private MethodReference m_Constructor = null;
        private MethodReference m_Target = null;
        private VariableDefinition m_Storage = null;

        /// <summary>
        /// Creates a new statement initializing a delegate variable.
        /// </summary>
        /// <param name="constructor">The constructor for the delegate type.</param>
        /// <param name="target">The target the delegate will invoke.</param>
        /// <param name="storage">The variable which will store the new delegate.</param>
        public InitDelegateStatement(MethodReference constructor, MethodReference target, VariableDefinition storage)
            : base(null)
        {
            this.m_Constructor = constructor;
            this.m_Target = target;
            this.m_Storage = storage;
        }

        public override void Generate(ILProcessor processor)
        {
            // Now add the IL that creates the delegate.
            processor.Append(Instruction.Create(OpCodes.Nop));
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldftn, this.m_Target));
            //processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Newobj, this.m_Constructor));
            processor.Append(Instruction.Create(OpCodes.Stloc, this.m_Storage));
        }
    }
}
