// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public class LoginStatusControl : UserControl
    {
        private Label labelStatus;
        private LinkLabel linkLabelAction;
        private IAuth _auth;

        public LoginStatusControl()
        {
            InitializeComponent();

            linkLabelAction.LinkColor = SystemInformation.HighContrast ? SystemColors.HotTrack : Color.FromArgb(0, 102, 204);
            linkLabelAction.FlatStyle = FlatStyle.System;
        }

        public IAuth Auth
        {
            set
            {
                DisposeOldAuth();
                _auth = value;

                // If the hosting control doesnt have an IAuth, we should hide ourselves
                // because there is no way to manage authentication.  This will happen when
                // we Mail is using the photo album feature and authentication is controlled by Mail
                if (_auth == null)
                {
                    this.Visible = false;
                    return;
                }
                else
                {
                    Visible = true;
                }

                _auth.LoginStatusChanged += _videoAuth_LoginStatusChanged;
                UpdateStatus();

                if (!_auth.IsLoggedIn)
                    TimerHelper.CallbackOnDelay(AutoLogin, 200);
            }
        }

        /// <summary>
        /// True to show the login hyperlink when the user is no logged in, false to hide it, most commonly because there is
        /// an external panel login on the tab
        /// </summary>
        private bool _showLoginButton = true;
        public bool ShowLoginButton
        {
            get
            {
                return _showLoginButton;
            }
            set
            {
                _showLoginButton = value;
            }
        }

        void AutoLogin()
        {
            if (_auth == null)
                return;

            _auth.Login(false, FindForm());
            UpdateStatus();
        }

        private void DisposeOldAuth()
        {
            if (_auth != null)
            {
                _auth.LoginStatusChanged -= _videoAuth_LoginStatusChanged;
                _auth = null;
            }

        }

        public event EventHandler LoginStatusChanged;
        public event EventHandler LoginClicked;
        public event EventHandler SwitchUserClicked;

        protected virtual void OnLoginStatusChanged()
        {
            if (LoginStatusChanged != null)
                LoginStatusChanged(this, EventArgs.Empty);
        }

        protected virtual void OnLoginClicked()
        {
            if (LoginClicked != null)
                LoginClicked(this, EventArgs.Empty);
        }

        protected virtual void OnSwitchUserClicked()
        {
            if (SwitchUserClicked != null)
                SwitchUserClicked(this, EventArgs.Empty);
        }

        void _videoAuth_LoginStatusChanged(object sender, EventArgs e)
        {
            UpdateStatus();
            OnLoginStatusChanged();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeOldAuth();
        }

        public void UpdateStatus()
        {
            if (_auth == null)
            {
                labelStatus.Text = "";
                return;
            }

            if (_auth.IsLoggedIn)
            {
                labelStatus.Text = _auth.Username;
                linkLabelAction.Text = Res.Get(StringId.Plugin_Video_Soapbox_Switch_User);
                linkLabelAction.Visible = true;
            }
            else
            {
                labelStatus.Text = Res.Get(StringId.Plugin_Video_Soapbox_Not_Logged_In);
                linkLabelAction.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Login);
                linkLabelAction.Visible = ShowLoginButton;
            }

            // Changing the width of the label in RTL mode after its already been
            // laid out causes UI issues, so we'll only do this in automation mode.
            if (ApplicationDiagnostics.AutomationMode)
            {
                linkLabelAction.Width = this.Width - 7;
                LayoutHelper.NaturalizeHeight(linkLabelAction);
            }
        }

        private void InitializeComponent()
        {
            this.labelStatus = new System.Windows.Forms.Label();
            this.linkLabelAction = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            //
            // labelStatus
            //
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelStatus.Location = new System.Drawing.Point(2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(154, 16);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Not Logged In";
            //
            // linkLabelAction
            //
            this.linkLabelAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelAction.Location = new System.Drawing.Point(0, 16);
            this.linkLabelAction.Name = "linkLabelAction";
            this.linkLabelAction.Size = new System.Drawing.Size(154, 16);
            this.linkLabelAction.TabIndex = 1;
            this.linkLabelAction.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.linkLabelAction.LinkColor = SystemColors.HotTrack;
            this.linkLabelAction.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelAction.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAction_LinkClicked);
            this.linkLabelAction.UseCompatibleTextRendering = false;
            //
            // LoginStatusControl
            //
            this.Controls.Add(this.linkLabelAction);
            this.Controls.Add(this.labelStatus);
            this.Name = "LoginStatusControl";
            this.Size = new System.Drawing.Size(161, 32);
            this.ResumeLayout(false);

        }

        private void linkLabelAction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _auth.Logout();

            if (_auth.IsLoggedIn)
            {
                OnSwitchUserClicked();
            }
            else
            {
                OnLoginClicked();
            }
        }
    }
}
