// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Configuration.Settings;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelComplete.
    /// </summary>
    public class WeblogConfigurationWizardPanelConfirmation : WeblogConfigurationWizardPanel
    {
        private System.Windows.Forms.CheckBox checkBoxSwitchToWeblog;
        private System.Windows.Forms.Label labelDesc;
        private System.Windows.Forms.Label labelDesc2;
        private System.Windows.Forms.TextBox textBoxWeblogName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Button buttonEditWeblogSettings;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private TemporaryBlogSettings _temporaryBlogSettings;

        public WeblogConfigurationWizardPanelConfirmation()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Text = Res.Get(StringId.CWConfirmThanks);
            checkBoxSwitchToWeblog.Text = Res.Get(StringId.CWConfirmSwitchToWeblog);
            labelDesc.Text = Res.Get(StringId.CWConfirmText);
            labelDesc2.Text = Res.Get(StringId.CWConfirmText2);
            labelDesc2.ForeColor = !SystemInformation.HighContrast ? Color.FromArgb(136, 136, 136) : SystemColors.GrayText;
            labelName.Text = Res.Get(StringId.WeblogNameColon);
            buttonEditWeblogSettings.Text = Res.Get(StringId.CWConfirmEditWeblogSettings);
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.Confirm; }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (shown)
            {
                RefreshLabels();
            }

        }

        private bool shown = false;
        public void ShowPanel(TemporaryBlogSettings temporaryBlogSettings, bool preventSwitchToWeblog)
        {
            shown = true;
            _temporaryBlogSettings = temporaryBlogSettings;
            textBoxWeblogName.Text = temporaryBlogSettings.BlogName;
            RefreshLabels();
            checkBoxSwitchToWeblog.Checked = temporaryBlogSettings.SwitchToWeblog;
            buttonEditWeblogSettings.Visible = false;
            //buttonEditWeblogSettings.Visible = temporaryBlogSettings.IsNewWeblog ;
            checkBoxSwitchToWeblog.Visible = temporaryBlogSettings.IsNewWeblog && !preventSwitchToWeblog;

            try
            {
                string appId = AppId;
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
            }
        }

        private string AppId
        {
            get
            {
                IBlogProvider provider = !string.IsNullOrEmpty(_temporaryBlogSettings.ProviderId)
                    ? BlogProviderManager.FindProvider(_temporaryBlogSettings.ProviderId)
                    : null;

                string appid = provider != null ? provider.AppId : null;

                if (string.IsNullOrEmpty(appid))
                {
                    IBlogProvider defaultProvider = BlogProviderManager.FindProvider("BAF4FE16-25FB-4a94-90C7-11A1B30CAD61");
                    if (defaultProvider != null)
                        appid = defaultProvider.AppId;
                }
                return appid;
            }
        }

        private void RefreshLabels()
        {
            // descriptive string for file upload settings
            string uploadText = "";
            switch (_temporaryBlogSettings.FileUploadSupport)
            {
                case FileUploadSupport.FTP:
                    uploadText = Res.Get(StringId.FileUploadFTPServer);
                    break;
                case FileUploadSupport.Weblog:
                    uploadText = Res.Get(StringId.FileUploadUploadToWeblog);
                    break;
                default:
                    Trace.Fail("Unexpected value for FileUploadSupport: " + _temporaryBlogSettings.FileUploadSupport.ToString());
                    goto case FileUploadSupport.Weblog;
            }
        }

        public string WeblogName
        {
            get { return textBoxWeblogName.Text.Trim(); }
        }

        public bool SwitchToWeblog
        {
            get { return checkBoxSwitchToWeblog.Checked; }
        }

        public override void NaturalizeLayout()
        {
            MaximizeWidth(labelDesc);
            MaximizeWidth(labelDesc2);
            MaximizeWidth(labelName);
            MaximizeWidth(checkBoxSwitchToWeblog);

            DisplayHelper.AutoFitSystemButton(buttonEditWeblogSettings, buttonEditWeblogSettings.Width, int.MaxValue);

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelName, textBoxWeblogName);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelDesc, labelDesc2);
            LayoutHelper.NaturalizeHeightAndDistribute(10,
                                                       new ControlGroup(labelDesc, labelDesc2),
                                                       new ControlGroup(labelName, textBoxWeblogName),
                                                       //buttonEditWeblogSettings,
                                                       checkBoxSwitchToWeblog);
        }

        public override bool ValidatePanel()
        {
            if (WeblogName == String.Empty)
            {
                DisplayMessage.Show(MessageId.RequiredFieldOmitted, FindForm(), Res.Get(StringId.CWConfirmWeblogName));
                textBoxWeblogName.Focus();
                return false;
            }

            return true;
        }

        private void buttonEditWeblogSettings_Click(object sender, System.EventArgs e)
        {
            try
            {
                WeblogSettingsManager.EditSettings(FindForm(), _temporaryBlogSettings, false, typeof(AccountPanel));
                RefreshLabels();
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(FindForm(), ex);
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
            this.checkBoxSwitchToWeblog = new System.Windows.Forms.CheckBox();
            this.labelDesc = new System.Windows.Forms.Label();
            this.labelDesc2 = new System.Windows.Forms.Label();
            this.textBoxWeblogName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.buttonEditWeblogSettings = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.buttonEditWeblogSettings);
            this.panelMain.Controls.Add(this.textBoxWeblogName);
            this.panelMain.Controls.Add(this.labelName);
            this.panelMain.Controls.Add(this.labelDesc);
            this.panelMain.Controls.Add(this.labelDesc2);
            this.panelMain.Controls.Add(this.checkBoxSwitchToWeblog);
            //
            // checkBoxSwitchToWeblog
            //
            this.checkBoxSwitchToWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSwitchToWeblog.Location = new System.Drawing.Point(20, 197);
            this.checkBoxSwitchToWeblog.Name = "checkBoxSwitchToWeblog";
            this.checkBoxSwitchToWeblog.Size = new System.Drawing.Size(324, 24);
            this.checkBoxSwitchToWeblog.TabIndex = 15;
            this.checkBoxSwitchToWeblog.Text = "&Switch to this weblog now";
            //
            // labelDesc
            //
            this.labelDesc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDesc.Location = new System.Drawing.Point(20, 0);
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.Size = new System.Drawing.Size(344, 80);
            this.labelDesc.TabIndex = 0;
            this.labelDesc.Text = "Please confirm that you would like to save these settings.";
            //
            // labelDesc2
            //
            this.labelDesc2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDesc2.Location = new System.Drawing.Point(20, 0);
            this.labelDesc2.Name = "labelDesc2";
            this.labelDesc2.Size = new System.Drawing.Size(344, 80);
            this.labelDesc2.TabIndex = 0;
            this.labelDesc2.Text = "Writer will periodically check for, and download, new configuration information for your blog.";
            //
            // textBoxWeblogName
            //
            this.textBoxWeblogName.Location = new System.Drawing.Point(20, 64);
            this.textBoxWeblogName.Name = "textBoxWeblogName";
            this.textBoxWeblogName.Size = new System.Drawing.Size(224, 20);
            this.textBoxWeblogName.TabIndex = 2;
            //
            // labelName
            //
            this.labelName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelName.Location = new System.Drawing.Point(20, 48);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(144, 16);
            this.labelName.TabIndex = 1;
            this.labelName.Text = "&Weblog Name:";
            //
            // buttonEditWeblogSettings
            //
            this.buttonEditWeblogSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEditWeblogSettings.Location = new System.Drawing.Point(20, 160);
            this.buttonEditWeblogSettings.Name = "buttonEditWeblogSettings";
            this.buttonEditWeblogSettings.Size = new System.Drawing.Size(130, 23);
            this.buttonEditWeblogSettings.TabIndex = 10;
            this.buttonEditWeblogSettings.Text = "Edit Settings...";
            this.buttonEditWeblogSettings.Click += new System.EventHandler(this.buttonEditWeblogSettings_Click);
            //
            // WeblogConfigurationWizardPanelConfirmation
            //
            this.Name = "WeblogConfigurationWizardPanelConfirmation";
            this.Size = new System.Drawing.Size(432, 283);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void linkLabelPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.CEIP);
        }

    }
}
