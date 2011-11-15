// 
//  Copyright 2010  Trust4 Developers
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Data4
{
    [Serializable()]
    public class FetchConfirmationMessage : ConfirmationMessage, ISerializable
    {
        public List<Entry> p_Values = null;

        public FetchConfirmationMessage(Dht dht, Message original, List<Entry> values) : base(dht, original, "")
        {
            this.p_Values = values;
        }

        public FetchConfirmationMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.p_Values = new List<Entry>();
            int count = info.GetInt32("fetch.count");
            for (int i = 0; i < count; i += 1)
                this.p_Values.Add(info.GetValue("fetch.entry." + i, typeof(Entry)) as Entry);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("fetch.count", this.p_Values.Count);
            for (int i = 0; i < this.p_Values.Count; i += 1)
                info.AddValue("fetch.entry." + i, this.p_Values[i], typeof(Entry));
        }

        /// <summary>
        /// The value values associated with the specified key.
        /// </summary>
        public List<Entry> Values
        {
            get { return this.p_Values; }
        }
    }
}

