// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor
{

    public class PluginsPreferencesPanel : PreferencesPanel
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label labelInstalledPlugins;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Panel panelPluginDetails;
        private System.Windows.Forms.ListView listViewInstalledPlugins;
        private System.Windows.Forms.GroupBox groupBoxPluginDetails;
        private System.Windows.Forms.PictureBox pictureBoxPluginImage;
        private System.Windows.Forms.RadioButton radioButtonEnablePlugin;
        private System.Windows.Forms.RadioButton radioButtonDisablePlugin;
        private System.Windows.Forms.Label labelPluginDescription;
        private System.Windows.Forms.ImageList imageListPlugins;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Label labelNoPluginSelected;
        private System.Windows.Forms.LinkLabel linkLabelPluginName;
        private ToolTip2 toolTip;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.LinkLabel linkLabelDownloadPlugins;
        private System.Windows.Forms.PictureBox pictureBoxAddPlugin;

        private PluginsPreferences _pluginsPreferences;

        public PluginsPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            columnHeaderName.Text = Res.Get(StringId.PluginPrefNameCol);
            columnHeaderStatus.Text = Res.Get(StringId.PluginPrefStatCol);
            labelInstalledPlugins.Text = Res.Get(StringId.PluginPrefInstalled);
            groupBoxPluginDetails.Text = Res.Get(StringId.PluginPrefDetails);
            buttonOptions.Text = Res.Get(StringId.OptionsButton);
            toolTip.SetToolTip(this.linkLabelPluginName, Res.Get(StringId.PluginPrefTooltip));
            labelPluginDescription.Text = "";
            radioButtonDisablePlugin.Text = Res.Get(StringId.PluginPrefDisable);
            radioButtonEnablePlugin.Text = Res.Get(StringId.PluginPrefEnable);
            labelNoPluginSelected.Text = Res.Get(StringId.PluginPrefNone);
            linkLabelDownloadPlugins.Text = Res.Get(StringId.PluginPrefLink);
            linkLabelDownloadPlugins.UseCompatibleTextRendering = false;
            labelCaption.Text = Res.Get(StringId.PluginPrefCaption);
            PanelName = Res.Get(StringId.PluginPrefName);

            //marketization
            if (!MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.WLGallery))
                linkLabelDownloadPlugins.Visible = false;
            else
            {
                pictureBoxAddPlugin.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.AddPlugin.png");
            }
            // set our bitmaps
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PluginsSmall.png");

            // paramaterize caption with product name
            labelCaption.Text = String.Format(CultureInfo.CurrentCulture, labelCaption.Text, ApplicationEnvironment.ProductName);

            // initialize preferences
            _pluginsPreferences = new PluginsPreferences();
            _pluginsPreferences.PreferencesModified += new EventHandler(_pluginsPreferences_PreferencesModified);

            // signup for events
            listViewInstalledPlugins.SelectedIndexChanged += new EventHandler(listViewInstalledPlugins_SelectedIndexChanged);
            radioButtonEnablePlugin.CheckedChanged += new EventHandler(radioButtonEnablePlugin_CheckedChanged);
            radioButtonDisablePlugin.CheckedChanged += new EventHandler(radioButtonEnablePlugin_CheckedChanged);
            linkLabelPluginName.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelPluginName_LinkClicked);
            linkLabelDownloadPlugins.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelDownloadPlugins_LinkClicked);

            // update list of plugins
            UpdatePluginList();

            // signup for global plugin-list changed event
            ContentSourceManager.GlobalContentSourceListChanged += new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, Controls);
                linkLabelDownloadPlugins.Top = pictureBoxAddPlugin.Top + 1;
                LayoutHelper.NaturalizeHeight(linkLabelPluginName);
                DisplayHelper.AutoFitSystemRadioButton(radioButtonEnablePlugin, 0, int.MaxValue);
                DisplayHelper.AutoFitSystemRadioButton(radioButtonDisablePlugin, 0, int.MaxValue);
                radioButtonEnablePlugin.BringToFront();
                DisplayHelper.AutoFitSystemButton(buttonOptions);
            }
        }

        private void ContentSourceManager_GlobalContentSourceListChanged(object sender, EventArgs e)
        {
            // post back to ourselves if necessary
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(ContentSourceManager_GlobalContentSourceListChanged), new object[] { sender, e });
                return;
            }

            // update plugin list
            UpdatePluginList();
        }

        private void UpdatePluginList()
        {
            // clear existing list
            listViewInstalledPlugins.Items.Clear();
            imageListPlugins.Images.Clear();

            // re-initialize list
            int imageIndex = 0;
            ContentSourceInfo[] pluginContentSources = ContentSourceManager.PluginContentSources;
            foreach (ContentSourceInfo pluginContentSource in pluginContentSources)
            {
                imageListPlugins.Images.Add(BidiHelper.Mirror(pluginContentSource.Image));
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Tag = pluginContentSource;
                listViewItem.ImageIndex = imageIndex++;
                RefreshListViewItem(listViewItem);
                listViewInstalledPlugins.Items.Add(listViewItem);
            }
            // select first item if possible
            if (listViewInstalledPlugins.Items.Count > 0)
                listViewInstalledPlugins.Items[0].Selected = true;

            // update the details pane
            UpdateDetailsPane();
        }

        public override void Save()
        {
            if (_pluginsPreferences.IsModified())
            {
                _pluginsPreferences.Save();

                DisplayMessage.Show(MessageId.PluginStatusChanged, this);
            }
        }

        private void _pluginsPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        private void listViewInstalledPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDetailsPane();
        }

        private void radioButtonEnablePlugin_CheckedChanged(object sender, EventArgs e)
        {
            ContentSourceInfo selectedContentSource = GetSelectedPlugin();
            if (selectedContentSource != null)
            {
                // if our underlying state has changed then update underlying prefs
                if (_pluginsPreferences.GetPluginEnabledState(selectedContentSource) != radioButtonEnablePlugin.Checked)
                {
                    _pluginsPreferences.SetPluginEnabledState(selectedContentSource, radioButtonEnablePlugin.Checked);
                    RefreshSelectedListViewItem();
                }
            }
        }

        private void linkLabelPluginName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ContentSourceInfo selectedContentSource = GetSelectedPlugin();
            if (selectedContentSource != null)
            {
                if (selectedContentSource.WriterPluginPublisherUrl != String.Empty)
                    ShellHelper.LaunchUrl(selectedContentSource.WriterPluginPublisherUrl);
            }
        }

        private void linkLabelDownloadPlugins_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.DownloadPlugins);
        }

        private void buttonOptions_Click(object sender, System.EventArgs e)
        {
            ContentSourceInfo selectedContentSource = GetSelectedPlugin();
            if (selectedContentSource != null)
            {
                if (selectedContentSource.WriterPluginHasEditableOptions)
                {
                    try
                    {
                        selectedContentSource.Instance.EditOptions(FindForm());
                    }
                    catch (NotImplementedException ex)
                    {
                        ContentSourceManager.DisplayContentRetreivalError(FindForm(), ex, selectedContentSource);
                    }
                    catch (Exception exception)
                    {
                        Trace.Fail(exception.ToString());
                        DisplayableException dex = new DisplayableException(
                            Res.Get(StringId.UnexpectedErrorPluginTitle),
                            string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UnexpectedErrorPluginDescription), selectedContentSource.Name, exception.Message));
                        DisplayableExceptionDisplayForm.Show(FindForm(), dex);
                    }
                }
            }
        }

        private void RefreshSelectedListViewItem()
        {
            if (listViewInstalledPlugins.SelectedItems.Count > 0)
                RefreshListViewItem(listViewInstalledPlugins.SelectedItems[0]);
        }

        private void RefreshListViewItem(ListViewItem listViewItem)
        {
            ContentSourceInfo itemContentSource = listViewItem.Tag as ContentSourceInfo;
            listViewItem.SubItems.Clear();
            listViewItem.Text = " " + itemContentSource.Name;
            listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem, _pluginsPreferences.GetPluginEnabledState(itemContentSource) ? Res.Get(StringId.Enabled) : Res.Get(StringId.Disabled)));
        }

        private void UpdateDetailsPane()
        {
            ContentSourceInfo selectedContentSource = GetSelectedPlugin();
            if (selectedContentSource != null)
            {
                panelPluginDetails.Visible = true;
                labelNoPluginSelected.Visible = false;

                pictureBoxPluginImage.Image = selectedContentSource.Image;
                linkLabelPluginName.Text = selectedContentSource.Name;
                labelPluginDescription.Text = selectedContentSource.WriterPluginDescription;
                radioButtonEnablePlugin.Checked = selectedContentSource.Enabled;
                radioButtonDisablePlugin.Checked = !selectedContentSource.Enabled;

                buttonOptions.Visible = selectedContentSource.WriterPluginHasEditableOptions;
            }
            else
            {
                labelNoPluginSelected.Visible = true;
                panelPluginDetails.Visible = false;
            }

        }

        private ContentSourceInfo GetSelectedPlugin()
        {
            if (listViewInstalledPlugins.SelectedItems.Count > 0)
                return listViewInstalledPlugins.SelectedItems[0].Tag as ContentSourceInfo;
            else
                return null;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ContentSourceManager.GlobalContentSourceListChanged -= new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);
                _pluginsPreferences.PreferencesModified -= new EventHandler(_pluginsPreferences_PreferencesModified);

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PluginsPreferencesPanel));
            this.listViewInstalledPlugins = new System.Windows.Forms.ListView();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
            this.imageListPlugins = new System.Windows.Forms.ImageList(this.components);
            this.labelInstalledPlugins = new System.Windows.Forms.Label();
            this.groupBoxPluginDetails = new System.Windows.Forms.GroupBox();
            this.panelPluginDetails = new System.Windows.Forms.Panel();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.linkLabelPluginName = new System.Windows.Forms.LinkLabel();
            this.labelPluginDescription = new System.Windows.Forms.Label();
            this.radioButtonDisablePlugin = new System.Windows.Forms.RadioButton();
            this.radioButtonEnablePlugin = new System.Windows.Forms.RadioButton();
            this.pictureBoxPluginImage = new System.Windows.Forms.PictureBox();
            this.labelNoPluginSelected = new System.Windows.Forms.Label();
            this.labelCaption = new System.Windows.Forms.Label();
            this.toolTip = new ToolTip2(this.components);
            this.linkLabelDownloadPlugins = new System.Windows.Forms.LinkLabel();
            this.pictureBoxAddPlugin = new System.Windows.Forms.PictureBox();
            this.groupBoxPluginDetails.SuspendLayout();
            this.panelPluginDetails.SuspendLayout();
            this.SuspendLayout();
            //
            // listViewInstalledPlugins
            //
            this.listViewInstalledPlugins.AutoArrange = false;
            this.listViewInstalledPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                       this.columnHeaderName,
                                                                                                       this.columnHeaderStatus});
            this.listViewInstalledPlugins.FullRowSelect = true;
            this.listViewInstalledPlugins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInstalledPlugins.HideSelection = false;
            this.listViewInstalledPlugins.Location = new System.Drawing.Point(8, 83);
            this.listViewInstalledPlugins.MultiSelect = false;
            this.listViewInstalledPlugins.Name = "listViewInstalledPlugins";
            this.listViewInstalledPlugins.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewInstalledPlugins.Size = new System.Drawing.Size(348, 148);
            this.listViewInstalledPlugins.SmallImageList = this.imageListPlugins;
            this.listViewInstalledPlugins.TabIndex = 3;
            this.listViewInstalledPlugins.View = System.Windows.Forms.View.Details;
            //
            // columnHeaderName
            //
            this.columnHeaderName.Text = "Plugin";
            this.columnHeaderName.Width = 229;
            //
            // columnHeaderStatus
            //
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 95;
            //
            // imageListPlugins
            //
            this.imageListPlugins.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListPlugins.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListPlugins.TransparentColor = System.Drawing.Color.Transparent;
            //
            // labelInstalledPlugins
            //
            this.labelInstalledPlugins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelInstalledPlugins.Location = new System.Drawing.Point(8, 66);
            this.labelInstalledPlugins.Name = "labelInstalledPlugins";
            this.labelInstalledPlugins.Size = new System.Drawing.Size(341, 15);
            this.labelInstalledPlugins.TabIndex = 2;
            this.labelInstalledPlugins.Text = "&Plugins currently installed:";
            //
            // groupBoxPluginDetails
            //
            this.groupBoxPluginDetails.Controls.Add(this.panelPluginDetails);
            this.groupBoxPluginDetails.Controls.Add(this.labelNoPluginSelected);
            this.groupBoxPluginDetails.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPluginDetails.Location = new System.Drawing.Point(8, 237);
            this.groupBoxPluginDetails.Name = "groupBoxPluginDetails";
            this.groupBoxPluginDetails.Size = new System.Drawing.Size(349, 149);
            this.groupBoxPluginDetails.TabIndex = 4;
            this.groupBoxPluginDetails.TabStop = false;
            this.groupBoxPluginDetails.Text = "Plugin details";
            //
            // panelPluginDetails
            //
            this.panelPluginDetails.Controls.Add(this.buttonOptions);
            this.panelPluginDetails.Controls.Add(this.linkLabelPluginName);
            this.panelPluginDetails.Controls.Add(this.labelPluginDescription);
            this.panelPluginDetails.Controls.Add(this.radioButtonDisablePlugin);
            this.panelPluginDetails.Controls.Add(this.radioButtonEnablePlugin);
            this.panelPluginDetails.Controls.Add(this.pictureBoxPluginImage);
            this.panelPluginDetails.Location = new System.Drawing.Point(7, 18);
            this.panelPluginDetails.Name = "panelPluginDetails";
            this.panelPluginDetails.Size = new System.Drawing.Size(339, 123);
            this.panelPluginDetails.TabIndex = 0;
            //
            // buttonOptions
            //
            this.buttonOptions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.buttonOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOptions.Location = new System.Drawing.Point(250, 61);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(83, 23);
            this.buttonOptions.TabIndex = 4;
            this.buttonOptions.Text = "Options...";
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            //
            // linkLabelPluginName
            //
            this.linkLabelPluginName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelPluginName.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelPluginName.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabelPluginName.Location = new System.Drawing.Point(21, 2);
            this.linkLabelPluginName.Name = "linkLabelPluginName";
            this.linkLabelPluginName.Size = new System.Drawing.Size(222, 16);
            this.linkLabelPluginName.TabIndex = 0;
            this.linkLabelPluginName.TabStop = true;
            this.linkLabelPluginName.Text = "YouTube Video Publisher";
            this.toolTip.SetToolTip(this.linkLabelPluginName, "Click here to find out more about this plugin.");
            //
            // labelPluginDescription
            //
            this.labelPluginDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPluginDescription.Location = new System.Drawing.Point(2, 21);
            this.labelPluginDescription.Name = "labelPluginDescription";
            this.labelPluginDescription.Size = new System.Drawing.Size(241, 100);
            this.labelPluginDescription.TabIndex = 1;
            this.labelPluginDescription.Text = "Publish videos to your weblog from YouTube, the leading free online video streami" +
                "ng service.";
            //
            // radioButtonDisablePlugin
            //
            this.radioButtonDisablePlugin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonDisablePlugin.Location = new System.Drawing.Point(254, 30);
            this.radioButtonDisablePlugin.Name = "radioButtonDisablePlugin";
            this.radioButtonDisablePlugin.Size = new System.Drawing.Size(84, 24);
            this.radioButtonDisablePlugin.TabIndex = 3;
            this.radioButtonDisablePlugin.Text = "&Disable";
            //
            // radioButtonEnablePlugin
            //
            this.radioButtonEnablePlugin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonEnablePlugin.Location = new System.Drawing.Point(254, 10);
            this.radioButtonEnablePlugin.Name = "radioButtonEnablePlugin";
            this.radioButtonEnablePlugin.Size = new System.Drawing.Size(84, 24);
            this.radioButtonEnablePlugin.TabIndex = 2;
            this.radioButtonEnablePlugin.Text = "&Enable";
            //
            // pictureBoxPluginImage
            //
            this.pictureBoxPluginImage.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxPluginImage.Image")));
            this.pictureBoxPluginImage.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxPluginImage.Name = "pictureBoxPluginImage";
            this.pictureBoxPluginImage.Size = new System.Drawing.Size(20, 18);
            this.pictureBoxPluginImage.TabIndex = 0;
            this.pictureBoxPluginImage.TabStop = false;
            //
            // labelNoPluginSelected
            //
            this.labelNoPluginSelected.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelNoPluginSelected.Location = new System.Drawing.Point(16, 43);
            this.labelNoPluginSelected.Name = "labelNoPluginSelected";
            this.labelNoPluginSelected.Size = new System.Drawing.Size(316, 23);
            this.labelNoPluginSelected.TabIndex = 1;
            this.labelNoPluginSelected.Text = "(No Plugin selected)";
            this.labelNoPluginSelected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Location = new System.Drawing.Point(8, 32);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(341, 32);
            this.labelCaption.TabIndex = 1;
            this.labelCaption.Text = "Plugins are programs that extend the functionality of {0}. You can enable and dis" +
                "able Plugins using this dialog.";
            //
            // linkLabelDownloadPlugins
            //
            this.linkLabelDownloadPlugins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelDownloadPlugins.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelDownloadPlugins.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabelDownloadPlugins.Location = new System.Drawing.Point(32, 394);
            this.linkLabelDownloadPlugins.Name = "linkLabelDownloadPlugins";
            this.linkLabelDownloadPlugins.Size = new System.Drawing.Size(316, 15);
            this.linkLabelDownloadPlugins.TabIndex = 5;
            this.linkLabelDownloadPlugins.TabStop = true;
            this.linkLabelDownloadPlugins.Text = "Add a Plugin...";
            this.linkLabelDownloadPlugins.AutoSize = true;
            //
            // pictureBoxAddPlugin
            //
            this.pictureBoxAddPlugin.Location = new System.Drawing.Point(13, 393);
            this.pictureBoxAddPlugin.Name = "pictureBoxAddPlugin";
            this.pictureBoxAddPlugin.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxAddPlugin.TabIndex = 6;
            this.pictureBoxAddPlugin.TabStop = false;
            //
            // PluginsPreferencesPanel
            //
            this.Controls.Add(this.pictureBoxAddPlugin);
            this.Controls.Add(this.labelCaption);
            this.Controls.Add(this.linkLabelDownloadPlugins);
            this.Controls.Add(this.groupBoxPluginDetails);
            this.Controls.Add(this.labelInstalledPlugins);
            this.Controls.Add(this.listViewInstalledPlugins);
            this.Name = "PluginsPreferencesPanel";
            this.PanelName = "Plugins";
            this.Size = new System.Drawing.Size(370, 428);
            this.Controls.SetChildIndex(this.listViewInstalledPlugins, 0);
            this.Controls.SetChildIndex(this.labelInstalledPlugins, 0);
            this.Controls.SetChildIndex(this.groupBoxPluginDetails, 0);
            this.Controls.SetChildIndex(this.linkLabelDownloadPlugins, 0);
            this.Controls.SetChildIndex(this.labelCaption, 0);
            this.Controls.SetChildIndex(this.pictureBoxAddPlugin, 0);
            this.groupBoxPluginDetails.ResumeLayout(false);
            this.panelPluginDetails.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
