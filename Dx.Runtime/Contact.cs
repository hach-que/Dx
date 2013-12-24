// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Contact.cs" company="Redpoint Software">
//   The MIT License (MIT)
//   
//   Copyright (c) 2013 James Rhodes
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// <summary>
//   The contact structure, which represents a remote contact / node in the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System.Net;

    using ProtoBuf;

    /// <summary>
    /// A contact is a reference to a remote node in the network, identified by
    /// it's IP address and port.
    /// </summary>
    [ProtoContract]
    public class Contact
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return new IPAddress(this.IPAddressBytes);
            }

            set
            {
                this.IPAddressBytes = value.GetAddressBytes();
            }
        }

        /// <summary>
        /// Gets or sets the IP address bytes.
        /// </summary>
        [ProtoMember(1)]
        public byte[] IPAddressBytes { get; set; }

        /// <summary>
        /// Gets the IP end point.
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get
            {
                return new IPEndPoint(this.IPAddress, this.Port);
            }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        [ProtoMember(2)]
        public int Port { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The == operator.  This returns whether or not the two contacts identify the same remote host.
        /// </summary>
        /// <param name="a">
        /// The first contact.
        /// </param>
        /// <param name="b">
        /// The second contact.
        /// </param>
        /// <returns>
        /// Whether the two contacts do identify the same remote host.
        /// </returns>
        public static bool operator ==(Contact a, Contact b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null))
            {
                return false;
            }

            if (ReferenceEquals(b, null))
            {
                return false;
            }

            return object.Equals(a.IPAddress, b.IPAddress) && a.Port == b.Port;
        }

        /// <summary>
        /// The != operator.  This returns whether or not the two contacts do not identify the same remote host.
        /// </summary>
        /// <param name="a">
        /// The first contact.
        /// </param>
        /// <param name="b">
        /// The second contact.
        /// </param>
        /// <returns>
        /// Whether the two contacts do not identify the same remote host.
        /// </returns>
        public static bool operator !=(Contact a, Contact b)
        {
            return !(a == b);
        }

        /// <summary>
        /// The Equals implementation, which value compares two contact objects.
        /// </summary>
        /// <param name="obj">
        /// The second contact object to compare against.
        /// </param>
        /// <returns>
        /// Whether the two contact objects are considered to identify the same remote host.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() && this.Equals((Contact)obj);
        }

        /// <summary>
        /// Returns the hash code of the current contact.
        /// </summary>
        /// <returns>
        /// The hash code of the current contact.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.IPAddressBytes != null ? this.IPAddressBytes.GetHashCode() : 0) * 397) ^ this.Port;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The Equals implementation, which value compares two contact objects.
        /// </summary>
        /// <param name="other">
        /// The second contact object to compare against.
        /// </param>
        /// <returns>
        /// Whether the two contact objects are considered to identify the same remote host.
        /// </returns>
        protected bool Equals(Contact other)
        {
            return object.Equals(this.IPAddress, other.IPAddress) && this.Port == other.Port;
        }

        #endregion
    }
}