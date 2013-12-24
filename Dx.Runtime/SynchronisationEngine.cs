// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronisationEngine.cs" company="Redpoint Software">
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
//   The synchronisation engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Dx.Runtime
{
    using System;

    /// <summary>
    /// The synchronisation engine.
    /// </summary>
    public class SynchronisationEngine
    {
        #region Public Methods and Operators

        /// <summary>
        /// Apply synchronisation local to the specified object.
        /// </summary>
        /// <param name="synchronised">
        /// The synchronised object.
        /// </param>
        /// <param name="store">
        /// The synchronisation store for the object.
        /// </param>
        /// <param name="authoritative">
        /// Whether this is an authoritative synchronisation.
        /// </param>
        public void Apply(ISynchronised synchronised, SynchronisationStore store, bool authoritative)
        {
            /*
             * TODO Use type information in synchronisation engine
             * 
             * We don't yet use the type information on the synchronisation store
             * for anything useful (we just do direct assignments).  However, we will need
             * this information in future when we want to do complex synchronisation of lists
             * (where we might want to only synchronise changed values).  In these scenarios
             * we will need the type information available to perform better heuristics.
             */

            var names = store.GetNames();
            var isFields = store.GetIsFields();
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var isField = isFields[i];
                if (isField)
                {
                    this.PerformFieldAssignment(synchronised, store, authoritative, name);
                }
                else
                {
                    this.PerformPropertyAssignment(synchronised, store, authoritative, name);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Perform assignment of a field in the synchronisation process.
        /// </summary>
        /// <param name="synchronised">
        /// The synchronised object.
        /// </param>
        /// <param name="store">
        /// The synchronisation store for the object.
        /// </param>
        /// <param name="authoritative">
        /// Whether this is an authoritative synchronisation.
        /// </param>
        /// <param name="name">
        /// The name of the field to synchronise.
        /// </param>
        private void PerformFieldAssignment(
            ISynchronised synchronised, 
            SynchronisationStore store, 
            bool authoritative, 
            string name)
        {
            var syncField = synchronised.GetType().GetField(name, BindingFlagsCombined.All);
            var storeProp = store.GetType().GetProperty(name, BindingFlagsCombined.All);
            if (syncField == null)
            {
                return;
            }

            var syncValue = syncField.GetValue(synchronised);
            var storeValue = storeProp.GetGetMethod().Invoke(store, new object[0]);
            if (authoritative)
            {
                if ((storeValue != null && !storeValue.Equals(syncValue))
                    || (syncValue != null && !syncValue.Equals(storeValue)))
                {
                    Console.WriteLine(storeValue);
                    Console.WriteLine(syncValue);
                    storeProp.GetSetMethod().Invoke(store, new[] { syncValue });
                }
            }
            else
            {
                syncField.SetValue(synchronised, storeValue);
            }
        }

        /// <summary>
        /// Perform assignment of a property in the synchronisation process.
        /// </summary>
        /// <param name="synchronised">
        /// The synchronised object.
        /// </param>
        /// <param name="store">
        /// The synchronisation store for the object.
        /// </param>
        /// <param name="authoritative">
        /// Whether this is an authoritative synchronisation.
        /// </param>
        /// <param name="name">
        /// The name of the property to synchronise.
        /// </param>
        private void PerformPropertyAssignment(
            ISynchronised synchronised, 
            SynchronisationStore store, 
            bool authoritative, 
            string name)
        {
            var syncProp = synchronised.GetType().GetProperty(name, BindingFlagsCombined.All);
            var storeProp = store.GetType().GetProperty(name, BindingFlagsCombined.All);
            if (syncProp == null)
            {
                return;
            }

            var syncValue = syncProp.GetGetMethod().Invoke(synchronised, new object[0]);
            var storeValue = storeProp.GetGetMethod().Invoke(store, new object[0]);
            if (authoritative)
            {
                if ((storeValue != null && !storeValue.Equals(syncValue))
                    || (syncValue != null && !syncValue.Equals(storeValue)))
                {
                    Console.WriteLine(storeValue);
                    Console.WriteLine(syncValue);
                    storeProp.GetSetMethod().Invoke(store, new[] { syncValue });
                }
            }
            else
            {
                syncProp.GetSetMethod().Invoke(synchronised, new[] { storeValue });
            }
        }

        #endregion
    }
}