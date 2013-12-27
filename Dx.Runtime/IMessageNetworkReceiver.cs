// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageNetworkReceiver.cs" company="Redpoint Software">
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
//   An interface that provides safe, connection-aware reading of messages
//   from a network socket.  <see cref="ActiveClientHandler" /> and
//   <see cref="ListeningClientHandler" /> both use this interface to
//   perform receiving in their threads.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// An interface that provides safe, connection-aware reading of messages
    /// from a network socket.  <see cref="ActiveClientHandler" /> and
    /// <see cref="ListeningClientHandler" /> both use this interface to
    /// perform receiving in their threads.
    /// </summary>
    public interface IMessageNetworkReceiver
    {
        #region Public Methods and Operators

        /// <summary>
        /// Continuously receive messages from a TCP client, calling "callback" until
        /// the client is no longer connected. 
        /// </summary>
        /// <param name="client">
        /// The TCP client.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        void Run(TcpClient client, Action<Message> callback);

        #endregion
    }
}