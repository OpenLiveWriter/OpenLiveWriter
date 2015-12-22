// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.LiveClipboard
{
    /// <summary>
    /// Summary description for LiveClipboardChangeHandlerForm.
    /// </summary>
    internal class LiveClipboardChangeHandlerForm : ApplicationDialog
    {
        private System.Windows.Forms.ColumnHeader columnHeaderHandler;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Label labelContentType;
        private System.Windows.Forms.Label labelFormatName;
        private System.Windows.Forms.PictureBox pictureBoxFormatIcon;
        private System.Windows.Forms.ListView listViewComponents;
        private System.Windows.Forms.ImageList imageListComponents;
        private System.ComponentModel.IContainer components;

        private LiveClipboardFormat _targetFormat;

        public LiveClipboardChangeHandlerForm(LiveClipboardFormatHandler existingHandler)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.columnHeaderHandler.Text = Res.Get(StringId.ChangeLiveClipboardHandlerComponent);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.labelCaption.Text = Res.Get(StringId.ChangeLiveClipboardHandlerCaption);
            this.Text = Res.Get(StringId.ChangeLiveClipboardHandlerTitle);

            // save and populate format info
            _targetFormat = existingHandler.Format;
            pictureBoxFormatIcon.Image = existingHandler.ContentSource.Image;
            labelFormatName.Text = existingHandler.FormatName;
            labelContentType.Text = existingHandler.FriendlyContentType;
            labelCaption.Text = String.Format(CultureInfo.CurrentCulture, labelCaption.Text, existingHandler.FormatName);

            // populate the list with content sources that support this format
            ContentSourceInfo[] contentSources = LiveClipboardManager.GetContentSourcesForFormat(existingHandler.Format);
            Array.Sort(contentSources, new ContentSourceInfo.NameComparer());
            foreach (ContentSourceInfo contentSource in contentSources)
            {
                LiveClipboardComponentDisplay componentDisplay = new LiveClipboardComponentDisplay(contentSource);
                imageListComponents.Images.Add(componentDisplay.Icon);
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Tag = contentSource;
                listViewItem.ImageIndex = imageListComponents.Images.Count - 1;
                listViewItem.Text = " " + componentDisplay.Name;
                listViewComponents.Items.Add(listViewItem);
                if (contentSource.Equals(existingHandler.ContentSource))
                    listViewItem.Selected = true;
            }
        }

        public LiveClipboardFormatHandler FormatHandler
        {
            get
            {
                // scan for the handler on this content source
                ContentSourceInfo contentSource = listViewComponents.SelectedItems[0].Tag as ContentSourceInfo;
                foreach (LiveClipboardFormatHandler formatHandler in contentSource.LiveClipboardFormatHandlers)
                    if (formatHandler.Format.Equals(_targetFormat))
                        return formatHandler;

                // handler not found (should be impossible)
                Trace.Fail("Unable to find valid handler!");
                return null;

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewComponents = new System.Windows.Forms.ListView();
            this.columnHeaderHandler = new System.Windows.Forms.ColumnHeader();
            this.imageListComponents = new System.Windows.Forms.ImageList(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelCaption = new System.Windows.Forms.Label();
            this.labelContentType = new System.Windows.Forms.Label();
            this.labelFormatName = new System.Windows.Forms.Label();
            this.pictureBoxFormatIcon = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            //
            // listViewComponents
            //
            this.listViewComponents.AutoArrange = false;
            this.listViewComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                 this.columnHeaderHandler});
            this.listViewComponents.FullRowSelect = true;
            this.listViewComponents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewComponents.HideSelection = false;
            this.listViewComponents.Location = new System.Drawing.Point(6, 68);
            this.listViewComponents.MultiSelect = false;
            this.listViewComponents.Name = "listViewComponents";
            this.listViewComponents.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewComponents.Size = new System.Drawing.Size(302, 140);
            this.listViewComponents.SmallImageList = this.imageListComponents;
            this.listViewComponents.TabIndex = 3;
            this.listViewComponents.View = System.Windows.Forms.View.Details;
            //
            // columnHeaderHandler
            //
            this.columnHeaderHandler.Text = "Component";
            this.columnHeaderHandler.Width = 275;
            //
            // imageListComponents
            //
            this.imageListComponents.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListComponents.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListComponents.TransparentColor = System.Drawing.Color.Transparent;
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(153, 215);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(233, 215);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Location = new System.Drawing.Point(9, 50);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(292, 16);
            this.labelCaption.TabIndex = 2;
            this.labelCaption.Text = "Components capable of handling \'{0}\':";
            //
            // labelContentType
            //
            this.labelContentType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelContentType.Location = new System.Drawing.Point(33, 27);
            this.labelContentType.Name = "labelContentType";
            this.labelContentType.Size = new System.Drawing.Size(270, 16);
            this.labelContentType.TabIndex = 1;
            this.labelContentType.Text = "vcalendar (application/xhtml+xml)";
            //
            // labelFormatName
            //
            this.labelFormatName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFormatName.Location = new System.Drawing.Point(33, 7);
            this.labelFormatName.Name = "labelFormatName";
            this.labelFormatName.Size = new System.Drawing.Size(267, 15);
            this.labelFormatName.TabIndex = 0;
            this.labelFormatName.Text = "iCalendar";
            //
            // pictureBoxFormatIcon
            //
            this.pictureBoxFormatIcon.Location = new System.Drawing.Point(7, 5);
            this.pictureBoxFormatIcon.Name = "pictureBoxFormatIcon";
            this.pictureBoxFormatIcon.Size = new System.Drawing.Size(20, 18);
            this.pictureBoxFormatIcon.TabIndex = 9;
            this.pictureBoxFormatIcon.TabStop = false;
            //
            // LiveClipboardChangeHandlerForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(316, 245);
            this.Controls.Add(this.labelContentType);
            this.Controls.Add(this.labelFormatName);
            this.Controls.Add(this.pictureBoxFormatIcon);
            this.Controls.Add(this.labelCaption);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewComponents);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LiveClipboardChangeHandlerForm";
            this.Text = "Change Format Handler";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
