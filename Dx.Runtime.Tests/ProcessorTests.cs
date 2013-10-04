using System;
using System.Reflection;
using System.IO;
using Dx.Process;
using System.Linq;
using Xunit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dx.Runtime.Tests
{
    public class ProcessorTests
    {
        private AssemblyDefinition ProcessAssembly()
        {
            var current = Assembly.GetExecutingAssembly().Location;
            var temp = Path.GetTempFileName();
            File.Copy(current, temp, true);
            File.Copy(
                typeof(ID).Assembly.Location,
                new FileInfo(temp).Directory.FullName + "/" + typeof(ID).Assembly.GetName().Name + ".dll",
                true);
            
            var processor = new DxProcessor();
            processor.AssemblyFile = temp;
            Assert.True(processor.Execute());
            
            return AssemblyDefinition.ReadAssembly(temp);
        }
        
        [Fact]
        public void InterceptsNewInstruction()
        {
            var assembly = this.ProcessAssembly();
            var type = assembly.Modules.First().Types.FirstOrDefault(x => x.Name == "InterceptNewInstructionTest");
            Assert.NotNull(type);
            
            var method = type.Methods.FirstOrDefault(x => x.Name == "PerformConstruction__Distributed0");
            Assert.NotNull(method);
            
            var body = method.Body;
            Assert.NotNull(body);
            
            var instructions = body.Instructions;
            Assert.NotNull(instructions);
            
            var pureInstructions = instructions.Where(x => x.OpCode != OpCodes.Nop).ToArray();
            
            Assert.Equal(7, pureInstructions.Length);
            
            Assert.Equal(OpCodes.Ldarg_0, pureInstructions[0].OpCode);
            Assert.Equal(OpCodes.Call, pureInstructions[1].OpCode);
            Assert.Equal(OpCodes.Newobj, pureInstructions[2].OpCode);
            Assert.Equal(OpCodes.Stloc_0, pureInstructions[3].OpCode);
            Assert.Equal(OpCodes.Br, pureInstructions[4].OpCode);
            Assert.Equal(OpCodes.Ldloc_0, pureInstructions[5].OpCode);
            Assert.Equal(OpCodes.Ret, pureInstructions[6].OpCode);
        }
    }
}

