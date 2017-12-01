// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ItemInsertionForm.
    /// </summary>
    [PersistentWindow("InsertImageFromUrlForm", Location = false, Size = false)]
    public class ImageUrlForm : ApplicationDialog
    {

        private Label label1;
        private TextBox txtUrl;
        private Button btnCancel;
        private Button btnOK;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageUrlForm()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            txtUrl.GotFocus += new EventHandler(txtUrl_GotFocus);

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
            this.label1 = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(8, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Image URL: ";
            //
            // txtUrl
            //
            this.txtUrl.Location = new System.Drawing.Point(8, 34);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(311, 21);
            this.txtUrl.TabIndex = 3;
            this.txtUrl.Text = "";
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(253, 84);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 26);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // btnOK
            //
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(178, 84);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(70, 26);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // ImageUrlForm
            //
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(335, 119);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Location = new System.Drawing.Point(20, 20);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageUrlForm";
            this.Text = "Insert Image from URL";
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                ActiveControl = txtUrl;
                this.txtUrl.Focus();
            }
        }

        private void GlobalContext_ServerDeath(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(GlobalContext_ServerDeath), new object[] { sender, e });
                return;
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtUrl_GotFocus(object sender, EventArgs e)
        {
            if (ImageUrl == HTTP_PREFIX)
                txtUrl.Select(0, txtUrl.Text.Length);
        }

        private const string HTTP_PREFIX = "http://";

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtUrl.Text == String.Empty)
            {
                DisplayMessage.Show(MessageId.NoLinkTextSpecified, this);
                DialogResult = DialogResult.None;
            }
            //valid url--http:// and file:/// will pass
            else if (UrlHelper.IsUrl(txtUrl.Text))
            {
                DialogResult = DialogResult.OK;
            }
            //try cleaning up the address
            else if ((!txtUrl.Text.StartsWith("http://")) && (UrlHelper.IsUrl("http://" + txtUrl.Text)))
            {
                txtUrl.Text = "http://" + txtUrl.Text;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DisplayMessage.Show(MessageId.NoValidHyperlinkSpecified, this);
                DialogResult = DialogResult.None;
            }
        }

        public string ImageUrl
        {
            get
            {
                return UrlHelper.FixUpUrl(txtUrl.Text);
            }
        }

    }
}
