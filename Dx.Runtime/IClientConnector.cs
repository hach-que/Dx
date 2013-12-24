// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClientConnector.cs" company="Redpoint Software">
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
//   The client connector interface, which provides provides a simple
//   mechanism for establishing a connection to a remote node.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Net;

    /// <summary>
    /// The client connector interface, which provides provides a simple
    /// mechanism for establishing a connection to a remote node.
    /// </summary>
    public interface IClientConnector
    {
        #region Public Methods and Operators

        /// <summary>
        /// Establish a connection to the specified remote host.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the remote connection was established, but messages could not successfully be sent.
        /// </exception>
        void Connect(IPAddress address, int port);

        #endregion
    }
}