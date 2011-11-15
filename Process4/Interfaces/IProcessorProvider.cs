using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Interfaces
{
    public interface IProcessorProvider
    {
        void Start();
        void Stop();

        /// <summary>
        /// Invokes a method on the object with ID, descending through fields / properties
        /// specified in flds to the specified method.  For example:
        /// <code>Invoke("abc", "MethodToCall", new object[] { });</code>
        /// would call "MethodToCall" with no arguments, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="method">The method to be invoked.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        object Invoke(string id, string method, object[] args);

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
        DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback);
    }
}
