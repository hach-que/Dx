// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionPongMessageHandler.cs" company="Redpoint Software">
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
//   The connection pong message handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// The connection pong message handler.
    /// </summary>
    internal class ConnectionPongMessageHandler : IMessageHandler
    {
        #region Fields

        /// <summary>
        /// The <see cref="IMessageSideChannel"/>.
        /// </summary>
        private readonly IMessageSideChannel m_MessageSideChannel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPongMessageHandler"/> class.
        /// </summary>
        /// <param name="messageSideChannel">
        /// The message side channel.
        /// </param>
        public ConnectionPongMessageHandler(IMessageSideChannel messageSideChannel)
        {
            this.m_MessageSideChannel = messageSideChannel;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns the type of message that this handler will handle.
        /// </summary>
        /// <returns>
        /// The message type (specified as an <see cref="int"/>).
        /// </returns>
        public int GetMessageType()
        {
            return MessageType.ConnectionPong;
        }

        /// <summary>
        /// Handle the specified message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Handle(Message message)
        {
            this.m_MessageSideChannel.Put(message);
        }

        #endregion
    }
}