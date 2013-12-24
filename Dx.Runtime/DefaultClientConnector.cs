// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultClientConnector.cs" company="Redpoint Software">
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
//   An implementation of <see cref="IClientConnector"/> that provides a simple
//   mechanism for establishing a connection to a remote node.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Net;

    /// <summary>
    /// An implementation of <see cref="IClientConnector"/> that provides a simple
    /// mechanism for establishing a connection to a remote node.
    /// </summary>
    public class DefaultClientConnector : IClientConnector
    {
        #region Fields

        /// <summary>
        /// The <see cref="IClientHandlerFactory"/> interface.
        /// </summary>
        private readonly IClientHandlerFactory m_ClientHandlerFactory;

        /// <summary>
        /// The <see cref="IClientLookup"/> interface.
        /// </summary>
        private readonly IClientLookup m_ClientLookup;

        /// <summary>
        /// The <see cref="IMessageConstructor"/> interface.
        /// </summary>
        private readonly IMessageConstructor m_MessageConstructor;

        /// <summary>
        /// The <see cref="IMessageSideChannel"/> interface.
        /// </summary>
        private readonly IMessageSideChannel m_MessageSideChannel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultClientConnector"/> class.
        /// </summary>
        /// <param name="clientLookup">
        /// The client lookup.
        /// </param>
        /// <param name="clientHandlerFactory">
        /// The client handler factory.
        /// </param>
        /// <param name="messageConstructor">
        /// The message constructor.
        /// </param>
        /// <param name="messageSideChannel">
        /// The message side channel.
        /// </param>
        public DefaultClientConnector(
            IClientLookup clientLookup, 
            IClientHandlerFactory clientHandlerFactory, 
            IMessageConstructor messageConstructor, 
            IMessageSideChannel messageSideChannel)
        {
            this.m_ClientLookup = clientLookup;
            this.m_ClientHandlerFactory = clientHandlerFactory;
            this.m_MessageConstructor = messageConstructor;
            this.m_MessageSideChannel = messageSideChannel;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Establish a connection to the specified remote host.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the remote connection was established, but messages could not successfully be sent.
        /// </exception>
        public void Connect(IPAddress address, int port)
        {
            var handler = this.m_ClientHandlerFactory.CreateActiveClientHandler(address, port);
            handler.Start();
            this.m_ClientLookup.Add(new IPEndPoint(address, port), handler);

            // Now we send a ConnectionPing message so that we know the remote host
            // is aware of our connection and is ready to accept messages.
            handler.Send(this.m_MessageConstructor.ConstructConnectionPingMessage());

            var pong =
                this.m_MessageSideChannel.WaitUntil(
                    x =>
                    object.Equals(x.Sender.IPAddress, address) && x.Sender.Port == port
                    && x.Type == MessageType.ConnectionPong, 
                    30000);

            if (pong == null)
            {
                handler.Stop();
                this.m_ClientLookup.Remove(new IPEndPoint(address, port));

                throw new InvalidOperationException("Unable to connect to the remote host!");
            }
        }

        #endregion
    }
}