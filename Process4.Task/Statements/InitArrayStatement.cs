using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Process4.Task.Statements
{
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
