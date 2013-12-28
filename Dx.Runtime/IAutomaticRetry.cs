// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Redpoint Software" file="IAutomaticRetry.cs">
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
//   An interface that provides the ability to automatically retry sending messages if the timeout expires.
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// An interface that provides the ability to automatically retry sending messages if the timeout expires.
    /// </summary>
    public interface IAutomaticRetry
    {
        /// <summary>
        /// Send a message, retrying on timeout.
        /// </summary>
        /// <param name="clientHandler">
        /// The client handler to send the message through.
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
        Message SendWithRetry(IClientHandler clientHandler, Message send, Func<Message, bool> predicate, int timeout, int retries);
    }
}