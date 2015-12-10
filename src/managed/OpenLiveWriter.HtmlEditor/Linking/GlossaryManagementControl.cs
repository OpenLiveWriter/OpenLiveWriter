// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for GlossaryManagementControl.
    /// </summary>
    public class GlossaryManagementControl : UserControl
    {
        private GlossaryListView listViewGlossary;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonDelete;
        private System.ComponentModel.IContainer components = new Container();

        public GlossaryManagementControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            buttonAdd.Text = Res.Get(StringId.AddButton);
            buttonEdit.Text = Res.Get(StringId.EditButton);
            buttonDelete.Text = Res.Get(StringId.DeleteButton);

            listViewGlossary.ListViewItemSorter = new AlphabeticalComparer();

            UpdateButtonEnabledStates();
        }

        private class AlphabeticalComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x is ListViewItem && y is ListViewItem)
                    return ((ListViewItem)x).Text.CompareTo(((ListViewItem)y).Text);
                return -1;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int gap = buttonAdd.Left - listViewGlossary.Right;
            LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Right, buttonDelete.Width, int.MaxValue,
                buttonAdd, buttonEdit, buttonDelete);
            listViewGlossary.Width = buttonAdd.Left - gap - listViewGlossary.Left;
        }

        private void ListViewGlossary_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabledStates();
        }

        private void UpdateButtonEnabledStates()
        {
            // manage availability of buttons
            bool itemSelected = listViewGlossary.SelectedItems.Count > 0;
            buttonEdit.Enabled = itemSelected;
            buttonDelete.Enabled = itemSelected;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                using (new WaitCursor())
                {
                    GlossaryLinkItem newEntry = GlossaryManager.Instance.AddFromForm(FindForm());
                    if (newEntry != null)
                    {
                        listViewGlossary.CheckForDelete(newEntry.Text, null);
                        ListViewItem item = listViewGlossary.AddGlossaryItem(newEntry);

                        // select the item that was added
                        listViewGlossary.SelectedItems.Clear();
                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();

                        // set focus to the list
                        listViewGlossary.Focus();
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            EditSelectedEntry();
        }

        private void ListViewGlossary_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedEntry();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedEntry();
        }

        /// <summary>
        /// Process keyboard accelerators
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="keyData">key data</param>
        /// <returns>true if processed</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // delete key deletes items
            if (keyData == Keys.Delete)
            {
                DeleteSelectedEntry();
                return true;
            }
            else // delegate to base
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewGlossary = new OpenLiveWriter.HtmlEditor.Linking.GlossaryListView(this.components, 248);
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listViewGlossary
            //
            this.listViewGlossary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewGlossary.FullRowSelect = true;
            this.listViewGlossary.HideSelection = false;
            this.listViewGlossary.LabelWrap = false;
            this.listViewGlossary.Location = new System.Drawing.Point(0, 0);
            this.listViewGlossary.MultiSelect = false;
            this.listViewGlossary.Name = "listViewGlossary";
            this.listViewGlossary.Size = new System.Drawing.Size(254, 245);
            this.listViewGlossary.TabIndex = 0;
            this.listViewGlossary.View = System.Windows.Forms.View.Details;
            this.listViewGlossary.DoubleClick += new System.EventHandler(this.ListViewGlossary_DoubleClick);
            this.listViewGlossary.SelectedIndexChanged += new System.EventHandler(this.ListViewGlossary_SelectedIndexChanged);
            this.listViewGlossary.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            //
            // buttonAdd
            //
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(261, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(84, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "&Add...";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // buttonEdit
            //
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEdit.Location = new System.Drawing.Point(261, 30);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(84, 23);
            this.buttonEdit.TabIndex = 2;
            this.buttonEdit.Text = "&Edit...";
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDelete.Location = new System.Drawing.Point(261, 60);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(84, 23);
            this.buttonDelete.TabIndex = 3;
            this.buttonDelete.Text = "&Delete...";
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            //
            // GlossaryManagementControl
            //
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listViewGlossary);
            this.Name = "GlossaryManagementControl";
            this.Size = new System.Drawing.Size(345, 245);
            this.ResumeLayout(false);

        }
        #endregion

        private void EditSelectedEntry()
        {
            if (SelectedEntry == null)
                return;

            try
            {
                using (new WaitCursor())
                {
                    GlossaryLinkItem revisedEntry = GlossaryManager.Instance.EditEntry(FindForm(), SelectedEntry.Text);

                    if (revisedEntry != null)
                    {
                        listViewGlossary.CheckForDelete(revisedEntry.Text, listViewGlossary.SelectedItems[0]);
                        // refresh contents of list-view item
                        listViewGlossary.PopulateListItem(listViewGlossary.SelectedItems[0], revisedEntry);
                    }

                    // set focus to the list
                    listViewGlossary.Focus();
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }

        }

        private void DeleteSelectedEntry()
        {
            if (SelectedEntry == null)
                return;

            try
            {
                if (DisplayMessage.Show(MessageId.ConfirmDeleteEntry) == DialogResult.Yes)
                {

                    GlossaryManager.Instance.DeleteEntry(SelectedEntry.Text);

                    // delete the entry
                    listViewGlossary.DeleteSelectedEntry();

                    // set focus to the list
                    listViewGlossary.Focus();

                    // update button enabled states
                    UpdateButtonEnabledStates();
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        private GlossaryLinkItem SelectedEntry
        {
            get
            {
                if (listViewGlossary.SelectedItems.Count == 0)
                    return null;
                return (GlossaryLinkItem)listViewGlossary.SelectedItems[0].Tag;
            }
        }

    }
}
