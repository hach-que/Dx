using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Process4.Task.Statements;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.IO;

namespace Process4.Task.Wrappers
{
    internal class ConstructorWrapper : IWrapper
    {
        private readonly MethodDefinition m_Constructor = null;
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;

        /// <summary>
        /// The log file this wrapper should use.
        /// </summary>
        public StreamWriter Log { get; set; }

        /// <summary>
        /// Creates a new constructor wrapper which will wrap the specified constructor.
        /// </summary>
        /// <param name="method">The constructor to wrap.</param>
        public ConstructorWrapper(MethodDefinition constructor)
        {
            this.m_Constructor = constructor;
            this.m_Type = constructor.DeclaringType;
            this.m_Module = constructor.Module;
        }

        /// <summary>
        /// Wraps the method.
        /// </summary>
        public void Wrap()
        {
            this.Log.WriteLine("  + m " + this.m_Constructor.Name);

            // Create the Process4.Providers.DpmEntrypoint::Construct method reference.
            MethodReference construct = new MethodReference("Construct", this.m_Type.Module.Import(typeof(void)), this.m_Type.Module.Import(typeof(Process4.Providers.DpmEntrypoint)));
            construct.Parameters.Add(new ParameterDefinition(this.m_Type.Module.Import(typeof(object))));

            // Prepend instructions.
            ILProcessor il = this.m_Constructor.Body.GetILProcessor();
            il.InsertBefore(this.m_Constructor.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            il.InsertAfter(this.m_Constructor.Body.Instructions[0], Instruction.Create(OpCodes.Call, construct));
        }
    }
}
