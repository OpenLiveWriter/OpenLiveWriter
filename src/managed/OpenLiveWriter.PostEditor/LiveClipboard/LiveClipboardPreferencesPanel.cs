// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.LiveClipboard;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.LiveClipboard
{
    public class LiveClipboardPreferencesPanel : PreferencesPanel
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label labelInstalledPlugins;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Panel panelFormatDetails;
        private System.Windows.Forms.Label labelNoFormatSelected;
        private ToolTip2 toolTip;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.ListView listViewFormats;
        private System.Windows.Forms.ImageList imageListFormats;
        private System.Windows.Forms.LinkLabel linkLabelMoreAboutLiveClipboard;
        private System.Windows.Forms.GroupBox groupBoxFormatDetails;
        private System.Windows.Forms.PictureBox pictureBoxLiveClipboardIcon;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.Label labelHandledByCaption;
        private System.Windows.Forms.Label labelContentSourceName;
        private System.Windows.Forms.Label labelContentTypeCaption;
        private System.Windows.Forms.PictureBox pictureBoxContentSource;
        private System.Windows.Forms.Label labelContentType;

        private LiveClipboardPreferences _liveClipboardPreferences;

        public LiveClipboardPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.columnHeaderFormat.Text = Res.Get(StringId.LCPrefFormat);
            this.columnHeaderDescription.Text = Res.Get(StringId.LCPrefDescription);
            this.labelInstalledPlugins.Text = Res.Get(StringId.LCPrefSupportedFormats);
            this.labelHandledByCaption.Text = Res.Get(StringId.LCPrefHandledBy);
            this.buttonChange.Text = Res.Get(StringId.LCPrefChangeButton);
            this.buttonOptions.Text = Res.Get(StringId.LCPrefOptionsButton);
            this.labelContentTypeCaption.Text = Res.Get(StringId.LCPrefContentType);
            this.labelNoFormatSelected.Text = Res.Get(StringId.LCPrefNoFormatSelected);
            this.linkLabelMoreAboutLiveClipboard.Text = Res.Get(StringId.LCPrefMoreAboutLiveClipboard);
            this.labelCaption.Text = Res.Get(StringId.LCPrefCaption);
            this.PanelName = Res.Get(StringId.LCPrefPanelName);

            // set our bitmap
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("LiveClipboard.Images.LiveClipboardSmall.png", true);

            pictureBoxLiveClipboardIcon.Image = ResourceHelper.LoadAssemblyResourceBitmap("LiveClipboard.Images.LiveClipboardIcon.png", true);

            // paramaterize caption with product name
            labelCaption.Text = String.Format(CultureInfo.CurrentCulture, labelCaption.Text, ApplicationEnvironment.ProductName);

            // initialize preferences
            _liveClipboardPreferences = new LiveClipboardPreferences();
            _liveClipboardPreferences.PreferencesModified += new EventHandler(_liveClipboardPreferences_PreferencesModified);

            // signup for events
            listViewFormats.SelectedIndexChanged += new EventHandler(listViewFormats_SelectedIndexChanged);
            linkLabelMoreAboutLiveClipboard.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelMoreAboutLiveClipboard_LinkClicked);

            // initialize list of formats
            PopulateFormatList();

            // select first item if possible
            if (listViewFormats.Items.Count > 0)
                listViewFormats.Items[0].Selected = true;

            // update the details pane
            UpdateDetailsPane();

            labelContentType.RightToLeft = System.Windows.Forms.RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                labelContentType.TextAlign = ContentAlignment.MiddleRight;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, Controls);
                linkLabelMoreAboutLiveClipboard.Top = pictureBoxLiveClipboardIcon.Top + 1;

                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Right, buttonChange.Width, int.MaxValue, buttonChange, buttonOptions);
            }

        }

        private void PopulateFormatList()
        {
            listViewFormats.BeginUpdate();
            listViewFormats.Items.Clear();
            imageListFormats.Images.Clear();
            LiveClipboardFormatHandler[] formatHandlers = LiveClipboardManager.LiveClipboardFormatHandlers;
            foreach (LiveClipboardFormatHandler formatHandler in formatHandlers)
            {
                ListViewItem listViewItem = new ListViewItem();
                UpdateListViewItem(listViewItem, formatHandler);
                listViewFormats.Items.Add(listViewItem);
            }
            listViewFormats.EndUpdate();
        }

        private void UpdateListViewItem(ListViewItem listViewItem, LiveClipboardFormatHandler formatHandler)
        {
            imageListFormats.Images.Add(BidiHelper.Mirror((Bitmap)formatHandler.FormatImage));
            listViewItem.Tag = formatHandler;
            listViewItem.ImageIndex = imageListFormats.Images.Count - 1;
            listViewItem.SubItems.Clear();
            listViewItem.Text = " " + formatHandler.FormatName;
            listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem, formatHandler.FormatDescription));
        }

        public override void Save()
        {
            if (_liveClipboardPreferences.IsModified())
            {
                _liveClipboardPreferences.Save();
            }
        }

        private void _liveClipboardPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        private void listViewFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDetailsPane();
        }

        private void listViewFormats_DoubleClick(object sender, System.EventArgs e)
        {
            if (buttonChange.Enabled)
                ChangeSelectedFormat();
        }

        private void buttonChange_Click(object sender, System.EventArgs e)
        {
            ChangeSelectedFormat();
        }

        private void ChangeSelectedFormat()
        {
            if (listViewFormats.SelectedItems.Count < 1)
                return; // should never happen

            ListViewItem selectedItem = listViewFormats.SelectedItems[0];
            LiveClipboardFormatHandler formatHandler = selectedItem.Tag as LiveClipboardFormatHandler;

            using (LiveClipboardChangeHandlerForm changeHandlerForm = new LiveClipboardChangeHandlerForm(formatHandler))
            {
                if (changeHandlerForm.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LiveClipboardFormatHandler newFormatHandler = changeHandlerForm.FormatHandler;
                    if (newFormatHandler != null)
                    {
                        LiveClipboardManager.SetContentSourceForFormat(newFormatHandler.Format, newFormatHandler.ContentSource.Id);
                        UpdateListViewItem(selectedItem, newFormatHandler);
                    }
                }
            }
        }

        private void buttonOptions_Click(object sender, System.EventArgs e)
        {
            LiveClipboardFormatHandler formatHandler = GetSelectedFormat();
            if (formatHandler != null)
            {
                if (formatHandler.ContentSource.WriterPluginHasEditableOptions)
                {
                    formatHandler.ContentSource.Instance.EditOptions(FindForm());
                }
            }
        }

        private void linkLabelMoreAboutLiveClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.MoreAboutLiveClipboard);
        }

        private void UpdateDetailsPane()
        {
            LiveClipboardFormatHandler formatHandler = GetSelectedFormat();
            if (formatHandler != null)
            {
                panelFormatDetails.Visible = true;
                labelNoFormatSelected.Visible = false;

                groupBoxFormatDetails.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LCPrefDetailsGroupBoxFormat), formatHandler.FormatName);
                LiveClipboardComponentDisplay componentDisplay = new LiveClipboardComponentDisplay(formatHandler.ContentSource);
                pictureBoxContentSource.Image = componentDisplay.Icon;
                labelContentSourceName.Text = componentDisplay.Name;
                labelContentType.Text = formatHandler.FriendlyContentType;
                buttonChange.Enabled = LiveClipboardManager.GetContentSourcesForFormat(formatHandler.Format).Length > 1;
                buttonOptions.Visible = formatHandler.ContentSource.WriterPluginHasEditableOptions;
            }
            else
            {
                labelNoFormatSelected.Visible = true;
                groupBoxFormatDetails.Text = Res.Get(StringId.LCPrefDetails);
                panelFormatDetails.Visible = false;
            }

        }

        private LiveClipboardFormatHandler GetSelectedFormat()
        {
            if (listViewFormats.SelectedItems.Count > 0)
                return listViewFormats.SelectedItems[0].Tag as LiveClipboardFormatHandler;
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
                _liveClipboardPreferences.PreferencesModified -= new EventHandler(_liveClipboardPreferences_PreferencesModified);

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
            this.listViewFormats = new System.Windows.Forms.ListView();
            this.columnHeaderFormat = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDescription = new System.Windows.Forms.ColumnHeader();
            this.imageListFormats = new System.Windows.Forms.ImageList(this.components);
            this.labelInstalledPlugins = new System.Windows.Forms.Label();
            this.groupBoxFormatDetails = new System.Windows.Forms.GroupBox();
            this.panelFormatDetails = new System.Windows.Forms.Panel();
            this.labelContentType = new System.Windows.Forms.Label();
            this.labelHandledByCaption = new System.Windows.Forms.Label();
            this.labelContentSourceName = new System.Windows.Forms.Label();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.labelContentTypeCaption = new System.Windows.Forms.Label();
            this.pictureBoxContentSource = new System.Windows.Forms.PictureBox();
            this.labelNoFormatSelected = new System.Windows.Forms.Label();
            this.linkLabelMoreAboutLiveClipboard = new System.Windows.Forms.LinkLabel();
            this.labelCaption = new System.Windows.Forms.Label();
            this.toolTip = new OpenLiveWriter.Controls.ToolTip2(this.components);
            this.pictureBoxLiveClipboardIcon = new System.Windows.Forms.PictureBox();
            this.groupBoxFormatDetails.SuspendLayout();
            this.panelFormatDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxContentSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLiveClipboardIcon)).BeginInit();
            this.SuspendLayout();
            //
            // listViewFormats
            //
            this.listViewFormats.AutoArrange = false;
            this.listViewFormats.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFormat,
            this.columnHeaderDescription});
            this.listViewFormats.FullRowSelect = true;
            this.listViewFormats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFormats.HideSelection = false;
            this.listViewFormats.Location = new System.Drawing.Point(8, 84);
            this.listViewFormats.MultiSelect = false;
            this.listViewFormats.Name = "listViewFormats";
            this.listViewFormats.Size = new System.Drawing.Size(348, 189);
            this.listViewFormats.SmallImageList = this.imageListFormats;
            this.listViewFormats.TabIndex = 3;
            this.listViewFormats.UseCompatibleStateImageBehavior = false;
            this.listViewFormats.View = System.Windows.Forms.View.Details;
            this.listViewFormats.DoubleClick += new System.EventHandler(this.listViewFormats_DoubleClick);
            //
            // columnHeaderFormat
            //
            this.columnHeaderFormat.Text = "Format";
            this.columnHeaderFormat.Width = 100;
            //
            // columnHeaderDescription
            //
            this.columnHeaderDescription.Text = "Description";
            this.columnHeaderDescription.Width = 223;
            //
            // imageListFormats
            //
            this.imageListFormats.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListFormats.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListFormats.TransparentColor = System.Drawing.Color.Transparent;
            //
            // labelInstalledPlugins
            //
            this.labelInstalledPlugins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelInstalledPlugins.Location = new System.Drawing.Point(8, 66);
            this.labelInstalledPlugins.Name = "labelInstalledPlugins";
            this.labelInstalledPlugins.Size = new System.Drawing.Size(341, 15);
            this.labelInstalledPlugins.TabIndex = 2;
            this.labelInstalledPlugins.Text = "&Supported formats:";
            //
            // groupBoxFormatDetails
            //
            this.groupBoxFormatDetails.Controls.Add(this.panelFormatDetails);
            this.groupBoxFormatDetails.Controls.Add(this.labelNoFormatSelected);
            this.groupBoxFormatDetails.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxFormatDetails.Location = new System.Drawing.Point(8, 282);
            this.groupBoxFormatDetails.Name = "groupBoxFormatDetails";
            this.groupBoxFormatDetails.Size = new System.Drawing.Size(349, 109);
            this.groupBoxFormatDetails.TabIndex = 4;
            this.groupBoxFormatDetails.TabStop = false;
            this.groupBoxFormatDetails.Text = "Details for \'vCalendar\' format";
            //
            // panelFormatDetails
            //
            this.panelFormatDetails.Controls.Add(this.labelContentType);
            this.panelFormatDetails.Controls.Add(this.labelHandledByCaption);
            this.panelFormatDetails.Controls.Add(this.labelContentSourceName);
            this.panelFormatDetails.Controls.Add(this.buttonChange);
            this.panelFormatDetails.Controls.Add(this.buttonOptions);
            this.panelFormatDetails.Controls.Add(this.labelContentTypeCaption);
            this.panelFormatDetails.Controls.Add(this.pictureBoxContentSource);
            this.panelFormatDetails.Location = new System.Drawing.Point(7, 16);
            this.panelFormatDetails.Name = "panelFormatDetails";
            this.panelFormatDetails.Size = new System.Drawing.Size(339, 89);
            this.panelFormatDetails.TabIndex = 0;
            //
            // labelContentType
            //
            this.labelContentType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelContentType.Location = new System.Drawing.Point(2, 67);
            this.labelContentType.Name = "labelContentType";
            this.labelContentType.Size = new System.Drawing.Size(224, 16);
            this.labelContentType.TabIndex = 5;
            this.labelContentType.Text = "vcalendar (application/xhtml+xml)";
            //
            // labelHandledByCaption
            //
            this.labelHandledByCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHandledByCaption.Location = new System.Drawing.Point(2, 7);
            this.labelHandledByCaption.Name = "labelHandledByCaption";
            this.labelHandledByCaption.Size = new System.Drawing.Size(186, 15);
            this.labelHandledByCaption.TabIndex = 0;
            this.labelHandledByCaption.Text = "Handled by:";
            //
            // labelContentSourceName
            //
            this.labelContentSourceName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelContentSourceName.Location = new System.Drawing.Point(26, 23);
            this.labelContentSourceName.Name = "labelContentSourceName";
            this.labelContentSourceName.Size = new System.Drawing.Size(136, 15);
            this.labelContentSourceName.TabIndex = 1;
            this.labelContentSourceName.Text = "Open Live Writer";
            //
            // buttonChange
            //
            this.buttonChange.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonChange.Location = new System.Drawing.Point(259, 10);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(75, 23);
            this.buttonChange.TabIndex = 2;
            this.buttonChange.Text = "Change...";
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            //
            // buttonOptions
            //
            this.buttonOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOptions.Location = new System.Drawing.Point(259, 40);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(75, 23);
            this.buttonOptions.TabIndex = 4;
            this.buttonOptions.Text = "Options...";
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            //
            // labelContentTypeCaption
            //
            this.labelContentTypeCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelContentTypeCaption.Location = new System.Drawing.Point(2, 51);
            this.labelContentTypeCaption.Name = "labelContentTypeCaption";
            this.labelContentTypeCaption.Size = new System.Drawing.Size(188, 16);
            this.labelContentTypeCaption.TabIndex = 3;
            this.labelContentTypeCaption.Text = "Content type:";
            //
            // pictureBoxContentSource
            //
            this.pictureBoxContentSource.Location = new System.Drawing.Point(2, 21);
            this.pictureBoxContentSource.Name = "pictureBoxContentSource";
            this.pictureBoxContentSource.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxContentSource.TabIndex = 0;
            this.pictureBoxContentSource.TabStop = false;
            //
            // labelNoFormatSelected
            //
            this.labelNoFormatSelected.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelNoFormatSelected.Location = new System.Drawing.Point(8, 41);
            this.labelNoFormatSelected.Name = "labelNoFormatSelected";
            this.labelNoFormatSelected.Size = new System.Drawing.Size(332, 23);
            this.labelNoFormatSelected.TabIndex = 1;
            this.labelNoFormatSelected.Text = "(No format selected)";
            this.labelNoFormatSelected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // linkLabelMoreAboutLiveClipboard
            //
            this.linkLabelMoreAboutLiveClipboard.AutoSize = true;
            this.linkLabelMoreAboutLiveClipboard.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelMoreAboutLiveClipboard.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelMoreAboutLiveClipboard.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabelMoreAboutLiveClipboard.Location = new System.Drawing.Point(34, 422);
            this.linkLabelMoreAboutLiveClipboard.Name = "linkLabelMoreAboutLiveClipboard";
            this.linkLabelMoreAboutLiveClipboard.Size = new System.Drawing.Size(144, 13);
            this.linkLabelMoreAboutLiveClipboard.TabIndex = 5;
            this.linkLabelMoreAboutLiveClipboard.TabStop = true;
            this.linkLabelMoreAboutLiveClipboard.Text = "More about Live Clipboard...";
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Location = new System.Drawing.Point(8, 32);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(341, 32);
            this.labelCaption.TabIndex = 1;
            this.labelCaption.Text = "You can paste web content that supports the Live Clipboard format into {0}.";
            //
            // pictureBoxLiveClipboardIcon
            //
            this.pictureBoxLiveClipboardIcon.Location = new System.Drawing.Point(14, 420);
            this.pictureBoxLiveClipboardIcon.Name = "pictureBoxLiveClipboardIcon";
            this.pictureBoxLiveClipboardIcon.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxLiveClipboardIcon.TabIndex = 6;
            this.pictureBoxLiveClipboardIcon.TabStop = false;
            //
            // LiveClipboardPreferencesPanel
            //
            this.AccessibleName = "Live Clipboard";
            this.Controls.Add(this.pictureBoxLiveClipboardIcon);
            this.Controls.Add(this.labelCaption);
            this.Controls.Add(this.linkLabelMoreAboutLiveClipboard);
            this.Controls.Add(this.groupBoxFormatDetails);
            this.Controls.Add(this.labelInstalledPlugins);
            this.Controls.Add(this.listViewFormats);
            this.Name = "LiveClipboardPreferencesPanel";
            this.PanelName = "Live Clipboard";
            this.Size = new System.Drawing.Size(370, 449);
            this.Controls.SetChildIndex(this.listViewFormats, 0);
            this.Controls.SetChildIndex(this.labelInstalledPlugins, 0);
            this.Controls.SetChildIndex(this.groupBoxFormatDetails, 0);
            this.Controls.SetChildIndex(this.linkLabelMoreAboutLiveClipboard, 0);
            this.Controls.SetChildIndex(this.labelCaption, 0);
            this.Controls.SetChildIndex(this.pictureBoxLiveClipboardIcon, 0);
            this.groupBoxFormatDetails.ResumeLayout(false);
            this.panelFormatDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxContentSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLiveClipboardIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
