using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Mono.Cecil.Cil;

namespace Process4.Task
{
    internal abstract class Statement
    {
        private Stack<OpCode> m_Opcodes = new Stack<OpCode>();
        private VariableDefinition m_Result = null;

        protected Statement(VariableDefinition result)
        {
            this.m_Result = result;
        }

        /// <summary>
        /// The variable to store the result of the statement in.
        /// </summary>
        public VariableDefinition Result
        {
            get { return this.m_Result; }
        }

        /// <summary>
        /// Generates the opcodes for this statement.
        /// </summary>
        public abstract void Generate(ILProcessor processor);
    }
}
