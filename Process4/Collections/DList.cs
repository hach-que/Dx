using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;
using Process4.Interfaces;
using System.Collections;
using Data4;
using System.Runtime.Serialization;
using Process4.Remoting;

namespace Process4.Collections
{
    [Distributed, Serializable]
    public class DList<T> : IList<T>, ITransparent, ISerializable
    {
        private List<T> m_List = new List<T>();

        public DList()
        {
            Process4.Providers.DpmEntrypoint.Construct(this);
        }

        public DList(int capacity)
        {
            Process4.Providers.DpmEntrypoint.Construct(this);

            this.m_List = new List<T>(capacity);
        }

        public DList(IEnumerable<T> collection)
        {
            Process4.Providers.DpmEntrypoint.Construct(this);

            this.m_List = new List<T>(collection);
        }

        /// <summary>
        /// Asks the local node to synchronise the list.
        /// </summary>
        private void Sync()
        {
            LocalNode.Singleton.Storage.Store(this.NetworkName, this);
        }

        private Node Owner
        {
            get
            {
                Contact owner = LocalNode.Singleton.Storage.FetchOwner(this.NetworkName);
                while (owner == null)
                    owner = LocalNode.Singleton.Storage.FetchOwner(this.NetworkName);
                if (LocalNode.Singleton.ID == owner.Identifier)
                    return LocalNode.Singleton;
                else
                    return new RemoteNode(LocalNode.Singleton.Contacts.First(value => value.Identifier == owner.Identifier));
            }
        }

        #region ITransparent Members

        private string m_NetworkName;
        public string NetworkName
        {
            get
            {
                return this.m_NetworkName;
            }
            set
            {
                this.m_NetworkName = value;
            }
        }

        #endregion

        #region ISerializable Members

        protected DList(SerializationInfo info, StreamingContext context)
        {
            Process4.Providers.DpmEntrypoint.Deserialize(this, info, context);

            /*this.NetworkName = info.GetString("name");
            if (!(context.Context is SerializationData))
                throw new SerializationException("A distributed list was attempting to deserialize with the appropriate context information.");
            if ((context.Context as SerializationData).IsMessage || (context.Context as SerializationData).Entry.Key == ID.NewHash(this.NetworkName))
            {
                // We are being deserialized directly from the Dht.
                this.m_List = new List<T>();
                for (int i = 0; i < info.GetInt32("count"); i += 1)
                    this.m_List.Add((T)info.GetValue(i.ToString(), typeof(T)));
            }
            else
            {
                // Otherwise we were serialized as part of another object and probably
                // need to get ourselves from the Dht itself (where we are also stored).
                DList<T> copy = ((context.Context as SerializationData).Storage as IStorageProvider).Fetch(this.NetworkName) as DList<T>;
                this.m_List = copy.m_List.ToList();
            }*/
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Process4.Providers.DpmEntrypoint.Serialize(this, info, context);

            /*if (!(context.Context is SerializationData))
                throw new SerializationException("A distributed list was attempting to serialize with the appropriate context information.");
            if ((context.Context as SerializationData).IsMessage || (context.Context as SerializationData).Root == this)
            {
                // We are storing ourselves in the Dht.
                info.AddValue("count", this.m_List.Count);
                for (int i = 0; i < this.m_List.Count; i += 1)
                    info.AddValue(i.ToString(), this.m_List[i]);
            }
            else
            {
                // We're being stored as part of another object.  All we need is
                // our NetworkName since we'll need to get the actual data directly
                // from the Dht later.
            }
            info.AddValue("name", this.NetworkName);*/
        }

        #endregion

        #region Public Methods (automatically remoted)

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return (int)this.Owner.Invoke(this.NetworkName, "_IndexOf", new object[] { item });
        }

        public void Insert(int index, T item)
        {
            this.Owner.Invoke(this.NetworkName, "_Insert", new object[] { index, item });
        }

        public void RemoveAt(int index)
        {
            this.Owner.Invoke(this.NetworkName, "_RemoveAt", new object[] { index });
        }

        public T this[int index]
        {
            get
            {
                return (T)this.Owner.Invoke(this.NetworkName, "_GetItemInternal", new object[] { index });
            }
            set
            {
                this.Owner.Invoke(this.NetworkName, "_SetItemInternal", new object[] { index, value });
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            this.Owner.Invoke(this.NetworkName, "_Add", new object[] { item });
        }

        public void Clear()
        {
            this.Owner.Invoke(this.NetworkName, "_Clear", new object[] { });
        }

        public bool Contains(T item)
        {
            return (bool)this.Owner.Invoke(this.NetworkName, "_Contains", new object[] { item });
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.Owner.Invoke(this.NetworkName, "_CopyTo", new object[] { array, arrayIndex });
        }

        public int Count
        {
            get
            {
                return (int)this.Owner.Invoke(this.NetworkName, "_Count", new object[] { });
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (bool)this.Owner.Invoke(this.NetworkName, "_IsReadOnly", new object[] { });
            }
        }

        public bool Remove(T item)
        {
            return (bool)this.Owner.Invoke(this.NetworkName, "_Remove", new object[] { item });
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)this.Owner.Invoke(this.NetworkName, "_GetEnumerator", new object[] { });
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.Owner.Invoke(this.NetworkName, "_IGetEnumerator", new object[] { });
        }

        #endregion

        #endregion

        #region Private Methods (operate locally)

        #region IList<T> Members

        private int _IndexOf(T item)
        {
            return this.m_List.IndexOf(item);
        }

        private void _Insert(int index, T item)
        {
            this.m_List.Insert(index, item);
            this.Sync();
        }

        private void _RemoveAt(int index)
        {
            this.m_List.RemoveAt(index);
            this.Sync();
        }

        private T _GetItemInternal(int index)
        {
            return this.m_List[index];
        }

        private void _SetItemInternal(int index, T value)
        {
            this.m_List[index] = value;
            this.Sync();
        }

        #endregion

        #region ICollection<T> Members

        private void _Add(T item)
        {
            this.m_List.Add(item);
            this.Sync();
        }

        private void _Clear()
        {
            this.m_List.Clear();
            this.Sync();
        }

        private bool _Contains(T item)
        {
            return this.m_List.Contains(item);
        }

        private void _CopyTo(T[] array, int arrayIndex)
        {
            this.m_List.CopyTo(array, arrayIndex);
        }

        private int _Count()
        {
            return this.m_List.Count;
        }

        private bool _IsReadOnly()
        {
            return false;
        }

        private bool _Remove(T item)
        {
            if (this.m_List.Remove(item))
            {
                this.Sync();
                return true;
            }
            else
                return false;
        }

        #endregion

        #region IEnumerable<T> Members

        private IEnumerator<T> _GetEnumerator()
        {
            return this.m_List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        private IEnumerator _IGetEnumerator()
        {
            return (this.m_List as IEnumerable).GetEnumerator();
        }

        #endregion

        #endregion
    }
}
