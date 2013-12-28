// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="Redpoint Software">
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
//   The message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using ProtoBuf;

    /// <summary>
    /// The message.
    /// </summary>
    [ProtoContract]
    public class Message
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the fetch key.
        /// </summary>
        [ProtoMember(3)]
        public ID FetchKey { get; set; }

        /// <summary>
        /// Gets or sets the fetch result.
        /// </summary>
        [ProtoMember(4)]
        public SerializedEntry[] FetchResult { get; set; }

        /// <summary>
        /// Gets or sets the get property message id.
        /// </summary>
        [ProtoMember(19)]
        public ID GetPropertyMessageID { get; set; }

        /// <summary>
        /// Gets or sets the get property object id.
        /// </summary>
        [ProtoMember(5)]
        public ID GetPropertyObjectID { get; set; }

        /// <summary>
        /// Gets or sets the get property property name.
        /// </summary>
        [ProtoMember(6)]
        public string GetPropertyPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the get property result.
        /// </summary>
        [ProtoMember(7)]
        public ObjectWithType GetPropertyResult { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [ProtoMember(1)]
        public ID ID { get; set; }

        /// <summary>
        /// Gets or sets the invoke arguments.
        /// </summary>
        [ProtoMember(13)]
        public ObjectWithType[] InvokeArguments { get; set; }

        /// <summary>
        /// Gets or sets the invoke event sender.
        /// </summary>
        [ProtoMember(9)]
        public ObjectWithType InvokeEventSender { get; set; }

        /// <summary>
        /// Gets or sets the invoke message id.
        /// </summary>
        [ProtoMember(20)]
        public ID InvokeMessageID { get; set; }

        /// <summary>
        /// Gets or sets the invoke method.
        /// </summary>
        [ProtoMember(12)]
        public string InvokeMethod { get; set; }

        /// <summary>
        /// Gets or sets the invoke object id.
        /// </summary>
        [ProtoMember(11)]
        public ID InvokeObjectID { get; set; }

        /// <summary>
        /// Gets or sets the invoke result.
        /// </summary>
        [ProtoMember(15)]
        public ObjectWithType InvokeResult { get; set; }

        /// <summary>
        /// Gets or sets the invoke type arguments.
        /// </summary>
        [ProtoMember(14)]
        public string[] InvokeTypeArguments { get; set; }

        /// <summary>
        /// Gets or sets the net serialized event arguments.
        /// </summary>
        [ProtoMember(10)]
        public byte[] NETSerializedEventArguments { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        public Contact Sender { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it was sent from a receiving thread.
        /// </summary>
        [ProtoMember(22)]
        public bool SentFromReceivingThread { get; set; }

        /// <summary>
        /// Gets or sets the set property object id.
        /// </summary>
        [ProtoMember(16)]
        public ID SetPropertyObjectID { get; set; }

        /// <summary>
        /// Gets or sets the set property message id.
        /// </summary>
        [ProtoMember(21)]
        public ID SetPropertyMessageID { get; set; }

        /// <summary>
        /// Gets or sets the set property property name.
        /// </summary>
        [ProtoMember(17)]
        public string SetPropertyPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the set property property value.
        /// </summary>
        [ProtoMember(18)]
        public ObjectWithType SetPropertyPropertyValue { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [ProtoMember(2)]
        public int Type { get; set; }

        #endregion
    }
}