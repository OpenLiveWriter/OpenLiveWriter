// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for LinkOptionsControl.
    /// </summary>
    public class LinkOptionsEditorControl : UserControl, ILinkOptionsEditor
    {
        private CheckBox cbNewWindow;
        private CheckBox cbEnableViewer;
        private Label label1;
        private TextBox txtGroupName;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private ImageViewer _imageViewer;

        public LinkOptionsEditorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.cbNewWindow.Text = Res.Get(StringId.ImgSBOpenInNewWindow);
            this.cbEnableViewer.Text = Res.Get(StringId.ImageViewerLabel);
            this.label1.Text = Res.Get(StringId.ImageViewerGroupLabel);
            txtGroupName.Enabled = false;
            cbEnableViewer.CheckedChanged += delegate
                                                 {
                                                     txtGroupName.Enabled = label1.Enabled = cbEnableViewer.Checked;
                                                     if (cbEnableViewer.Checked)
                                                         cbNewWindow.Checked = false;
                                                 };
            cbNewWindow.CheckedChanged += delegate
                                              {
                                                  if (cbNewWindow.Checked)
                                                      cbEnableViewer.Checked = false;
                                              };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int deltaX = label1.Right - txtGroupName.Left;
            if ((txtGroupName.Width - deltaX) >= 80)
            {
                txtGroupName.Left += deltaX;
                txtGroupName.Width -= deltaX;
            }
            else
            {
                txtGroupName.Left = label1.Left;
                int deltaY = label1.Bottom - txtGroupName.Top + Convert.ToInt32(DisplayHelper.ScaleY(3));
                txtGroupName.Top += deltaY;
                Parent.Height += deltaY;
                Height += deltaY;
            }

            int origCbHeight = cbEnableViewer.Height;

            // WinLive 116628: The call to LayoutHelper.NaturalizeHeight causes the cbEnableViewer to change location.
            // This is due to a bug in our attempt to identify if a control is aligned right. For now this is just a
            // workaround until we have time to fix LayoutHelper.NaturalizeHeight (and general RTL mirroring in WinForms).
            Point origCbLocation = cbEnableViewer.Location;
            LayoutHelper.NaturalizeHeight(cbEnableViewer);
            cbEnableViewer.Location = origCbLocation;

            int deltaY2 = cbEnableViewer.Height - origCbHeight;
            txtGroupName.Top += deltaY2;
            label1.Top += deltaY2;
            Parent.Height += deltaY2;
            Height += deltaY2;

            //Height = txtGroupName.Bottom + 3;
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
            this.cbNewWindow = new System.Windows.Forms.CheckBox();
            this.cbEnableViewer = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // cbNewWindow
            //
            this.cbNewWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbNewWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbNewWindow.Location = new System.Drawing.Point(0, 0);
            this.cbNewWindow.Name = "cbNewWindow";
            this.cbNewWindow.Size = new System.Drawing.Size(281, 16);
            this.cbNewWindow.TabIndex = 0;
            this.cbNewWindow.Text = "Open in new window";
            this.cbNewWindow.CheckedChanged += new System.EventHandler(this.cbNewWindow_CheckedChanged);
            //
            // cbEnableViewer
            //
            this.cbEnableViewer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbEnableViewer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbEnableViewer.Location = new System.Drawing.Point(0, 19);
            this.cbEnableViewer.Name = "cbEnableViewer";
            this.cbEnableViewer.Size = new System.Drawing.Size(269, 16);
            this.cbEnableViewer.TabIndex = 1;
            this.cbEnableViewer.Text = "&Enable {0}";
            this.cbEnableViewer.UseVisualStyleBackColor = true;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Group (optional):";
            //
            // txtGroupName
            //
            this.txtGroupName.Location = new System.Drawing.Point(114, 39);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(155, 20);
            this.txtGroupName.TabIndex = 3;
            this.txtGroupName.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            this.txtGroupName.MaxLength = 100;
            //
            // LinkOptionsEditorControl
            //
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.cbNewWindow);
            this.Controls.Add(this.cbEnableViewer);
            this.Name = "LinkOptionsEditorControl";
            this.Size = new System.Drawing.Size(281, 67);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public ILinkOptions LinkOptions
        {
            get
            {
                return new LinkOptions(cbNewWindow.Checked, cbEnableViewer.Checked, txtGroupName.Text.Trim());
            }
            set
            {
                cbNewWindow.Checked = value.ShowInNewWindow;
                cbEnableViewer.Checked = value.UseImageViewer;
                txtGroupName.Text = value.ImageViewerGroupName ?? "";
            }
        }

        public IEditorOptions EditorOptions
        {
            set
            {
                string imageViewer = value.DhtmlImageViewer;
                if (imageViewer != null)
                {
                    _imageViewer = DhtmlImageViewers.GetImageViewer(imageViewer);
                }

                int deltaY = 0;

                if (_imageViewer == null)
                {
                    cbEnableViewer.Visible = false;
                    label1.Visible = false;
                    txtGroupName.Visible = false;

                    deltaY = txtGroupName.Bottom - cbNewWindow.Bottom;
                }
                else
                {
                    cbEnableViewer.Text = string.Format(CultureInfo.CurrentCulture, cbEnableViewer.Text, DhtmlImageViewers.GetLocalizedName(_imageViewer.Name));
                    if (_imageViewer.GroupSupport == ImageViewerGroupSupport.None)
                    {
                        txtGroupName.Visible = false;
                        label1.Visible = false;
                        deltaY = txtGroupName.Bottom - cbEnableViewer.Bottom;
                    }
                }

                if (deltaY != 0)
                {
                    Height -= deltaY;
                    Parent.Height -= deltaY;
                }
            }
        }

        public event EventHandler LinkOptionsChanged;
        protected virtual void OnLinkOptionsChanged(EventArgs e)
        {
            if (LinkOptionsChanged != null)
                LinkOptionsChanged(this, e);
        }

        private void cbNewWindow_CheckedChanged(object sender, EventArgs e)
        {
            OnLinkOptionsChanged(EventArgs.Empty);
        }
    }
}

