using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace Data4
{
    [Serializable]
    public class Entry : ISerializable
    {
        private Dht m_Dht;
        private Contact p_Owner;
        private ID p_Key;
        private object m_StrongValue = null;
        private WeakReference m_Value = null;

        public Entry(Dht dht, Contact owner, ID key, object obj)
        {
            this.m_Dht = dht;
            this.p_Owner = owner;
            this.p_Key = key;
            this.m_Value = new WeakReference(obj);
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

        /// <summary>
        /// Whether the entry is still alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (this.m_StrongValue != null)
                    return true;
                if (this.m_Value == null)
                    return false;
                return (this.m_Value.IsAlive);
            }
        }

        /// <summary>
        /// The value of this DHT entry.
        /// </summary>
        public object Value
        {
            get
            {
                if (this.m_StrongValue != null)
                    return this.m_StrongValue;
                else
                    return this.m_Value.Target;
            }
        }

        #region Serialization Methods

        public Entry(SerializationInfo info, StreamingContext context)
        {
            string type = info.GetString("type");
            this.m_StrongValue = info.GetValue("object", Type.GetType(type));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Get a reference to the object (so that it continues to be
            // valid throughout the execution of this method).
            if (this.m_Value == null)
                throw new InvalidOperationException("Attempted to serialize an entry that contains no data.");
            if (!this.m_Value.IsAlive)
                throw new InvalidOperationException("Attempted to serialize an entry that contains exoired data.");
            object o = this.m_Value.Target;
            info.AddValue("type", o.GetType().AssemblyQualifiedName);
            info.AddValue("object", o);
        }

        #endregion
    }
}
