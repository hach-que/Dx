using System;
using System.IO;
using System.Runtime.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Dx.Runtime
{
    public class DefaultObjectWithTypeSerializer : IObjectWithTypeSerializer
    {
        private readonly ILocalNode m_LocalNode;

        public DefaultObjectWithTypeSerializer(ILocalNode localNode)
        {
            this.m_LocalNode = localNode;
        }

        public ObjectWithType Serialize(object obj)
        {
            if (obj == null)
            {
                return new ObjectWithType { AssemblyQualifiedTypeName = null, SerializedObject = null };
            }

            byte[] serializedObject;
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, obj);
                var length = (int)memory.Position;
                memory.Seek(0, SeekOrigin.Begin);
                serializedObject = new byte[length];
                memory.Read(serializedObject, 0, length);
            }

            return new ObjectWithType
            {
                AssemblyQualifiedTypeName = obj.GetType().AssemblyQualifiedName,
                SerializedObject = serializedObject
            };
        }

        public object Deserialize(ObjectWithType owt)
        {
            if (owt.AssemblyQualifiedTypeName == null)
            {
                return null;
            }

            var type = Type.GetType(owt.AssemblyQualifiedTypeName);
            if (type == null)
            {
                throw new TypeLoadException();
            }

            using (var memory = new MemoryStream(owt.SerializedObject))
            {
                object instance;
                if (type == typeof(string))
                {
                    instance = string.Empty;
                }
                else
                {
                    instance = FormatterServices.GetUninitializedObject(type);
                }
                
                var value = RuntimeTypeModel.Default.Deserialize(memory, instance, type);
                GraphWalker.Apply(value, this.m_LocalNode);
                return value;
            }
        }
    }
}
