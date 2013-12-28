// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Redpoint Software" file="DefaultAutomaticRetry.cs">
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
//   The default implementation of <see cref="IAutomaticRetry" />.
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// The default implementation of <see cref="IAutomaticRetry" />.
    /// </summary>
    public class DefaultAutomaticRetry : IAutomaticRetry
    {
        /// <summary>
        /// The message side channel.
        /// </summary>
        private readonly IMessageSideChannel m_MessageSideChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAutomaticRetry"/> class.
        /// </summary>
        /// <param name="messageSideChannel">
        /// The message side channel.
        /// </param>
        public DefaultAutomaticRetry(IMessageSideChannel messageSideChannel)
        {
            this.m_MessageSideChannel = messageSideChannel;
        }

        /// <summary>
        /// The send with retry.
        /// </summary>
        /// <param name="clientHandler">
        /// The client handler.
        /// </param>
        /// <param name="send">
        /// The send.
        /// </param>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <param name="retries">
        /// The retries.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no response.
        /// </exception>
        public Message SendWithRetry(
            IClientHandler clientHandler, 
            Message send, 
            Func<Message, bool> predicate, 
            int timeout, 
            int retries)
        {
            for (var i = 0; i < Math.Max(retries, 1); i++)
            {
                clientHandler.Send(send);

                var message = this.m_MessageSideChannel.WaitUntil(predicate, timeout);

                if (message == null)
                {
                    continue;
                }

                return message;
            }

            throw new InvalidOperationException("No response after " + retries);
        }
    }
}