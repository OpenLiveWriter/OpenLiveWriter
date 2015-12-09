using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.PostEditor.ContentSources.Common;

namespace OpenLiveWriter.PostEditor.Video
{
    partial class VideoPublishSource
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.comboBoxPermissions = new System.Windows.Forms.ComboBox();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.llTerms = new System.Windows.Forms.LinkLabel();
            this.cbAcceptTerms = new System.Windows.Forms.CheckBox();
            this.lblPermissions = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblVideos = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnFileOpen = new System.Windows.Forms.Button();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.llSafety = new System.Windows.Forms.LinkLabel();
            this.videoLoginStatusControl = new OpenLiveWriter.PostEditor.ContentSources.Common.LoginStatusControl();
            this.txtAcceptTerms = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // txtTitle
            //
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(118, 107);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(322, 20);
            this.txtTitle.TabIndex = 20;
            this.txtTitle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textboxKeyDown);
            //
            // comboBoxPermissions
            //
            this.comboBoxPermissions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPermissions.FormattingEnabled = true;
            this.comboBoxPermissions.Location = new System.Drawing.Point(298, 267);
            this.comboBoxPermissions.Name = "comboBoxPermissions";
            this.comboBoxPermissions.Size = new System.Drawing.Size(142, 21);
            this.comboBoxPermissions.TabIndex = 28;
            //
            // comboBoxCategory
            //
            this.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(118, 267);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(174, 21);
            this.comboBoxCategory.TabIndex = 26;
            //
            // txtTags
            //
            this.txtTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTags.Location = new System.Drawing.Point(118, 223);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(322, 20);
            this.txtTags.TabIndex = 24;
            this.txtTags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textboxKeyDown);
            this.txtTags.Leave += new System.EventHandler(this.txtTags_Leave);
            //
            // txtDescription
            //
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(118, 151);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(322, 48);
            this.txtDescription.TabIndex = 22;
            this.txtDescription.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textboxKeyDown);
            //
            // llTerms
            //
            this.llTerms.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.llTerms.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.llTerms.Location = new System.Drawing.Point(136, 379);
            this.llTerms.Name = "llTerms";
            this.llTerms.Size = new System.Drawing.Size(304, 17);
            this.llTerms.TabIndex = 30;
            this.llTerms.TabStop = true;
            this.llTerms.Text = "View Terms of Use";
            //
            // cbAcceptTerms
            //
            this.cbAcceptTerms.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAcceptTerms.Location = new System.Drawing.Point(118, 296);
            this.cbAcceptTerms.Name = "cbAcceptTerms";
            this.cbAcceptTerms.Size = new System.Drawing.Size(20, 20);
            this.cbAcceptTerms.TabIndex = 29;
            this.cbAcceptTerms.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAcceptTerms.UseVisualStyleBackColor = true;
            //
            // lblPermissions
            //
            this.lblPermissions.AutoSize = true;
            this.lblPermissions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblPermissions.Location = new System.Drawing.Point(300, 250);
            this.lblPermissions.Name = "lblPermissions";
            this.lblPermissions.Size = new System.Drawing.Size(65, 13);
            this.lblPermissions.TabIndex = 27;
            this.lblPermissions.Text = "Permissions:";
            //
            // lblCategory
            //
            this.lblCategory.AutoSize = true;
            this.lblCategory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblCategory.Location = new System.Drawing.Point(118, 250);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 25;
            this.lblCategory.Text = "Category:";
            //
            // lblTags
            //
            this.lblTags.AutoSize = true;
            this.lblTags.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblTags.Location = new System.Drawing.Point(118, 206);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(34, 13);
            this.lblTags.TabIndex = 23;
            this.lblTags.Text = "Tags:";
            //
            // lblDescription
            //
            this.lblDescription.AutoSize = true;
            this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDescription.Location = new System.Drawing.Point(118, 134);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 21;
            this.lblDescription.Text = "Description:";
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblTitle.Location = new System.Drawing.Point(118, 90);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(30, 13);
            this.lblTitle.TabIndex = 19;
            this.lblTitle.Text = "Title:";
            //
            // lblVideos
            //
            this.lblVideos.AutoSize = true;
            this.lblVideos.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblVideos.Location = new System.Drawing.Point(118, 46);
            this.lblVideos.Name = "lblVideos";
            this.lblVideos.Size = new System.Drawing.Size(56, 13);
            this.lblVideos.TabIndex = 16;
            this.lblVideos.Text = "Video File:";
            //
            // txtFile
            //
            this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFile.Location = new System.Drawing.Point(118, 63);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(284, 20);
            this.txtFile.TabIndex = 17;
            this.txtFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textboxKeyDown);
            //
            // btnFileOpen
            //
            this.btnFileOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFileOpen.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnFileOpen.Location = new System.Drawing.Point(408, 61);
            this.btnFileOpen.Name = "btnFileOpen";
            this.btnFileOpen.Size = new System.Drawing.Size(32, 23);
            this.btnFileOpen.TabIndex = 18;
            this.btnFileOpen.Text = "...";
            this.btnFileOpen.UseVisualStyleBackColor = true;
            this.btnFileOpen.Click += new System.EventHandler(this.btnFileOpen_Click);
            //
            // llSafety
            //
            this.llSafety.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.llSafety.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.llSafety.Location = new System.Drawing.Point(136, 400);
            this.llSafety.Name = "llSafety";
            this.llSafety.Size = new System.Drawing.Size(304, 16);
            this.llSafety.TabIndex = 33;
            this.llSafety.TabStop = true;
            this.llSafety.Text = "View Safety Tips";
            //
            // videoLoginStatusControl
            //
            this.videoLoginStatusControl.Location = new System.Drawing.Point(3, 0);
            this.videoLoginStatusControl.Name = "videoLoginStatusControl";
            this.videoLoginStatusControl.ShowLoginButton = true;
            this.videoLoginStatusControl.Size = new System.Drawing.Size(437, 32);
            this.videoLoginStatusControl.TabIndex = 34;
            //
            // txtAcceptTerms
            //
            this.txtAcceptTerms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAcceptTerms.Location = new System.Drawing.Point(138, 297);
            this.txtAcceptTerms.Multiline = true;
            this.txtAcceptTerms.Name = "txtAcceptTerms";
            this.txtAcceptTerms.ReadOnly = true;
            this.txtAcceptTerms.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAcceptTerms.Size = new System.Drawing.Size(301, 76);
            this.txtAcceptTerms.TabIndex = 35;
            //
            // VideoPublishSource
            //
            this.Controls.Add(this.txtAcceptTerms);
            this.Controls.Add(this.videoLoginStatusControl);
            this.Controls.Add(this.llSafety);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.comboBoxPermissions);
            this.Controls.Add(this.comboBoxCategory);
            this.Controls.Add(this.txtTags);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.llTerms);
            this.Controls.Add(this.cbAcceptTerms);
            this.Controls.Add(this.lblPermissions);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.lblTags);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblVideos);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnFileOpen);
            this.Name = "VideoPublishSource";
            this.Size = new System.Drawing.Size(450, 435);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.ComboBox comboBoxPermissions;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.LinkLabel llTerms;
        private System.Windows.Forms.CheckBox cbAcceptTerms;
        private System.Windows.Forms.Label lblPermissions;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVideos;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnFileOpen;
        private System.Windows.Forms.OpenFileDialog ofd;
        private LinkLabel llSafety;
        private LoginStatusControl videoLoginStatusControl;
        private TextBox txtAcceptTerms;
    }
}
