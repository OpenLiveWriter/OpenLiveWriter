// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    /// <summary>
    /// Summary description for AccountPanel.
    /// </summary>
    public class AuthoringPanel : StaticSitePreferencesPanel
    {
        private Label labelPostsPath;
        private TextBox textBoxPostsPath;
        private GroupBox groupBoxPosts;
        private CheckBox checkBoxDraftsEnabled;
        private TextBox textBoxDraftsPath;
        private Label labelDraftsPath;
        private GroupBox groupBoxPages;
        private CheckBox checkBoxPagesStoredInRoot;
        private CheckBox checkBoxPagesEnabled;
        private TextBox textBoxPagesPath;
        private Label labelPagesPath;
        private GroupBox groupBoxImages;
        private CheckBox checkBoxImagesEnabled;
        private TextBox textBoxImagesPath;
        private Label labelImagesPath;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public string PostsPath
        {
            get => textBoxPostsPath.Text;
            set => textBoxPostsPath.Text = value;
        }

        public bool DraftsEnabled
        {
            get => checkBoxDraftsEnabled.Checked;
            set => checkBoxDraftsEnabled.Checked = value;
        }

        public string DraftsPath
        {
            get => textBoxDraftsPath.Text;
            set => textBoxDraftsPath.Text = value;
        }

        public bool PagesEnabled
        {
            get => checkBoxPagesEnabled.Checked;
            set => checkBoxPagesEnabled.Checked = value;
        }

        public string PagesPath
        {
            get => textBoxPagesPath.Text;
            set => textBoxPagesPath.Text = value;
        }

        public bool PagesStoredInRoot
        {
            get => checkBoxPagesStoredInRoot.Checked;
            set => checkBoxPagesStoredInRoot.Checked = value;
        }

        public bool ImagesEnabled
        {
            get => checkBoxImagesEnabled.Checked;
            set => checkBoxImagesEnabled.Checked = value;
        }

        public string ImagesPath
        {
            get => textBoxImagesPath.Text;
            set => textBoxImagesPath.Text = value;
        }

        public AuthoringPanel() : base()
        {
            InitializeComponent();
            LocalizeStrings();
        }

        public AuthoringPanel(StaticSitePreferencesController controller)
            : base(controller)
        {
            InitializeComponent();
            LocalizeStrings();
        }

        private void LocalizeStrings()
        {
            PanelName = Res.Get(StringId.SSGConfigAuthoringTitle);

            groupBoxPosts.Text = Res.Get(StringId.SSGConfigAuthoringPostsDraftsGroup);
            labelPostsPath.Text = Res.Get(StringId.SSGConfigAuthoringPostsPath);
            checkBoxDraftsEnabled.Text = Res.Get(StringId.SSGConfigAuthoringEnableDrafts);
            labelDraftsPath.Text = Res.Get(StringId.SSGConfigAuthoringDraftsPath);

            groupBoxPages.Text = Res.Get(StringId.SSGConfigAuthoringPagesGroup);
            checkBoxPagesEnabled.Text = Res.Get(StringId.SSGConfigAuthoringEnablePages);
            labelPagesPath.Text = Res.Get(StringId.SSGConfigAuthoringPagesPath);
            checkBoxPagesStoredInRoot.Text = Res.Get(StringId.SSGConfigAuthoringPagesInRoot);

            groupBoxImages.Text = Res.Get(StringId.SSGConfigAuthoringImagesGroup);
            checkBoxImagesEnabled.Text = Res.Get(StringId.SSGConfigAuthoringEnableImages);
            labelImagesPath.Text = Res.Get(StringId.SSGConfigAuthoringImagesPath);
        }

        public override void LoadConfig()
        {
            PostsPath = _controller.Config.PostsPath;
            DraftsEnabled = _controller.Config.DraftsEnabled;
            DraftsPath = _controller.Config.DraftsPath;
            PagesEnabled = _controller.Config.PagesEnabled;
            PagesPath = _controller.Config.PagesPath;
            PagesStoredInRoot = _controller.Config.PagesPath == ".";
            ImagesEnabled = _controller.Config.ImagesEnabled;
            ImagesPath = _controller.Config.ImagesPath;
        }

        public override void ValidateConfig()
            => _controller.Config.Validator
            .ValidatePostsPath()
            .ValidateDraftsPath()
            .ValidatePagesPath()
            .ValidateImagesPath();

        public override void Save()
        {
            _controller.Config.PostsPath = PostsPath;
            _controller.Config.DraftsEnabled = DraftsEnabled;
            _controller.Config.DraftsPath = DraftsPath;
            _controller.Config.PagesEnabled = PagesEnabled;
            _controller.Config.PagesPath = PagesStoredInRoot ? "." : PagesPath;
            _controller.Config.ImagesEnabled = ImagesEnabled;
            _controller.Config.ImagesPath = ImagesPath;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecomputeEnabledStates();

            LayoutHelper.NaturalizeHeight(labelPostsPath, textBoxPostsPath, checkBoxDraftsEnabled, labelDraftsPath, textBoxDraftsPath);
            LayoutHelper.NaturalizeHeight(checkBoxPagesEnabled, labelPagesPath, textBoxPagesPath, checkBoxPagesStoredInRoot);
            LayoutHelper.NaturalizeHeight(checkBoxImagesEnabled, labelImagesPath, textBoxImagesPath);
        }

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
            this.labelPostsPath = new System.Windows.Forms.Label();
            this.textBoxPostsPath = new System.Windows.Forms.TextBox();
            this.groupBoxPosts = new System.Windows.Forms.GroupBox();
            this.checkBoxDraftsEnabled = new System.Windows.Forms.CheckBox();
            this.textBoxDraftsPath = new System.Windows.Forms.TextBox();
            this.labelDraftsPath = new System.Windows.Forms.Label();
            this.groupBoxPages = new System.Windows.Forms.GroupBox();
            this.checkBoxPagesStoredInRoot = new System.Windows.Forms.CheckBox();
            this.checkBoxPagesEnabled = new System.Windows.Forms.CheckBox();
            this.textBoxPagesPath = new System.Windows.Forms.TextBox();
            this.labelPagesPath = new System.Windows.Forms.Label();
            this.groupBoxImages = new System.Windows.Forms.GroupBox();
            this.checkBoxImagesEnabled = new System.Windows.Forms.CheckBox();
            this.textBoxImagesPath = new System.Windows.Forms.TextBox();
            this.labelImagesPath = new System.Windows.Forms.Label();
            this.groupBoxPosts.SuspendLayout();
            this.groupBoxPages.SuspendLayout();
            this.groupBoxImages.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelPostsPath
            // 
            this.labelPostsPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPostsPath.Location = new System.Drawing.Point(16, 19);
            this.labelPostsPath.Name = "labelPostsPath";
            this.labelPostsPath.Size = new System.Drawing.Size(144, 16);
            this.labelPostsPath.TabIndex = 0;
            this.labelPostsPath.Text = "Posts Path:";
            // 
            // textBoxPostsPath
            // 
            this.textBoxPostsPath.Location = new System.Drawing.Point(16, 38);
            this.textBoxPostsPath.Name = "textBoxPostsPath";
            this.textBoxPostsPath.Size = new System.Drawing.Size(316, 23);
            this.textBoxPostsPath.TabIndex = 1;
            // 
            // groupBoxPosts
            // 
            this.groupBoxPosts.Controls.Add(this.checkBoxDraftsEnabled);
            this.groupBoxPosts.Controls.Add(this.textBoxPostsPath);
            this.groupBoxPosts.Controls.Add(this.textBoxDraftsPath);
            this.groupBoxPosts.Controls.Add(this.labelPostsPath);
            this.groupBoxPosts.Controls.Add(this.labelDraftsPath);
            this.groupBoxPosts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPosts.Location = new System.Drawing.Point(8, 32);
            this.groupBoxPosts.Name = "groupBoxPosts";
            this.groupBoxPosts.Size = new System.Drawing.Size(345, 144);
            this.groupBoxPosts.TabIndex = 1;
            this.groupBoxPosts.TabStop = false;
            this.groupBoxPosts.Text = "Posts and Drafts";
            // 
            // checkBoxDraftsEnabled
            // 
            this.checkBoxDraftsEnabled.AutoSize = true;
            this.checkBoxDraftsEnabled.Location = new System.Drawing.Point(16, 70);
            this.checkBoxDraftsEnabled.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.checkBoxDraftsEnabled.Name = "checkBoxDraftsEnabled";
            this.checkBoxDraftsEnabled.Size = new System.Drawing.Size(95, 19);
            this.checkBoxDraftsEnabled.TabIndex = 2;
            this.checkBoxDraftsEnabled.Text = "Enable Drafts";
            this.checkBoxDraftsEnabled.UseVisualStyleBackColor = true;
            this.checkBoxDraftsEnabled.CheckedChanged += new System.EventHandler(this.CheckBoxDraftsEnabled_CheckedChanged);
            // 
            // textBoxDraftsPath
            // 
            this.textBoxDraftsPath.Location = new System.Drawing.Point(16, 108);
            this.textBoxDraftsPath.Name = "textBoxDraftsPath";
            this.textBoxDraftsPath.Size = new System.Drawing.Size(316, 23);
            this.textBoxDraftsPath.TabIndex = 4;
            // 
            // labelDraftsPath
            // 
            this.labelDraftsPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDraftsPath.Location = new System.Drawing.Point(16, 89);
            this.labelDraftsPath.Name = "labelDraftsPath";
            this.labelDraftsPath.Size = new System.Drawing.Size(144, 16);
            this.labelDraftsPath.TabIndex = 3;
            this.labelDraftsPath.Text = "Drafts Path:";
            // 
            // groupBoxPages
            // 
            this.groupBoxPages.Controls.Add(this.checkBoxPagesStoredInRoot);
            this.groupBoxPages.Controls.Add(this.checkBoxPagesEnabled);
            this.groupBoxPages.Controls.Add(this.textBoxPagesPath);
            this.groupBoxPages.Controls.Add(this.labelPagesPath);
            this.groupBoxPages.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPages.Location = new System.Drawing.Point(8, 182);
            this.groupBoxPages.Name = "groupBoxPages";
            this.groupBoxPages.Size = new System.Drawing.Size(345, 120);
            this.groupBoxPages.TabIndex = 2;
            this.groupBoxPages.TabStop = false;
            this.groupBoxPages.Text = "Pages";
            // 
            // checkBoxPagesStoredInRoot
            // 
            this.checkBoxPagesStoredInRoot.AutoSize = true;
            this.checkBoxPagesStoredInRoot.Location = new System.Drawing.Point(16, 89);
            this.checkBoxPagesStoredInRoot.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.checkBoxPagesStoredInRoot.Name = "checkBoxPagesStoredInRoot";
            this.checkBoxPagesStoredInRoot.Size = new System.Drawing.Size(175, 19);
            this.checkBoxPagesStoredInRoot.TabIndex = 3;
            this.checkBoxPagesStoredInRoot.Text = "Pages Stored In Project Root";
            this.checkBoxPagesStoredInRoot.UseVisualStyleBackColor = true;
            this.checkBoxPagesStoredInRoot.CheckedChanged += new System.EventHandler(this.CheckBoxPagesStoredInRoot_CheckedChanged);
            // 
            // checkBoxPagesEnabled
            // 
            this.checkBoxPagesEnabled.AutoSize = true;
            this.checkBoxPagesEnabled.Location = new System.Drawing.Point(16, 22);
            this.checkBoxPagesEnabled.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.checkBoxPagesEnabled.Name = "checkBoxPagesEnabled";
            this.checkBoxPagesEnabled.Size = new System.Drawing.Size(95, 19);
            this.checkBoxPagesEnabled.TabIndex = 0;
            this.checkBoxPagesEnabled.Text = "Enable Pages";
            this.checkBoxPagesEnabled.UseVisualStyleBackColor = true;
            this.checkBoxPagesEnabled.CheckedChanged += new System.EventHandler(this.CheckBoxPagesEnabled_CheckedChanged);
            // 
            // textBoxPagesPath
            // 
            this.textBoxPagesPath.Location = new System.Drawing.Point(16, 63);
            this.textBoxPagesPath.Name = "textBoxPagesPath";
            this.textBoxPagesPath.Size = new System.Drawing.Size(316, 23);
            this.textBoxPagesPath.TabIndex = 2;
            // 
            // labelPagesPath
            // 
            this.labelPagesPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPagesPath.Location = new System.Drawing.Point(16, 44);
            this.labelPagesPath.Name = "labelPagesPath";
            this.labelPagesPath.Size = new System.Drawing.Size(144, 16);
            this.labelPagesPath.TabIndex = 1;
            this.labelPagesPath.Text = "Pages Path:";
            // 
            // groupBoxImages
            // 
            this.groupBoxImages.Controls.Add(this.checkBoxImagesEnabled);
            this.groupBoxImages.Controls.Add(this.textBoxImagesPath);
            this.groupBoxImages.Controls.Add(this.labelImagesPath);
            this.groupBoxImages.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxImages.Location = new System.Drawing.Point(8, 308);
            this.groupBoxImages.Name = "groupBoxImages";
            this.groupBoxImages.Size = new System.Drawing.Size(345, 101);
            this.groupBoxImages.TabIndex = 3;
            this.groupBoxImages.TabStop = false;
            this.groupBoxImages.Text = "Images";
            // 
            // checkBoxImagesEnabled
            // 
            this.checkBoxImagesEnabled.AutoSize = true;
            this.checkBoxImagesEnabled.Location = new System.Drawing.Point(16, 22);
            this.checkBoxImagesEnabled.Name = "checkBoxImagesEnabled";
            this.checkBoxImagesEnabled.Size = new System.Drawing.Size(102, 19);
            this.checkBoxImagesEnabled.TabIndex = 0;
            this.checkBoxImagesEnabled.Text = "Enable Images";
            this.checkBoxImagesEnabled.UseVisualStyleBackColor = true;
            this.checkBoxImagesEnabled.CheckedChanged += new System.EventHandler(this.CheckBoxImagesEnabled_CheckedChanged);
            // 
            // textBoxImagesPath
            // 
            this.textBoxImagesPath.Location = new System.Drawing.Point(16, 63);
            this.textBoxImagesPath.Name = "textBoxImagesPath";
            this.textBoxImagesPath.Size = new System.Drawing.Size(316, 23);
            this.textBoxImagesPath.TabIndex = 2;
            // 
            // labelImagesPath
            // 
            this.labelImagesPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelImagesPath.Location = new System.Drawing.Point(16, 44);
            this.labelImagesPath.Name = "labelImagesPath";
            this.labelImagesPath.Size = new System.Drawing.Size(144, 16);
            this.labelImagesPath.TabIndex = 1;
            this.labelImagesPath.Text = "Images Path:";
            // 
            // AuthoringPanel
            // 
            this.AccessibleName = "Authoring";
            this.Controls.Add(this.groupBoxImages);
            this.Controls.Add(this.groupBoxPages);
            this.Controls.Add(this.groupBoxPosts);
            this.Name = "AuthoringPanel";
            this.PanelName = "Authoring";
            this.Size = new System.Drawing.Size(370, 425);
            this.Controls.SetChildIndex(this.groupBoxPosts, 0);
            this.Controls.SetChildIndex(this.groupBoxPages, 0);
            this.Controls.SetChildIndex(this.groupBoxImages, 0);
            this.groupBoxPosts.ResumeLayout(false);
            this.groupBoxPosts.PerformLayout();
            this.groupBoxPages.ResumeLayout(false);
            this.groupBoxPages.PerformLayout();
            this.groupBoxImages.ResumeLayout(false);
            this.groupBoxImages.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void CheckBoxDraftsEnabled_CheckedChanged(object sender, EventArgs e)
            => RecomputeEnabledStates();

        private void CheckBoxPagesEnabled_CheckedChanged(object sender, EventArgs e)
            => RecomputeEnabledStates();

        private void CheckBoxPagesStoredInRoot_CheckedChanged(object sender, EventArgs e)
        {
            RecomputeEnabledStates();
            if (checkBoxPagesStoredInRoot.Checked) textBoxPagesPath.Text = ".";
        }

        private void CheckBoxImagesEnabled_CheckedChanged(object sender, EventArgs e)
            => RecomputeEnabledStates();

        private void RecomputeEnabledStates()
        {
            textBoxDraftsPath.Enabled = checkBoxDraftsEnabled.Checked;

            textBoxPagesPath.Enabled = checkBoxPagesEnabled.Checked && !checkBoxPagesStoredInRoot.Checked;
            checkBoxPagesStoredInRoot.Enabled = checkBoxPagesEnabled.Checked;

            textBoxImagesPath.Enabled = checkBoxImagesEnabled.Checked;
        }
    }
}
