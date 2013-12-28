// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListeningClientHandler.cs" company="Redpoint Software">
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
//   A client handler implementation that is used by <see cref="IConnectionHandler"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// A client handler implementation that is used by <see cref="IConnectionHandler"/>.
    /// </summary>
    public class ListeningClientHandler : IClientHandler
    {
        #region Fields

        /// <summary>
        /// The TCP client that this handler communicates through.
        /// </summary>
        private readonly TcpClient m_Client;

        /// <summary>
        /// A calculated map of message type integers to the associated handlers.
        /// </summary>
        private readonly Dictionary<int, IMessageHandler> m_HandlerMappings;

        /// <summary>
        /// The <see cref="IMessageIO"/> interface, which is used to transmit messages over the network.
        /// </summary>
        private readonly IMessageIO m_MessageIo;

        /// <summary>
        /// The <see cref="IMessageNetworkReceiver"/> interface, which is used to safely receive messages.
        /// </summary>
        private readonly IMessageNetworkReceiver m_MessageNetworkReceiver;

        /// <summary>
        /// The thread on which messages are received.
        /// </summary>
        private readonly Thread m_ReceiveThread;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ListeningClientHandler"/> class.
        /// </summary>
        /// <param name="messageIo">
        /// The <see cref="IMessageIO"/> interface.
        /// </param>
        /// <param name="messageNetworkReceiver">
        /// The <see cref="IMessageNetworkReceiver"/> interface.
        /// </param>
        /// <param name="client">
        /// The TCP client that is associated with this handler.
        /// </param>
        /// <param name="messageHandlers">
        /// The message handlers.
        /// </param>
        public ListeningClientHandler(
            IMessageIO messageIo,
            IMessageNetworkReceiver messageNetworkReceiver,
            TcpClient client, 
            IEnumerable<IMessageHandler> messageHandlers)
        {
            this.m_MessageIo = messageIo;
            this.m_MessageNetworkReceiver = messageNetworkReceiver;
            this.m_Client = client;
            this.m_ReceiveThread = new Thread(this.Run) { Name = "Receive Thread for " + client.Client.RemoteEndPoint };

            this.m_HandlerMappings = messageHandlers.ToDictionary(k => k.GetMessageType(), v => v);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Handle the specified message using the appropriate message handler.  This
        /// is called when a message has been received over the network.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Receive(Message message)
        {
            this.m_HandlerMappings[message.Type].Handle(message);
        }

        /// <summary>
        /// Send a message to the remote host that this client handler is connected to.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Send(Message message)
        {
            try
            {
                NetworkThreadContext.AssertSendIsValid((IPEndPoint)this.m_Client.Client.RemoteEndPoint);

                lock (this.m_Client)
                {
                    this.m_MessageIo.Send(this.m_Client.GetStream(), message);
                }
            }
            catch (ObjectDisposedException)
            {
                // The client is no longer available.
                // TODO: Remove the client from the lookup list.
            }
        }

        /// <summary>
        /// Start the client handler, initiating connection to the remote host.
        /// </summary>
        public void Start()
        {
            this.m_ReceiveThread.Start();
        }

        /// <summary>
        /// Stop the client handler, disconnecting from the remote host and
        /// finalizing the receive thread cleanly.
        /// </summary>
        public void Stop()
        {
            this.m_Client.Close();

            // Wait for the thread to close cleanly.
            while (this.m_ReceiveThread.IsAlive)
            {
                Thread.Sleep(0);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The main message receive loop used by the thread.
        /// </summary>
        private void Run()
        {
            this.m_MessageNetworkReceiver.Run(this.m_Client, this.Receive);
        }

        #endregion
    }
}