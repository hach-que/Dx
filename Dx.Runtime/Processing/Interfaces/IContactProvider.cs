using System.Collections.Generic;

namespace Dx.Runtime
{
    public interface IContactProvider : IEnumerable<Contact>
    {
        /// <summary>
        /// Indicates whether the IStorageProvider must be started before
        /// the network is.
        /// </summary>
        bool StorageStartRequired { get; }

        /// <summary>
        /// Adds the specified contact to the contact provider.
        /// </summary>
        /// <param name="contact">The contact to add.</param>
        void Add(Contact contact);

        /// <summary>
        /// Removes the specified contact from the contact provider.
        /// </summary>
        /// <param name="contact">The contact to remove.</param>
        void Remove(Contact contact);

        /// <summary>
        /// Clears all of the contacts.
        /// </summary>
        void Clear();
    }
}
