// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    /// <summary>
    /// Summary description for WebProxyPreferencesPanel.
    /// </summary>
    public class WebProxyPreferencesPanel : PreferencesPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private GroupBox groupBoxProxy;
        private Panel panelProxySettings;
        private TextBox proxyPassword;
        private Label proxyPasswordLabel;
        private TextBox proxyUsername;
        private Label proxyUsernameLabel;
        private TextBox proxyPort;
        private Label proxyPortLabel;
        private TextBox proxyServer;
        private Label proxyServerLabel;
        private CheckBox proxyEnabled;

        private WebProxyPreferences _webProxyPreferences;

        public WebProxyPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            groupBoxProxy.Text = Res.Get(StringId.ProxyPrefCustom);
            proxyPasswordLabel.Text = Res.Get(StringId.ProxyPrefPassword);
            proxyPassword.AccessibleName = ControlHelper.ToAccessibleName(proxyPasswordLabel.Text);
            proxyUsernameLabel.Text = Res.Get(StringId.ProxyPrefUsername);
            proxyUsername.AccessibleName = ControlHelper.ToAccessibleName(proxyUsernameLabel.Text);
            proxyPortLabel.Text = Res.Get(StringId.ProxyPrefPort);
            proxyPort.AccessibleName = ControlHelper.ToAccessibleName(proxyPortLabel.Text);
            proxyServerLabel.Text = Res.Get(StringId.ProxyPrefServer);
            proxyServer.AccessibleName = ControlHelper.ToAccessibleName(proxyServerLabel.Text);
            proxyEnabled.Text = Res.Get(StringId.ProxyPrefEnabled);
            PanelName = Res.Get(StringId.ProxyPrefName);

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Preferences.Images.WebProxyPanelBitmap.png");

            _webProxyPreferences = new WebProxyPreferences();
            _webProxyPreferences.PreferencesModified += new EventHandler(_connectionsPreferences_PreferencesModified);

            proxyEnabled.Checked = _webProxyPreferences.ProxyEnabled;
            proxyServer.Text = _webProxyPreferences.Hostname;
            proxyPort.Text = _webProxyPreferences.Port.ToString(CultureInfo.CurrentCulture);
            proxyUsername.Text = _webProxyPreferences.Username;
            proxyPassword.Text = _webProxyPreferences.Password;

            // give password field nice round circle
            proxyPassword.PasswordChar = Res.PasswordChar;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(groupBoxProxy, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeight(proxyEnabled);
                LayoutHelper.FitControlsBelow(8, proxyEnabled);
                LayoutHelper.NaturalizeHeight(proxyServerLabel, proxyPortLabel);
                AlignHelper.AlignBottom(proxyServerLabel, proxyPortLabel);
                LayoutHelper.FitControlsBelow(3, proxyServerLabel);
            }
        }

        public override void Save()
        {
            ApplyProxyPortToPreferences();

            //the no proxy address is set, then disable the proxy settings.
            if (proxyEnabled.Checked)
            {
                if (proxyServer.Text == String.Empty)
                    proxyEnabled.Checked = false;
            }
            if (proxyServer.Text == String.Empty)
                proxyEnabled.Checked = false;

            if (_webProxyPreferences.IsModified())
                _webProxyPreferences.Save();
        }

        private void _connectionsPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        private void proxyEnabled_CheckedChanged(object sender, EventArgs e)
        {
            panelProxySettings.Enabled = proxyEnabled.Checked;
            _webProxyPreferences.ProxyEnabled = proxyEnabled.Checked;
        }

        private void proxyServer_TextChanged(object sender, EventArgs e)
        {
            _webProxyPreferences.Hostname = proxyServer.Text;
        }

        private void proxyPort_TextChanged(object sender, EventArgs e)
        {
            //don't use the value in the control yet since it may not yet be a number.
            //set the modified flag so that the apply button gets enabled.
            OnModified(EventArgs.Empty);
        }

        private void proxyPort_Leave(object sender, EventArgs e)
        {
            ApplyProxyPortToPreferences();
        }

        private void ApplyProxyPortToPreferences()
        {
            try
            {
                int portValue = Int32.Parse(proxyPort.Text, CultureInfo.CurrentCulture);
                if (_webProxyPreferences.Port != portValue)
                    _webProxyPreferences.Port = portValue;
            }
            catch (Exception)
            {
                //the number is malformed, so revert the value
                proxyPort.Text = _webProxyPreferences.Port.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void proxyUsername_TextChanged(object sender, EventArgs e)
        {
            _webProxyPreferences.Username = proxyUsername.Text;
        }

        private void proxyPassword_TextChanged(object sender, EventArgs e)
        {
            _webProxyPreferences.Password = proxyPassword.Text;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webProxyPreferences.PreferencesModified -= new EventHandler(_connectionsPreferences_PreferencesModified);

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
            this.groupBoxProxy = new System.Windows.Forms.GroupBox();
            this.panelProxySettings = new System.Windows.Forms.Panel();
            this.proxyPassword = new System.Windows.Forms.TextBox();
            this.proxyPasswordLabel = new System.Windows.Forms.Label();
            this.proxyUsername = new System.Windows.Forms.TextBox();
            this.proxyUsernameLabel = new System.Windows.Forms.Label();
            this.proxyPort = new System.Windows.Forms.TextBox();
            this.proxyPortLabel = new System.Windows.Forms.Label();
            this.proxyServer = new System.Windows.Forms.TextBox();
            this.proxyServerLabel = new System.Windows.Forms.Label();
            this.proxyEnabled = new System.Windows.Forms.CheckBox();
            this.groupBoxProxy.SuspendLayout();
            this.panelProxySettings.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxProxy
            //
            this.groupBoxProxy.Controls.Add(this.panelProxySettings);
            this.groupBoxProxy.Controls.Add(this.proxyEnabled);
            this.groupBoxProxy.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxProxy.Location = new System.Drawing.Point(8, 32);
            this.groupBoxProxy.Name = "groupBoxProxy";
            this.groupBoxProxy.Size = new System.Drawing.Size(345, 155);
            this.groupBoxProxy.TabIndex = 1;
            this.groupBoxProxy.TabStop = false;
            this.groupBoxProxy.Text = "Custom proxy settings";
            //
            // panelProxySettings
            //
            this.panelProxySettings.Controls.Add(this.proxyPassword);
            this.panelProxySettings.Controls.Add(this.proxyPasswordLabel);
            this.panelProxySettings.Controls.Add(this.proxyUsername);
            this.panelProxySettings.Controls.Add(this.proxyUsernameLabel);
            this.panelProxySettings.Controls.Add(this.proxyPort);
            this.panelProxySettings.Controls.Add(this.proxyPortLabel);
            this.panelProxySettings.Controls.Add(this.proxyServer);
            this.panelProxySettings.Controls.Add(this.proxyServerLabel);
            this.panelProxySettings.Enabled = false;
            this.panelProxySettings.Location = new System.Drawing.Point(26, 42);
            this.panelProxySettings.Name = "panelProxySettings";
            this.panelProxySettings.Size = new System.Drawing.Size(303, 95);
            this.panelProxySettings.TabIndex = 2;
            //
            // proxyPassword
            //
            this.proxyPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyPassword.Location = new System.Drawing.Point(152, 64);
            this.proxyPassword.Name = "proxyPassword";
            this.proxyPassword.PasswordChar = '*';
            this.proxyPassword.Size = new System.Drawing.Size(142, 20);
            this.proxyPassword.TabIndex = 7;
            this.proxyPassword.Text = "";
            this.proxyPassword.Leave += new System.EventHandler(this.proxyPassword_TextChanged);
            //
            // label12
            //
            this.proxyPasswordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyPasswordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.proxyPasswordLabel.Location = new System.Drawing.Point(152, 49);
            this.proxyPasswordLabel.Name = "proxyPasswordLabel";
            this.proxyPasswordLabel.Size = new System.Drawing.Size(135, 18);
            this.proxyPasswordLabel.TabIndex = 6;
            this.proxyPasswordLabel.Text = "Pass&word:";
            this.proxyPasswordLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyUsername
            //
            this.proxyUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyUsername.Location = new System.Drawing.Point(0, 64);
            this.proxyUsername.Name = "proxyUsername";
            this.proxyUsername.Size = new System.Drawing.Size(144, 20);
            this.proxyUsername.TabIndex = 5;
            this.proxyUsername.Text = "";
            this.proxyUsername.Leave += new System.EventHandler(this.proxyUsername_TextChanged);
            //
            // label10
            //
            this.proxyUsernameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.proxyUsernameLabel.Location = new System.Drawing.Point(0, 49);
            this.proxyUsernameLabel.Name = "proxyUsernameLabel";
            this.proxyUsernameLabel.Size = new System.Drawing.Size(137, 18);
            this.proxyUsernameLabel.TabIndex = 4;
            this.proxyUsernameLabel.Text = "User&name:";
            this.proxyUsernameLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyPort
            //
            this.proxyPort.Location = new System.Drawing.Point(218, 18);
            this.proxyPort.Name = "proxyPort";
            this.proxyPort.Size = new System.Drawing.Size(76, 20);
            this.proxyPort.TabIndex = 3;
            this.proxyPort.Text = "8080";
            this.proxyPort.TextChanged += new System.EventHandler(this.proxyPort_TextChanged);
            this.proxyPort.Leave += new System.EventHandler(this.proxyPort_Leave);
            //
            // label9
            //
            this.proxyPortLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.proxyPortLabel.Location = new System.Drawing.Point(218, 3);
            this.proxyPortLabel.Name = "proxyPortLabel";
            this.proxyPortLabel.Size = new System.Drawing.Size(76, 18);
            this.proxyPortLabel.TabIndex = 2;
            this.proxyPortLabel.Text = "&Port:";
            this.proxyPortLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyServer
            //
            this.proxyServer.Location = new System.Drawing.Point(0, 18);
            this.proxyServer.Name = "proxyServer";
            this.proxyServer.Size = new System.Drawing.Size(210, 20);
            this.proxyServer.TabIndex = 1;
            this.proxyServer.Text = "";
            this.proxyServer.TextChanged += new System.EventHandler(this.proxyServer_TextChanged);
            //
            // label8
            //
            this.proxyServerLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.proxyServerLabel.Location = new System.Drawing.Point(0, 3);
            this.proxyServerLabel.Name = "proxyServerLabel";
            this.proxyServerLabel.Size = new System.Drawing.Size(208, 18);
            this.proxyServerLabel.TabIndex = 0;
            this.proxyServerLabel.Text = "Proxy &server address:";
            this.proxyServerLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyEnabled
            //
            this.proxyEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.proxyEnabled.Location = new System.Drawing.Point(10, 20);
            this.proxyEnabled.Name = "proxyEnabled";
            this.proxyEnabled.Size = new System.Drawing.Size(324, 18);
            this.proxyEnabled.TabIndex = 0;
            this.proxyEnabled.Text = "&Specify custom proxy server settings";
            this.proxyEnabled.CheckedChanged += new System.EventHandler(this.proxyEnabled_CheckedChanged);
            //
            // WebProxyPreferencesPanel
            //
            this.AccessibleName = "Web Proxy";
            this.Controls.Add(this.groupBoxProxy);
            this.Name = "WebProxyPreferencesPanel";
            this.PanelName = "Web Proxy";
            this.Controls.SetChildIndex(this.groupBoxProxy, 0);
            this.groupBoxProxy.ResumeLayout(false);
            this.panelProxySettings.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
