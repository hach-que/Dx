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

        private Node Owner
        {
            get
            {
                if (this.LocallyOwned || this.NetworkName == null)
                    return LocalNode.Singleton;
                else
                {
                    Contact owner = LocalNode.Singleton.Storage.FetchOwner(this.NetworkName);
                    while (owner == null)
                        owner = LocalNode.Singleton.Storage.FetchOwner(this.NetworkName);
                    if (LocalNode.Singleton.ID == owner.Identifier)
                    {
                        // We are locally owned, but the variable isn't set.  Speed up
                        // future requests by setting us as locally owned.
                        this.LocallyOwned = true;
                        return LocalNode.Singleton;
                    }
                    else
                        return new RemoteNode(LocalNode.Singleton.Contacts.First(value => value.Identifier == owner.Identifier));
                }
            }
        }

        private bool InvokeLocally
        {
            get
            {
                return (this.LocallyOwned || this.NetworkName == null);
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

        private bool m_LocallyOwned;
        public bool LocallyOwned
        {
            get
            {
                return this.m_LocallyOwned;
            }
            set
            {
                this.m_LocallyOwned = value;
            }
        }

        private bool m_IsImmutablyPushed;
        public bool IsImmutablyPushed
        {
            get
            {
                return this.m_IsImmutablyPushed;
            }
            set
            {
                this.m_IsImmutablyPushed = value;
            }
        }

        #endregion

        #region ISerializable Members

        protected DList(SerializationInfo info, StreamingContext context)
        {
            Process4.Providers.DpmEntrypoint.Deserialize(this, info, context);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Process4.Providers.DpmEntrypoint.Serialize(this, info, context);
        }

        #endregion

        #region Public Methods (automatically remoted)

        #region IList<T> Members

        public int IndexOf(T item)
        {
            if (this.InvokeLocally)
                return this._IndexOf(item);
            else
                return (int)this.Owner.Invoke(this.NetworkName, "_IndexOf", new Type[0], new object[] { item });
        }

        public void Insert(int index, T item)
        {
            if (this.InvokeLocally)
                this._Insert(index, item);
            else
                this.Owner.Invoke(this.NetworkName, "_Insert", new Type[0], new object[] { index, item });
        }

        public void RemoveAt(int index)
        {
            if (this.InvokeLocally)
                this._RemoveAt(index);
            else
                this.Owner.Invoke(this.NetworkName, "_RemoveAt", new Type[0], new object[] { index });
        }

        public T this[int index]
        {
            get
            {
                if (this.InvokeLocally)
                    return this._GetItemInternal(index);
                else
                    return (T)this.Owner.Invoke(this.NetworkName, "_GetItemInternal", new Type[0], new object[] { index });
            }
            set
            {
                if (this.InvokeLocally)
                    this._SetItemInternal(index, value);
                else
                    this.Owner.Invoke(this.NetworkName, "_SetItemInternal", new Type[0], new object[] { index, value });
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (this.InvokeLocally)
                this._Add(item);
            else
                this.Owner.Invoke(this.NetworkName, "_Add", new Type[0], new object[] { item });
        }

        public void Clear()
        {
            if (this.InvokeLocally)
                this._Clear();
            else
                this.Owner.Invoke(this.NetworkName, "_Clear", new Type[0], new object[] { });
        }

        public bool Contains(T item)
        {
            if (this.InvokeLocally)
                return this._Contains(item);
            else
                return (bool)this.Owner.Invoke(this.NetworkName, "_Contains", new Type[0], new object[] { item });
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (this.InvokeLocally)
                this._CopyTo(array, arrayIndex);
            else
                this.Owner.Invoke(this.NetworkName, "_CopyTo", new Type[0], new object[] { array, arrayIndex });
        }

        public int Count
        {
            get
            {
                if (this.InvokeLocally)
                    return this._Count();
                else
                    return (int)this.Owner.Invoke(this.NetworkName, "_Count", new Type[0], new object[] { });
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this.InvokeLocally)
                    return this._IsReadOnly();
                else
                    return (bool)this.Owner.Invoke(this.NetworkName, "_IsReadOnly", new Type[0], new object[] { });
            }
        }

        public bool Remove(T item)
        {
            if (this.InvokeLocally)
                return this._Remove(item);
            else
                return (bool)this.Owner.Invoke(this.NetworkName, "_Remove", new Type[0], new object[] { item });
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.InvokeLocally)
                return this._GetEnumerator();
            else
                return (IEnumerator<T>)this.Owner.Invoke(this.NetworkName, "_GetEnumerator", new Type[0], new object[] { });
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.InvokeLocally)
                return this._IGetEnumerator();
            else
                return (IEnumerator)this.Owner.Invoke(this.NetworkName, "_IGetEnumerator", new Type[0], new object[] { });
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
        }

        private void _RemoveAt(int index)
        {
            this.m_List.RemoveAt(index);
        }

        private T _GetItemInternal(int index)
        {
            return this.m_List[index];
        }

        private void _SetItemInternal(int index, T value)
        {
            this.m_List[index] = value;
        }

        #endregion

        #region ICollection<T> Members

        private void _Add(T item)
        {
            this.m_List.Add(item);
        }

        private void _Clear()
        {
            this.m_List.Clear();
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
            return this.m_List.Remove(item);
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
