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
using System.Runtime.Serialization;
using System.IO;

namespace Data4
{
    [Serializable()]
    public class Entry : ISerializable
    {
        private Dht m_Dht;
        private Contact p_Owner;
        private ID p_Key;
        private string p_Value;
        // TODO: Add expiry and ownership properties.

        public Entry(Dht dht, Contact owner, ID key, string value)
        {
            this.m_Dht = dht;
            this.p_Owner = owner;
            this.p_Key = key;
            this.p_Value = value;
        }

        public Entry(SerializationInfo info, StreamingContext context)
        {
            this.p_Key = info.GetValue("key", typeof(ID)) as ID;
            this.p_Value = info.GetString("value");
        }

        public Contact Owner
        {
            get { return this.p_Owner; }
            set
            {
                if (this.p_Owner == null)
                    this.p_Owner = value;
                else
                    throw new ArgumentException("Can not change owner of Entry once assigned.");
            }
        }

        public ID Key
        {
            get { return this.p_Key; }
        }

        public string Value
        {
            get { return this.p_Value; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("key", this.p_Key, typeof(ID));
            info.AddValue("value", this.p_Value);
        }

        /// <summary>
        /// Converts the entry to the specified .NET type.
        /// </summary>
        /// <typeparam name="T">The type to convert this entry to.</typeparam>
        public T As<T>()
        {
            StreamingContext old = this.m_Dht.Formatter.Context;
            this.m_Dht.Formatter.Context = new StreamingContext(this.m_Dht.Formatter.Context.State, new SerializationData { Dht = this.m_Dht, Entry = this });
            T t = (T)(this.m_Dht.Formatter.Deserialize(new System.IO.MemoryStream(Convert.FromBase64String(this.p_Value))));
            this.m_Dht.Formatter.Context = old;
            return t;
        }

        /// <summary>
        /// Converts the specified .NET object to a string value using
        /// the specified Dht's formatter.
        /// </summary>
        /// <typeparam name="T">The type of the object being serialized.</typeparam>
        /// <param name="obj"></param>
        /// <remarks>This function is currently not exposed due to the complexities it would introduce into SerializationData.</remarks>
        private static string Serialize<T>(Dht dht, T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                StreamingContext old = dht.Formatter.Context;
                dht.Formatter.Context = new StreamingContext(dht.Formatter.Context.State, new SerializationData { Dht = dht });
                dht.Formatter.Serialize(stream, obj);
                dht.Formatter.Context = old;
                stream.Close();
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
    }
}

