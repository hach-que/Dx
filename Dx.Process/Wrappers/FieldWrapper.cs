//-----------------------------------------------------------------------
// <copyright file="FieldWrapper.cs" company="Redpoint Software">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;
using Dx.Process;
using System.Diagnostics;

namespace Dx.Process
{
    internal class FieldWrapper : IWrapper
    {
        private readonly FieldDefinition m_Field = null;
        private readonly TypeDefinition m_Type = null;
        private readonly ModuleDefinition m_Module = null;
        
        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;

        /// <summary>
        /// Creates a new field wrapper which will wrap the specified field.
        /// </summary>
        /// <param name="field">The field to wrap.</param>
        public FieldWrapper(FieldDefinition field)
        {
            this.m_Field = field;
            this.m_Type = field.DeclaringType;
            this.m_Module = field.Module;
            this.m_TraceSource = new TraceSource("FieldWrapper");
        }

        /// <summary>
        /// Wraps the field.
        /// </summary>
        public void Wrap()
        {
            // Throw an exception if it's not a compiler generated field.
            if (this.m_Field.CustomAttributes.Where(c => c.AttributeType.Name == "CompilerGeneratedAttribute").Count() == 0 &&
                !this.IsEvent(this.m_Field.FieldType) &&
                !DxProcessor.HasAttribute(this.m_Field.CustomAttributes, "LocalAttribute"))
                throw new PostProcessingException(this.m_Type.FullName, this.m_Field.Name, "The field '" + this.m_Field.Name + "' was found.  Distributed types may not contain fields as they can not be hooked successfully.  Use auto-generated properties instead.");
        }

        /// <summary>
        /// Determines whether a specified type inherits from EventHandler
        /// somewhere along it's hierarchy.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public bool IsEvent(TypeReference type)
        {
            if (type == null)
                return false;
            else if (type.FullName == "System.EventHandler")
                return true;
            else if (type.FullName == "System.Object")
                return false;
            else
                return this.IsEvent(type.Resolve().BaseType);
        }
    }
}
