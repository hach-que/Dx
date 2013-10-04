using System;
using System.IO;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Collections.Generic;
using Process4.Task.Wrappers;
using System.Reflection;

namespace Dx.Process
{
    public class DxProcessor : ITask
    {
        [Required]
        public string AssemblyFile { get; set; }

        /// <summary>
        /// The log file to write to.
        /// </summary>
        public StreamWriter Log { get; set; }

        static void Main(string[] args)
        {
            var p = new DxProcessor();
            p.AssemblyFile = args[0];
            p.Execute();
        }

        /// <summary>
        /// Called when the MSBuild task executes.  Also invoked by Program.Main
        /// when run from the command-line.
        /// </summary>
        /// <returns>Whether the build task succeeded.</returns>
        public bool Execute()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(this.AssemblyFile));

            this.Log = new StreamWriter("./Process4.Task.Output.txt", false);
            this.Log.WriteLine("== BEGIN (" + DateTime.Now.ToString() + ") ==");

            try
            {
                // Get the assembly based on the path.
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                    this.AssemblyFile,
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
                        this.Log.WriteLine("- " + type.Name);
                        continue;
                    }

                    // Check to see whether this type has a DistributedAttribute
                    // attached to it.
                    if (!DxProcessor.HasAttribute(type, "DistributedAttribute"))
                    {
                        this.Log.WriteLine("- " + type.Name);
                        continue;
                    }

                    // Check to see whether this type has a ProcessedAttribute
                    // attached to it.
                    if (DxProcessor.HasAttributeSpecific(type, "ProcessedAttribute"))
                    {
                        this.Log.WriteLine("+ " + type.Name + " (already processed)");
                        continue;
                    }

                    // This type is marked as distributed, so we need to perform
                    // wrapping on it.
                    this.Log.WriteLine("+ " + type.Name);
                    TypeWrapper wrapper = new TypeWrapper(type);
                    wrapper.Log = this.Log;
                    wrapper.Wrap();
                }

                assembly.Write(this.AssemblyFile, new WriterParameters { WriteSymbols = true });
                this.Log.WriteLine("== SUCCESS ==");
                this.Log.Close();

                return true;
            }
            catch (PostProcessingException e)
            {
                this.Log.WriteLine("Post Processing Exception Occurred!");
                this.Log.WriteLine(e.OffendingMember + " in " + e.OffendingType + " caused:");
                this.Log.WriteLine(e.GetType().FullName);
                this.Log.WriteLine(e.Message);
                this.Log.WriteLine(e.StackTrace);
                this.Log.WriteLine("== ERROR: EXIT ==");
                if (this.BuildEngine != null)
                    this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Post Processing", "E0002", e.OffendingType + "." + e.OffendingMember, 0, 0, 0, 0, e.Message, "", ""));
                this.Log.Close();
                return false;
            }
            catch (Exception e)
            {
                this.Log.WriteLine("Exception Occurred!");
                this.Log.WriteLine(e.GetType().FullName);
                this.Log.WriteLine(e.Message);
                this.Log.WriteLine(e.StackTrace);
                this.Log.WriteLine("== FATAL: EXIT ==");
                if (this.BuildEngine != null)
                    this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("General", "E0001", "", 0, 0, 0, 0, e.Message, "", ""));
                this.Log.Close();
                return false;
            }
        }

        #region Utility Functions

        internal static bool HasAttribute(TypeDefinition type, string name)
        {
            while (type != null)
            {
                if (DxProcessor.HasAttributeSpecific(type, name))
                    return true;
                if (type.BaseType != null)
                    type = type.BaseType.Resolve();
                else
                    type = null;
            }
            return false;
        }

        internal static bool HasAttribute(Collection<CustomAttribute> attributes, string name)
        {
            foreach (CustomAttribute ca in attributes)
            {
                if (DxProcessor.AttributeMatches(ca.AttributeType, name))
                    return true;
            }
            return false;
        }

        private static bool HasAttributeSpecific(TypeDefinition type, string name)
        {
            foreach (CustomAttribute ca in type.CustomAttributes)
            {
                if (DxProcessor.AttributeMatches(ca.AttributeType, name))
                    return true;
            }
            return false;
        }

        private static bool AttributeMatches(TypeReference type, string name)
        {
            if (type.Name == name)
                return true;
            else if (type.Name == "Attribute")
                return (name == "Attribute");
            else
                return DxProcessor.AttributeMatches(type.Resolve().BaseType, name);
        }

        #endregion

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
