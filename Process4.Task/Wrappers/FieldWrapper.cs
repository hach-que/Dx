using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;

namespace Process4.Task.Wrappers
{
    internal class FieldWrapper : IWrapper
    {
        private readonly FieldDefinition m_Field = null;
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;

        /// <summary>
        /// The log file this wrapper should use.
        /// </summary>
        public StreamWriter Log { get; set; }

        /// <summary>
        /// Creates a new field wrapper which will wrap the specified field.
        /// </summary>
        /// <param name="field">The field to wrap.</param>
        public FieldWrapper(FieldDefinition field)
        {
            this.m_Field = field;
            this.m_Type = field.DeclaringType;
            this.m_Module = field.Module;
        }

        /// <summary>
        /// Wraps the field.
        /// </summary>
        public void Wrap()
        {
            // Throw an exception if it's not a compiler generated field.
            if (this.m_Field.CustomAttributes.Where(c => c.AttributeType.Name == "CompilerGeneratedAttribute").Count() == 0 &&
                !this.IsEvent(this.m_Field.FieldType))
                throw new PostProcessingException(this.m_Type.FullName, this.m_Field.Name, "The field '" + this.m_Field.Name + "' was found.  Distributed types may not contain fields as they can not be hooked successfully.  Use auto-generated properties instead.");
        }

        /// <summary>
        /// Determines whether a specified type inherits from EventHandler
        /// somewhere along it's hierarchy.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public bool IsEvent(TypeReference type)
        {
            if (type.FullName == "System.EventHandler")
                return true;
            else if (type.FullName == "System.Object")
                return false;
            else
                return this.IsEvent(type.Resolve().BaseType);
        }
    }
}
