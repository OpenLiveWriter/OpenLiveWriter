// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.FileDestinations;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    /// <summary>
    /// Summary description for FTPSettingsForm.
    /// </summary>
    public class FTPSettingsForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.PictureBox pictureBoxHelp;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelUrlMapping;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Label labelHostName;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxUrlMapping;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.TextBox textBoxHostName;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string _originalHostname;
        private string _originalUsername;
        private string _originalPassword;
        private string _originalPath;
        private string _originalUrlMapping;
        private System.Windows.Forms.CheckBox checkBoxSavePassword;
        private FtpUploaderSettings _ftpSettings;

        public FTPSettingsForm(FtpUploaderSettings ftpSettings)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            buttonCancel.Text = Res.Get(StringId.CancelButton);
            buttonOK.Text = Res.Get(StringId.OKButtonText);
            linkLabel1.Text = Res.Get(StringId.FtpHelp);
            labelUrlMapping.Text = Res.Get(StringId.FtpFolderUrl);
            textBoxUrlMapping.RightToLeft = RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                textBoxUrlMapping.TextAlign = HorizontalAlignment.Right;
            labelPath.Text = Res.Get(StringId.FtpFolderPath);
            labelHostName.Text = Res.Get(StringId.FtpHostname);
            labelPassword.Text = Res.Get(StringId.FtpPassword);
            labelUserName.Text = Res.Get(StringId.FtpUsername);
            checkBoxSavePassword.Text = Res.Get(StringId.RememberPassword);
            Text = Res.Get(StringId.FtpText);

            ToolTip2 tip = new ToolTip2();
            tip.SetToolTip(buttonBrowse, Res.Get(StringId.PublishFolderPickerToolTip));

            //marketization
            if (!MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.FTPHelp))
            {
                linkLabel1.Visible = false;
                pictureBoxHelp.Visible = false;
            }

            // pretty password chars
            textBoxPassword.PasswordChar = Res.PasswordChar;

            // save reference to settings
            _ftpSettings = ftpSettings;

            // populate controls
            _originalHostname = ftpSettings.FtpServer;
            textBoxHostName.Text = ftpSettings.FtpServer;

            _originalUsername = ftpSettings.Username;
            textBoxUsername.Text = ftpSettings.Username;

            _originalPassword = ftpSettings.Password;
            textBoxPassword.Text = ftpSettings.Password;
            checkBoxSavePassword.Checked = ftpSettings.Password != String.Empty;

            _originalPath = ftpSettings.PublishPath;
            textBoxPath.Text = ftpSettings.PublishPath;

            _originalUrlMapping = ftpSettings.UrlMapping;
            textBoxUrlMapping.Text = ftpSettings.UrlMapping;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelPath, new ControlGroup(textBoxPath, buttonBrowse));
                LayoutHelper.NaturalizeHeightAndDistribute(3, labelUrlMapping, textBoxUrlMapping);
                LayoutHelper.NaturalizeHeightAndDistribute(8, new ControlGroup(labelPath, textBoxPath, buttonBrowse), new ControlGroup(labelUrlMapping, textBoxUrlMapping), new ControlGroup(pictureBoxHelp, linkLabel1), new ControlGroup(buttonOK, buttonCancel));
            }
        }

        private string HostName
        {
            get { return textBoxHostName.Text.Trim(); }
        }

        private string Username
        {
            get { return textBoxUsername.Text.Trim(); }
        }

        private string Password
        {
            get { return textBoxPassword.Text.Trim(); }
        }

        private string Path
        {
            get { return textBoxPath.Text.Trim(); }
        }

        private string UrlMapping
        {
            get { return textBoxUrlMapping.Text.Trim(); }
        }

        private bool Dirty
        {
            get
            {
                return !(HostName == _originalHostname &&
                    Password == _originalPassword &&
                    Username == _originalUsername &&
                    Path == _originalPath &&
                    UrlMapping == _originalUrlMapping);
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (ValidateForm(false))
            {
                _ftpSettings.FtpServer = HostName;
                _ftpSettings.Username = Username;
                _ftpSettings.Password = checkBoxSavePassword.Checked ? Password : String.Empty;
                _ftpSettings.PublishPath = Path;
                _ftpSettings.UrlMapping = UrlMapping;

                DialogResult = DialogResult.OK;
            }

        }

        private bool ValidateForm(bool requirePassword)
        {
            using (new WaitCursor())
            {
                if (!ValidateRequiredFields(true, checkBoxSavePassword.Checked))
                    return false;

                if (!Dirty)
                    return true;

                if (!ValidateFTPConnection(GetDestinationForFields(this.textBoxPath.Text.Trim()), true, true))
                    return false;

                return true;
            }
        }

        private bool ValidateRequiredFields(bool validateMapping, bool requirePassword)
        {
            if (!ValidateTextBox(textBoxHostName))
            {
                DisplayMessage.Show(MessageId.HostNameRequired, FindForm());
                textBoxHostName.Focus();
                return false;
            }

            if (!ValidateTextBox(textBoxUsername))
            {
                DisplayMessage.Show(MessageId.UsernameAndPasswordRequired, FindForm());
                textBoxUsername.Focus();
                return false;
            }

            if (requirePassword && !ValidateTextBox(textBoxPassword))
            {
                DisplayMessage.Show(MessageId.UsernameAndPasswordRequired, FindForm());
                textBoxPassword.Focus();
                return false;
            }

            if (validateMapping && !ValidateTextBox(textBoxUrlMapping))
            {
                DisplayMessage.Show(MessageId.UrlMappingRequired, FindForm());
                textBoxUrlMapping.Focus();
                return false;
            }
            return true;
        }

        private bool ValidateTextBox(TextBox textBox)
        {
            return textBox.Text.Trim().Length > 0;
        }

        private WinInetFTPFileDestination GetDestinationForFields(string initialPath)
        {
            try
            {
                return new WinInetFTPFileDestination(
                    HostName,
                    initialPath,
                    Username,
                    Password
                    );
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool ValidateFTPConnection(WinInetFTPFileDestination destination, bool verifyMapping, bool permitIgnoringErrors)
        {
            if (destination == null)
            {
                DisplayMessage.Show(MessageId.ErrorConnecting, FindForm());
                return false;
            }

            using (DestinationValidator validator = new DestinationValidator(destination))
            {
                try
                {
                    validator.Validate(verifyMapping ? textBoxUrlMapping.Text : null);
                }
                catch (DestinationValidator.DestinationValidationException ex)
                {
                    if (ex is DestinationValidator.DestinationLoginFailedException)
                    {
                        DisplayMessage.Show(MessageId.LoginFailed, FindForm(), ApplicationEnvironment.ProductNameQualified);
                        return false;
                    }
                    else if (ex is DestinationValidator.DestinationServerFailedException)
                    {
                        if (permitIgnoringErrors)
                        {
                            DialogResult result = DisplayMessage.Show(MessageId.ErrorConnectingPromptContinue, FindForm());
                            if (result == DialogResult.No)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            DisplayMessage.Show(MessageId.ErrorConnecting, FindForm());
                            return false;
                        }
                    }
                    else if (ex is DestinationValidator.DestinationUrlMappingFailedException)
                    {
                        if (verifyMapping)
                        {
                            DialogResult result = DisplayMessage.Show(MessageId.MappingVerificationFailed, FindForm());
                            if (result == DialogResult.No)
                                return false;
                            else
                                return true;
                        }
                        else
                            return true;
                    }
                    else
                        Trace.Fail("Unknown destination validation exception: " + ex.ToString());
                }
            }
            return true;
        }

        private void textBoxUrlMapping_Leave(object sender, EventArgs e)
        {
            textBoxUrlMapping.Text = UrlHelper.FixUpUrl(textBoxUrlMapping.Text);
        }

        private void HandleError(Exception e)
        {
            Trace.WriteLine("Handling error: " + e.ToString());
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {

            using (new WaitCursor())
            {
                if (!ValidateRequiredFields(false, true))
                    return;

                WinInetFTPFileDestination destination = GetDestinationForFields("/");

                string path = this.textBoxPath.Text.Trim();

                if (!ValidateFTPConnection(destination, false, false))
                    return;

                string newPath = PublishFolderPicker.BrowseFTPDestination(this.textBoxHostName.Text, destination, path, new ErrorHandler(HandleError), this);
                if (newPath != null)
                    this.textBoxPath.Text = newPath;
            }
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.FTPHelp);
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FTPSettingsForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.pictureBoxHelp = new System.Windows.Forms.PictureBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.labelUrlMapping = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.labelHostName = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.labelUserName = new System.Windows.Forms.Label();
            this.textBoxUrlMapping = new System.Windows.Forms.TextBox();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.textBoxHostName = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.checkBoxSavePassword = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(238, 239);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(158, 239);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // linkLabel1
            //
            this.linkLabel1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabel1.Location = new System.Drawing.Point(27, 210);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(284, 16);
            this.linkLabel1.TabIndex = 12;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Help with configuring FTP settings...";
            this.linkLabel1.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = SystemColors.HotTrack;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            //
            // pictureBoxHelp
            //
            this.pictureBoxHelp.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxHelp.Image")));
            this.pictureBoxHelp.Location = new System.Drawing.Point(3, 207);
            this.pictureBoxHelp.Name = "pictureBoxHelp";
            this.pictureBoxHelp.Size = new System.Drawing.Size(24, 21);
            this.pictureBoxHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxHelp.TabIndex = 59;
            this.pictureBoxHelp.TabStop = false;
            //
            // buttonBrowse
            //
            this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonBrowse.Location = new System.Drawing.Point(286, 141);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(26, 22);
            this.buttonBrowse.TabIndex = 9;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            //
            // labelUrlMapping
            //
            this.labelUrlMapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUrlMapping.Location = new System.Drawing.Point(8, 165);
            this.labelUrlMapping.Name = "labelUrlMapping";
            this.labelUrlMapping.Size = new System.Drawing.Size(299, 14);
            this.labelUrlMapping.TabIndex = 10;
            this.labelUrlMapping.Text = "URL of image publishing &folder:";
            //
            // labelPath
            //
            this.labelPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPath.Location = new System.Drawing.Point(8, 125);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(299, 14);
            this.labelPath.TabIndex = 7;
            this.labelPath.Text = "Publish &images into this folder:";
            //
            // labelHostName
            //
            this.labelHostName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHostName.Location = new System.Drawing.Point(9, 8);
            this.labelHostName.Name = "labelHostName";
            this.labelHostName.Size = new System.Drawing.Size(301, 14);
            this.labelHostName.TabIndex = 0;
            this.labelHostName.Text = "FTP &hostname:";
            //
            // labelPassword
            //
            this.labelPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPassword.Location = new System.Drawing.Point(148, 48);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(159, 14);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "&Password:";
            //
            // labelUserName
            //
            this.labelUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelUserName.Location = new System.Drawing.Point(9, 48);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(131, 14);
            this.labelUserName.TabIndex = 2;
            this.labelUserName.Text = "&Username:";
            //
            // textBoxUrlMapping
            //
            this.textBoxUrlMapping.Location = new System.Drawing.Point(8, 181);
            this.textBoxUrlMapping.Name = "textBoxUrlMapping";
            this.textBoxUrlMapping.Size = new System.Drawing.Size(304, 21);
            this.textBoxUrlMapping.TabIndex = 11;
            this.textBoxUrlMapping.Text = "";
            this.textBoxUrlMapping.Leave += new System.EventHandler(this.textBoxUrlMapping_Leave);
            //
            // textBoxPath
            //
            this.textBoxPath.Location = new System.Drawing.Point(8, 141);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(276, 21);
            this.textBoxPath.TabIndex = 8;
            this.textBoxPath.Text = "";
            //
            // textBoxHostName
            //
            this.textBoxHostName.Location = new System.Drawing.Point(9, 24);
            this.textBoxHostName.Name = "textBoxHostName";
            this.textBoxHostName.Size = new System.Drawing.Size(304, 21);
            this.textBoxHostName.TabIndex = 1;
            this.textBoxHostName.Text = "";
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(146, 64);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(167, 21);
            this.textBoxPassword.TabIndex = 5;
            this.textBoxPassword.Text = "";
            //
            // textBoxUsername
            //
            this.textBoxUsername.Location = new System.Drawing.Point(9, 64);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(133, 21);
            this.textBoxUsername.TabIndex = 3;
            this.textBoxUsername.Text = "";
            //
            // checkBoxSavePassword
            //
            this.checkBoxSavePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(147, 88);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(165, 30);
            this.checkBoxSavePassword.TabIndex = 6;
            this.checkBoxSavePassword.Text = "&Save Password";
            this.checkBoxSavePassword.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // FTPSettingsForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(322, 269);
            this.Controls.Add(this.checkBoxSavePassword);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureBoxHelp);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.labelUrlMapping);
            this.Controls.Add(this.labelPath);
            this.Controls.Add(this.labelHostName);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.labelUserName);
            this.Controls.Add(this.textBoxUrlMapping);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.textBoxHostName);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FTPSettingsForm";
            this.Text = "FTP Server Configuration";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Helper for showing the FTP settings dialog which also takes care of clearing the cached credentials.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="blogSettings"></param>
        /// <returns></returns>
        public static DialogResult ShowFTPSettingsForm(IWin32Window owner, IBlogSettingsAccessor blogSettings)
        {
            using (FTPSettingsForm settingsForm = new FTPSettingsForm(new FtpUploaderSettings(blogSettings.FileUploadSettings)))
            {
                using (new WaitCursor())
                {
                    DialogResult result = settingsForm.ShowDialog(owner);
                    if (result == DialogResult.OK)
                    {
                        //be sure to clear the cached FTP credentials
                        FTPBlogFileUploader.ClearCachedCredentials(blogSettings.Id);
                    }
                    return result;
                }
            }

        }
    }
}
