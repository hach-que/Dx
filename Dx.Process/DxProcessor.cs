using System;
using System.IO;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.Diagnostics;

namespace Dx.Process
{
    public class DxProcessor : ITask
    {
        [Required]
        public string AssemblyFile { get; set; }

        /// <summary>
        /// Called when the MSBuild task executes.  Also invoked by Program.Main
        /// when run from the command-line.
        /// </summary>
        /// <returns>Whether the build task succeeded.</returns>
        public bool Execute()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(this.AssemblyFile));

            var source = new TraceSource("Processor", SourceLevels.All);
            source.TraceEvent(TraceEventType.Information, 0, "Processor started at {0:G}", DateTime.Now);

            try
            {
                // Get the assembly based on the path.
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                    Path.GetFileName(this.AssemblyFile),
                    new ReaderParameters
                    {
                        ReadSymbols = File.Exists(this.AssemblyFile + ".mdb"),
                    });

                // Get all of the types in the assembly.
                TypeDefinition[] types = assembly.MainModule.Types.ToArray();
                foreach (TypeDefinition type in types)
                {
                    // Skip the module and Program classes.
                    if (type.Name == "<Module>" || type.Name == "Program")
                    {
                        source.TraceEvent(TraceEventType.Information, 0, "Skipped {0} because it is a module", type.Name);
                        continue;
                    }
                    
                    // Apply synchronisation wrapper.
                    new SynchronisationWrapper(type).Wrap();

                    // Check to see whether this type has a DistributedAttribute
                    // attached to it.
                    if (!Utility.HasAttribute(type, "DistributedAttribute"))
                    {
                        source.TraceEvent(TraceEventType.Information, 0, "Skipped {0} because it does not have DistributedAttribute", type.Name);
                        continue;
                    }

                    // Check to see whether this type has a ProcessedAttribute
                    // attached to it.
                    if (Utility.HasAttributeSpecific(type, "ProcessedAttribute"))
                    {
                        source.TraceEvent(TraceEventType.Information, 0, "Skipped {0} because it has already been processed", type.Name);
                        continue;
                    }

                    // This type is marked as distributed, so we need to perform
                    // wrapping on it.
                    source.TraceEvent(TraceEventType.Information, 0, "Starting processing of {0}", type.Name);
                    new TypeWrapper(type).Wrap();
                    source.TraceEvent(TraceEventType.Information, 0, "Finished processing of {0}", type.Name);
                }

                assembly.Write(Path.GetFileName(this.AssemblyFile), new WriterParameters { WriteSymbols = true });
                source.TraceEvent(TraceEventType.Information, 0, "Processor completed successfully at {0:G}", DateTime.Now);

                return true;
            }
            catch (PostProcessingException e)
            {
                source.TraceEvent(
                    TraceEventType.Critical,
                    0,
@"Post Processing Exception Occurred!
{0} in {1} caused:
{2}
{3}
{4}",
                    e.OffendingMember, 
                    e.OffendingType,
                    e.GetType().FullName,
                    e.Message,
                    e.StackTrace);
                source.TraceEvent(TraceEventType.Stop, 0, "Processor failed at {0:G}", DateTime.Now);
                if (this.BuildEngine != null)
                    this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Post Processing", "E0002", e.OffendingType + "." + e.OffendingMember, 0, 0, 0, 0, e.Message, "", ""));
                return false;
            }
            catch (Exception e)
            {
                source.TraceEvent(
                    TraceEventType.Critical,
                    0,
@"Exception Occurred!
{0}
{1}
{2}",
                    e.GetType().FullName,
                    e.Message,
                    e.StackTrace);
                source.TraceEvent(TraceEventType.Stop, 0, "Processor failed at {0:G}", DateTime.Now);
                if (this.BuildEngine != null)
                    this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("General", "E0001", "", 0, 0, 0, 0, e.Message, "", ""));
                return false;
            }
        }

        #region ITask Members

        public IBuildEngine BuildEngine
        {
            get;
            set;
        }

        public ITaskHost HostObject
        {
            get;
            set;
        }

        #endregion
    }
}

