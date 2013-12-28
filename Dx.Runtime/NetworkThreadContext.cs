// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Redpoint Software" file="NetworkThreadContext.cs">
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
//   The current network thread context.
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Net;

    /// <summary>
    /// The current network thread context.
    /// </summary>
    public static class NetworkThreadContext
    {
        /// <summary>
        /// The current endpoint, or null if we are invoked from local code.
        /// </summary>
        [ThreadStatic]
        private static IPEndPoint m_CallerEndPoint;

        /// <summary>
        /// If this network context was instigated by a message sent from another receiving thread.
        /// </summary>
        [ThreadStatic]
        private static bool m_SentFromAnotherReceivingThread;

        /// <summary>
        /// Asserts that sending a message to the specified endpoint would not
        /// result in a deadlock.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint the message is about to be sent to
        /// </param>
        /// <exception cref="DeadlockDetectedException">
        /// Thrown if a deadlock would be caused by the network operation.
        /// </exception>
        public static void AssertSendIsValid(IPEndPoint endpoint)
        {
            if (m_SentFromAnotherReceivingThread)
            {
                if (object.Equals(m_CallerEndPoint, endpoint))
                {
                    throw new DeadlockDetectedException();
                }
            }
        }

        public static bool IsSentFromReceivingThread()
        {
            return m_CallerEndPoint != null;
        }

        /// <summary>
        /// Specifies that we are entering a network context with the endpoint as the target.
        /// </summary>
        /// <param name="sentFromReceivingThread">
        /// Whether this network context was instigated by a message being sent from another receiving thread.
        /// </param>
        /// <param name="endpoint">
        /// The endpoint that we are being called from
        /// </param>
        public static void EnterNetworkContext(bool sentFromReceivingThread, IPEndPoint endpoint)
        {
            if (m_CallerEndPoint != null)
            {
                throw new InvalidOperationException();
            }

            m_SentFromAnotherReceivingThread = sentFromReceivingThread;
            m_CallerEndPoint = endpoint;
        }

        /// <summary>
        /// The exit network context.
        /// </summary>
        public static void ExitNetworkContext()
        {
            m_CallerEndPoint = null;
        }
    }
}