// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleUnhandledExceptionLog.cs" company="Redpoint Software">
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
//   An implementation of <see cref="IUnhandledExceptionLog" /> that outputs
//   exceptions to the console.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// An implementation of <see cref="IUnhandledExceptionLog" /> that outputs
    /// exceptions to the console.
    /// </summary>
    public class ConsoleUnhandledExceptionLog : IUnhandledExceptionLog
    {
        #region Public Methods and Operators

        /// <summary>
        /// Log an exception to the console.
        /// </summary>
        /// <param name="context">
        /// The context of where the exception occurred.
        /// </param>
        /// <param name="ex">
        /// The exception itself.
        /// </param>
        public void Log(string context, Exception ex)
        {
            Console.Error.WriteLine("Unhandled exception occurred in context '" + context + "'");
            Console.Error.WriteLine(ex);
        }

        #endregion
    }
}