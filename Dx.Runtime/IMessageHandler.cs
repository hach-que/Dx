// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageHandler.cs" company="Redpoint Software">
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
//   An interface for implementations that can handle messages as they
//   are received.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// An interface for implementations that can handle messages as they are received.
    /// </summary>
    public interface IMessageHandler
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns the type of message that this handler will handle.
        /// </summary>
        /// <returns>
        /// The message type (specified as an <see cref="int" />).
        /// </returns>
        int GetMessageType();

        /// <summary>
        /// Handle the specified message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Handle(Message message);

        #endregion
    }
}