using System.Linq;
using Mono.Cecil;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class ProcessorUtilities
    {
        public void AssertTypesAreIdentical(TypeDefinition expected, TypeDefinition actual)
        {
            Assert.Equal(expected.Fields.Count, actual.Fields.Count);
            Assert.Equal(expected.Methods.Count, actual.Methods.Count);
            Assert.Equal(expected.Properties.Count, actual.Properties.Count);

            // Verify generic parameters.
            this.AssertGenericParametersAreIdentical(expected, actual);

            // Verify fields.
            var expectedFieldNames = expected.Fields.Select(x => x.Name);
            var actualFieldNames = actual.Fields.Select(x => x.Name);
            foreach (var name in expectedFieldNames)
            {
                Assert.Contains(name, actualFieldNames);
                this.AssertFieldsAreIdentical(
                    expected.Fields.First(x => x.Name == name),
                    actual.Fields.First(x => x.Name == name));
            }

            // Verify properties.
            var expectedPropertyNames = expected.Properties.Select(x => x.Name);
            var actualPropertyNames = actual.Properties.Select(x => x.Name);
            foreach (var name in expectedPropertyNames)
            {
                Assert.Contains(name, actualPropertyNames);
                this.AssertPropertiesAreIdentical(
                    expected.Properties.First(x => x.Name == name),
                    actual.Properties.First(x => x.Name == name));
            }

            // Verify methods.
            var expectedMethodNames = expected.Methods.Select(x => x.Name);
            var actualMethodNames = actual.Methods.Select(x => x.Name);
            foreach (var name in expectedMethodNames)
            {
                Assert.Contains(name, actualMethodNames);
                this.AssertMethodsAreIdentical(
                    expected.Methods.First(x => x.Name == name),
                    actual.Methods.First(x => x.Name == name));
            }
        }

        private void AssertFieldsAreIdentical(FieldDefinition expected, FieldDefinition actual)
        {
            Assert.Equal(expected.Attributes, actual.Attributes);
            Assert.Equal(expected.FieldType.FullName, actual.FieldType.FullName);
        }

        private void AssertPropertiesAreIdentical(PropertyDefinition expected, PropertyDefinition actual)
        {
            Assert.Equal(expected.Attributes, actual.Attributes);
            Assert.Equal(expected.PropertyType.FullName, expected.PropertyType.FullName);
        }

        private void AssertMethodsAreIdentical(MethodDefinition expected, MethodDefinition actual)
        {
            // Verify return type.
            Assert.Equal(expected.ReturnType.Name, actual.ReturnType.Name);

            // Verify generic parameters.
            this.AssertGenericParametersAreIdentical(expected, actual);

            // Verify normal parameters.
            Assert.Equal(expected.Parameters.Count, actual.Parameters.Count);
            for (var i = 0; i < expected.Parameters.Count; i++)
            {
                Assert.Equal(
                    expected.Parameters[i].ParameterType.FullName,
                    actual.Parameters[i].ParameterType.FullName);
            }

            // Verify body.
            Assert.Equal(expected.HasBody, actual.HasBody);
            Assert.Equal(expected.Body.InitLocals, actual.Body.InitLocals);

            // Verifying instructions is damn near impossible due to compiler
            // optimizations.  We can however find all instructions with
            // operands that are TypeReferences and MethodReferences.  If the
            // code is identical, then the order should be the same for these
            // instructions as well (and because there's operands, there's no
            // short opcode versions of these instructions).
            var expectedTypeInstructions = expected.Body.Instructions.Where(x => x.Operand is TypeReference).ToArray();
            var actualTypeInstructions = actual.Body.Instructions.Where(x => x.Operand is TypeReference).ToArray();
            var expectedMethodInstructions = expected.Body.Instructions.Where(x => x.Operand is MethodReference).ToArray();
            var actualMethodInstructions = actual.Body.Instructions.Where(x => x.Operand is MethodReference).ToArray();

            // Verify instructions with type references.
            Assert.Equal(expectedTypeInstructions.Length, actualTypeInstructions.Length);
            for (var i = 0; i < expectedTypeInstructions.Length; i++)
            {
                Assert.Equal(expectedTypeInstructions[i].OpCode, actualTypeInstructions[i].OpCode);
                this.AssertTypeReferencesAreIdentical(
                    (TypeReference)expectedTypeInstructions[i].Operand,
                    (TypeReference)actualTypeInstructions[i].Operand);
            }

            // Verify instructions with method references.
            Assert.Equal(expectedMethodInstructions.Length, actualMethodInstructions.Length);
            for (var i = 0; i < expectedMethodInstructions.Length; i++)
            {
                Assert.Equal(expectedMethodInstructions[i].OpCode, actualMethodInstructions[i].OpCode);
                this.AssertMethodReferencesAreIdentical(
                    (MethodReference)expectedMethodInstructions[i].Operand,
                    (MethodReference)actualMethodInstructions[i].Operand);
            }
        }

        private void AssertGenericParametersAreIdentical(IGenericParameterProvider expected, IGenericParameterProvider actual)
        {
            Assert.Equal(expected.HasGenericParameters, actual.HasGenericParameters);
            for (var i = 0; i < expected.GenericParameters.Count; i += 1)
            {
                var ap = expected.GenericParameters[i];
                var bp = actual.GenericParameters[i];
                Assert.Equal(ap.Name, bp.Name);
                Assert.Equal(ap.Position, bp.Position);
            }
        }

        private void AssertGenericArgumentsAreIdentical(IGenericInstance expected, IGenericInstance actual)
        {
            Assert.Equal(expected.HasGenericArguments, actual.HasGenericArguments);
            for (var i = 0; i < expected.GenericArguments.Count; i += 1)
            {
                var aa = expected.GenericArguments[i];
                var ba = actual.GenericArguments[i];
                Assert.Equal(aa.Name, ba.Name);
            }
        }

        private void AssertTypeReferencesAreIdentical(TypeReference expected, TypeReference actual)
        {
            Assert.Equal(expected.GetType(), actual.GetType());
            this.AssertGenericParametersAreIdentical(expected, actual);
            if (expected is GenericInstanceType)
            {
                this.AssertGenericArgumentsAreIdentical((GenericInstanceType)expected, (GenericInstanceType)actual);
            }
        }

        private void AssertMethodReferencesAreIdentical(MethodReference expected, MethodReference actual)
        {
            Assert.Equal(expected.GetType(), actual.GetType());
            this.AssertGenericParametersAreIdentical(expected, actual);
            this.AssertTypeReferencesAreIdentical(expected.DeclaringType, actual.DeclaringType);
            if (expected is GenericInstanceMethod)
            {
                this.AssertGenericArgumentsAreIdentical((GenericInstanceMethod)expected, (GenericInstanceMethod)actual);
            }
        }
    }
}
