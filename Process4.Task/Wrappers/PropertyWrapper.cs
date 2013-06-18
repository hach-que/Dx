using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;

namespace Process4.Task.Wrappers
{
    internal class PropertyWrapper : IWrapper
    {
        private readonly PropertyDefinition m_Property = null;
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;

        /// <summary>
        /// The log file this wrapper should use.
        /// </summary>
        public StreamWriter Log { get; set; }

        public List<string> Exclusions { get; set; }

        /// <summary>
        /// Creates a new property wrapper which will wrap the specified property.
        /// </summary>
        /// <param name="property">The property to wrap.</param>
        public PropertyWrapper(PropertyDefinition property)
        {
            this.m_Property = property;
            this.m_Type = property.DeclaringType;
            this.m_Module = property.Module;
        }

        /// <summary>
        /// Wraps the property.
        /// </summary>
        public void Wrap()
        {
            // Check to ensure property type has a distributed attribute or is a value type.
            if (this.m_Property.PropertyType.IsValueType ||
                this.m_Property.PropertyType.Resolve().IsInterface ||
                this.m_Property.PropertyType.FullName == "System.String" ||
                Process4Assembler.HasAttribute(this.m_Property.PropertyType.Resolve(), "DistributedAttribute"))
            {
                // This is a valid type.
                this.Log.WriteLine("  + p " + this.m_Property.Name);
            }
            else if (Process4Assembler.HasAttribute(this.m_Property.CustomAttributes, "LocalAttribute"))
            {
                // This is a localized property; add the get and set methods to our
                // exclusion list.
                this.Exclusions.Add(this.m_Property.GetMethod.Name);
                this.Exclusions.Add(this.m_Property.SetMethod.Name);
            }
            else
            {
                // This is an invalid type (throw an exception).
                throw new PostProcessingException(this.m_Type.FullName, this.m_Property.Name,
                    "The property '" + this.m_Property.Name + "' is of type '" + this.m_Property.PropertyType.Name + "', but it is not a distributed or value type.  Distributed types may only contain properties that are value types or types which are also distributed.");
            }
        }
    }
}
