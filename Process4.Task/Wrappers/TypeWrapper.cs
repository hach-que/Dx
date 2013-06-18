using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;

namespace Process4.Task.Wrappers
{
    internal class TypeWrapper : IWrapper
    {
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;

        /// <summary>
        /// The log file this wrapper should use.
        /// </summary>
        public StreamWriter Log { get; set; }

        /// <summary>
        /// Creates a new type wrapper which will wrap the specified type.
        /// </summary>
        /// <param name="type"></param>
        public TypeWrapper(TypeDefinition type)
        {
            this.m_Type = type;
            this.m_Module = type.Module;
        }

        /// <summary>
        /// Wraps the type.
        /// </summary>
        public void Wrap()
        {
            // Add attributes.
            Utility.AddAttribute(this.m_Type, typeof(Process4.Attributes.ProcessedAttribute), this.m_Type.Module);
            this.m_Type.Attributes |= TypeAttributes.Serializable;

            // Add appropriate interfaces.
            this.m_Type.Interfaces.Add(this.m_Module.Import(typeof(Process4.Interfaces.ITransparent)));
            this.m_Type.Interfaces.Add(this.m_Module.Import(typeof(System.Runtime.Serialization.ISerializable)));

            // Add required properties.
            Utility.AddAutoProperty(this.m_Type, "NetworkName", typeof(string));
            Utility.AddAutoProperty(this.m_Type, "IsImmutablyPushed", typeof(bool));

            // Define a list of exclusions for wrapping.
            List<string> exclusions = new List<string>
            {
                "NetworkName",
                "get_NetworkName",
                "set_NetworkName",
                "IsImmutablyPushed",
                "get_IsImmutablyPushed",
                "set_IsImmutablyPushed"
            };

            // Wrap all of our other properties, methods and fields.
            foreach (PropertyDefinition p in this.m_Type.Properties.Where(value => !exclusions.Contains(value.Name)).ToList())
                (new PropertyWrapper(p) { Log = this.Log, Exclusions = exclusions }).Wrap();
            foreach (PropertyDefinition p in this.m_Type.Properties.Where(value => exclusions.Contains(value.Name)).ToList())
                this.Log.WriteLine("  - p " + p.Name + " (excluded)");
            foreach (MethodDefinition m in this.m_Type.Methods.Where(value => !exclusions.Contains(value.Name) && !value.IsStatic && value.Name != ".ctor").ToList())
                (new MethodWrapper(m) { Log = this.Log }).Wrap();
            foreach (MethodDefinition m in this.m_Type.Methods.Where(value => (exclusions.Contains(value.Name) || value.IsStatic) && value.Name != ".ctor").ToList())
                this.Log.WriteLine("  - m " + m.Name + " (excluded)");
            foreach (MethodDefinition m in this.m_Type.Methods.Where(value => value.Name == ".ctor").ToList())
                (new ConstructorWrapper(m) { Log = this.Log }).Wrap();
            foreach (FieldDefinition f in this.m_Type.Fields.ToList())
                (new FieldWrapper(f) { Log = this.Log }).Wrap();

            // Add required constructors / methods.
            Utility.AddDeserializationConstructor(this.m_Type);
            Utility.AddSerializationMethod(this.m_Type);
        }
    }
}
