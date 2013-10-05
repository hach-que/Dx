//-----------------------------------------------------------------------
// <copyright file="TypeWrapper.cs" company="Redpoint Software">
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
    using System.Linq;
    using Dx.Runtime;
    using Mono.Cecil;

    /// <summary>
    /// Wraps a type for distributed behaviour.
    /// </summary>
    internal class TypeWrapper : IWrapper
    {
        /// <summary>
        /// The type to be wrapped.
        /// </summary>
        private readonly TypeDefinition m_Type;
        
        /// <summary>
        /// The module containing the type.
        /// </summary>
        private readonly ModuleDefinition m_Module;
        
        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Process4.Task.Wrappers.TypeWrapper"/> class.
        /// </summary>
        /// <param name="type">Type to be wrapped.</param>
        public TypeWrapper(TypeDefinition type)
        {
            this.m_Type = type;
            this.m_Module = type.Module;
            this.m_TraceSource = new TraceSource("TypeWrapper");
        }

        /// <summary>
        /// Wraps the type.
        /// </summary>
        public void Wrap()
        {
            // Add attributes.
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Adding ProcessedAttribute to {0}", this.m_Type.FullName);
            Utility.AddAttribute(this.m_Type, typeof(ProcessedAttribute), this.m_Type.Module);
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Marking {0} as Serializable", this.m_Type.FullName);
            this.m_Type.Attributes |= TypeAttributes.Serializable;

            // Add appropriate interfaces.
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Adding interface ITransparent to {0}", this.m_Type.FullName);
            this.m_Type.Interfaces.Add(this.m_Module.Import(typeof(ITransparent)));
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Adding interface ISerializable to {0}", this.m_Type.FullName);
            this.m_Type.Interfaces.Add(this.m_Module.Import(typeof(System.Runtime.Serialization.ISerializable)));

            // Add required properties.
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Implementing auto-property NetworkName on {0}", this.m_Type.FullName);
            Utility.AddAutoProperty(this.m_Type, "NetworkName", typeof(string));
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Implementing auto-property Node on {0}", this.m_Type.FullName);
            Utility.AddAutoProperty(this.m_Type, "Node", typeof(ILocalNode));

            // Define a list of exclusions for wrapping.
            List<string> exclusions = new List<string>
            {
                "NetworkName",
                "get_NetworkName",
                "set_NetworkName",
                "Node",
                "get_Node",
                "set_Node"
            };

            // Wrap all of our other properties, methods and fields.
            foreach (var p in this.m_Type.Properties.Where(value => !exclusions.Contains(value.Name)).ToList())
            {
                (new PropertyWrapper(p) { Exclusions = exclusions }).Wrap();
            }
            
            foreach (var p in this.m_Type.Properties.Where(value => exclusions.Contains(value.Name)).ToList())
            {
                this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Skipping property wrapping on {0} because it is an excluded property", p.Name);
            }
            
            foreach (var m in this.m_Type.Methods.Where(value => !exclusions.Contains(value.Name) && !value.IsStatic && value.Name != ".ctor").ToList())
            {
                (new MethodWrapper(m)).Wrap();
            }
            
            foreach (var m in this.m_Type.Methods.Where(value => (exclusions.Contains(value.Name) || value.IsStatic) && value.Name != ".ctor").ToList())
            {
                this.m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Skipping method wrapping on {0} because it is an excluded method", m.Name);
            }
            
            foreach (var m in this.m_Type.Methods.Where(value => value.Name == ".ctor").ToList())
            {
                (new ConstructorWrapper(m)).Wrap();
            }
            
            foreach (var f in this.m_Type.Fields.ToList())
            {
                (new FieldWrapper(f)).Wrap();
            }

            // Add required constructors / methods.
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Implementing deserialization constructor on {0}", this.m_Type.FullName);
            Utility.AddDeserializationConstructor(this.m_Type);
            this.m_TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Implementing serialization method on {0}", this.m_Type.FullName);
            Utility.AddSerializationMethod(this.m_Type);
        }
    }
}
