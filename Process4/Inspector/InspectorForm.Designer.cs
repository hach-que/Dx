namespace Data4.Inspector
{
    partial class InspectorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Inspector was opened.", "requested");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectorForm));
            this.c_ContactsGroupBox = new System.Windows.Forms.GroupBox();
            this.c_ContactsListBox = new System.Windows.Forms.ListBox();
            this.c_NamedEntriesGroupBox = new System.Windows.Forms.GroupBox();
            this.c_NamedEntriesListBox = new System.Windows.Forms.ListBox();
            this.c_RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.c_LogListView = new System.Windows.Forms.ListView();
            this.c_IconImageList = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_ContactsGroupBox.SuspendLayout();
            this.c_NamedEntriesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // c_ContactsGroupBox
            // 
            this.c_ContactsGroupBox.Controls.Add(this.c_ContactsListBox);
            this.c_ContactsGroupBox.Location = new System.Drawing.Point(12, 12);
            this.c_ContactsGroupBox.Name = "c_ContactsGroupBox";
            this.c_ContactsGroupBox.Size = new System.Drawing.Size(472, 190);
            this.c_ContactsGroupBox.TabIndex = 0;
            this.c_ContactsGroupBox.TabStop = false;
            this.c_ContactsGroupBox.Text = "Contacts";
            // 
            // c_ContactsListBox
            // 
            this.c_ContactsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c_ContactsListBox.FormattingEnabled = true;
            this.c_ContactsListBox.IntegralHeight = false;
            this.c_ContactsListBox.Location = new System.Drawing.Point(3, 16);
            this.c_ContactsListBox.Name = "c_ContactsListBox";
            this.c_ContactsListBox.Size = new System.Drawing.Size(466, 171);
            this.c_ContactsListBox.TabIndex = 1;
            // 
            // c_NamedEntriesGroupBox
            // 
            this.c_NamedEntriesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c_NamedEntriesGroupBox.Controls.Add(this.c_NamedEntriesListBox);
            this.c_NamedEntriesGroupBox.Location = new System.Drawing.Point(490, 12);
            this.c_NamedEntriesGroupBox.Name = "c_NamedEntriesGroupBox";
            this.c_NamedEntriesGroupBox.Size = new System.Drawing.Size(612, 190);
            this.c_NamedEntriesGroupBox.TabIndex = 1;
            this.c_NamedEntriesGroupBox.TabStop = false;
            this.c_NamedEntriesGroupBox.Text = "Named Entries";
            // 
            // c_NamedEntriesListBox
            // 
            this.c_NamedEntriesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c_NamedEntriesListBox.FormattingEnabled = true;
            this.c_NamedEntriesListBox.IntegralHeight = false;
            this.c_NamedEntriesListBox.Location = new System.Drawing.Point(3, 16);
            this.c_NamedEntriesListBox.Name = "c_NamedEntriesListBox";
            this.c_NamedEntriesListBox.Size = new System.Drawing.Size(606, 171);
            this.c_NamedEntriesListBox.TabIndex = 0;
            // 
            // c_RefreshTimer
            // 
            this.c_RefreshTimer.Enabled = true;
            this.c_RefreshTimer.Tick += new System.EventHandler(this.c_RefreshTimer_Tick);
            // 
            // c_LogListView
            // 
            this.c_LogListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c_LogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.c_LogListView.FullRowSelect = true;
            this.c_LogListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.c_LogListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.c_LogListView.Location = new System.Drawing.Point(15, 208);
            this.c_LogListView.MultiSelect = false;
            this.c_LogListView.Name = "c_LogListView";
            this.c_LogListView.Size = new System.Drawing.Size(1087, 224);
            this.c_LogListView.SmallImageList = this.c_IconImageList;
            this.c_LogListView.TabIndex = 2;
            this.c_LogListView.UseCompatibleStateImageBehavior = false;
            this.c_LogListView.View = System.Windows.Forms.View.Details;
            this.c_LogListView.Resize += new System.EventHandler(this.c_LogListView_Resize);
            // 
            // c_IconImageList
            // 
            this.c_IconImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("c_IconImageList.ImageStream")));
            this.c_IconImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.c_IconImageList.Images.SetKeyName(0, "retrieved");
            this.c_IconImageList.Images.SetKeyName(1, "sent");
            this.c_IconImageList.Images.SetKeyName(2, "updated_cache");
            this.c_IconImageList.Images.SetKeyName(3, "requested");
            this.c_IconImageList.Images.SetKeyName(4, "retrieved_success");
            this.c_IconImageList.Images.SetKeyName(5, "retrieved_failure");
            this.c_IconImageList.Images.SetKeyName(6, "object_vanished");
            this.c_IconImageList.Images.SetKeyName(7, "contact");
            this.c_IconImageList.Images.SetKeyName(8, "object_new");
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 540;
            // 
            // InspectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1114, 444);
            this.Controls.Add(this.c_LogListView);
            this.Controls.Add(this.c_NamedEntriesGroupBox);
            this.Controls.Add(this.c_ContactsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "InspectorForm";
            this.Text = "Distributed Application Inspector";
            this.c_ContactsGroupBox.ResumeLayout(false);
            this.c_NamedEntriesGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox c_ContactsGroupBox;
        private System.Windows.Forms.ListBox c_ContactsListBox;
        private System.Windows.Forms.GroupBox c_NamedEntriesGroupBox;
        private System.Windows.Forms.ListBox c_NamedEntriesListBox;
        private System.Windows.Forms.Timer c_RefreshTimer;
        private System.Windows.Forms.ListView c_LogListView;
        private System.Windows.Forms.ImageList c_IconImageList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}

