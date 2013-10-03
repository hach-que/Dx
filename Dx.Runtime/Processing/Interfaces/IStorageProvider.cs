using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data4;

namespace Process4.Interfaces
{
    public interface IStorageProvider
    {
        void Start();
        void Stop();

        /// <summary>
        /// Sets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>SetProperty("abc", "PropertyToSet", 5);</code>
        /// would set "PropertyToSet" to 5, on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="flds">The property to store the value in.</param>
        /// <param name="value">The value to set the property to.</param>
        void SetProperty(string id, string property, object value);

        /// <summary>
        /// Gets the last specified property of the object with ID, descending through fields / properties
        /// specified in flds to the specified value.  For example:
        /// <code>GetProperty("abc", new string[] { "Something", "PropertyToGet" });</code>
        /// would get the value of "PropertyToGet", on the abc object.
        /// </summary>
        /// <param name="id">The network ID of the object.</param>
        /// <param name="flds">The property to fetch.</param>
        object GetProperty(string id, string property);

        /// <summary>
        /// Stores an object with a global identifier.
        /// </summary>
        /// <param name="id">The global identifier.</param>
        /// <param name="o">The object to store.</param>
        void Store(string id, object o);

        /// <summary>
        /// Fetches an object with a global identifier.
        /// </summary>
        /// <param name="id">The global identifier.</param>
        object Fetch(string id);

        /// <summary>
        /// Retrieves the contact owner for the object with the global identifier.
        /// </summary>
        /// <param name="id">The global identifier.</param>
        Contact FetchOwner(string id);
    }
}
