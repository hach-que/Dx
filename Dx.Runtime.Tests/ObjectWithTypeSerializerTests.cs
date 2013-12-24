using ProtoBuf;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class ObjectWithTypeSerializerTests
    {
        private IObjectWithTypeSerializer GetSerializer()
        {
            return new DefaultObjectWithTypeSerializer(null);
        }

        [Fact]
        public void NullIsSerializedCorrectly()
        {
            var serializer = this.GetSerializer();

            Assert.DoesNotThrow(() => serializer.Serialize(null));
            Assert.NotEqual(null, serializer.Serialize(null));
            Assert.Equal(null, serializer.Serialize(null).AssemblyQualifiedTypeName);
            Assert.Equal(null, serializer.Serialize(null).SerializedObject);
        }

        [Fact]
        public void NullIsDeserializedCorrectly()
        {
            var serializer = this.GetSerializer();

            var owt = new ObjectWithType { AssemblyQualifiedTypeName = null, SerializedObject = null };

            Assert.DoesNotThrow(() => serializer.Deserialize(owt));
            Assert.Equal(null, serializer.Deserialize(owt));
        }

        private void ValueIsSerializedAndDeserializedCorrectly(object value)
        {
            var serializer = this.GetSerializer();

            Assert.DoesNotThrow(() => serializer.Serialize(value));

            var serializedValue = serializer.Serialize(value);
            var deserializedValue = serializer.Deserialize(serializedValue);

            Assert.Equal(value, deserializedValue);
        }

        [Fact]
        public void StringIsSerializedAndDeserializedCorrectly()
        {
            this.ValueIsSerializedAndDeserializedCorrectly("string");
        }

        [Fact]
        public void IntegerIsSerializedAndDeserializedCorrectly()
        {
            this.ValueIsSerializedAndDeserializedCorrectly(5);
        }

        [ProtoContract]
        private class ComplexType : ITransparent
        {
            [ProtoMember(1)]
            public int A { get; set; }

            [ProtoMember(2)]
            public int B { get; set; }

            [ProtoMember(3)]
            public string C { get; set; }

            [ProtoMember(4)]
            public string NetworkName { get; set; }

            public ILocalNode Node { get; set; }
        }

        [Fact]
        public void ComplexTypeIsSerializedAndDeserializedCorrectly()
        {
            var serializer = this.GetSerializer();

            var value = new ComplexType { A = 5, B = 10, C = "hello" };

            Assert.DoesNotThrow(() => serializer.Serialize(value));

            var serializedValue = serializer.Serialize(value);
            var deserializedValue = serializer.Deserialize(serializedValue);

            Assert.IsType<ComplexType>(deserializedValue);

            var typeCastedValue = (ComplexType)deserializedValue;

            Assert.Equal(value.A, typeCastedValue.A);
            Assert.Equal(value.B, typeCastedValue.B);
            Assert.Equal(value.C, typeCastedValue.C);
        }
    }
}
