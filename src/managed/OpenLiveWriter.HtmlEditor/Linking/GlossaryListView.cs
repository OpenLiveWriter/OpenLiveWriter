// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for GlossaryListView.
    /// </summary>
    public class GlossaryListView : ListView
    {
        private IContainer components;
        private int totalWidth;

        public GlossaryListView(IContainer container, int width)
        {
            totalWidth = width;
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            Debug.Assert(Columns[0].Text == Res.Get(StringId.GlossaryTextCol));
            Debug.Assert(Columns[1].Text == Res.Get(StringId.GlossaryUrlCol));

            Debug.Assert(Columns[1].Width == totalWidth - Columns[0].Width - 8);

            Initialize();
        }

        public GlossaryListView()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            // These resourced strings need to be set in InitializeComponent, because
            // for some reason the ListView doesn't respond to changes to the column
            // header text once it's set. If the VS.NET designer pounds over the
            // Res.Get calls in InitializeComponent, these asserts should catch that
            // in localized configs.
            Debug.Assert(Columns[0].Text == Res.Get(StringId.GlossaryTextCol));
            Debug.Assert(Columns[1].Text == Res.Get(StringId.GlossaryUrlCol));
            Debug.Assert(Columns[1].Width == totalWidth - Columns[0].Width - 8);

            Initialize();
        }

        private void Initialize()
        {
            if (!DesignMode)
                LoadData();
        }

        public void CheckForDelete(string repeat, ListViewItem orig)
        {
            ListViewItem found = Find(repeat, orig);
            if (found != null)
            {
                found.Remove();
            }
        }

        public ListViewItem Find(string itemText, ListViewItem orig)
        {
            foreach (ListViewItem x in Items)
            {
                if (x != orig)
                {
                    if (0 == String.Compare(x.Text, itemText, true, CultureInfo.InvariantCulture))
                    {
                        return x;
                    }
                }
            }
            return null;
        }

        public ListViewItem AddGlossaryItem(GlossaryLinkItem entry)
        {
            // create a new list view item
            ListViewItem listViewItem = new ListViewItem();

            PopulateListItem(listViewItem, entry);

            // add the item to the list and return it
            Items.Add(listViewItem);
            return listViewItem;
        }

        public void PopulateListItem(ListViewItem item, GlossaryLinkItem entry)
        {
            // Set tag (back-reference to profile)
            item.Tag = entry;
            item.SubItems.Clear();

            // set text
            item.Text = String.Format(CultureInfo.InvariantCulture, "{0}", entry.Text);
            item.SubItems.Add(String.Format(CultureInfo.InvariantCulture, "{0}", entry.Url));
        }

        public void DeleteSelectedEntry()
        {
            int selectedIndex = 0;
            if (SelectedIndices.Count == 1)
                selectedIndex = SelectedIndices[0];

            // remove it from the list
            Items.Remove(SelectedItems[0]);

            int previousItemIndex = Math.Max(0, selectedIndex - 1);
            if (Items.Count > previousItemIndex)
            {
                Items[previousItemIndex].Selected = true;
                Items[previousItemIndex].Focused = true;
            }
        }

        private void LoadData()
        {
            BeginUpdate();

            Items.Clear();

            // load the profiles
            foreach (GlossaryLinkItem item in GlossaryManager.Instance.GetItems())
                AddGlossaryItem(item);

            // select the first item in the list
            if (Items.Count > 0)
                Items[0].Selected = true;

            EndUpdate();
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
            //
            // GlossaryListView
            //
            this.AutoArrange = false;
            this.FullRowSelect = true;
            this.HideSelection = false;
            this.LabelWrap = false;
            this.MultiSelect = false;
            this.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.View = System.Windows.Forms.View.Details;
            this.Columns.Add(new ColumnHeader());
            Columns[0].Text = Res.Get(StringId.GlossaryTextCol);
            this.Columns[0].Width = 110;
            this.Columns.Add(new ColumnHeader());
            Columns[1].Text = Res.Get(StringId.GlossaryUrlCol);
            this.Columns[1].Width = totalWidth - this.Columns[0].Width - 8;
        }
        #endregion
    }
}
