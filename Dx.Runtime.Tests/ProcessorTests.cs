using System.Linq;
using Dx.Runtime.Tests.Data;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class ProcessorTests
    {
        private AssemblyDefinition GetAssembly()
        {
            return AssemblyDefinition.ReadAssembly(typeof(InterceptNewInstructionTest).Assembly.Location);
        }

        [Fact, Trait("Type", "Processor")]
        public void InterceptsNewInstruction()
        {
            var assembly = this.GetAssembly();
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
            // Don't check instruction 4, it varies between Mono and MS compilers.
            Assert.Equal(OpCodes.Ldloc_0, pureInstructions[5].OpCode);
            Assert.Equal(OpCodes.Ret, pureInstructions[6].OpCode);
        }

        [Fact, Trait("Type", "Processor")]
        public void ProducesIdenticalCodeToPreprocessedGenericType()
        {
            var assembly = this.GetAssembly();
            var preprocessed = assembly.Modules.First().Types.First(x => x.Name == "PreprocessedGenericType`3");
            var postprocessed = assembly.Modules.First().Types.First(x => x.Name == "GenericType`3");
            var utilities = new ProcessorUtilities();
            utilities.AssertTypesAreIdentical(preprocessed, postprocessed);
        }

        [Fact, Trait("Type", "Processor")]
        public void ProducesIdenticalCodeToPreprocessedGenericMethod()
        {
            var assembly = this.GetAssembly();
            var preprocessed = assembly.Modules.First().Types.First(x => x.Name == "PreprocessedGenericMethod");
            var postprocessed = assembly.Modules.First().Types.First(x => x.Name == "GenericMethod");
            var utilities = new ProcessorUtilities();
            utilities.AssertTypesAreIdentical(preprocessed, postprocessed);
        }

        [Fact, Trait("Type", "Processor")]
        public void ProducesIdenticalCodeToPreprocessedGenericTypeAndMethod()
        {
            var assembly = this.GetAssembly();
            var preprocessed = assembly.Modules.First().Types.First(x => x.Name == "PreprocessedGenericTypeAndMethod`2");
            var postprocessed = assembly.Modules.First().Types.First(x => x.Name == "GenericTypeAndMethod`2");
            var utilities = new ProcessorUtilities();
            utilities.AssertTypesAreIdentical(preprocessed, postprocessed);
        }
    }
}

