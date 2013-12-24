using Mono.Cecil.Cil;

namespace Dx.Process
{
    internal class StatementProcessor
    {
        private ILProcessor m_IL = null;

        /// <summary>
        /// Creates a new statement processor targetting the underlying
        /// IL processor.
        /// </summary>
        /// <param name="processor">The ILProcessor to target.</param>
        public StatementProcessor(ILProcessor processor)
        {
            this.m_IL = processor;
        }

        /// <summary>
        /// Adds a statement.
        /// </summary>
        /// <param name="statement">The statement to add.</param>
        public void Add(Statement statement)
        {
            statement.Generate(this.m_IL);
            if (statement.Result != null)
            {
                if (statement.Result.VariableType.Name.ToLower() == "void")
                {
                    // Void type.  Just return.
                    this.m_IL.Append(Instruction.Create(OpCodes.Ret));
                }
                else
                {
                    // Other type.  Return the value.
                    Instruction ldloc = Instruction.Create(OpCodes.Ldloc, statement.Result);
                    this.m_IL.Append(ldloc);
                    this.m_IL.Append(Instruction.Create(OpCodes.Ret));
                    this.m_IL.InsertBefore(ldloc, Instruction.Create(OpCodes.Br_S, ldloc));
                }
            }
        }
    }
}
