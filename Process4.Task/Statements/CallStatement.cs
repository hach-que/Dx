using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Process4.Task.Statements
{
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
