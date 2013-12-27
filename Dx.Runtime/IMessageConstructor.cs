// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageConstructor.cs" company="Redpoint Software">
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
//   The message constructor interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// The message constructor interface.  This provides methods for constructing
    /// various different types of messages.
    /// </summary>
    public interface IMessageConstructor
    {
        #region Public Methods and Operators

        /// <summary>
        /// Construct a "connection ping" message.
        /// </summary>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructConnectionPingMessage();

        /// <summary>
        /// Construct a "connection pong" message.
        /// </summary>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructConnectionPongMessage();

        /// <summary>
        /// Construct a "fetch confirmation" message.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructFetchResultMessage(ID key, SerializedEntry[] results);

        /// <summary>
        /// Construct a "fetch" message.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructFetchMessage(ID key);

        /// <summary>
        /// Construct a "get property" message.
        /// </summary>
        /// <param name="objectID">
        /// The object id.
        /// </param>
        /// <param name="property">
        /// The property.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructGetPropertyMessage(ID objectID, string property);

        /// <summary>
        /// Construct a "get property result" message.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructGetPropertyResultMessage(ID messageID, object result);

        /// <summary>
        /// Construct an "invoke" message.
        /// </summary>
        /// <param name="objectID">
        /// The object id.
        /// </param>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="typeArguments">
        /// The type arguments.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructInvokeMessage(ID objectID, string method, Type[] typeArguments, object[] arguments);

        /// <summary>
        /// Construct an "invoke result" message.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructInvokeResultMessage(ID messageID, object result);

        /// <summary>
        /// Construct a "set property" message.
        /// </summary>
        /// <param name="objectID">
        /// The object id.
        /// </param>
        /// <param name="property">
        /// The property.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructSetPropertyMessage(ID objectID, string property, object value);

        /// <summary>
        /// Construct a "set property confirmation" message.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message ConstructSetPropertyConfirmationMessage(ID messageID);

        #endregion
    }
}