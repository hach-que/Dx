using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Process4.Task.Statements
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
            processor.Append(Instruction.Create(OpCodes.Newobj, this.m_Constructor));
            processor.Append(Instruction.Create(OpCodes.Stloc, this.m_Storage));
        }
    }
}
