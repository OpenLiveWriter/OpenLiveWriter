// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Summary description for PassportLoginControl.
    /// </summary>
    public class BlogClientLoginControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.CheckBox checkBoxSavePassword;
        private System.Windows.Forms.Label textboxLoginDomain;
        private System.Windows.Forms.Label textboxLoginDomainDescription;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public BlogClientLoginControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            textBoxPassword.PasswordChar = Res.PasswordChar;

            label1.Text = Res.Get(StringId.UsernameLabel);
            label2.Text = Res.Get(StringId.PasswordLabel);
            checkBoxSavePassword.Text = Res.Get(StringId.RememberPassword);

            if (!DesignMode)
            {
                textboxLoginDomain.Text = String.Empty;
                textboxLoginDomainDescription.Text = String.Empty;
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

                if (_domainIcon != null)
                    _domainIcon.Dispose();
                if (_domainImage != null)
                    _domainImage.Dispose();
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
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxEmail = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.checkBoxSavePassword = new System.Windows.Forms.CheckBox();
            this.textboxLoginDomain = new System.Windows.Forms.Label();
            this.textboxLoginDomainDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(24, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Username:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(24, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Password:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // textBoxEmail
            //
            this.textBoxEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxEmail.Location = new System.Drawing.Point(24, 56);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new System.Drawing.Size(189, 20);
            this.textBoxEmail.TabIndex = 1;
            //
            // textBoxPassword
            //
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(24, 96);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(189, 20);
            this.textBoxPassword.TabIndex = 3;
            //
            // checkBoxSavePassword
            //
            this.checkBoxSavePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSavePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(24, 120);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(189, 32);
            this.checkBoxSavePassword.TabIndex = 4;
            this.checkBoxSavePassword.Text = "&Remember my password";
            this.checkBoxSavePassword.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // textboxLoginDomain
            //
            this.textboxLoginDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxLoginDomain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.textboxLoginDomain.Location = new System.Drawing.Point(24, 0);
            this.textboxLoginDomain.Name = "textboxLoginDomain";
            this.textboxLoginDomain.Size = new System.Drawing.Size(189, 16);
            this.textboxLoginDomain.TabIndex = 4;
            this.textboxLoginDomain.Text = "Blog Service";
            //
            // textboxLoginDomainDescription
            //
            this.textboxLoginDomainDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxLoginDomainDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.textboxLoginDomainDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textboxLoginDomainDescription.Location = new System.Drawing.Point(24, 16);
            this.textboxLoginDomainDescription.Name = "textboxLoginDomainDescription";
            this.textboxLoginDomainDescription.Size = new System.Drawing.Size(189, 24);
            this.textboxLoginDomainDescription.TabIndex = 5;
            this.textboxLoginDomainDescription.Text = "(Blog Title)";
            //
            // BlogClientLoginControl
            //
            this.Controls.Add(this.textboxLoginDomainDescription);
            this.Controls.Add(this.textboxLoginDomain);
            this.Controls.Add(this.checkBoxSavePassword);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxEmail);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BlogClientLoginControl";
            this.Size = new System.Drawing.Size(216, 168);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, label1, textBoxEmail);
                LayoutHelper.NaturalizeHeightAndDistribute(3, label2, textBoxPassword, checkBoxSavePassword);
                LayoutHelper.NaturalizeHeightAndDistribute(8, new ControlGroup(label1, textBoxEmail), new ControlGroup(label2, textBoxPassword, checkBoxSavePassword));
            }

            //force the initial focus to the password control if there is already a user name
            if (!string.IsNullOrEmpty(textBoxEmail.Text))
                textBoxPassword.Select();
        }

        public string UserName
        {
            get { return textBoxEmail.Text; }
            set { textBoxEmail.Text = value; }
        }

        public string Password
        {
            get { return textBoxPassword.Text; }
            set { textBoxPassword.Text = value; }
        }

        public bool SavePassword
        {
            get { return checkBoxSavePassword.Checked; }
            set { checkBoxSavePassword.Checked = value; }
        }

        public ICredentialsDomain Domain
        {
            get { return _domain; }
            set
            {
                _domain = value;
                if (_domainIcon != null)
                    _domainIcon.Dispose();
                _domainIcon = null;

                if (_domainImage != null)
                    _domainImage.Dispose();
                _domainImage = null;

                if (_domain != null)
                {
                    textboxLoginDomain.Text = _domain.Name != null ? _domain.Name : String.Empty;
                    textboxLoginDomainDescription.Text = _domain.Description != null ? _domain.Description : String.Empty;
                }
            }
        }
        private ICredentialsDomain _domain;
        private Icon _domainIcon;
        private Image _domainImage;

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            _domainImageSize = new Size((int)(16 * scale.X), (int)(16 * scale.Y));

            if (_domain != null)
            {
                checkBoxSavePassword.Visible = _domain.AllowsSavePassword;

                if (_domainImage == null && _domainIcon == null)
                {
                    try
                    {
                        if (_domain.Image != null)
                        {
                            _domainImage = new Bitmap(new MemoryStream(_domain.Image));
                        }
                        else if (_domain.Icon != null)
                        {
                            _domainIcon = new Icon(new MemoryStream(_domain.Icon), _domainImageSize.Width, _domainImageSize.Height);
                        }
                        else
                        {
                            Icon ico = ApplicationEnvironment.ProductIconSmall;
                            _domainIcon = new Icon(ico, ico.Size);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (_domainImage == null && _domainIcon == null)
            {
                Icon appIcon = ApplicationEnvironment.ProductIconSmall;
                _domainIcon = new Icon(appIcon, appIcon.Width, appIcon.Height);
            }
        }
        private Size _domainImageSize;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);
            // draw icon
            if (_domainImage != null)
                g.DrawImage(false, _domainImage, new Rectangle(0, 0, _domainImageSize.Width, _domainImageSize.Height));
            else if (_domainIcon != null)
                g.DrawIcon(false, _domainIcon, new Rectangle(0, 0, _domainImageSize.Width, _domainImageSize.Height));
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }

        private PointF scale = new PointF(1f, 1f);

    }
}
