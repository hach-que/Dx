// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DList.cs" company="Redpoint Software">
//   The MIT License (MIT)
//   
//   Copyright (c) 2013 James Rhodes
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// <summary>
//   Defines the distributed list type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dx.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [Distributed]
    public class DList<T> : IList<T>, ITransparent
    {
        private List<T> m_List = new List<T>();

        public DList()
        {
            DpmEntrypoint.Construct(this);
        }

        public DList(int capacity)
        {
            DpmEntrypoint.Construct(this);

            this.m_List = new List<T>(capacity);
        }

        public DList(IEnumerable<T> collection)
        {
            DpmEntrypoint.Construct(this);

            this.m_List = new List<T>(collection);
        }

        #region ITransparent Members

        public string NetworkName
        {
            get;
            set;
        }

        public bool LocallyOwned
        {
            get;
            set;
        }

        public ILocalNode Node
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                if (this.InvokeLocally)
                {
                    return this._Count();
                }

                return (int)this.Owner.Invoke(this.NetworkName, "_Count", new Type[0], new object[] { });
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this.InvokeLocally)
                {
                    return this._IsReadOnly();
                }

                return (bool)this.Owner.Invoke(this.NetworkName, "_IsReadOnly", new Type[0], new object[] { });
            }
        }

        private ILocalNode Owner
        {
            get
            {
                return this.Node;
            }
        }

        private bool InvokeLocally
        {
            get
            {
                return this.LocallyOwned || this.NetworkName == null;
            }
        }

        #endregion

        public T this[int index]
        {
            get
            {
                if (this.InvokeLocally)
                {
                    return this._GetItemInternal(index);
                }

                return (T)this.Owner.Invoke(this.NetworkName, "_GetItemInternal", new Type[0], new object[] { index });
            }

            set
            {
                if (this.InvokeLocally)
                {
                    this._SetItemInternal(index, value);
                }
                else
                {
                    this.Owner.Invoke(this.NetworkName, "_SetItemInternal", new Type[0], new object[] { index, value });
                }
            }
        }

        #region Public Methods (automatically remoted)

        #region IList<T> Members

        public int IndexOf(T item)
        {
            if (this.InvokeLocally)
            {
                return this._IndexOf(item);
            }

            return (int)this.Owner.Invoke(this.NetworkName, "_IndexOf", new Type[0], new object[] { item });
        }

        public void Insert(int index, T item)
        {
            if (this.InvokeLocally)
            {
                this._Insert(index, item);
            }
            else
            {
                this.Owner.Invoke(this.NetworkName, "_Insert", new Type[0], new object[] { index, item });
            }
        }

        public void RemoveAt(int index)
        {
            if (this.InvokeLocally)
            {
                this._RemoveAt(index);
            }
            else
            {
                this.Owner.Invoke(this.NetworkName, "_RemoveAt", new Type[0], new object[] { index });
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (this.InvokeLocally)
            {
                this._Add(item);
            }
            else
            {
                this.Owner.Invoke(this.NetworkName, "_Add", new Type[0], new object[] { item });
            }
        }

        public void Clear()
        {
            if (this.InvokeLocally)
            {
                this._Clear();
            }
            else
            {
                this.Owner.Invoke(this.NetworkName, "_Clear", new Type[0], new object[] { });
            }
        }

        public bool Contains(T item)
        {
            if (this.InvokeLocally)
            {
                return this._Contains(item);
            }
            
            return (bool)this.Owner.Invoke(this.NetworkName, "_Contains", new Type[0], new object[] { item });
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (this.InvokeLocally)
            {
                this._CopyTo(array, arrayIndex);
            }
            else
            {
                this.Owner.Invoke(this.NetworkName, "_CopyTo", new Type[0], new object[] { array, arrayIndex });
            }
        }

        public bool Remove(T item)
        {
            if (this.InvokeLocally)
            {
                return this._Remove(item);
            }
            
            return (bool)this.Owner.Invoke(this.NetworkName, "_Remove", new Type[0], new object[] { item });
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.InvokeLocally)
            {
                return this._GetEnumerator();
            }

            return (IEnumerator<T>)this.Owner.Invoke(this.NetworkName, "_GetEnumerator", new Type[0], new object[] { });
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.InvokeLocally)
            {
                return this._IGetEnumerator();
            }

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
            // Protobuf deserializes the list by calling Add(), which means
            // we need to set up the local field correctly.
            if (this.m_List == null)
            {
                this.m_List = new List<T>();
            }

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
