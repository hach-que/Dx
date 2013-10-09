using System;

namespace Dx.Runtime.Tests.Data
{
    public class PreprocessedSyncTest : ISynchronised
    {
        private PreprocessedSyncTest_SynchronisedStore m_SyncStore;
    
        public SynchronisationStore GetSynchronisationStore(ILocalNode node, string name)
        {
            if (this.m_SyncStore == null)
                this.m_SyncStore = new Distributed<PreprocessedSyncTest_SynchronisedStore>(node, name);
            return this.m_SyncStore;
        }
    
        public class PreprocessedSyncTest_SynchronisedStore : SynchronisationStore
        {
            public override string[] GetNames()
            {
                return new string[]
                {
                    "X",
                    "Y",
                    "Z"
                };
            }
            
            public override Type[] GetTypes()
            {
                return new Type[]
                {
                    typeof(int),
                    typeof(float),
                    typeof(Bar)
                };
            }
            
            public override bool[] GetIsFields()
            {
                return new bool[]
                {
                    false,
                    true,
                    false
                };
            }
        }
    }
}

