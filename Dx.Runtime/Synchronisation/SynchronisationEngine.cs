using System;

namespace Dx.Runtime
{
    public class SynchronisationEngine
    {
        public void Apply(
            ISynchronised synchronised,
            SynchronisationStore store,
            bool authoritive)
        {
            var names = store.GetNames();
            var types = store.GetTypes();
            var isFields = store.GetIsFields();
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var type = types[i];
                var isField = isFields[i];
                if (isField)
                    this.PerformFieldAssignment(
                        synchronised,
                        store,
                        authoritive,
                        name,
                        type);
                else
                    this.PerformPropertyAssignment(
                        synchronised,
                        store,
                        authoritive,
                        name,
                        type);
            }
        }
        
        private void PerformPropertyAssignment(
            ISynchronised synchronised,
            SynchronisationStore store,
            bool authoritive,
            string name,
            Type type)
        {
            var syncProp = synchronised.GetType().GetProperty(name, BindingFlagsCombined.All);
            var storeProp = store.GetType().GetProperty(name, BindingFlagsCombined.All);
            var syncValue = syncProp.GetGetMethod().Invoke(synchronised, new object[0]);
            var storeValue = storeProp.GetGetMethod().Invoke(store, new object[0]);
            if (authoritive)
            {
                if ((storeValue != null && !storeValue.Equals(syncValue)) || (syncValue != null && !syncValue.Equals(storeValue)))
                {
                    Console.WriteLine(storeValue);
                    Console.WriteLine(syncValue);
                    storeProp.GetSetMethod().Invoke(store, new object[] { syncValue });
                }
            }
            else
                syncProp.GetSetMethod().Invoke(synchronised, new object[] { storeValue });
        }
        
        private void PerformFieldAssignment(
            ISynchronised synchronised,
            SynchronisationStore store,
            bool authoritive,
            string name,
            Type type)
        {
            var syncField = synchronised.GetType().GetField(name, BindingFlagsCombined.All);
            var storeProp = store.GetType().GetProperty(name, BindingFlagsCombined.All);
            var syncValue = syncField.GetValue(synchronised);
            var storeValue = storeProp.GetGetMethod().Invoke(store, new object[0]);
            if (authoritive)
            {
                if ((storeValue != null && !storeValue.Equals(syncValue)) || (syncValue != null && !syncValue.Equals(storeValue)))
                {
                    Console.WriteLine(storeValue);
                    Console.WriteLine(syncValue);
                    storeProp.GetSetMethod().Invoke(store, new object[] { syncValue });
                }
            }
            else
                syncField.SetValue(synchronised, storeValue);
        }
    }
}

