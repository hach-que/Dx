using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class FetchConfirmationMessage : ConfirmationMessage, ISerializable
    {
        private List<Entry> m_Values;

        public FetchConfirmationMessage(Dht dht, Message original, List<Entry> values)
            : base(dht, original, "")
        {
            this.m_Values = values;
        }

        public FetchConfirmationMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_Values = new List<Entry>();
            int count = info.GetInt32("fetch.count");
            for (int i = 0; i < count; i += 1)
                this.m_Values.Add(info.GetValue("fetch.entry." + i, typeof(Entry)) as Entry);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("fetch.count", this.m_Values.Count);
            for (int i = 0; i < this.m_Values.Count; i += 1)
                info.AddValue("fetch.entry." + i, this.m_Values[i], typeof(Entry));
        }

        /// <summary>
        /// The value values associated with the specified key.
        /// </summary>
        public List<Entry> Values
        {
            get { return this.m_Values; }
        }
    }
}

