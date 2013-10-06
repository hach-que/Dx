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
            foreach (var name in store.GetNames())
            {
                var syncProp = synchronised.GetType().GetProperty(name, BindingFlagsCombined.All);
                var storeProp = store.GetType().GetProperty(name, BindingFlagsCombined.All);
                if (authoritive)
                    storeProp.GetSetMethod().Invoke(store, new object[] { syncProp.GetGetMethod().Invoke(synchronised, new object[0]) });
                else
                    syncProp.GetSetMethod().Invoke(synchronised, new object[] { storeProp.GetGetMethod().Invoke(store, new object[0]) });
            }
        }
    }
}

