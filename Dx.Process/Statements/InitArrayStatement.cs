//-----------------------------------------------------------------------
// <copyright file="InitArrayStatement.cs" company="Redpoint Software">
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
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal class InitArrayStatement : Statement
    {
        private TypeReference m_Type = null;
        private VariableDefinition m_Storage = null;
        private Action<ILProcessor, int> m_Filler = null;

        private bool m_CountIsStatic = false;
        private sbyte m_CountStatic = 0;
        private VariableDefinition m_CountStorage = null;

        /// <summary>
        /// Creates a new statement initializing an array of the specified type, with a fixed number of elements,
        /// storing the result of the initialization in the specified variable.
        /// </summary>
        /// <param name="type">The type of objects in the array.</param>
        /// <param name="count">The length of the array.</param>
        /// <param name="storage">The storage variable to place the new array in.</param>
        public InitArrayStatement(TypeReference type, sbyte count, VariableDefinition storage, Action<ILProcessor, int> filler)
            : base(null)
        {
            this.m_Type = type;
            this.m_CountIsStatic = true;
            this.m_CountStatic = count;
            this.m_Storage = storage;
            this.m_Filler = filler;
        }

        /// <summary>
        /// Creates a new statement initializing an array of the specified type, with the length contained in the
        /// specified variable, storing the result of the initialization in the specified variable.
        /// </summary>
        /// <param name="type">The type of objects in the array.</param>
        /// <param name="count">The variable that is holding the length of the array.</param>
        /// <param name="storage">The storage variable to place the new array in.</param>
        public InitArrayStatement(TypeReference type, VariableDefinition count, VariableDefinition storage)
            : base(null)
        {
            this.m_Type = type;
            this.m_CountIsStatic = false;
            this.m_CountStorage = count;
            this.m_Storage = storage;
        }

        public override void Generate(ILProcessor processor)
        {
            // Add the IL that creates the object[] array of arguments.
            if (this.m_CountIsStatic)
                processor.Append(Instruction.Create(OpCodes.Ldc_I4_S, this.m_CountStatic));
            else
                processor.Append(Instruction.Create(OpCodes.Ldloc, this.m_CountStorage));
            processor.Append(Instruction.Create(OpCodes.Newarr, this.m_Type));
            processor.Append(Instruction.Create(OpCodes.Stloc, this.m_Storage));

            // Run the filler function over the entire array (if it is an array
            // with a preknown amount of array elements).
            if (this.m_CountIsStatic && this.m_Filler != null)
            {
                for (int i = 0; i < this.m_CountStatic; i++)
                {
                    this.m_Filler(processor, i);
                }
            }
        }
    }
}
