// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAttribute.cs" company="Redpoint Software">
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
//   Specifies that this property, method or field always resides locally
//   on the machine and is never shared between computers.  Useful for
//   server-client architectures since it allows you to include non-distributed
//   data inside distributed classes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// Specifies that this property, method or field always resides locally
    /// on the machine and is never shared between computers.  Useful for
    /// server-client architectures since it allows you to include non-distributed
    /// data inside distributed classes.
    /// </summary>
    public class LocalAttribute : Attribute
    {
    }
}