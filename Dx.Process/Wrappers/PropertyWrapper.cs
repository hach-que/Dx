//-----------------------------------------------------------------------
// <copyright file="PropertyWrapper.cs" company="Redpoint Software">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using Mono.Cecil;

    /// <summary>
    /// Wraps a property for distributed behaviour.
    /// </summary>
    internal class PropertyWrapper : IWrapper
    {
        /// <summary>
        /// The property being wrapped.
        /// </summary>
        private readonly PropertyDefinition m_Property;
        
        /// <summary>
        /// The type that declares the property being wrapped.
        /// </summary>
        private readonly TypeDefinition m_Type;
        
        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dx.Process.PropertyWrapper"/> class.
        /// </summary>
        /// <param name="property">The property to wrap.</param>
        public PropertyWrapper(PropertyDefinition property)
        {
            this.m_Property = property;
            this.m_Type = property.DeclaringType;
            this.m_TraceSource = new TraceSource("PropertyWrapper");
        }
        
        /// <summary>
        /// Gets or sets the exclusions where automatically implemented methods of localised properties will be appended.
        /// </summary>
        /// <value>The exclusions where automatically implemented methods of localised properties will be appended.</value>
        public List<string> Exclusions { get; set; }

        /// <summary>
        /// Wraps the property.
        /// </summary>
        public void Wrap(WrapContext context)
        {
            // Check to ensure property type has a distributed attribute or is a value type.
            if (this.m_Property.PropertyType.Resolve() == null)
            {
                // Not sure what to do with this?
                var warning = 
                    "Property {0} has type {1} which does not resolve to a full type.  If this is a " +
                    "generic type, ensure that the generic type is restricted to value types or other " +
                    "distributed types.  If a non-distributed type is used, runtime exceptions may occur.";
                this.m_TraceSource.TraceEvent(
                    TraceEventType.Warning, 
                    0,
                    warning,
                    this.m_Property.Name,
                    this.m_Property.PropertyType.Name);
            }
            else if (this.m_Property.PropertyType.IsValueType ||
                this.m_Property.PropertyType.IsArray ||
                this.m_Property.PropertyType.Resolve().IsInterface ||
                this.m_Property.PropertyType.FullName == "System.String" ||
                Utility.HasAttribute(this.m_Property.PropertyType.Resolve(), "DistributedAttribute"))
            {
                // This is a valid type.
                this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Recognised {0} as valid property", this.m_Property.Name);
            }
            else if (Utility.HasAttribute(this.m_Property.CustomAttributes, "LocalAttribute"))
            {
                // This is a localized property; add the get and set methods to our
                // exclusion list.
                this.m_TraceSource.TraceEvent(
                    TraceEventType.Verbose,
                    0,
                    "Adding getter / setters of {0} to exclusions list because the property is marked with LocalAttribute",
                    this.m_Property.Name);
                this.Exclusions.Add(this.m_Property.GetMethod.Name);
                this.Exclusions.Add(this.m_Property.SetMethod.Name);
            }
            else
            {
                // This is an invalid type (throw an exception).
                this.m_TraceSource.TraceEvent(TraceEventType.Error, 0, "Property {0} isn't of a valid type", this.m_Property.Name);
                throw new PostProcessingException(
                    this.m_Type.FullName,
                    this.m_Property.Name,
                    "The property '" + this.m_Property.Name + "' is of type '" + this.m_Property.PropertyType.Name + "', but it is not a distributed or value type.  Distributed types may only contain properties that are value types or types which are also distributed.");
            }
        }
    }
}
