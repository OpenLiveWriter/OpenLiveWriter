// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Accounts
{
    /// <summary>
    /// Summary description for WeblogAccountListView.
    /// </summary>
    public class WeblogAccountListView : ListView
    {
        private IContainer components;
        private ArrayList _blogIcons = new ArrayList();

        public WeblogAccountListView(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            Initialize();
        }

        public WeblogAccountListView()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            Columns[0].Text = Res.Get(StringId.Name);
            if (!DesignMode)
                LoadData();
        }

        public BlogSettings SelectedWeblog
        {
            get
            {
                if (SelectedItems.Count > 0)
                    return SelectedItems[0].Tag as BlogSettings;
                else
                    return null;
            }
        }

        public ListViewItem AddWeblogItem(BlogSettings settings)
        {
            // create a new list view item
            ListViewItem listViewItem = new ListViewItem();

            // populate it from the published folder
            EditWeblogItem(listViewItem, settings);

            // add the item to the list and return it
            Items.Add(listViewItem);
            return listViewItem;
        }

        public void EditWeblogItem(ListViewItem item, BlogSettings settings)
        {
            // Set tag (back-reference to profile)
            item.Tag = settings;

            // set text
            item.Text = String.Format(CultureInfo.CurrentCulture, " {0}", settings.BlogName);
        }

        public void DeleteSelectedWeblog()
        {
            // delete the underlying profile
            SelectedWeblog.Delete();

            // dispose the settings object
            BlogSettings settings = SelectedItems[0].Tag as BlogSettings;
            settings.Dispose();

            // remove it from the list
            Items.Remove(SelectedItems[0]);

            // reselect the top-item (if one exists)
            if (TopItem != null)
            {
                TopItem.Selected = true;
                TopItem.Focused = true;
            }
        }

        private void LoadData()
        {
            // take note of which profile is the default
            string defaultBlogId = BlogSettings.DefaultBlogId;

            BeginUpdate();

            Items.Clear();

            // load the profiles
            foreach (BlogDescriptor blog in BlogSettings.GetBlogs(true))
                AddWeblogItem(BlogSettings.ForBlogId(blog.Id));

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
                try
                {
                    // dispose weblog profiles
                    foreach (ListViewItem item in Items)
                    {
                        BlogSettings settings = item.Tag as BlogSettings;
                        if (settings != null)
                            settings.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception disposing BlogSettings: " + ex.ToString());
                }

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
            // WeblogAccountListView
            //
            this.AutoArrange = false;
            this.FullRowSelect = true;
            this.HideSelection = false;
            this.LabelWrap = false;
            this.MultiSelect = false;
            this.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.View = System.Windows.Forms.View.Details;
            this.Columns.Add(new ColumnHeader());
            this.Columns[0].Text = "Name";
            this.Columns[0].Width = 240;
            this.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }
        #endregion

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)
            {
                if (Columns.Count == 1)
                    Columns[0].Width = Width - 8;
            }
        }

    }
}
