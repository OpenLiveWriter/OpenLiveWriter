// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Summary description for GDataCaptchaForm.
    /// </summary>
    public class GDataCaptchaForm : BaseForm
    {
        private System.Windows.Forms.TextBox txtCaptcha;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.PictureBox picCaptcha;
        private System.Windows.Forms.LinkLabel linkLabel1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public GDataCaptchaForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.label1.Text = Res.Get(StringId.GDataCaptchaPrompt);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.btnOK.Text = Res.Get(StringId.OKButtonText);
            this.linkLabel1.Text = Res.Get(StringId.GDataCaptchaAlternate);
            this.Text = Res.Get(StringId.GDataCaptchaTitle);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(4, label1, linkLabel1);
                LayoutHelper.DistributeVertically(12, false, linkLabel1, picCaptcha);
                LayoutHelper.DistributeVertically(4, picCaptcha, txtCaptcha);
                LayoutHelper.DistributeVertically(12, false, txtCaptcha, new ControlGroup(btnOK, btnCancel));
                LayoutHelper.FixupOKCancel(btnOK, btnCancel);
            }
        }

        public string Reply { get { return txtCaptcha.Text; } }

        public void SetImage(Image img)
        {
            picCaptcha.Image = img;
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
            this.picCaptcha = new System.Windows.Forms.PictureBox();
            this.txtCaptcha = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).BeginInit();
            this.SuspendLayout();
            //
            // picCaptcha
            //
            this.picCaptcha.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picCaptcha.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picCaptcha.Location = new System.Drawing.Point(10, 91);
            this.picCaptcha.Name = "picCaptcha";
            this.picCaptcha.Size = new System.Drawing.Size(197, 80);
            this.picCaptcha.TabIndex = 0;
            this.picCaptcha.TabStop = false;
            //
            // txtCaptcha
            //
            this.txtCaptcha.Location = new System.Drawing.Point(10, 178);
            this.txtCaptcha.Name = "txtCaptcha";
            this.txtCaptcha.Size = new System.Drawing.Size(197, 23);
            this.txtCaptcha.TabIndex = 0;
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 37);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type the characters you see in the picture below.";
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(130, 190);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(77, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            //
            // btnOK
            //
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(43, 190);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(77, 26);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            //
            // linkLabel1
            //
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabel1.Location = new System.Drawing.Point(10, 46);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(197, 36);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Visually impaired users can access an audio version of the test here.";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            //
            // GDataCaptchaForm
            //
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(216, 224);
            this.ControlBox = false;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtCaptcha);
            this.Controls.Add(this.picCaptcha);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "GDataCaptchaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Unlock Google Account";
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.google.com/accounts/DisplayUnlockCaptcha?service=blogger");
            DialogResult = DialogResult.Cancel;
        }
    }

    internal class GDataCaptchaHelper
    {
        private readonly IWin32Window _owner;
        private readonly string _imageUrl;
        private DialogResult _dialogResult;
        private string _reply;

        public GDataCaptchaHelper(IWin32Window owner, string imageUrl)
        {
            _owner = owner;
            _imageUrl = UrlHelper.UrlCombineIfRelative("http://www.google.com/accounts/", imageUrl);
        }

        public DialogResult DialogResult { get { return _dialogResult; } }
        public string Reply { get { return _reply; } }

        public void ShowCaptcha()
        {
            HttpWebResponse response = HttpRequestHelper.SendRequest(_imageUrl);
            Image image;
            using (Stream s = response.GetResponseStream())
            {
                image = Bitmap.FromStream(new MemoryStream(StreamHelper.AsBytes(s)));
            }

            using (image)
            {
                using (GDataCaptchaForm form = new GDataCaptchaForm())
                {
                    form.SetImage(image);
                    _dialogResult = form.ShowDialog(_owner);
                    if (_dialogResult == DialogResult.OK)
                        _reply = form.Reply;
                }
            }
        }
    }
}
