using System;
using System.Net;

namespace Dx.Runtime.Tests
{
    public class InternalTestNetwork : INetworkProvider
    {
        private static readonly ID NodeAID =
            new ID(
                Guid.Parse("4549ab05-2032-4eb2-bc53-7881bc6e3795"),
                Guid.Parse("e3c3eb59-1ec8-4779-a623-980912afe007"),
                Guid.Parse("b896ec52-8903-4b84-ad48-ed5c444ba2c7"),
                Guid.Parse("e5213727-1781-4b46-beb7-9bc552e31481"));
        private static readonly ID NodeBID =
            new ID(
                Guid.Parse("6872125f-9ebe-4e8a-ad84-632c26a7058f"),
                Guid.Parse("2770f4d6-e96d-469d-b809-7fb5603c2e33"),
                Guid.Parse("4def4118-ffc6-4077-9900-303f4780982b"),
                Guid.Parse("57d5f986-ddce-4c59-b822-185652be624c"));

        public InternalTestNetwork(ILocalNode node, bool isNodeA)
        {
            this.Node = node;
            this.IsNodeA = isNodeA;
        }

        public bool IsNodeA
        {
            get;
            private set;
        }

        #region INetworkProvider Members

        public ILocalNode Node { get; private set; }

        public ID ID
        {
            get
            {
                if (this.IsNodeA)
                    return NodeAID;
                return NodeBID;
            }
        }

        public int DiscoveryPort { get { return 9736; } }

        public int MessagingPort
        {
            get
            {
                if (this.IsNodeA)
                    return 9737;
                return 9738;
            }
        }

        public IPAddress IPAddress
        {
            get
            {
                return IPAddress.Loopback;
            }
        }

        public bool IsFirst { get { return this.IsNodeA; } }

        #endregion

        public void Join(ID id)
        {
            // We don't care about the ID.
            Contact contact;
            if (this.IsNodeA)
                contact = new Contact(NodeBID, new IPEndPoint(IPAddress.Loopback, 9738));
            else
                contact = new Contact(NodeAID, new IPEndPoint(IPAddress.Loopback, 9737));
            this.Node.Contacts.Add(contact);
        }

        public void Leave()
        {
            // Don't care about leaving either.
        }
    }
}
