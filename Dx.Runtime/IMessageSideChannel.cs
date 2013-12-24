// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageSideChannel.cs" company="Redpoint Software">
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
//   Provides a "side channel" for message communication, allowing a message handler
//   to place a message in the side channel and code elsewhere to pick it up.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// Provides a "side channel" for message communication, allowing a message handler
    /// to place a message in the side channel and code elsewhere to pick it up.
    /// </summary>
    public interface IMessageSideChannel
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns whether the message side channel has a message matching the predicate.
        /// </summary>
        /// <param name="predicate">
        /// The predicate to match a message against.
        /// </param>
        /// <returns>
        /// Whether the message side channel contains a message matching the predicate.
        /// </returns>
        bool Has(Func<Message, bool> predicate);

        /// <summary>
        /// Place a message in the message side channel.
        /// </summary>
        /// <param name="message">
        /// The message to place in the side channel.
        /// </param>
        void Put(Message message);

        /// <summary>
        /// Wait until a message matching the specified predicate arrives in the message
        /// side channel.  If there is already a matching message, this method will return
        /// it immediately.  Once the message has been returned from this method, it is
        /// removed from the message side channel.
        /// </summary>
        /// <param name="predicate">
        /// Predicate for matching a message.
        /// </param>
        /// <param name="timeout">
        /// Timeout in milliseconds.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/> from the side channel.
        /// </returns>
        Message WaitUntil(Func<Message, bool> predicate, int timeout);

        #endregion
    }
}