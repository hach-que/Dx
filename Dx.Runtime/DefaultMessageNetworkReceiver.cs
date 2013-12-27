// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMessageNetworkReceiver.cs" company="Redpoint Software">
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
//   The default message network receiver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// The default message network receiver.
    /// </summary>
    public class DefaultMessageNetworkReceiver : IMessageNetworkReceiver
    {
        #region Fields

        /// <summary>
        /// The <see cref="IMessageIO"/> interface.
        /// </summary>
        private readonly IMessageIO m_MessageIo;

        /// <summary>
        /// The <see cref="IUnhandledExceptionLog"/> interface.
        /// </summary>
        private readonly IUnhandledExceptionLog m_UnhandledExceptionLog;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMessageNetworkReceiver"/> class.
        /// </summary>
        /// <param name="messageIo">
        /// The <see cref="IMessageIO"/> interface.
        /// </param>
        /// <param name="unhandledExceptionLog">
        /// The <see cref="IUnhandledExceptionLog"/> interface.
        /// </param>
        public DefaultMessageNetworkReceiver(IMessageIO messageIo, IUnhandledExceptionLog unhandledExceptionLog)
        {
            this.m_MessageIo = messageIo;
            this.m_UnhandledExceptionLog = unhandledExceptionLog;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Continuously receive messages from a TCP client, calling "callback" until
        /// the client is no longer connected. 
        /// </summary>
        /// <param name="client">
        /// The TCP client.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public void Run(TcpClient client, Action<Message> callback)
        {
            try
            {
                while (client.Connected)
                {
                    Message message = null;
                    try
                    {
                        message = this.m_MessageIo.Receive(client.GetStream());
                    }
                    catch (EndOfStreamException)
                    {
                        // This occurs when we are attempting to retrieve the next message
                        // but the stream is terminated (and so we reach the end of the network
                        // stream).  We can't read any more messages from the stream regardless
                        // of whether we are connected, so we need to return.
                        try
                        {
                            client.Close();
                        }
                        catch (SocketException)
                        {
                        }
                        catch (ObjectDisposedException)
                        {
                        }

                        return;
                    }
                    catch (InvalidOperationException)
                    {
                        if (!client.Connected)
                        {
                            // The client disconnected between checking in the while loop
                            // and actually attempt to retrieve the stream (or read from it).
                            // Break out of the while loop since we are no longer connected
                            // anyway.
                            return;
                        }
                    }
                    catch (IOException ex)
                    {
                        // If we don't have an inner exception, then we rethrow (we are only
                        // interested in networking related exceptions here, and they may be
                        // wrapped in IO exception due to reading from a stream).
                        var innerException = ex.InnerException;
                        if (innerException == null)
                        {
                            throw;
                        }

                        // Try and convert the inner exception to a socket exception; if it
                        // is, check if it's an exception that we might be expecting (for
                        // example if the connection was closed from another thread, as is
                        // the case when Stop() is called).
                        var socketException = innerException as SocketException;
                        if (socketException != null)
                        {
                            switch (socketException.ErrorCode)
                            {
                                case 10004:
                                    // A blocking operation was interrupted by a call to WSACancelBlockingCall.
                                    // This occurs when Stop() is called and the connection is forcibly closed.
                                    return;
                                default:
                                    // We don't know what kind of socket exception this was, so we rethrow it.
                                    throw;
                            }
                        }

                        // Try and convert the inner exception to an object disposed exception;
                        // if it is, then the client has been disposed and we should return.
                        var objectDisposedException = innerException as ObjectDisposedException;
                        if (objectDisposedException != null)
                        {
                            return;
                        }

                        // Try and convert the inner exception to a thread abort exception;
                        // if it is, then the client is being terminated and we should return.
                        var threadAbortException = innerException as ThreadAbortException;
                        if (threadAbortException != null)
                        {
                            return;
                        }

                        throw;
                    }

                    if (message == null)
                    {
                        continue;
                    }

                    var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    message.Sender = new Contact { IPAddress = endpoint.Address, Port = endpoint.Port };

                    // Attempt to call the callback, which may execute user code based on the message.
                    // Since this is executing user code, there's the potential that an exception might
                    // occur, and that exception finds it's way back up to this code.  We need to ensure
                    // that any exceptions occurring in user code do not impact our ability to accept
                    // future messages, so we pass the exception off to the IUnhandledExceptionLog
                    // interface.
                    try
                    {
                        callback(message);
                    }
                    catch (Exception ex)
                    {
                        this.m_UnhandledExceptionLog.Log(
                            "user code",
                            ex);
                        throw;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // This occurs when the thread is being forcibly aborted.  This seems to be much more
                // common under the Mono runtime than .NET.  We can't do anything here but return.
            }
            catch (Exception ex)
            {
                // Under Mono, we can not allow any exceptions to escape from a thread.  Mono does not
                // generally handle exceptions escaping from threads correctly, and it usually causes
                // a native crash from which there is no recovery (because at that point, C# is not
                // executing any more).
                //
                // Under all platforms, the best option is to log it via the unhandled exception log,
                // close the client (if possible), and return.
                this.m_UnhandledExceptionLog.Log(
                    "receive handler",
                    ex);

                try
                {
                    client.Close();
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception exx)
                {
                    this.m_UnhandledExceptionLog.Log(
                        "while closing client",
                        exx);
                }
            }
        }

        #endregion
    }
}