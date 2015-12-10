// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    /// <summary>
    /// Summary description for ImagesPanel.
    /// </summary>
    public class ImagesPanel : WeblogSettingsPanel
    {
        private System.Windows.Forms.GroupBox groupBoxUpload;
        private System.Windows.Forms.Label labelDesc;
        private System.Windows.Forms.RadioButton radioButtonFtp;
        private System.Windows.Forms.RadioButton radioButtonWeblog;
        private System.Windows.Forms.Button buttonConfigureFtp;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ImagesPanel()
            : base()
        {
            InitializeComponent();
            UpdateStrings();
        }

        public ImagesPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings)
            : base(targetBlogSettings, editableBlogSettings)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            UpdateStrings();

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Settings.Images.ImagesPanelBitmap.png");

            InitializeSettings();

            // event handlers
            radioButtonWeblog.CheckedChanged += new EventHandler(radioButtonUpload_CheckedChanged);
            radioButtonFtp.CheckedChanged += new EventHandler(radioButtonUpload_CheckedChanged);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                using (new AutoGrow(groupBoxUpload, AnchorStyles.Bottom, true))
                {
                    int scaledPadding = (int)DisplayHelper.ScaleY(12);
                    LayoutHelper.NaturalizeHeight(labelDesc);
                    LayoutHelper.FitControlsBelow(scaledPadding, labelDesc);
                    DisplayHelper.AutoFitSystemButton(buttonConfigureFtp, buttonConfigureFtp.Width, int.MaxValue);

                    LayoutHelper.NaturalizeHeightAndDistribute(8, radioButtonWeblog, radioButtonFtp, buttonConfigureFtp);
                }
            }
        }

        private void UpdateStrings()
        {
            groupBoxUpload.Text = Res.Get(StringId.ImagesUpload);
            labelDesc.Text = Res.Get(StringId.ImagesText);
            radioButtonFtp.Text = Res.Get(StringId.ImagesFtpOpt);
            radioButtonWeblog.Text = Res.Get(StringId.ImagesBlogOpt);
            buttonConfigureFtp.Text = Res.Get(StringId.ImagesFtpConfig);
            PanelName = Res.Get(StringId.ImagesPanel);
        }

        private void InitializeSettings()
        {
            // set value for file upload support
            FileUploadSupport = TemporaryBlogSettings.FileUploadSupport;

            // Manage controls
            ManageControls();
        }

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            if (FileUploadSupport == FileUploadSupport.FTP)
            {
                FtpUploaderSettings ftpUploaderSettings = new FtpUploaderSettings(TemporaryBlogSettings.FileUploadSettings);
                if (ftpUploaderSettings.FtpServer == String.Empty)
                {
                    switchToPanel();
                    DisplayMessage.Show(MessageId.FtpSettingsRequired, FindForm());
                    buttonConfigureFtp.Focus();
                    return false;
                }
            }

            return true;

        }


        private FileUploadSupport FileUploadSupport
        {
            get
            {
                if (radioButtonWeblog.Checked)
                    return FileUploadSupport.Weblog;
                else if (radioButtonFtp.Checked)
                    return FileUploadSupport.FTP;
                else
                {
                    Trace.Fail("invalid radio button state");
                    return FileUploadSupport.Weblog;
                }
            }
            set
            {
                switch (value)
                {
                    case FileUploadSupport.Weblog:
                        radioButtonWeblog.Checked = true;
                        break;

                    case FileUploadSupport.FTP:
                        radioButtonFtp.Checked = true;
                        break;
                }
            }
        }

        private void buttonConfigureFtp_Click(object sender, System.EventArgs e)
        {
            DialogResult result = FTPSettingsForm.ShowFTPSettingsForm(FindForm(), TemporaryBlogSettings);
            if (result == DialogResult.OK)
            {
                TemporaryBlogSettingsModified = true;
            }
        }

        private void radioButtonUpload_CheckedChanged(object sender, EventArgs e)
        {
            TemporaryBlogSettings.FileUploadSupport = FileUploadSupport;
            TemporaryBlogSettingsModified = true;
            ManageControls();
        }

        private void ManageControls()
        {
            buttonConfigureFtp.Enabled = (FileUploadSupport == FileUploadSupport.FTP);
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
            this.groupBoxUpload = new System.Windows.Forms.GroupBox();
            this.buttonConfigureFtp = new System.Windows.Forms.Button();
            this.labelDesc = new System.Windows.Forms.Label();
            this.radioButtonWeblog = new System.Windows.Forms.RadioButton();
            this.radioButtonFtp = new System.Windows.Forms.RadioButton();
            this.groupBoxUpload.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxUpload
            //
            this.groupBoxUpload.Controls.Add(this.buttonConfigureFtp);
            this.groupBoxUpload.Controls.Add(this.labelDesc);
            this.groupBoxUpload.Controls.Add(this.radioButtonWeblog);
            this.groupBoxUpload.Controls.Add(this.radioButtonFtp);
            this.groupBoxUpload.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxUpload.Location = new System.Drawing.Point(8, 32);
            this.groupBoxUpload.Name = "groupBoxUpload";
            this.groupBoxUpload.Size = new System.Drawing.Size(345, 180);
            this.groupBoxUpload.TabIndex = 1;
            this.groupBoxUpload.TabStop = false;
            this.groupBoxUpload.Text = "Upload";
            //
            // buttonConfigureFtp
            //
            this.buttonConfigureFtp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonConfigureFtp.Location = new System.Drawing.Point(54, 142);
            this.buttonConfigureFtp.Name = "buttonConfigureFtp";
            this.buttonConfigureFtp.Size = new System.Drawing.Size(111, 23);
            this.buttonConfigureFtp.TabIndex = 8;
            this.buttonConfigureFtp.Text = "&Configure FTP";
            this.buttonConfigureFtp.Click += new System.EventHandler(this.buttonConfigureFtp_Click);
            //
            // labelDesc
            //
            this.labelDesc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDesc.Location = new System.Drawing.Point(16, 21);
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.Size = new System.Drawing.Size(320, 48);
            this.labelDesc.TabIndex = 4;
            this.labelDesc.Text = "When you include images in your Weblog entries, they can be automatically uploade" +
                "d when you post your entry. Please select how images should be uploaded:";
            //
            // radioButtonWeblog
            //
            this.radioButtonWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonWeblog.Location = new System.Drawing.Point(16, 93);
            this.radioButtonWeblog.Name = "radioButtonWeblog";
            this.radioButtonWeblog.Size = new System.Drawing.Size(313, 18);
            this.radioButtonWeblog.TabIndex = 6;
            this.radioButtonWeblog.Text = "&Upload images to my weblog";
            this.radioButtonWeblog.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // radioButtonFtp
            //
            this.radioButtonFtp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonFtp.Location = new System.Drawing.Point(16, 119);
            this.radioButtonFtp.Name = "radioButtonFtp";
            this.radioButtonFtp.Size = new System.Drawing.Size(313, 18);
            this.radioButtonFtp.TabIndex = 7;
            this.radioButtonFtp.Text = "Upload images to an &FTP server:";
            this.radioButtonFtp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // ImagesPanel
            //
            this.AccessibleName = "Images";
            this.Controls.Add(this.groupBoxUpload);
            this.Name = "ImagesPanel";
            this.PanelName = "Images";
            this.Controls.SetChildIndex(this.groupBoxUpload, 0);
            this.groupBoxUpload.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
