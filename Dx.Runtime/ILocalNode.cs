// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocalNode.cs" company="Redpoint Software">
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
//   The node interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// The node interface.  Although <see cref="LocalNode"/> implements this interface, it is unlikely
    /// you will ever call methods on it directly.  The methods in this interface are called by the
    /// runtime such that when your code is interacting with distributed objects, the logic for invoking
    /// methods, getting and setting properties and raising events is routed through the current local node
    /// that the object is associated with.
    /// </summary>
    public interface ILocalNode
    {
        #region Public Properties

        /// <summary>
        /// Gets the architecture of the network.
        /// </summary>
        Architecture Architecture { get; }

        /// <summary>
        /// Gets the caching mode of the network.
        /// </summary>
        Caching Caching { get; }

        /// <summary>
        /// Gets a value indicating whether the current node is a server node (in the server-client architecture).
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// Gets the contact that represents this node.
        /// </summary>
        Contact Self { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Binds this node to the specified address and port, waiting for inbound connections.
        /// </summary>
        /// <param name="address">
        /// The address to bind to.
        /// </param>
        /// <param name="port">
        /// The port to bind to.
        /// </param>
        void Bind(IPAddress address, int port);

        /// <summary>
        /// Closes the node, terminating all connections.
        /// </summary>
        void Close();

        /// <summary>
        /// Fetch an object from storage.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the object.
        /// </param>
        /// <returns>
        /// The object, or null if none was found.
        /// </returns>
        object Fetch(string id);

        /// <summary>
        /// Gets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>
        /// GetProperty("abc", new string[] { "Something", "PropertyToGet" });
        /// </code>
        /// would get the value of "PropertyToGet", on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="property">
        /// The property to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules", 
            "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        object GetProperty(string id, string property);

        /// <summary>
        /// Retrieves a service associated with this node.  Dx uses an inversion of control library internally
        /// so that components can be bound together.  You can use this method to locate a given service
        /// and retrieve the concrete implementation that it is bound to.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to retrieve.
        /// </typeparam>
        /// <returns>
        /// The concrete instance of <see cref="T"/>.
        /// </returns>
        T GetService<T>();

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>
        /// Invoke("abc", "MethodToCall", new object[] { });
        /// </code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="method">
        /// The method to be invoked.
        /// </param>
        /// <param name="targs">
        /// The type arguments to pass to the method.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the method.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules", 
            "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        object Invoke(string id, string method, Type[] targs, object[] args);

        /// <summary>
        /// Sets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>
        /// SetProperty("abc", "PropertyToSet", 5);
        /// </code>
        /// would set "PropertyToSet" to 5, on the abc object.
        /// </summary>
        /// <param name="id">
        /// The network ID of the object.
        /// </param>
        /// <param name="property">
        /// The property to store the value in.
        /// </param>
        /// <param name="value">
        /// The value to set the property to.
        /// </param>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules", 
            "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        void SetProperty(string id, string property, object value);

        /// <summary>
        /// Store an object into storage, with the current node as the owner.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the object.
        /// </param>
        /// <param name="data">
        /// The object itself.
        /// </param>
        void Store(string id, object data);

        /// <summary>
        /// Applies values from the synchronisation store to the target
        /// object.  One or more properties of the target must have the
        /// [Synchronised] attribute for this function to do anything
        /// useful.
        /// </summary>
        /// <param name="target">
        /// The target to synchronise.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="authoritative">
        /// Whether the instance is considered to be the authoritative instance.
        /// This changes the direction of synchronisation.
        /// </param>
        void Synchronise(object target, string name, bool authoritative);

        #endregion
    }
}