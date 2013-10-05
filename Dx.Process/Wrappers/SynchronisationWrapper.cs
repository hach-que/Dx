using System;
using System.Diagnostics;
using Mono.Cecil;

namespace Dx.Process
{
    public class SynchronisationWrapper : IWrapper
    {
        /// <summary>
        /// The type on which synchronisation may be applied.
        /// </summary>
        private readonly TypeDefinition m_Type;

        /// <summary>
        /// The trace source on which logging will be done.
        /// </summary>
        private readonly TraceSource m_TraceSource;
    
        public SynchronisationWrapper(TypeDefinition type)
        {
            this.m_Type = type;
            this.m_TraceSource = new TraceSource("SynchronisationWrapper");
        }
        
        public void Wrap()
        {
            var hasSynchronisedProperty = false;
            foreach (var property in this.m_Type.Properties)
            {
                if (Utility.HasAttribute(property.CustomAttributes, "SynchronisedAttribute"))
                {
                    hasSynchronisedProperty = true;
                }
            }
            
            if (!hasSynchronisedProperty)
                return;
                
            this.m_TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "Detected synchronisation attribute on one or more properties of {0}",
                this.m_Type.FullName);
        }
    }
}

