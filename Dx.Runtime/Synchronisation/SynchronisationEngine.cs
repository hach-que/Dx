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
            if (authoritive)
                storeProp.GetSetMethod().Invoke(store, new object[] { syncProp.GetGetMethod().Invoke(synchronised, new object[0]) });
            else
                syncProp.GetSetMethod().Invoke(synchronised, new object[] { storeProp.GetGetMethod().Invoke(store, new object[0]) });
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
            if (authoritive)
                storeProp.GetSetMethod().Invoke(store, new object[] { syncField.GetValue(synchronised) });
            else
                syncField.SetValue(synchronised, storeProp.GetGetMethod().Invoke(store, new object[0]));
        }
    }
}

