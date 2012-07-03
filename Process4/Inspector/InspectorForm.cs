using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Process4.Providers;

namespace Data4.Inspector
{
    public partial class InspectorForm : Form
    {
        private DhtWrapper m_Dht = null;

        internal InspectorForm(DhtWrapper wrapper)
        {
            this.m_Dht = wrapper;

            InitializeComponent();
        }

        private void c_RefreshTimer_Tick(object sender, EventArgs e)
        {
            this.c_ContactsGroupBox.Text = "Contacts (" + this.m_Dht.Count() + ")";
            this.c_ContactsListBox.Items.Clear();
            foreach (Contact c in this.m_Dht.ToArray())
                this.c_ContactsListBox.Items.Add(c);
            this.c_NamedEntriesGroupBox.Text = "Named Entries (" + this.m_Dht.Dht.OwnedEntries.Count + ")";
            this.c_NamedEntriesListBox.Items.Clear();
            foreach (Entry en in this.m_Dht.Dht.OwnedEntries.ToArray())
                this.c_NamedEntriesListBox.Items.Add(en.Key);
        }

        public void Log(string message, string type)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                    {
                        this.c_LogListView.Items.Add(new ListViewItem(message, type));
                        this.c_LogListView.TopItem = this.c_LogListView.Items[this.c_LogListView.Items.Count - 1];
                    }));
            }
        }

        private void c_LogListView_Resize(object sender, EventArgs e)
        {
            this.c_LogListView.Columns[0].Width = this.Width - 60;
        }
    }
}
