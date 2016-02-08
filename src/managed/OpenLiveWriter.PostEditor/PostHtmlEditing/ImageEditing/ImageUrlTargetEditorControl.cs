// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageTargetEditorControl.
    /// </summary>
    public class ImageUrlTargetEditorControl : UserControl, ILinkOptionsEditor
    {
        private TextBox textBoxUrl;
        private LinkOptionsEditorControl linkOptionsControl1;
        private Label label1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageUrlTargetEditorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.label1.Text = Res.Get(StringId.LinkToWebPageURL);
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
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.linkOptionsControl1 = new OpenLiveWriter.PostEditor.PostHtmlEditing.LinkOptionsEditorControl();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // textBoxUrl
            //
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.Location = new System.Drawing.Point(0, 16);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(288, 20);
            this.textBoxUrl.TabIndex = 0;
            this.textBoxUrl.Text = "http://";
            this.textBoxUrl.TextChanged += new System.EventHandler(this.textBoxUrl_TextChanged);
            this.textBoxUrl.Leave += new System.EventHandler(this.textBoxUrl_Leave);
            //
            // linkOptionsControl1
            //
            this.linkOptionsControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.linkOptionsControl1.Location = new System.Drawing.Point(0, 40);
            this.linkOptionsControl1.Name = "linkOptionsControl1";
            this.linkOptionsControl1.Size = new System.Drawing.Size(284, 20);
            this.linkOptionsControl1.TabIndex = 1;
            this.linkOptionsControl1.LinkOptionsChanged += new System.EventHandler(this.linkOptionsControl1_LinkOptionsChanged);
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Link to webpage URL:";
            //
            // ImageUrlTargetEditorControl
            //
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkOptionsControl1);
            this.Controls.Add(this.textBoxUrl);
            this.Name = "ImageUrlTargetEditorControl";
            this.Size = new System.Drawing.Size(288, 60);
            this.ResumeLayout(false);

        }
        #endregion

        public string Url
        {
            get
            {
                return UrlHelper.FixUpUrl(textBoxUrl.Text);
            }
            set
            {
                this.textBoxUrl.Text = value;
            }
        }

        public ILinkOptions LinkOptions
        {
            get
            {
                return linkOptionsControl1.LinkOptions;
            }
            set
            {
                linkOptionsControl1.LinkOptions = value;
            }
        }

        public event EventHandler UrlSettingsChanged;
        protected virtual void OnUrlSettingsChanged(EventArgs evt)
        {
            if (UrlSettingsChanged != null)
                UrlSettingsChanged(this, evt);
        }

        private void textBoxUrl_TextChanged(object sender, EventArgs e)
        {
            OnUrlSettingsChanged(e);
        }

        private void linkOptionsControl1_LinkOptionsChanged(object sender, EventArgs e)
        {
            OnUrlSettingsChanged(e);
        }

        private void textBoxUrl_Leave(object sender, System.EventArgs e)
        {
            textBoxUrl.Text = UrlHelper.FixUpUrl(textBoxUrl.Text);
        }
    }
}
