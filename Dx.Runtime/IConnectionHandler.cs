// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConnectionHandler.cs" company="Redpoint Software">
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
//   An interface that provides listening for client connections on a specified
//   address, and automatically instantiating the concrete <see cref="ListeningClientHandler"/>
//   class when clients connect.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System.Net;

    /// <summary>
    /// An interface that provides listening for client connections on a specified
    /// address, and automatically instantiating the concrete <see cref="ListeningClientHandler"/>
    /// class when clients connect.
    /// </summary>
    public interface IConnectionHandler
    {
        #region Public Methods and Operators

        /// <summary>
        /// Start listening for clients on the specified IP address and port.
        /// </summary>
        /// <param name="address">
        /// The IP address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        void Start(IPAddress address, int port);

        /// <summary>
        /// Stop listening for clients, and terminate all existing connections.
        /// </summary>
        void Stop();

        #endregion
    }
}