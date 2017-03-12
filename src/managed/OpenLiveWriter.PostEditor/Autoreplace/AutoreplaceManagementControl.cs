// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    public partial class AutoreplaceManagementControl : UserControl
    {
        private AutoreplacePreferences _preferences;

        public AutoreplaceManagementControl()
        {
            InitializeComponent();

            buttonAdd.Text = Res.Get(StringId.AddButton);
            buttonEdit.Text = Res.Get(StringId.EditButton);
            buttonDelete.Text = Res.Get(StringId.DeleteButton);

            listViewItems.ListViewItemSorter = new AutoreplaceComparer();
        }

        private class AutoreplaceComparer : IComparer
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
            listViewItems.Columns[0].Width = listViewItems.ClientSize.Width / 3;
            listViewItems.Columns[1].Width = listViewItems.ClientSize.Width - listViewItems.Columns[0].Width;
        }

        public AutoreplacePreferences Preferences
        {
            get
            {
                return _preferences;
            }
            set
            {
                _preferences = value;
                RefreshEntries();
                SelectFirstItem();
                SetButtonEnableState();
            }
        }

        private void RefreshEntries()
        {
            // TODO: Review text

            listViewItems.Items.Clear();
            if (_preferences != null)
            {
                foreach (AutoreplacePhrase phrase in _preferences.GetAutoreplacePhrases())
                {
                    ListViewItem item = GetItem(phrase.Phrase, phrase.ReplaceValue);
                    listViewItems.Items.Add(item);
                }
            }
            listViewItems.Sort();
        }

        private void SelectItem(string phrase)
        {
            foreach (ListViewItem item in listViewItems.Items)
            {
                if (item.Text == phrase)
                {
                    item.Selected = true;
                    listViewItems.EnsureVisible(item.Index);
                    return;
                }
            }
        }

        private void SelectFirstItem()
        {
            ListViewItem firstItem = listViewItems.GetItemAt(5, 5);
            if (firstItem != null)
                firstItem.Selected = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // delete key deletes items
            if (keyData == Keys.Delete)
            {
                DeleteSelectedItem();
                return true;
            }
            else // delegate to base
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (AutoreplaceEditForm form = new AutoreplaceEditForm(_preferences))
            {
                DialogResult result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    _preferences.SetAutoreplacePhrase(form.Phrase, form.ReplaceValue);
                    RefreshEntries();
                    SelectItem(form.Phrase);
                }
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            using (AutoreplaceEditForm form = new AutoreplaceEditForm(_preferences))
            {
                AutoreplacePhrase phrase = _preferences.GetPhrase(listViewItems.SelectedItems[0].Text);
                form.Phrase = phrase.Phrase;
                form.ReplaceValue = phrase.ReplaceValue;
                DialogResult result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    _preferences.SetAutoreplacePhrase(form.Phrase, form.ReplaceValue);
                    RefreshEntries();
                    SelectItem(form.Phrase);
                }

            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedItem();
        }

        private void DeleteSelectedItem()
        {
            DialogResult result = DisplayMessage.Show(MessageId.ConfirmDeleteAutoReplaceEntry);
            if (result == DialogResult.Yes)
            {
                int selectedIndex = 0;
                if (listViewItems.SelectedIndices.Count == 1)
                    selectedIndex = listViewItems.SelectedIndices[0];

                _preferences.RemoveAutoreplacePhrase(listViewItems.SelectedItems[0].Text);
                RefreshEntries();

                int previousItemIndex = Math.Max(0, selectedIndex - 1);
                if (listViewItems.Items.Count > previousItemIndex)
                    listViewItems.Items[previousItemIndex].Selected = true;
            }
        }

        public ListViewItem GetItem(string phrase, string replacement)
        {
            ListViewItem item = new ListViewItem(phrase);
            item.SubItems.Add(new ListViewItem.ListViewSubItem(item, replacement));
            return item;
        }

        private void listViewItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(listViewItems.SelectedIndices.Count < 2, "List view for auto replace should be single select only.");

            SetButtonEnableState();
        }

        private void SetButtonEnableState()
        {
            bool enabled = (listViewItems.SelectedIndices.Count == 1);
            buttonDelete.Enabled = buttonEdit.Enabled = enabled;
        }
    }

}
