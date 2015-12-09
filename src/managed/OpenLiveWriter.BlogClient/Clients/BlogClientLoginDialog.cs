// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Summary description for BlogClientLoginDialog.
    /// </summary>
    public class BlogClientLoginDialog : BaseForm
    {
        private Button buttonCancel;
        private Button buttonOK;
        private OpenLiveWriter.BlogClient.Clients.BlogClientLoginControl blogClientLoginControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public BlogClientLoginDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.Login);

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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.blogClientLoginControl = new OpenLiveWriter.BlogClient.Clients.BlogClientLoginControl();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(96, 154);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(184, 154);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            //
            // blogClientLoginControl
            //
            this.blogClientLoginControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | AnchorStyles.Right)));
            this.blogClientLoginControl.Domain = null;
            this.blogClientLoginControl.Location = new System.Drawing.Point(8, 8);
            this.blogClientLoginControl.Name = "blogClientLoginControl";
            this.blogClientLoginControl.Password = "";
            this.blogClientLoginControl.SavePassword = false;
            this.blogClientLoginControl.Size = new System.Drawing.Size(253, 165);
            this.blogClientLoginControl.TabIndex = 0;
            this.blogClientLoginControl.UserName = "";
            //
            // BlogClientLoginDialog
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(266, 184);
            this.ControlBox = false;
            this.Controls.Add(this.blogClientLoginControl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlogClientLoginDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "te";
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (LayoutHelper.SuspendAnchoring(blogClientLoginControl, buttonOK, buttonCancel))
            {
                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
                LayoutHelper.NaturalizeHeightAndDistribute(10, blogClientLoginControl, new ControlGroup(buttonOK, buttonCancel));
                ClientSize = new Size(ClientSize.Width, buttonOK.Bottom + 8);
            }

            if (buttonOK.Left < blogClientLoginControl.Left)
            {
                Width += (blogClientLoginControl.Left - buttonOK.Left);
            }

        }

        public string UserName
        {
            get { return blogClientLoginControl.UserName; }
            set { blogClientLoginControl.UserName = value; }
        }

        public string Password
        {
            get { return blogClientLoginControl.Password; }
            set { blogClientLoginControl.Password = value; }
        }

        public bool SavePassword
        {
            get
            {
                return blogClientLoginControl.SavePassword;
            }
            set
            {
                blogClientLoginControl.SavePassword = value;
            }
        }

        public ICredentialsDomain Domain
        {
            get { return blogClientLoginControl.Domain; }
            set { blogClientLoginControl.Domain = value; }
        }
    }
}
