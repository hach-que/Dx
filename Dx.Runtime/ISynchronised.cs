// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISynchronised.cs" company="Redpoint Software">
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
//   An interface that is implemented by the Dx post-processor to provide
//   synchronisation storage against a synchronised object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    /// <summary>
    /// An interface that is implemented by the Dx post-processor to provide
    /// synchronisation storage against a synchronised object.
    /// </summary>
    public interface ISynchronised
    {
        #region Public Methods and Operators

        /// <summary>
        /// Retrieve the synchronisation store against the specified object.
        /// </summary>
        /// <param name="node">
        /// The local node that this object is to be associated with.
        /// </param>
        /// <param name="name">
        /// The network name of the object.
        /// </param>
        /// <returns>
        /// The <see cref="SynchronisationStore"/>.
        /// </returns>
        SynchronisationStore GetSynchronisationStore(ILocalNode node, string name);

        #endregion
    }
}