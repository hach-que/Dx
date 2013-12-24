// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObjectWithTypeSerializer.cs" company="Redpoint Software">
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
//   An interface to serializing any type that has a <see cref="ProtoContractAttribute" />
//   and creating an <see cref="ObjectWithType" /> instance to represent it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using ProtoBuf;

    /// <summary>
    /// An interface to serializing any type that has a <see cref="ProtoContractAttribute" />
    /// and creating an <see cref="ObjectWithType" /> instance to represent it.
    /// </summary>
    public interface IObjectWithTypeSerializer
    {
        #region Public Methods and Operators

        /// <summary>
        /// Deserialize an <see cref="ObjectWithType"/> instance back into the original object.
        /// </summary>
        /// <param name="owt">
        /// The <see cref="ObjectWithType"/> instance to deserialize.
        /// </param>
        /// <returns>
        /// The <see cref="object"/> represented by the <see cref="ObjectWithType"/>.
        /// </returns>
        object Deserialize(ObjectWithType owt);

        /// <summary>
        /// Serialize a generic object into a <see cref="ObjectWithType"/> structure.  The object
        /// must support serialization with protocol buffers, or the serialization will fail.
        /// </summary>
        /// <param name="obj">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The <see cref="ObjectWithType"/> representing the object.
        /// </returns>
        ObjectWithType Serialize(object obj);

        #endregion
    }
}