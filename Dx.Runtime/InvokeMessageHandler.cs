using System;
using System.Linq;
using Ninject.Planning.Targets;

namespace Dx.Runtime
{
    public class InvokeMessageHandler : IMessageHandler
    {
        private readonly ILocalNode m_LocalNode;

        private readonly IObjectStorage m_ObjectStorage;

        private readonly IObjectWithTypeSerializer m_ObjectWithTypeSerializer;

        private readonly IMessageConstructor m_MessageConstructor;

        private readonly IClientLookup m_ClientLookup;

        public InvokeMessageHandler(
            ILocalNode localNode,
            IObjectStorage objectStorage,
            IObjectWithTypeSerializer objectWithTypeSerializer,
            IMessageConstructor messageConstructor,
            IClientLookup clientLookup)
        {
            this.m_LocalNode = localNode;
            this.m_ObjectStorage = objectStorage;
            this.m_ObjectWithTypeSerializer = objectWithTypeSerializer;
            this.m_MessageConstructor = messageConstructor;
            this.m_ClientLookup = clientLookup;
        }

        public int GetMessageType()
        {
            return MessageType.Invoke;
        }

        public void Handle(Message message)
        {
            if (message.InvokeTypeArguments == null)
            {
                message.InvokeTypeArguments = new string[0];
            }

            if (message.InvokeArguments == null)
            {
                message.InvokeArguments = new ObjectWithType[0];
            }

            var entry = this.m_ObjectStorage.Find(message.InvokeObjectID).FirstOrDefault();

            if (entry == null)
            {
                throw new InvalidOperationException("Invoke message received but we don't own the object!");
            }

            var obj = entry.Value;

            var mi = obj.GetType().GetMethod(message.InvokeMethod, BindingFlagsCombined.All);
            if (mi == null)
            {
                throw new MissingMethodException(obj.GetType().FullName, message.InvokeMethod);
            }

            var targs = message.InvokeTypeArguments.Select(Type.GetType).ToArray();
            var args = message.InvokeArguments.Select(x => this.m_ObjectWithTypeSerializer.Deserialize(x)).ToArray();

            var allowed = false;

            if (this.m_LocalNode.Architecture == Architecture.PeerToPeer)
            {
                // In peer-to-peer, all methods are callable.
                allowed = true;
            }
            else if (this.m_LocalNode.Architecture == Architecture.ServerClient)
            {
                if (message.Sender == this.m_LocalNode.Self)
                {
                    // This message is coming from the server.
                    allowed = true;
                }
                else
                {
                    var originalName = mi.Name.Substring(0, mi.Name.Length - "__Distributed0".Length);
                    var originalMethod = obj.GetType().GetMethod(originalName, BindingFlagsCombined.All);

                    // The client is calling this method, ensure they are allowed to call it.
                    allowed = originalMethod.GetCustomAttributes(typeof(ClientCallableAttribute), false).Any();
                }
            }

            if (!allowed)
            {
                // If the sender is not permitted to call this method, we just return (we
                // don't even bother giving them a response since they're either using an
                // out-of-date client or attempting to bypass security).
                throw new InvalidOperationException("Received a call to invoke " + message.InvokeMethod + " on " + obj.GetType());
            }

            var result = DpmEntrypoint.InvokeDynamic(
                obj.GetType(),
                mi,
                obj,
                targs,
                args);

            var client = this.m_ClientLookup.Lookup(message.Sender.IPEndPoint);
            client.Send(this.m_MessageConstructor.ConstructInvokeResultMessage(message.ID, result));
        }
    }
}
