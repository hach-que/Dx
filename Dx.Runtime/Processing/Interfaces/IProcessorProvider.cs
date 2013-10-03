using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Remoting;
using Data4;

namespace Process4.Interfaces
{
    public interface IProcessorProvider
    {
        void Start();
        void Stop();

        /// <summary>
        /// A list of "agreed references"; that is IDs set for external or
        /// non-distributed objects.  Used by the event system.
        /// </summary>
        Dictionary<ID, object> AgreedReferences
        {
            get;
            set;
        }

        /// <summary>
        /// Adds the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        void AddEvent(EventTransport transport);

        /// <summary>
        /// Removes the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        void RemoveEvent(EventTransport transport);

        /// <summary>
        /// Invokes the event based on the event transport information.
        /// </summary>
        /// <param name="transport">The event transport information.</param>
        void InvokeEvent(EventTransport transport, object sender, EventArgs e);

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>Invoke("abc", "MethodToCall", new object[] { });</code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="method">The method to be invoked.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        object Invoke(string id, string method, Type[] targs, object[] args);

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
        DTask<object> InvokeAsync(string id, string method, Type[] targs, object[] args, Delegate callback);
    }
}