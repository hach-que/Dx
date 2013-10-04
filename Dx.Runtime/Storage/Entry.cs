using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    [Serializable]
    public class Entry : ISerializable
    {
        private Dht m_Dht;
        private Contact p_Owner;
        private ID p_Key;
        private object m_StrongValue = null;
        private WeakReference m_Value = null;
        private bool p_DeadOnArrival;

        public Entry(Dht dht, Contact owner, ID key, object obj)
        {
            this.m_Dht = dht;
            this.p_Owner = owner;
            this.p_Key = key;
            this.m_Value = new WeakReference(obj);
            this.p_DeadOnArrival = false;
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
        /// Whether the entry is dead on arrival at the remote end (this can happen due
        /// to race conditions with WeakReferences, where we are too late in the sender's
        /// process to terminate the send operation, but the reference has since expired
        /// so we can't send useful data).
        /// </summary>
        public bool DeadOnArrival
        {
            get { return this.p_DeadOnArrival; }
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
            internal set
            {
                this.m_StrongValue = null;
                this.m_Value = new WeakReference(value);
            }
        }

        #region Serialization Methods

        public Entry(SerializationInfo info, StreamingContext context)
        {
            this.p_DeadOnArrival = false;
            try
            {
                string type = info.GetString("type");
                this.m_StrongValue = info.GetValue("object", Type.GetType(type));
                this.p_Key = info.GetValue("key", typeof(ID)) as ID;
            }
            catch (SerializationException)
            {
                this.p_DeadOnArrival = true;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Get a reference to the object (so that it continues to be
            // valid throughout the execution of this method).
            if (this.m_Value == null)
                throw new InvalidOperationException("Attempted to serialize an entry that contains no data.");
            if (!this.m_Value.IsAlive)
                return; // Message will be dead on arrival.
            object o = this.m_Value.Target;
            info.AddValue("type", o.GetType().AssemblyQualifiedName);
            info.AddValue("object", o);
            info.AddValue("key", this.p_Key);
        }

        #endregion
    }
}
