// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Video;
using BrowserHelper = OpenLiveWriter.CoreServices.BrowserHelper;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public class PanelLoginControl : BorderControl, IRtlAware
    {
        private Label lblUsername;
        private TextBoxOrComboBox txtUsername;
        private Label lblPassword;
        private Button btnLogin;
        private PictureBox pictureBoxLogo;
        private CheckBox ckBoxSavePassword;
        private Label lblEmailExample;
        private TextBox txtPassword;
        private Label lblStatus;
        private LinkLabel linkLabelCreateMicrosoftAccountID;
        private LinkLabel linkLabelPrivacy;

        public PanelLoginControl()
        {
            InitializeComponent();

            lblEmailExample.SizeChanged += new EventHandler(lblEmailExample_SizeChanged);

            BackColor = SystemInformation.HighContrast ? SystemColors.Window : SystemColors.ControlLightLight;

            txtPassword.PasswordChar = Res.PasswordChar;
            btnLogin.Text = Res.Get(StringId.Plugin_Video_Soapbox_Passport_Login);
            pictureBoxLogo.RightToLeft = RightToLeft.No;

            if (BidiHelper.IsRightToLeft)
            {
                RightToLeft = RightToLeft.Yes;
                BidiHelper.RtlLayoutFixup(this, true, true, Controls);
            }

            if (BidiHelper.IsRightToLeft)
                pictureBoxLogo.Left = txtUsername.Right - pictureBoxLogo.Width;
            else
                pictureBoxLogo.Left = txtUsername.Left;

            lblStatus.Font = Res.GetFont(FontSize.XLarge, FontStyle.Regular);
            lblStatus.ForeColor = SystemColors.ControlDarkDark;

            linkLabelCreateMicrosoftAccountID.Text = Res.Get(StringId.CWLiveIDCreateAccount2);
            linkLabelCreateMicrosoftAccountID.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
            linkLabelPrivacy.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
            linkLabelPrivacy.Text = Res.Get(StringId.LiveIDPrivacy);
            lblPassword.FlatStyle = FlatStyle.System;
            lblEmailExample.FlatStyle = FlatStyle.System;
            lblUsername.FlatStyle = FlatStyle.System;
            linkLabelCreateMicrosoftAccountID.FlatStyle = FlatStyle.System;
            linkLabelCreateMicrosoftAccountID.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabelPrivacy.FlatStyle = FlatStyle.System;
            linkLabelPrivacy.LinkBehavior = LinkBehavior.HoverUnderline;
        }

        void lblEmailExample_SizeChanged(object sender, EventArgs e)
        {

        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            base.ScaleCore(dx, dy);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshLayout();
        }

        public void SelectUserName()
        {
            txtUsername.Select();
        }

        public void SetService(IMediaSource source)
        {
            _auth = source.Auth;

            VideoServiceSettings settings = VideoSettings.GetServiceSettings(source.Id);
            txtUsername.Text = settings.Username;
            ckBoxSavePassword.Checked = !_auth.PasswordRequired(settings.Username);

            ckBoxSavePassword.Text = _auth.LoginSavePasswordText;
            lblEmailExample.Text = _auth.LoginExampleText;
            lblUsername.Text = _auth.LoginUsernameLabel;
            lblPassword.Text = _auth.LoginPasswordLabel;

            pictureBoxLogo.Image = _auth.LoginLogo;
            if (pictureBoxLogo.Image != null)
                pictureBoxLogo.Size = pictureBoxLogo.Image.Size;
            else
                pictureBoxLogo.Size = new Size(0, 0);

            RefreshLayout();
        }

        private IAuth _auth;

        private void RefreshLayout()
        {
            if (_auth == null)
                return;

            SuspendLayout();
            ckBoxSavePassword.Width = Width - ckBoxSavePassword.Left - 10;
            linkLabelCreateMicrosoftAccountID.Width = Width - linkLabelCreateMicrosoftAccountID.Left - 10;
            LayoutHelper.NaturalizeHeight(pictureBoxLogo, lblUsername, txtUsername, lblEmailExample, lblPassword, txtPassword, ckBoxSavePassword, linkLabelCreateMicrosoftAccountID, linkLabelPrivacy, btnLogin);

            if (ShowCreateMicrosoftAccountID)
                LayoutHelper.DistributeVertically(4, false, pictureBoxLogo, lblUsername, txtUsername, lblEmailExample, lblPassword, txtPassword, ckBoxSavePassword, linkLabelCreateMicrosoftAccountID, linkLabelPrivacy, btnLogin);
            else
                LayoutHelper.DistributeVertically(4, false, pictureBoxLogo, lblUsername, txtUsername, lblEmailExample, lblPassword, txtPassword, ckBoxSavePassword, btnLogin);

            ckBoxSavePassword.Visible = _auth.AllowSavePassword;

            lblUsername.Width =
                lblPassword.Width = lblEmailExample.Width = txtUsername.Width;
            lblUsername.Left =
                lblPassword.Left = lblEmailExample.Left = txtUsername.Left;

            if (BidiHelper.IsRightToLeft)
                pictureBoxLogo.Left = txtUsername.Right - pictureBoxLogo.Width;
            else
                pictureBoxLogo.Left = txtUsername.Left;

            Controls.Remove(lblStatus);

            DisplayHelper.AutoFitSystemButton(btnLogin, btnLogin.Width, int.MaxValue);
            LayoutHelper.FitControlsBelow(10, pictureBoxLogo);

            LayoutHelper.NaturalizeHeightAndDistribute(3, lblUsername, txtUsername, lblEmailExample);
            LayoutHelper.FitControlsBelow(10, lblEmailExample);
            if (ShowCreateMicrosoftAccountID)
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, lblPassword, txtPassword, ckBoxSavePassword);
                LayoutHelper.NaturalizeHeightAndDistribute(15, ckBoxSavePassword, new ControlGroup(linkLabelCreateMicrosoftAccountID, linkLabelPrivacy), btnLogin);
            }
            else
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, lblPassword, txtPassword, ckBoxSavePassword);
                LayoutHelper.NaturalizeHeightAndDistribute(15, ckBoxSavePassword, btnLogin);
            }

            if (BidiHelper.IsRightToLeft)
            {
                btnLogin.Left = txtPassword.Left;
                ckBoxSavePassword.Left = txtPassword.Right - ckBoxSavePassword.Width;
                linkLabelCreateMicrosoftAccountID.Left = txtPassword.Right - linkLabelCreateMicrosoftAccountID.Width;
                linkLabelPrivacy.Left = txtPassword.Right - linkLabelPrivacy.Width;
            }
            else
            {
                btnLogin.Left = txtPassword.Right - btnLogin.Width;
                ckBoxSavePassword.Left = linkLabelCreateMicrosoftAccountID.Left = linkLabelPrivacy.Left = txtPassword.Left;
                linkLabelCreateMicrosoftAccountID.Left -= 2;
                linkLabelPrivacy.Left -= 2;
            }

            Controls.Add(lblStatus);
            ResumeLayout();
        }

        private void InitializeComponent()
        {
            lblUsername = new Label();
            txtUsername = new TextBoxOrComboBox();
            lblPassword = new Label();
            lblStatus = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            pictureBoxLogo = new PictureBox();
            ckBoxSavePassword = new CheckBox();
            lblEmailExample = new Label();
            linkLabelCreateMicrosoftAccountID = new LinkLabel();
            linkLabelPrivacy = new LinkLabel();
            SuspendLayout();
            //
            // lblUsername
            //
            lblUsername.Location = new Point(48, 126);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(216, 18);
            lblUsername.TabIndex = 7;
            lblUsername.Text = "&Microsoft Account ID:";
            lblUsername.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                 | AnchorStyles.Right)));
            //
            // txtUsername
            //
            txtUsername.Location = new Point(48, 144);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(216, 23);
            txtUsername.TabIndex = 8;
            txtUsername.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                 | AnchorStyles.Right)));

            //
            // lblPassword
            //
            lblPassword.Location = new Point(48, 190);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(216, 18);
            lblPassword.TabIndex = 9;
            lblPassword.Text = "&Password:";
            lblPassword.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                 | AnchorStyles.Right)));
            //
            // linkLabelCreateMicrosoftAccountID
            //
            linkLabelCreateMicrosoftAccountID.Location = new Point(48, 100);
            linkLabelCreateMicrosoftAccountID.Name = "linkLabelCreateMicrosoftAccountID";
            linkLabelCreateMicrosoftAccountID.Size = new Size(216, 40);
            linkLabelCreateMicrosoftAccountID.Text = "Create Microsoft Account ID";
            linkLabelCreateMicrosoftAccountID.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelCreateMicrosoftAccountID_LinkClicked);
            linkLabelCreateMicrosoftAccountID.Visible = false;
            linkLabelCreateMicrosoftAccountID.TabIndex = 13;
            //
            // linkLabelPrivacy
            //
            linkLabelPrivacy.Location = new Point(48, 140);
            linkLabelPrivacy.Name = "linkLabelPrivacy";
            linkLabelPrivacy.Size = new Size(216, 40);
            linkLabelPrivacy.Text = "Privacy Policy";
            linkLabelPrivacy.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabelPrivacy_LinkClicked);
            linkLabelPrivacy.Visible = false;
            linkLabelCreateMicrosoftAccountID.TabIndex = 14;

            //
            // txtPassword
            //
            txtPassword.Location = new Point(48, 208);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(216, 20);
            txtPassword.TabIndex = 10;
            txtPassword.Text = "";
            txtPassword.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                             | AnchorStyles.Right)));
            //
            // btnLogin
            //
            btnLogin.FlatStyle = FlatStyle.System;
            btnLogin.Location = new Point(189, 256);
            btnLogin.Name = "btnLogin";
            btnLogin.TabIndex = 20;
            btnLogin.Text = "&Login";
            btnLogin.Click += new EventHandler(btnLogin_Click);
            btnLogin.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right));
            //
            // pictureBoxLogo
            //
            pictureBoxLogo.Location = new Point(48, 40);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(216, 50);
            pictureBoxLogo.TabStop = false;
            pictureBoxLogo.Click += new EventHandler(pictureBox1_Click);
            pictureBoxLogo.MouseEnter += new EventHandler(pictureBox1_MouseEnter);
            pictureBoxLogo.MouseLeave += new EventHandler(pictureBox1_MouseLeave);
            //
            // ckBoxSavePassword
            //
            ckBoxSavePassword.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                         )));
            ckBoxSavePassword.FlatStyle = FlatStyle.System;
            ckBoxSavePassword.Location = new Point(50, 232);
            ckBoxSavePassword.Name = "ckBoxSavePassword";
            ckBoxSavePassword.Size = new Size(216, 16);
            ckBoxSavePassword.TabIndex = 11;
            ckBoxSavePassword.Text = "Remember my &password";
            //
            // lblEmailExample
            //
            lblEmailExample.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
                                                       )));
            lblEmailExample.ForeColor = SystemColors.ControlDarkDark;
            lblEmailExample.FlatStyle = FlatStyle.System;
            lblEmailExample.Location = new Point(48, 168);
            lblEmailExample.Name = "lblEmailExample";
            lblEmailExample.Size = new Size(216, 16);
            lblEmailExample.Text = "(example555@hotmail.com)";

            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.FlatStyle = FlatStyle.System;
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Size = new Size(309, 56);
            lblStatus.Location = new Point(3, 165);

            //
            // SoapboxLoginControl
            //
            Controls.Add(linkLabelCreateMicrosoftAccountID);
            Controls.Add(linkLabelPrivacy);
            Controls.Add(lblEmailExample);
            Controls.Add(ckBoxSavePassword);
            Controls.Add(pictureBoxLogo);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblStatus);
            Name = "SoapboxLoginControl";
            Size = new Size(322, 399);
            Controls.SetChildIndex(btnLogin, 0);
            Controls.SetChildIndex(txtPassword, 0);
            Controls.SetChildIndex(lblPassword, 0);
            Controls.SetChildIndex(txtUsername, 0);
            Controls.SetChildIndex(lblUsername, 0);
            Controls.SetChildIndex(pictureBoxLogo, 0);
            Controls.SetChildIndex(ckBoxSavePassword, 0);
            Controls.SetChildIndex(lblEmailExample, 0);
            Controls.SetChildIndex(linkLabelPrivacy, 0);
            Controls.SetChildIndex(linkLabelCreateMicrosoftAccountID, 0);
            Controls.SetChildIndex(lblStatus, 0);
            ResumeLayout(false);
        }

        public List<Control> GetAccessibleControls()
        {
            List<Control> controls = new List<Control>();
            controls.Add(txtUsername);
            controls.Add(txtPassword);
            controls.Add(ckBoxSavePassword);
            controls.Add(linkLabelCreateMicrosoftAccountID);
            controls.Add(linkLabelPrivacy);
            controls.Add(btnLogin);
            return controls;
        }

        void linkLabelPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ShellHelper.LaunchUrl(GLink.Instance.PrivacyStatement);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception navigating to privacy statement: " + GLink.Instance.PrivacyStatement + ", " + ex.ToString());
            }
        }

        static void linkLabelCreateMicrosoftAccountID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ShellHelper.LaunchUrl(GLink.Instance.CreateMicrosoftAccountID);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception navigating to create live id page: " + GLink.Instance.CreateMicrosoftAccountID + ", " + ex.ToString());
            }
        }

        private bool _showCreateMicrosoftAccountID = false;
        public bool ShowCreateMicrosoftAccountID
        {
            get
            {
                Debug.Assert(linkLabelPrivacy.Visible == linkLabelCreateMicrosoftAccountID.Visible, "Link labels controlled by ShowCreateMicrosoftAccountID not in sync.");
                return _showCreateMicrosoftAccountID;
            }
            set
            {
                _showCreateMicrosoftAccountID = value;
                linkLabelPrivacy.Visible = value;
                linkLabelCreateMicrosoftAccountID.Visible = value;
                txtUsername.UseComboBox = value;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && !linkLabelCreateMicrosoftAccountID.Focused && !linkLabelPrivacy.Focused)
            {
                Login();
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void LoginStatus(bool loggingIn)
        {
            lblStatus.AutoSize = true;
            lblStatus.Text = Res.Get(StringId.Plugin_Video_Soapbox_LoggingIn); ;
            lblStatus.Visible = loggingIn;
            lblStatus.Top = Height / 2;
            lblStatus.Left = Width / 2 - lblStatus.Width / 2;
            lblStatus.BringToFront();

            txtUsername.Visible = txtPassword.Visible = pictureBoxLogo.Visible = btnLogin.Visible
                = lblUsername.Visible = lblPassword.Visible = lblEmailExample.Visible = !loggingIn;
            linkLabelPrivacy.Visible = linkLabelCreateMicrosoftAccountID.Visible
                = !loggingIn && ShowCreateMicrosoftAccountID;
            ckBoxSavePassword.Visible = (!loggingIn && _auth.AllowSavePassword);

            Refresh();
        }

        private void Login()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            MessageId msg = MessageId.UsernameAndPasswordRequired;

            if (username == String.Empty)
            {
                DisplayMessage.Show(msg, this, null);
                txtUsername.Focus();
                return;
            }
            if (password == String.Empty)
            {
                DisplayMessage.Show(msg, this, null);
                txtPassword.Focus();
                return;
            }
            LoginStatus(true);
            if (!_auth.Login(username, password, ckBoxSavePassword.Checked, true, FindForm()))
            {
                txtPassword.Focus();
            }
            LoginStatus(false);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            BrowserHelper.DisplayUrl(_auth.ServiceUrl);
        }

        internal void Clear()
        {
            txtUsername.Text = String.Empty;
            txtPassword.Text = String.Empty;
            ckBoxSavePassword.Checked = false;
        }

        void IRtlAware.Layout()
        {

        }
    }

    internal class TextBoxOrComboBox : UserControl
    {
        private readonly TextBox textBox = new TextBox();
        private readonly ComboBox comboBox = new ComboBox();
        private bool useComboBox = false;

        public TextBoxOrComboBox()
        {
            textBox.Dock = DockStyle.Fill;
            textBox.Name = "textBoxUsername";

            comboBox.Dock = DockStyle.Fill;
            comboBox.Name = "comboBoxUsername";

            Controls.Add(textBox);
            Controls.Add(comboBox);

            comboBox.Visible = false;
        }

        public bool UseComboBox
        {
            get
            {
                return useComboBox;
            }
            set
            {
                if (useComboBox != value)
                {
                    useComboBox = value;
                    comboBox.Visible = value;
                    textBox.Visible = !value;
                }
            }
        }

        public TextBox TextBox
        {
            get { return textBox; }
        }

        public ComboBox ComboBox
        {
            get { return comboBox; }
        }

        public new string Text
        {
            get
            {
                return UseComboBox ? comboBox.Text : textBox.Text;
            }
            set
            {
                (UseComboBox ? (Control)comboBox : textBox).Text = value;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            (UseComboBox ? (Control)comboBox : textBox).Focus();
        }
    }
}
