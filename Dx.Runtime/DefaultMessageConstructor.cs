// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMessageConstructor.cs" company="Redpoint Software">
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
//   An implementation of the <see cref="IMessageConstructor" /> interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Linq;

    /// <summary>
    /// An implementation of the <see cref="IMessageConstructor" /> interface.
    /// </summary>
    public class DefaultMessageConstructor : IMessageConstructor
    {
        #region Fields

        /// <summary>
        /// The object with type serializer.
        /// </summary>
        private readonly IObjectWithTypeSerializer m_ObjectWithTypeSerializer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMessageConstructor"/> class.
        /// </summary>
        /// <param name="objectWithTypeSerializer">
        /// The object with type serializer.
        /// </param>
        public DefaultMessageConstructor(IObjectWithTypeSerializer objectWithTypeSerializer)
        {
            this.m_ObjectWithTypeSerializer = objectWithTypeSerializer;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Construct a "connection ping" message.
        /// </summary>
        /// <returns>
        /// The <see cref="Message" />.
        /// </returns>
        public Message ConstructConnectionPingMessage()
        {
            return new Message { ID = ID.NewRandom(), Type = MessageType.ConnectionPing };
        }

        /// <summary>
        /// Construct a "connection pong" message.
        /// </summary>
        /// <returns>
        /// The <see cref="Message" />.
        /// </returns>
        public Message ConstructConnectionPongMessage()
        {
            return new Message { ID = ID.NewRandom(), Type = MessageType.ConnectionPong };
        }

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
        public Message ConstructFetchResultMessage(ID key, SerializedEntry[] results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.FetchResult, 
                FetchKey = key, 
                FetchResult = results
            };
        }

        /// <summary>
        /// Construct a "fetch" message.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        public Message ConstructFetchMessage(ID key)
        {
            return new Message { ID = ID.NewRandom(), Type = MessageType.Fetch, FetchKey = key };
        }

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
        public Message ConstructGetPropertyMessage(ID objectID, string property)
        {
            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.GetProperty, 
                GetPropertyObjectID = objectID, 
                GetPropertyPropertyName = property
            };
        }

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
        public Message ConstructGetPropertyResultMessage(ID messageID, object result)
        {
            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.GetPropertyResult, 
                GetPropertyMessageID = messageID, 
                GetPropertyResult = this.m_ObjectWithTypeSerializer.Serialize(result)
            };
        }

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
        public Message ConstructInvokeMessage(ID objectID, string method, Type[] typeArguments, object[] arguments)
        {
            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.Invoke, 
                InvokeObjectID = objectID, 
                InvokeMethod = method, 
                InvokeTypeArguments = typeArguments.Select(x => x.AssemblyQualifiedName).ToArray(), 
                InvokeArguments = arguments.Select(x => this.m_ObjectWithTypeSerializer.Serialize(x)).ToArray()
            };
        }

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
        public Message ConstructInvokeResultMessage(ID messageID, object result)
        {
            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.InvokeResult, 
                InvokeMessageID = messageID, 
                InvokeResult = this.m_ObjectWithTypeSerializer.Serialize(result)
            };
        }

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
        public Message ConstructSetPropertyMessage(ID objectID, string property, object value)
        {
            return new Message
            {
                ID = ID.NewRandom(), 
                Type = MessageType.SetProperty, 
                SetPropertyObjectID = objectID, 
                SetPropertyPropertyName = property, 
                SetPropertyPropertyValue = this.m_ObjectWithTypeSerializer.Serialize(value)
            };
        }

        /// <summary>
        /// Construct a "set property confirmation" message.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        public Message ConstructSetPropertyConfirmationMessage(ID messageID)
        {
            return new Message
            {
                ID = ID.NewRandom(),
                Type = MessageType.SetPropertyConfirmation,
                SetPropertyMessageID = messageID
            };
        }

        #endregion
    }
}