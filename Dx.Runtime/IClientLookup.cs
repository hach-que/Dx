// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClientLookup.cs" company="Redpoint Software">
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
//   The ClientLookup interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System.Collections.Generic;
    using System.Net;

    /// <summary>
    /// An interface that provides a mechanism to lookup known clients and retrieve them.
    /// </summary>
    public interface IClientLookup
    {
        #region Public Methods and Operators

        /// <summary>
        /// Add a client handler, mapped to the specified IP endpoint.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="clientHandler">
        /// The client handler.
        /// </param>
        void Add(IPEndPoint endpoint, IClientHandler clientHandler);

        /// <summary>
        /// Retrieve all of the client handler mappings as an enumeration of key-value pairs.
        /// </summary>
        /// <returns>
        /// The enumerable of key-value pairs, with IP endpoints being the keys and client handlers being the values.
        /// </returns>
        IEnumerable<KeyValuePair<IPEndPoint, IClientHandler>> GetAll();

        /// <summary>
        /// Lookup a client handler by the IP endpoint.
        /// </summary>
        /// <param name="endpoint">
        /// The IP endpoint.
        /// </param>
        /// <returns>
        /// The <see cref="IClientHandler"/> associated with this endpoint.
        /// </returns>
        IClientHandler Lookup(IPEndPoint endpoint);

        /// <summary>
        /// Remove a client handler based on the IP endpoint.
        /// </summary>
        /// <param name="endpoint">
        /// The IP endpoint.
        /// </param>
        void Remove(IPEndPoint endpoint);

        #endregion
    }
}