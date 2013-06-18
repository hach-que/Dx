using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;
using Process4.Remoting;

namespace Process4
{
    public abstract class Node
    {
        /// <summary>
        /// Sets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>SetProperty("abc", "PropertyToSet", 5);</code>
        /// would set "PropertyToSet" to 5, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="property">The property to store the value in.</param>
        /// <param name="value">The value to set the property to.</param>
        internal abstract void SetProperty(string id, string property, object value);

        /// <summary>
        /// Gets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>GetProperty("abc", new string[] { "Something", "PropertyToGet" });</code>
        /// would get the value of "PropertyToGet", on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="property">The property to fetch.</param>
        internal abstract object GetProperty(string id, string property);

        /// <summary>
        /// Adds the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        internal abstract void AddEvent(EventTransport transport);

        /// <summary>
        /// Removes the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        internal abstract void RemoveEvent(EventTransport transport);

        /// <summary>
        /// Invokes the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        internal abstract void InvokeEvent(EventTransport transport, object sender, EventArgs e);

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>Invoke("abc", "MethodToCall", new object[] { });</code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="method">The method to be invoked.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        internal abstract object Invoke(string id, string method, object[] args);

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>Invoke("abc", "MethodToCall", new object[] { });</code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="method">The method to be invoked.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <param name="callback">The callback to issue when the method completes.</param>
        internal abstract DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback);

        /// <summary>
        /// The ID of this node.
        /// </summary>
        public ID ID { get; internal set; }
    }
}
