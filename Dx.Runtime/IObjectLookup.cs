// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObjectLookup.cs" company="Redpoint Software">
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
//   The object lookup interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// The object lookup interface.  An implementation of this interface will search
    /// not only the local node, but also request remote nodes if the specified object
    /// can not be found locally.
    /// </summary>
    public interface IObjectLookup
    {
        #region Public Methods and Operators

        /// <summary>
        /// Get the first object that matches the specified key in the network.
        /// </summary>
        /// <param name="key">
        /// The key identifying the object.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds.
        /// </param>
        /// <returns>
        /// The <see cref="LiveEntry"/>, or null if no such object exists.
        /// </returns>
        LiveEntry GetFirst(ID key, int timeout);

        #endregion
    }
}