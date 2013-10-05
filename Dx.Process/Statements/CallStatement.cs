//-----------------------------------------------------------------------
// <copyright file="CallStatement.cs" company="Redpoint Software">
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
    using Mono.Cecil.Cil;

    internal class CallStatement : Statement
    {
        private TypeReference m_ReturnType = null;
        private MethodReference m_Target = null;
        private VariableDefinition[] m_Parameters = null;

        public CallStatement(MethodReference target, VariableDefinition[] parameters, TypeReference returnType, VariableDefinition result) : base(result)
        {
            // target - method to be invoked
            // parameters - the variables containing parameters to be passed
            // returnType - the expected return type (for casting purposes)
            // result - where returned variable is stored
            this.m_Target = target;
            this.m_ReturnType = returnType;
            this.m_Parameters = parameters;
        }

        public override void Generate(ILProcessor processor)
        {
            // Load the arguments.
            foreach (VariableDefinition v in m_Parameters)
            {
                processor.Append(Instruction.Create(OpCodes.Ldloc, v));
            }

            // Call the delegate.
            processor.Append(Instruction.Create(OpCodes.Call, this.m_Target));

            // Handle the return type.
            if (this.m_ReturnType.FullName == this.m_Target.Module.Import(typeof(void)).FullName)
            {
                // Return value is void.  Discard any result and return.
                processor.Append(Instruction.Create(OpCodes.Pop));
            }
            else if (this.m_ReturnType.IsValueType || this.m_ReturnType.IsGenericParameter)
            {
                // Return value is value type (not reference).  Unbox and return it.
                processor.Append(Instruction.Create(OpCodes.Unbox_Any, this.m_ReturnType));
                processor.Append(Instruction.Create(OpCodes.Stloc, this.Result));
            }
            else 
            {
                // Return value is reference type.  Cast it and return it.
                processor.Append(Instruction.Create(OpCodes.Isinst, this.m_ReturnType));
                processor.Append(Instruction.Create(OpCodes.Stloc, this.Result));
            }
        }
    }
}
