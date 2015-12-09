// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.InternalWriterPlugin
{

    internal class MapOptionsDialog : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxPushpins;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelPushpinUrl;
        private System.Windows.Forms.TextBox textBoxPushpinUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabelViewPushpin;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private MapOptions _mapOptions;

        public MapOptionsDialog(MapOptions mapOptions)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            _mapOptions = mapOptions;

            textBoxPushpinUrl.Text = _mapOptions.PushpinUrl;

            linkLabelViewPushpin.Visible = _mapOptions.MoreAboutPushpinsUrl != String.Empty;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MapOptionsDialog));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxPushpins = new System.Windows.Forms.GroupBox();
            this.linkLabelViewPushpin = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelPushpinUrl = new System.Windows.Forms.Label();
            this.textBoxPushpinUrl = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBoxPushpins.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.AccessibleDescription = resources.GetString("buttonOK.AccessibleDescription");
            this.buttonOK.AccessibleName = resources.GetString("buttonOK.AccessibleName");
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("buttonOK.Anchor")));
            this.buttonOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonOK.BackgroundImage")));
            this.buttonOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("buttonOK.Dock")));
            this.buttonOK.Enabled = ((bool)(resources.GetObject("buttonOK.Enabled")));
            this.buttonOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("buttonOK.FlatStyle")));
            this.buttonOK.Font = ((System.Drawing.Font)(resources.GetObject("buttonOK.Font")));
            this.buttonOK.Image = ((System.Drawing.Image)(resources.GetObject("buttonOK.Image")));
            this.buttonOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonOK.ImageAlign")));
            this.buttonOK.ImageIndex = ((int)(resources.GetObject("buttonOK.ImageIndex")));
            this.buttonOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("buttonOK.ImeMode")));
            this.buttonOK.Location = ((System.Drawing.Point)(resources.GetObject("buttonOK.Location")));
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("buttonOK.RightToLeft")));
            this.buttonOK.Size = ((System.Drawing.Size)(resources.GetObject("buttonOK.Size")));
            this.buttonOK.TabIndex = ((int)(resources.GetObject("buttonOK.TabIndex")));
            this.buttonOK.Text = resources.GetString("buttonOK.Text");
            this.buttonOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonOK.TextAlign")));
            this.buttonOK.Visible = ((bool)(resources.GetObject("buttonOK.Visible")));
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.AccessibleDescription = resources.GetString("buttonCancel.AccessibleDescription");
            this.buttonCancel.AccessibleName = resources.GetString("buttonCancel.AccessibleName");
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("buttonCancel.Anchor")));
            this.buttonCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonCancel.BackgroundImage")));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("buttonCancel.Dock")));
            this.buttonCancel.Enabled = ((bool)(resources.GetObject("buttonCancel.Enabled")));
            this.buttonCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("buttonCancel.FlatStyle")));
            this.buttonCancel.Font = ((System.Drawing.Font)(resources.GetObject("buttonCancel.Font")));
            this.buttonCancel.Image = ((System.Drawing.Image)(resources.GetObject("buttonCancel.Image")));
            this.buttonCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonCancel.ImageAlign")));
            this.buttonCancel.ImageIndex = ((int)(resources.GetObject("buttonCancel.ImageIndex")));
            this.buttonCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("buttonCancel.ImeMode")));
            this.buttonCancel.Location = ((System.Drawing.Point)(resources.GetObject("buttonCancel.Location")));
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("buttonCancel.RightToLeft")));
            this.buttonCancel.Size = ((System.Drawing.Size)(resources.GetObject("buttonCancel.Size")));
            this.buttonCancel.TabIndex = ((int)(resources.GetObject("buttonCancel.TabIndex")));
            this.buttonCancel.Text = resources.GetString("buttonCancel.Text");
            this.buttonCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonCancel.TextAlign")));
            this.buttonCancel.Visible = ((bool)(resources.GetObject("buttonCancel.Visible")));
            //
            // groupBoxPushpins
            //
            this.groupBoxPushpins.AccessibleDescription = resources.GetString("groupBoxPushpins.AccessibleDescription");
            this.groupBoxPushpins.AccessibleName = resources.GetString("groupBoxPushpins.AccessibleName");
            this.groupBoxPushpins.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBoxPushpins.Anchor")));
            this.groupBoxPushpins.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBoxPushpins.BackgroundImage")));
            this.groupBoxPushpins.Controls.Add(this.linkLabelViewPushpin);
            this.groupBoxPushpins.Controls.Add(this.label1);
            this.groupBoxPushpins.Controls.Add(this.labelPushpinUrl);
            this.groupBoxPushpins.Controls.Add(this.textBoxPushpinUrl);
            this.groupBoxPushpins.Controls.Add(this.pictureBox1);
            this.groupBoxPushpins.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBoxPushpins.Dock")));
            this.groupBoxPushpins.Enabled = ((bool)(resources.GetObject("groupBoxPushpins.Enabled")));
            this.groupBoxPushpins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPushpins.Font = ((System.Drawing.Font)(resources.GetObject("groupBoxPushpins.Font")));
            this.groupBoxPushpins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBoxPushpins.ImeMode")));
            this.groupBoxPushpins.Location = ((System.Drawing.Point)(resources.GetObject("groupBoxPushpins.Location")));
            this.groupBoxPushpins.Name = "groupBoxPushpins";
            this.groupBoxPushpins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBoxPushpins.RightToLeft")));
            this.groupBoxPushpins.Size = ((System.Drawing.Size)(resources.GetObject("groupBoxPushpins.Size")));
            this.groupBoxPushpins.TabIndex = ((int)(resources.GetObject("groupBoxPushpins.TabIndex")));
            this.groupBoxPushpins.TabStop = false;
            this.groupBoxPushpins.Text = resources.GetString("groupBoxPushpins.Text");
            this.groupBoxPushpins.Visible = ((bool)(resources.GetObject("groupBoxPushpins.Visible")));
            //
            // linkLabelViewPushpin
            //
            this.linkLabelViewPushpin.AccessibleDescription = resources.GetString("linkLabelViewPushpin.AccessibleDescription");
            this.linkLabelViewPushpin.AccessibleName = resources.GetString("linkLabelViewPushpin.AccessibleName");
            this.linkLabelViewPushpin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabelViewPushpin.Anchor")));
            this.linkLabelViewPushpin.AutoSize = ((bool)(resources.GetObject("linkLabelViewPushpin.AutoSize")));
            this.linkLabelViewPushpin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabelViewPushpin.Dock")));
            this.linkLabelViewPushpin.Enabled = ((bool)(resources.GetObject("linkLabelViewPushpin.Enabled")));
            this.linkLabelViewPushpin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelViewPushpin.Font = ((System.Drawing.Font)(resources.GetObject("linkLabelViewPushpin.Font")));
            this.linkLabelViewPushpin.Image = ((System.Drawing.Image)(resources.GetObject("linkLabelViewPushpin.Image")));
            this.linkLabelViewPushpin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabelViewPushpin.ImageAlign")));
            this.linkLabelViewPushpin.ImageIndex = ((int)(resources.GetObject("linkLabelViewPushpin.ImageIndex")));
            this.linkLabelViewPushpin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabelViewPushpin.ImeMode")));
            this.linkLabelViewPushpin.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabelViewPushpin.LinkArea")));
            this.linkLabelViewPushpin.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelViewPushpin.Location = ((System.Drawing.Point)(resources.GetObject("linkLabelViewPushpin.Location")));
            this.linkLabelViewPushpin.Name = "linkLabelViewPushpin";
            this.linkLabelViewPushpin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabelViewPushpin.RightToLeft")));
            this.linkLabelViewPushpin.Size = ((System.Drawing.Size)(resources.GetObject("linkLabelViewPushpin.Size")));
            this.linkLabelViewPushpin.TabIndex = ((int)(resources.GetObject("linkLabelViewPushpin.TabIndex")));
            this.linkLabelViewPushpin.TabStop = true;
            this.linkLabelViewPushpin.Text = resources.GetString("linkLabelViewPushpin.Text");
            this.linkLabelViewPushpin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabelViewPushpin.TextAlign")));
            this.linkLabelViewPushpin.Visible = ((bool)(resources.GetObject("linkLabelViewPushpin.Visible")));
            this.linkLabelViewPushpin.LinkColor = SystemColors.HotTrack;
            this.linkLabelViewPushpin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelViewPushpin_LinkClicked);
            //
            // label1
            //
            this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
            this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
            this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
            this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
            this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
            this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
            this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
            this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
            this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
            this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
            this.label1.Name = "label1";
            this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
            this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
            this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
            this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
            //
            // labelPushpinUrl
            //
            this.labelPushpinUrl.AccessibleDescription = resources.GetString("labelPushpinUrl.AccessibleDescription");
            this.labelPushpinUrl.AccessibleName = resources.GetString("labelPushpinUrl.AccessibleName");
            this.labelPushpinUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelPushpinUrl.Anchor")));
            this.labelPushpinUrl.AutoSize = ((bool)(resources.GetObject("labelPushpinUrl.AutoSize")));
            this.labelPushpinUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelPushpinUrl.Dock")));
            this.labelPushpinUrl.Enabled = ((bool)(resources.GetObject("labelPushpinUrl.Enabled")));
            this.labelPushpinUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPushpinUrl.Font = ((System.Drawing.Font)(resources.GetObject("labelPushpinUrl.Font")));
            this.labelPushpinUrl.Image = ((System.Drawing.Image)(resources.GetObject("labelPushpinUrl.Image")));
            this.labelPushpinUrl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelPushpinUrl.ImageAlign")));
            this.labelPushpinUrl.ImageIndex = ((int)(resources.GetObject("labelPushpinUrl.ImageIndex")));
            this.labelPushpinUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelPushpinUrl.ImeMode")));
            this.labelPushpinUrl.Location = ((System.Drawing.Point)(resources.GetObject("labelPushpinUrl.Location")));
            this.labelPushpinUrl.Name = "labelPushpinUrl";
            this.labelPushpinUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelPushpinUrl.RightToLeft")));
            this.labelPushpinUrl.Size = ((System.Drawing.Size)(resources.GetObject("labelPushpinUrl.Size")));
            this.labelPushpinUrl.TabIndex = ((int)(resources.GetObject("labelPushpinUrl.TabIndex")));
            this.labelPushpinUrl.Text = resources.GetString("labelPushpinUrl.Text");
            this.labelPushpinUrl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelPushpinUrl.TextAlign")));
            this.labelPushpinUrl.Visible = ((bool)(resources.GetObject("labelPushpinUrl.Visible")));
            //
            // textBoxPushpinUrl
            //
            this.textBoxPushpinUrl.AccessibleDescription = resources.GetString("textBoxPushpinUrl.AccessibleDescription");
            this.textBoxPushpinUrl.AccessibleName = resources.GetString("textBoxPushpinUrl.AccessibleName");
            this.textBoxPushpinUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBoxPushpinUrl.Anchor")));
            this.textBoxPushpinUrl.AutoSize = ((bool)(resources.GetObject("textBoxPushpinUrl.AutoSize")));
            this.textBoxPushpinUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBoxPushpinUrl.BackgroundImage")));
            this.textBoxPushpinUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBoxPushpinUrl.Dock")));
            this.textBoxPushpinUrl.Enabled = ((bool)(resources.GetObject("textBoxPushpinUrl.Enabled")));
            this.textBoxPushpinUrl.Font = ((System.Drawing.Font)(resources.GetObject("textBoxPushpinUrl.Font")));
            this.textBoxPushpinUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBoxPushpinUrl.ImeMode")));
            this.textBoxPushpinUrl.Location = ((System.Drawing.Point)(resources.GetObject("textBoxPushpinUrl.Location")));
            this.textBoxPushpinUrl.MaxLength = ((int)(resources.GetObject("textBoxPushpinUrl.MaxLength")));
            this.textBoxPushpinUrl.Multiline = ((bool)(resources.GetObject("textBoxPushpinUrl.Multiline")));
            this.textBoxPushpinUrl.Name = "textBoxPushpinUrl";
            this.textBoxPushpinUrl.PasswordChar = ((char)(resources.GetObject("textBoxPushpinUrl.PasswordChar")));
            this.textBoxPushpinUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBoxPushpinUrl.RightToLeft")));
            this.textBoxPushpinUrl.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBoxPushpinUrl.ScrollBars")));
            this.textBoxPushpinUrl.Size = ((System.Drawing.Size)(resources.GetObject("textBoxPushpinUrl.Size")));
            this.textBoxPushpinUrl.TabIndex = ((int)(resources.GetObject("textBoxPushpinUrl.TabIndex")));
            this.textBoxPushpinUrl.Text = resources.GetString("textBoxPushpinUrl.Text");
            this.textBoxPushpinUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBoxPushpinUrl.TextAlign")));
            this.textBoxPushpinUrl.Visible = ((bool)(resources.GetObject("textBoxPushpinUrl.Visible")));
            this.textBoxPushpinUrl.WordWrap = ((bool)(resources.GetObject("textBoxPushpinUrl.WordWrap")));
            //
            // pictureBox1
            //
            this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
            this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
            this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
            this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
            this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
            this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
            this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
            this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
            this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
            //
            // MapOptionsDialog
            //
            this.AcceptButton = this.buttonOK;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.buttonCancel;
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.groupBoxPushpins);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "MapOptionsDialog";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.groupBoxPushpins.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (ValidatePushpinUrl())
            {
                _mapOptions.PushpinUrl = PushpinUrl;
                DialogResult = DialogResult.OK;
            }
        }

        private void linkLabelViewPushpin_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(_mapOptions.MoreAboutPushpinsUrl);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error launching pushpin url " + _mapOptions.MoreAboutPushpinsUrl + ": " + ex.ToString());
            }
        }

        private string PushpinUrl
        {
            get { return UrlHelper.FixUpUrl(textBoxPushpinUrl.Text.Trim()); }
        }

        private bool ValidatePushpinUrl()
        {
            using (new WaitCursor())
            {
                // empty url is ok, means no custom pushpin
                if (PushpinUrl == String.Empty)
                    return true;

                // try to validate (take no more than 5 seconds)
                try
                {
                    PluginHttpRequest request = new PluginHttpRequest(PushpinUrl, HttpRequestCacheLevel.CacheIfAvailable);
                    using (Stream response = request.GetResponse(5000))
                    {
                        if (response == null)
                            return ShowPushpinValidationError();
                        else
                            return true;
                    }
                }
                catch (Exception)
                {
                    return ShowPushpinValidationError();
                }
            }
        }

        private bool ShowPushpinValidationError()
        {
            DialogResult result = DisplayMessage.Show(MessageId.PushpinValidationError);
            return result == DialogResult.Yes;
        }
    }
}
