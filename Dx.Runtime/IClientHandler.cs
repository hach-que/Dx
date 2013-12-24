// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClientHandler.cs" company="Redpoint Software">
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
//   The client handler interface, which defines an interface to a remote host.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// The client handler interface, which defines an interface to a remote host.
    /// </summary>
    public interface IClientHandler
    {
        #region Public Methods and Operators

        /// <summary>
        /// Handle the specified message using the appropriate message handler.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Receive(Message message);

        /// <summary>
        /// Send a message to the remote host that this client handler is connected to.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Send(Message message);

        /// <summary>
        /// Start the client handler, initiating connection to the remote host.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the client handler, disconnecting from the remote host and
        /// finalizing the receive thread cleanly.
        /// </summary>
        void Stop();

        #endregion
    }
}