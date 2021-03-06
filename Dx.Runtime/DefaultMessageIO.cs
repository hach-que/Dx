﻿using System;
using System.IO;
using System.Linq;
using ProtoBuf;

namespace Dx.Runtime
{
    public class DefaultMessageIO : IMessageIO
    {
        public Message Receive(Stream stream)
        {
            if (!stream.CanRead)
            {
                return null;
            }

            var reader = new BinaryReader(stream);
            var length = reader.ReadInt32();
            var checksum = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);


            if (this.CalculateChecksum(length, bytes) != checksum)
            {
                throw new InvalidOperationException("Message was corrupt on arrival!");
            }

            using (var memory = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<Message>(memory);
            }
        }

        public void Send(Stream stream, Message message)
        {
            if (!stream.CanWrite)
            {
                return;
            }

            if (message.ID == null)
            {
                throw new InvalidOperationException("Can not serialize a message without an ID!");
            }

            message.SentFromReceivingThread = NetworkThreadContext.IsSentFromReceivingThread();

            var writer = new BinaryWriter(stream);
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, message);
                var length = (int)memory.Position;
                memory.Seek(0, SeekOrigin.Begin);
                var reader = new BinaryReader(memory);
                var bytes = reader.ReadBytes(length);
                var checksum = this.CalculateChecksum(length, bytes);


                writer.Write(length);
                writer.Write(checksum);
                writer.Write(bytes);
            }
        }

        private int CalculateChecksum(int length, byte[] bytes)
        {
            unchecked
            {
                return length + bytes.Select(x => (int)x).Aggregate((a, b) => a ^ b);
            }
        }
    }
}
