// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageType.cs" company="Redpoint Software">
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
//   The message type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// The message type.
    /// </summary>
    public static class MessageType
    {
        #region Constants

        /// <summary>
        /// The "connection ping" message type.
        /// </summary>
        public const int ConnectionPing = 11;

        /// <summary>
        /// The "connection pong" message type.
        /// </summary>
        public const int ConnectionPong = 12;

        /// <summary>
        /// The "fetch" message type.
        /// </summary>
        public const int Fetch = 1;

        /// <summary>
        /// The "fetch result" message type.
        /// </summary>
        public const int FetchResult = 2;

        /// <summary>
        /// The "get property" message type.
        /// </summary>
        public const int GetProperty = 6;

        /// <summary>
        /// The "get property result" message type.
        /// </summary>
        public const int GetPropertyResult = 7;

        /// <summary>
        /// The "invoke" message type.
        /// </summary>
        public const int Invoke = 9;

        /// <summary>
        /// The "invoke result" message type.
        /// </summary>
        public const int InvokeResult = 10;

        /// <summary>
        /// The "set property" message type.
        /// </summary>
        public const int SetProperty = 8;

        /// <summary>
        /// The "set property confirmation" message type.
        /// </summary>
        public const int SetPropertyConfirmation = 13;

        #endregion
    }
}