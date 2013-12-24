// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClientHandlerFactory.cs" company="Redpoint Software">
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
//   The client handler factory interface, which constructs concrete classes that
//   implement <see cref="IClientHandler"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// The client handler factory interface, which constructs concrete classes that
    /// implement <see cref="IClientHandler"/>.
    /// </summary>
    public interface IClientHandlerFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// Create an active client handler with the specified IP address and port number.
        /// </summary>
        /// <param name="address">
        /// The IP address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// The <see cref="ActiveClientHandler"/>.
        /// </returns>
        ActiveClientHandler CreateActiveClientHandler(IPAddress address, int port);

        /// <summary>
        /// Create a listening client handler that uses the specified TCP client.
        /// </summary>
        /// <param name="client">
        /// The TCP client.
        /// </param>
        /// <returns>
        /// The <see cref="ListeningClientHandler"/>.
        /// </returns>
        ListeningClientHandler CreateListeningClientHandler(TcpClient client);

        #endregion
    }
}