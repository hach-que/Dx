// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelfClientHandler.cs" company="Redpoint Software">
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
//   The self client handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The client handler implementation that routes messages back to the local machine.
    /// </summary>
    public class SelfClientHandler : IClientHandler
    {
        #region Fields

        /// <summary>
        /// The handler mappings.
        /// </summary>
        private readonly Dictionary<int, IMessageHandler> m_HandlerMappings;

        /// <summary>
        /// The local node.
        /// </summary>
        private readonly ILocalNode m_LocalNode;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfClientHandler"/> class.
        /// </summary>
        /// <param name="localNode">
        /// The local node.
        /// </param>
        /// <param name="messageHandlers">
        /// The message handlers.
        /// </param>
        public SelfClientHandler(ILocalNode localNode, IEnumerable<IMessageHandler> messageHandlers)
        {
            this.m_LocalNode = localNode;
            this.m_HandlerMappings = messageHandlers.ToDictionary(k => k.GetMessageType(), v => v);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Handle the specified message using the appropriate message handler.  This
        /// is called when a message has been received over the network.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Receive(Message message)
        {
            if (!this.m_HandlerMappings.ContainsKey(message.Type))
            {
                throw new InvalidOperationException("No handler for message type " + message.Type);
            }

            this.m_HandlerMappings[message.Type].Handle(message);
        }

        /// <summary>
        /// Send a message to the remote host that this client handler is connected to.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Send(Message message)
        {
            message.Sender = this.m_LocalNode.Self;
            this.Receive(message);
        }

        /// <summary>
        /// Start the client handler, initiating connection to the remote host.
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Stop the client handler, disconnecting from the remote host and
        /// finalizing the receive thread cleanly.
        /// </summary>
        public void Stop()
        {
        }

        #endregion
    }
}