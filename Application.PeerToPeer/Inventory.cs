using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;
using Process4.Collections;
using System.Collections;

namespace Application.PeerToPeer
{
    [Distributed]
    public class Item
    {
        public string ID { get; private set; }

        /// <summary>
        /// Creates a new item with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        public Item(string id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return this.ID;
        }
    }

    [Distributed]
    public class Inventory : IEnumerable<Item>
    {
        public DList<Item> m_Items { get; set; }

        /// <summary>
        /// Creates a new empty inventory.
        /// </summary>
        public Inventory()
        {
            this.m_Items = new DList<Item>();
            Console.WriteLine(this.m_Items.NetworkName);
        }

        /// <summary>
        /// Creates a new inventory with the specified list of items.
        /// </summary>
        /// <param name="items">The items to store.</param>
        public Inventory(IEnumerable<Item> items)
            : this()
        {
            foreach (Item i in items)
                this.m_Items.Add(i);
        }

        /// <summary>
        /// Takes an item from this inventory and returns it.  The item
        /// is removed from the inventory.
        /// </summary>
        /// <returns>An item.</returns>
        public Item Take(string id)
        {
            Item i = this.Fetch(id);
            if (i == null)
                return null;

            this.m_Items.Remove(i);
            return i;
        }

        /// <summary>
        /// Fetches and item from the inventory and returns it.  The item
        /// is not removed from the inventory.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <returns></returns>
        public Item Fetch(string id)
        {
            foreach (Item i in this.m_Items)
                if (i.ID == id)
                    return i;
            return null;
        }

        /// <summary>
        /// Store a new item in the inventory.
        /// </summary>
        /// <param name="item">The item to store.</param>
        public void Store(Item item)
        {
            this.m_Items.Add(item);
        }

        #region IEnumerable<Item> Members

        public IEnumerator<Item> GetEnumerator()
        {
            return this.m_Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this.m_Items as IEnumerable).GetEnumerator();
        }

        #endregion
    }
}
