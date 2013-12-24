﻿using System;
using System.Linq;

namespace Dx.Runtime
{
    public class SetPropertyMessageHandler : IMessageHandler
    {
        private readonly ILocalNode m_LocalNode;

        private readonly IObjectStorage m_Lookup;

        private readonly IMessageConstructor m_MessageConstructor;

        private readonly IClientLookup m_ClientLookup;

        private readonly IObjectStorage m_ObjectStorage;

        private readonly IObjectWithTypeSerializer m_ObjectWithTypeSerializer;

        public SetPropertyMessageHandler(
            ILocalNode localNode,
            IObjectStorage lookup,
            IMessageConstructor messageConstructor,
            IClientLookup clientLookup,
            IObjectStorage objectStorage,
            IObjectWithTypeSerializer objectWithTypeSerializer)
        {
            this.m_LocalNode = localNode;
            this.m_Lookup = lookup;
            this.m_MessageConstructor = messageConstructor;
            this.m_ClientLookup = clientLookup;
            this.m_ObjectStorage = objectStorage;
            this.m_ObjectWithTypeSerializer = objectWithTypeSerializer;
        }

        public int GetMessageType()
        {
            return MessageType.SetProperty;
        }

        public void Handle(Message message)
        {
            var entry = this.m_ObjectStorage.Find(message.SetPropertyObjectID).FirstOrDefault();

            if (entry == null)
            {
                throw new InvalidOperationException("Set property message received but we don't own the object!");
            }

            var obj = entry.Value;

            var mi = obj.GetType().GetMethod("set_" + message.SetPropertyPropertyName + "__Distributed0", BindingFlagsCombined.All);
            if (mi == null)
            {
                throw new MissingMethodException(obj.GetType().FullName, "set_" + message.SetPropertyPropertyName + "__Distributed0");
            }

            var value = this.m_ObjectWithTypeSerializer.Deserialize(message.SetPropertyPropertyValue);

            DpmEntrypoint.InvokeDynamic(obj.GetType(), mi, obj, new Type[0], new[] { value });
            this.m_ObjectStorage.UpdateOrPut(new LiveEntry 
                { 
                    Key = message.SetPropertyObjectID, 
                    Value = obj, 
                    Owner = this.m_LocalNode.Self
                });
        }
    }
}
