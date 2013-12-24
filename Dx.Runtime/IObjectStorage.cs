// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObjectStorage.cs" company="Redpoint Software">
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
//   The object storage interface.  An implementation of this interface will store
//   references to distributed objects so that they can be looked up by their
//   identifier in future.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System.Collections.Generic;

    /// <summary>
    /// The object storage interface.  An implementation of this interface will store
    /// references to distributed objects so that they can be looked up by their
    /// identifier in future.
    /// </summary>
    public interface IObjectStorage
    {
        #region Public Methods and Operators

        /// <summary>
        /// Find entries whose key matches the specified key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The enumerable of matching entries.
        /// </returns>
        IEnumerable<LiveEntry> Find(ID key);

        /// <summary>
        /// Put an entry into object storage.
        /// </summary>
        /// <param name="liveEntry">
        /// The live entry.
        /// </param>
        void Put(LiveEntry liveEntry);

        /// <summary>
        /// Remove entries from object storage based on their key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        void Remove(ID key);

        /// <summary>
        /// Update or put an entry into object storage.
        /// </summary>
        /// <param name="liveEntry">
        /// The live entry.
        /// </param>
        void UpdateOrPut(LiveEntry liveEntry);

        #endregion
    }
}