using System;
using System.Net;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class Contact : ISerializable, IEquatable<Contact>
    {
        private IPEndPoint p_EndPoint;
        private ID p_Identifier;

        public Contact(ID identifier, IPEndPoint endpoint)
        {
            this.p_Identifier = identifier;
            this.p_EndPoint = endpoint;
        }

        public Contact(SerializationInfo info, StreamingContext context)
        {
            this.p_Identifier = info.GetValue("identifier", typeof(ID)) as ID;
            this.p_EndPoint = info.GetValue("endpoint", typeof(IPEndPoint)) as IPEndPoint;
        }

        public IPEndPoint EndPoint
        {
            get { return this.p_EndPoint; }
        }

        public ID Identifier
        {
            get { return this.p_Identifier; }
        }

        public static bool operator ==(Contact a, Contact b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                return true;
            if (object.ReferenceEquals(a, null))
                return false;
            if (object.ReferenceEquals(b, null))
                return false;

            return ( a.EndPoint == b.EndPoint && a.Identifier == b.Identifier );
        }

        public static bool operator !=(Contact a, Contact b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                return false;
            if (object.ReferenceEquals(a, null))
                return true;
            if (object.ReferenceEquals(b, null))
                return true;

            return ( a.EndPoint != b.EndPoint || a.Identifier != b.Identifier );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Contact)
                return ( this.EndPoint == ( obj as Contact ).EndPoint && this.Identifier == ( obj as Contact ).Identifier );
            else
                return false;
        }

        public override string ToString()
        {
            return string.Format("[Contact: EndPoint={0}, Identifier={1}]", this.EndPoint, this.Identifier);
        }

        #region ISerializable implementation
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.p_Identifier == null || this.p_EndPoint == null)
                throw new SerializationException("The contact's unique identifier or endpoint is invalid.");

            info.AddValue("identifier", this.p_Identifier, typeof(ID));
            info.AddValue("endpoint", this.p_EndPoint, typeof(IPEndPoint));
        }
        #endregion

        #region IEquatable[Contact] implementation
        bool IEquatable<Contact>.Equals(Contact other)
        {
            return ( other.EndPoint == this.p_EndPoint && other.Identifier == this.p_Identifier );
        }
        #endregion
    }
}

