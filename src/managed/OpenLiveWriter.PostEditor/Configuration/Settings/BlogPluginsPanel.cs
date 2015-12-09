// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    public partial class BlogPluginsPanel : WeblogSettingsPanel
    {
        public BlogPluginsPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings) : base(targetBlogSettings, editableBlogSettings)
        {
            InitializeComponent();

            button1.FlatStyle = FlatStyle.System;
            button2.FlatStyle = FlatStyle.System;

            PanelName = Res.Get(StringId.PluginPrefName);
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PluginsSmall.png");
            lblDescription.Text = Res.Get(StringId.BlogPluginsDescription);
            colName.Text = Res.Get(StringId.Name);
            button1.Text = Res.Get(StringId.MoveUp);
            button2.Text = Res.Get(StringId.MoveDown);
            listViewPlugins.RightToLeftLayout = true;
            UpdatePluginList();
        }

        public BlogPluginsPanel()
        {
            Debug.Assert(DesignMode);
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.NaturalizeHeight(lblDescription);
            LayoutHelper.FitControlsBelow(8, lblDescription);
            int origButtonWidth = button1.Width;
            LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Right, origButtonWidth, int.MaxValue, button1, button2);
            int deltaX = button1.Width - origButtonWidth;
            listViewPlugins.Width -= deltaX;

            listViewPlugins.ItemChecked += listViewPlugins_ItemChecked;
        }

        private void UpdatePluginList()
        {
            BlogPublishingPluginSettings settings = TemporaryBlogSettings.PublishingPluginSettings;

            List<ContentSourceInfo> plugins = new List<ContentSourceInfo>(
                Join(ContentSourceManager.EnabledPublishNotificationPlugins, ContentSourceManager.EnabledHeaderFooterPlugins));
            plugins.Sort(ContentSourceManager.CreateComparison(settings));

            listViewPlugins.BeginUpdate();
            try
            {
                listViewPlugins.Items.Clear();
                imgListPlugins.Images.Clear();

                foreach (ContentSourceInfo csi in plugins)
                {
                    imgListPlugins.Images.Add(BidiHelper.Mirror(csi.Image));
                    ListViewItem item = new ListViewItem();
                    item.Tag = csi;
                    item.Text = " " + csi.Name;
                    item.ImageIndex = imgListPlugins.Images.Count - 1;
                    item.Checked = settings.IsEnabled(csi.Id) ?? false;
                    listViewPlugins.Items.Add(item);
                }

                if (listViewPlugins.Items.Count > 0)
                    listViewPlugins.Items[0].Selected = true;
            }
            finally
            {
                listViewPlugins.EndUpdate();
            }

            ManageMoveButtons();
        }

        private static IEnumerable<TElement> Join<TElement>(params IEnumerable<TElement>[] enumerables)
        {
            foreach (IEnumerable<TElement> enumerable in enumerables)
            {
                foreach (TElement element in enumerable)
                    yield return element;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MoveSelected(-1);
            button1.Select();
            UpdateSettings();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MoveSelected(1);
            button2.Select();
            UpdateSettings();
        }

        private void MoveSelected(int offset)
        {
            if (listViewPlugins.SelectedIndices.Count != 1)
                return;

            int selected = listViewPlugins.SelectedIndices[0];
            int dest = selected + offset;
            if (dest < 0 || dest >= listViewPlugins.Items.Count)
                return;

            ListViewItem item = listViewPlugins.Items[selected];
            listViewPlugins.BeginUpdate();
            try
            {
                listViewPlugins.Items.RemoveAt(selected);
                listViewPlugins.Items.Insert(dest, item);
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
            }
            finally
            {
                listViewPlugins.EndUpdate();
            }
        }

        private void listViewPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            ManageMoveButtons();
        }

        private void ManageMoveButtons()
        {
            if (listViewPlugins.SelectedIndices.Count != 1)
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                int idx = listViewPlugins.SelectedIndices[0];
                button1.Enabled = idx > 0;
                button2.Enabled = idx < listViewPlugins.Items.Count - 1;
            }
        }

        private void listViewPlugins_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            TemporaryBlogSettingsModified = true;
        }

        public override void Save()
        {
            BlogPublishingPluginSettings settings = TemporaryBlogSettings.PublishingPluginSettings;
            settings.ClearOrder();
            int i = 0;
            foreach (ListViewItem lvi in listViewPlugins.Items)
            {
                ContentSourceInfo csi = (ContentSourceInfo)lvi.Tag;
                settings.Set(csi.Id, lvi.Checked, i++);
            }

            base.Save();
        }

    }
}
