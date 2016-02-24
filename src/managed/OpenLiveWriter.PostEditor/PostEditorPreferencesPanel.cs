// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.WordCount;
using System.IO;
using OpenLiveWriter.PostEditor.JumpList;

namespace OpenLiveWriter.PostEditor
{

    public class PostEditorPreferencesPanel : PreferencesPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private GroupBox groupBoxPublishing;
        private CheckBox checkBoxViewWeblog;

        private PostEditorPreferences _postEditorPreferences;
        private System.Windows.Forms.CheckBox checkBoxCloseWindow;
        private System.Windows.Forms.GroupBox groupBoxPostWindows;
        private System.Windows.Forms.RadioButton radioButtonOpenNewWindowIfDirty;
        private System.Windows.Forms.RadioButton radioButtonUseSameWindow;
        private System.Windows.Forms.RadioButton radioButtonOpenNewWindow;
        private System.Windows.Forms.CheckBox checkBoxCategoryReminder;
        private System.Windows.Forms.CheckBox checkBoxTagReminder;
        private System.Windows.Forms.CheckBox checkBoxTitleReminder;
        private System.Windows.Forms.CheckBox checkBoxAutoSaveDrafts;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.CheckBox checkBoxWordCount;
        private System.Windows.Forms.GroupBox groupBoxWeblogPostsFolder;
        private System.Windows.Forms.TextBox textBoxWeblogPostsFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button buttonBrowserDialog;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private WordCountPreferences _wordCountPreferences;
        private string _originalFolder = string.Empty;

        public PostEditorPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            if (!DesignMode)
            {
                this.groupBoxPostWindows.Text = Res.Get(StringId.PostEditorPrefPostWindows);
                this.groupBoxPublishing.Text = Res.Get(StringId.PostEditorPrefPublishing);
                checkBoxTagReminder.Text = Res.Get(StringId.PostEditorPrefRemind);
                checkBoxCategoryReminder.Text = Res.Get(StringId.PostEditorPrefRemindCat);
                checkBoxCloseWindow.Text = Res.Get(StringId.PostEditorPrefClose);
                checkBoxViewWeblog.Text = Res.Get(StringId.PostEditorPrefView);
                radioButtonOpenNewWindowIfDirty.Text = Res.Get(StringId.PostEditorPrefUnsave);
                radioButtonUseSameWindow.Text = Res.Get(StringId.PostEditorPrefSingle);
                radioButtonOpenNewWindow.Text = Res.Get(StringId.PostEditorPrefNew);
                checkBoxTitleReminder.Text = Res.Get(StringId.PostEditorPrefTitle);
                groupBoxGeneral.Text = Res.Get(StringId.PostEditorPrefGeneral);
                checkBoxAutoSaveDrafts.Text = Res.Get(StringId.PostEditorPrefAuto);
                checkBoxWordCount.Text = Res.Get(StringId.ShowRealTimeWordCount);
                PanelName = Res.Get(StringId.PostEditorPrefName);
                this.groupBoxWeblogPostsFolder.Text = Res.Get(StringId.PostEditorPrefPostLocation);
            }

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PreferencesOther.png");

            _postEditorPreferences = PostEditorPreferences.Instance;
            _postEditorPreferences.PreferencesModified += _writerPreferences_PreferencesModified;

            switch (_postEditorPreferences.PostWindowBehavior)
            {
                case PostWindowBehavior.UseSameWindow:
                    radioButtonUseSameWindow.Checked = true;
                    break;
                case PostWindowBehavior.OpenNewWindow:
                    radioButtonOpenNewWindow.Checked = true;
                    break;
                case PostWindowBehavior.OpenNewWindowIfDirty:
                    this.radioButtonOpenNewWindowIfDirty.Checked = true;
                    break;
            }

            checkBoxViewWeblog.Checked = _postEditorPreferences.ViewPostAfterPublish;

            checkBoxCloseWindow.Checked = _postEditorPreferences.CloseWindowOnPublish;

            checkBoxTitleReminder.Checked = _postEditorPreferences.TitleReminder;
            checkBoxCategoryReminder.Checked = _postEditorPreferences.CategoryReminder;
            checkBoxTagReminder.Checked = _postEditorPreferences.TagReminder;

            textBoxWeblogPostsFolder.Text = _postEditorPreferences.WeblogPostsFolder;
            _originalFolder = _postEditorPreferences.WeblogPostsFolder;
            textBoxWeblogPostsFolder.TextChanged += TextBoxWeblogPostsFolder_TextChanged;

            buttonBrowserDialog.MouseClick += ButtonBrowserDialog_MouseClick;

            checkBoxAutoSaveDrafts.Checked = _postEditorPreferences.AutoSaveDrafts;
            checkBoxAutoSaveDrafts.CheckedChanged += new EventHandler(checkBoxAutoSaveDrafts_CheckedChanged);

            _wordCountPreferences = new WordCountPreferences();
            _wordCountPreferences.PreferencesModified += _wordCountPreferences_PreferencesModified;
            checkBoxWordCount.Checked = _wordCountPreferences.EnableRealTimeWordCount;
            checkBoxWordCount.CheckedChanged += new EventHandler(checkBoxWordCount_CheckedChanged);

            radioButtonUseSameWindow.CheckedChanged += new EventHandler(radioButtonPostWindowBehavior_CheckedChanged);
            radioButtonOpenNewWindow.CheckedChanged += new EventHandler(radioButtonPostWindowBehavior_CheckedChanged);
            radioButtonOpenNewWindowIfDirty.CheckedChanged += new EventHandler(radioButtonPostWindowBehavior_CheckedChanged);

            checkBoxViewWeblog.CheckedChanged += new EventHandler(checkBoxViewWeblog_CheckedChanged);
            checkBoxCloseWindow.CheckedChanged += new EventHandler(checkBoxCloseWindow_CheckedChanged);

            checkBoxTitleReminder.CheckedChanged += new EventHandler(checkBoxTitleReminder_CheckedChanged);
            checkBoxCategoryReminder.CheckedChanged += new EventHandler(checkBoxCategoryReminder_CheckedChanged);
            checkBoxTagReminder.CheckedChanged += new EventHandler(checkBoxTagReminder_CheckedChanged);

        }

        private void ButtonBrowserDialog_MouseClick(object sender, MouseEventArgs e)
        {
            folderBrowserDialog.SelectedPath = textBoxWeblogPostsFolder.Text;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxWeblogPostsFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private bool _layedOut = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode && !_layedOut)
            {
                LayoutHelper.FixupGroupBox(this.groupBoxPostWindows);
                LayoutHelper.FixupGroupBox(this.groupBoxPublishing);
                LayoutHelper.FixupGroupBox(this.groupBoxGeneral);
                LayoutHelper.FixupGroupBox(this.groupBoxWeblogPostsFolder);
                LayoutHelper.NaturalizeHeightAndDistribute(8, groupBoxPostWindows, groupBoxPublishing, groupBoxGeneral, groupBoxWeblogPostsFolder);
                _layedOut = true;
            }
        }

        public override void Save()
        {
            string destinationRecentPosts = Path.Combine(_postEditorPreferences.WeblogPostsFolder + "\\Recent Posts");
            string destinationDrafts = Path.Combine(_postEditorPreferences.WeblogPostsFolder + "\\Drafts");
            Directory.CreateDirectory(destinationRecentPosts);
            Directory.CreateDirectory(destinationDrafts);

            _postEditorPreferences.SaveWebLogPostFolder();

            if (string.Compare(_originalFolder, _postEditorPreferences.WeblogPostsFolder, true, CultureInfo.CurrentUICulture) != 0)
            {
                string message = "You  have updated the default location for your blog posts, would you like to move any existing posts?";
                string caption = "Move existing posts";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);

                if (result == DialogResult.Yes)
                {
                    MovePosts(Path.Combine(_originalFolder + @"\\Recent Posts\\"), destinationRecentPosts);

                    MovePosts(Path.Combine(_originalFolder + @"\\Drafts\\"), destinationDrafts);
                    
                    PostEditorForm frm = Application.OpenForms?[0] as PostEditorForm;
                    if (frm != null)
                    { 
                        PostEditorMainControl ctrl = frm.Controls?[0] as PostEditorMainControl;
                        if (ctrl != null)
                        {
                            ctrl.FirePostListChangedEvent();
                        }
                    }
                }
            }

            if (_postEditorPreferences.IsModified())
                _postEditorPreferences.Save();

            if (_wordCountPreferences.IsModified())
                _wordCountPreferences.Save();

            _originalFolder = _postEditorPreferences.WeblogPostsFolder;
        }

        private void checkBoxViewWeblog_CheckedChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.ViewPostAfterPublish = checkBoxViewWeblog.Checked;
        }

        private void radioButtonPostWindowBehavior_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonUseSameWindow.Checked)
                _postEditorPreferences.PostWindowBehavior = PostWindowBehavior.UseSameWindow;
            else if (radioButtonOpenNewWindow.Checked)
                _postEditorPreferences.PostWindowBehavior = PostWindowBehavior.OpenNewWindow;
            else if (radioButtonOpenNewWindowIfDirty.Checked)
                _postEditorPreferences.PostWindowBehavior = PostWindowBehavior.OpenNewWindowIfDirty;
        }

        private void checkBoxCloseWindow_CheckedChanged(object sender, EventArgs e)
        {
            UpdateClosePreferences();
        }

        private void UpdateClosePreferences()
        {
            _postEditorPreferences.CloseWindowOnPublish = checkBoxCloseWindow.Checked;
        }

        private void _writerPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        private void checkBoxTitleReminder_CheckedChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.TitleReminder = checkBoxTitleReminder.Checked;
        }

        private void checkBoxCategoryReminder_CheckedChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.CategoryReminder = checkBoxCategoryReminder.Checked;
        }

        private void checkBoxTagReminder_CheckedChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.TagReminder = checkBoxTagReminder.Checked;
        }

        private void checkBoxAutoSaveDrafts_CheckedChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.AutoSaveDrafts = checkBoxAutoSaveDrafts.Checked;
        }

        void _wordCountPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        void checkBoxWordCount_CheckedChanged(object sender, EventArgs e)
        {
            _wordCountPreferences.EnableRealTimeWordCount = checkBoxWordCount.Checked;
        }

        private void TextBoxWeblogPostsFolder_TextChanged(object sender, EventArgs e)
        {
            _postEditorPreferences.WeblogPostsFolder = textBoxWeblogPostsFolder.Text;
        }

        private void MovePosts(string sourceFolder, string destinationFolder)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder);
            foreach (string s in files)
            {
                string fileName = Path.GetFileName(s);
                string destFile = Path.Combine(destinationFolder, fileName);

                MoveFile(s, destFile);
            }
        }

        private void MoveFile(string sourcefile, string destinationfile)
        {
            if (File.Exists(destinationfile))
            {
                string newdestfilename = 
                    Path.GetDirectoryName(destinationfile) + @"\" +
                    Path.GetFileNameWithoutExtension(destinationfile) + "_Copy" +
                    Path.GetExtension(destinationfile);

                MoveFile(sourcefile, newdestfilename);
            }
            else
            {
                File.Move(sourcefile, destinationfile);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _postEditorPreferences.PreferencesModified -= new EventHandler(_writerPreferences_PreferencesModified);

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
            this.groupBoxPublishing = new System.Windows.Forms.GroupBox();
            this.checkBoxTitleReminder = new System.Windows.Forms.CheckBox();
            this.checkBoxTagReminder = new System.Windows.Forms.CheckBox();
            this.checkBoxCategoryReminder = new System.Windows.Forms.CheckBox();
            this.checkBoxCloseWindow = new System.Windows.Forms.CheckBox();
            this.checkBoxViewWeblog = new System.Windows.Forms.CheckBox();
            this.groupBoxPostWindows = new System.Windows.Forms.GroupBox();
            this.radioButtonOpenNewWindowIfDirty = new System.Windows.Forms.RadioButton();
            this.radioButtonUseSameWindow = new System.Windows.Forms.RadioButton();
            this.radioButtonOpenNewWindow = new System.Windows.Forms.RadioButton();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoSaveDrafts = new System.Windows.Forms.CheckBox();
            this.checkBoxWordCount = new System.Windows.Forms.CheckBox();
            this.groupBoxWeblogPostsFolder = new System.Windows.Forms.GroupBox();
            this.textBoxWeblogPostsFolder = new TextBox();
            this.folderBrowserDialog = new FolderBrowserDialog();
            this.buttonBrowserDialog = new Button();
            this.flowLayoutPanel = new FlowLayoutPanel();
            this.flowLayoutPanel.SuspendLayout();
            this.groupBoxPublishing.SuspendLayout();
            this.groupBoxPostWindows.SuspendLayout();
            this.groupBoxGeneral.SuspendLayout();
            this.groupBoxWeblogPostsFolder.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxPublishing
            //
            this.groupBoxPublishing.Controls.Add(this.checkBoxTitleReminder);
            this.groupBoxPublishing.Controls.Add(this.checkBoxTagReminder);
            this.groupBoxPublishing.Controls.Add(this.checkBoxCategoryReminder);
            this.groupBoxPublishing.Controls.Add(this.checkBoxCloseWindow);
            this.groupBoxPublishing.Controls.Add(this.checkBoxViewWeblog);
            this.groupBoxPublishing.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPublishing.Location = new System.Drawing.Point(8, 154);
            this.groupBoxPublishing.Name = "groupBoxPublishing";
            this.groupBoxPublishing.Size = new System.Drawing.Size(345, 174);
            this.groupBoxPublishing.TabIndex = 2;
            this.groupBoxPublishing.TabStop = false;
            this.groupBoxPublishing.Text = "Publishing";
            //
            // checkBoxTitleReminder
            //
            this.checkBoxTitleReminder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxTitleReminder.Location = new System.Drawing.Point(16, 93);
            this.checkBoxTitleReminder.Name = "checkBoxTitleReminder";
            this.checkBoxTitleReminder.Size = new System.Drawing.Size(312, 21);
            this.checkBoxTitleReminder.TabIndex = 3;
            this.checkBoxTitleReminder.Text = "&Remind me to specify a title before publishing";
            this.checkBoxTitleReminder.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxTagReminder
            //
            this.checkBoxTagReminder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxTagReminder.Location = new System.Drawing.Point(16, 135);
            this.checkBoxTagReminder.Name = "checkBoxTagReminder";
            this.checkBoxTagReminder.Size = new System.Drawing.Size(312, 21);
            this.checkBoxTagReminder.TabIndex = 5;
            this.checkBoxTagReminder.Text = "Remind me to add &tags before publishing";
            this.checkBoxTagReminder.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxCategoryReminder
            //
            this.checkBoxCategoryReminder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxCategoryReminder.Location = new System.Drawing.Point(16, 114);
            this.checkBoxCategoryReminder.Name = "checkBoxCategoryReminder";
            this.checkBoxCategoryReminder.Size = new System.Drawing.Size(312, 21);
            this.checkBoxCategoryReminder.TabIndex = 4;
            this.checkBoxCategoryReminder.Text = "Remind me to add &categories before publishing";
            this.checkBoxCategoryReminder.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxCloseWindow
            //
            this.checkBoxCloseWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxCloseWindow.Location = new System.Drawing.Point(16, 42);
            this.checkBoxCloseWindow.Name = "checkBoxCloseWindow";
            this.checkBoxCloseWindow.Size = new System.Drawing.Size(312, 24);
            this.checkBoxCloseWindow.TabIndex = 1;
            this.checkBoxCloseWindow.Text = "Close &window after publishing: ";
            this.checkBoxCloseWindow.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxViewWeblog
            //
            this.checkBoxViewWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxViewWeblog.Location = new System.Drawing.Point(16, 21);
            this.checkBoxViewWeblog.Name = "checkBoxViewWeblog";
            this.checkBoxViewWeblog.Size = new System.Drawing.Size(312, 21);
            this.checkBoxViewWeblog.TabIndex = 0;
            // Modified on 2/19/2016 by @kathweaver to resolve Issue #377
            this.checkBoxViewWeblog.Text = "&View blog after publishing";
            this.checkBoxViewWeblog.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // groupBoxPostWindows
            //
            this.groupBoxPostWindows.Controls.Add(this.radioButtonOpenNewWindowIfDirty);
            this.groupBoxPostWindows.Controls.Add(this.radioButtonUseSameWindow);
            this.groupBoxPostWindows.Controls.Add(this.radioButtonOpenNewWindow);
            this.groupBoxPostWindows.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxPostWindows.Location = new System.Drawing.Point(8, 32);
            this.groupBoxPostWindows.Name = "groupBoxPostWindows";
            this.groupBoxPostWindows.Size = new System.Drawing.Size(345, 116);
            this.groupBoxPostWindows.TabIndex = 1;
            this.groupBoxPostWindows.TabStop = false;
            this.groupBoxPostWindows.Text = "Post Windows";
            //
            // radioButtonOpenNewWindowIfDirty
            //
            this.radioButtonOpenNewWindowIfDirty.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonOpenNewWindowIfDirty.Location = new System.Drawing.Point(16, 69);
            this.radioButtonOpenNewWindowIfDirty.Name = "radioButtonOpenNewWindowIfDirty";
            this.radioButtonOpenNewWindowIfDirty.Size = new System.Drawing.Size(312, 30);
            this.radioButtonOpenNewWindowIfDirty.TabIndex = 2;
            this.radioButtonOpenNewWindowIfDirty.Text = "Open a new window &only when there are unsaved changes to the current post";
            this.radioButtonOpenNewWindowIfDirty.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // radioButtonUseSameWindow
            //
            this.radioButtonUseSameWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonUseSameWindow.Location = new System.Drawing.Point(16, 21);
            this.radioButtonUseSameWindow.Name = "radioButtonUseSameWindow";
            this.radioButtonUseSameWindow.Size = new System.Drawing.Size(312, 24);
            this.radioButtonUseSameWindow.TabIndex = 0;
            this.radioButtonUseSameWindow.Text = "Use a &single window for editing all posts";
            this.radioButtonUseSameWindow.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // radioButtonOpenNewWindow
            //
            this.radioButtonOpenNewWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonOpenNewWindow.Location = new System.Drawing.Point(16, 44);
            this.radioButtonOpenNewWindow.Name = "radioButtonOpenNewWindow";
            this.radioButtonOpenNewWindow.Size = new System.Drawing.Size(312, 24);
            this.radioButtonOpenNewWindow.TabIndex = 1;
            this.radioButtonOpenNewWindow.Text = "Open a new window for &each post";
            this.radioButtonOpenNewWindow.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // groupBoxGeneral
            //
            this.groupBoxGeneral.Controls.Add(this.checkBoxAutoSaveDrafts);
            this.groupBoxGeneral.Controls.Add(this.checkBoxWordCount);
            this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxGeneral.Location = new System.Drawing.Point(8, 154);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(345, 174);
            this.groupBoxGeneral.TabIndex = 3;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General";
            //
            // checkBoxAutoSaveDrafts
            //
            this.checkBoxAutoSaveDrafts.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxAutoSaveDrafts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAutoSaveDrafts.Location = new System.Drawing.Point(16, 21);
            this.checkBoxAutoSaveDrafts.Name = "checkBoxAutoSaveDrafts";
            this.checkBoxAutoSaveDrafts.Size = new System.Drawing.Size(312, 18);
            this.checkBoxAutoSaveDrafts.TabIndex = 0;
            this.checkBoxAutoSaveDrafts.Text = "Automatically save &drafts every:";
            this.checkBoxAutoSaveDrafts.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxWordCount
            //
            this.checkBoxWordCount.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxWordCount.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxWordCount.Location = new System.Drawing.Point(16, 156);
            this.checkBoxWordCount.Name = "checkBoxWordCount";
            this.checkBoxWordCount.Size = new System.Drawing.Size(312, 18);
            this.checkBoxWordCount.TabIndex = 1;
            this.checkBoxWordCount.Text = "Show real time &word count in status bar";
            this.checkBoxWordCount.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxWordCount.UseVisualStyleBackColor = true;
            //
            // textBoxWeblogPostsFolder
            //
            this.textBoxWeblogPostsFolder.Name = "textBoxWeblogPostsFolder";
            this.textBoxWeblogPostsFolder.Size = new System.Drawing.Size(314, 22);
            this.textBoxWeblogPostsFolder.AutoSize = false;
            this.textBoxWeblogPostsFolder.TabIndex = 1;
            this.textBoxWeblogPostsFolder.Text = "Show default post save location";
            this.textBoxWeblogPostsFolder.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxWeblogPostsFolder.Location = new System.Drawing.Point(16, 21);
            this.textBoxWeblogPostsFolder.BorderStyle = BorderStyle.FixedSingle;
            this.textBoxWeblogPostsFolder.Font = Res.DefaultFont;
            //
            // buttonBrowserDialog
            //
            this.buttonBrowserDialog.Name = "buttonBrowserDialog";
            this.buttonBrowserDialog.Text = Res.Get(StringId.PostEditorPrefBrowseFolder);
            this.buttonBrowserDialog.TabIndex = 2;
            this.buttonBrowserDialog.Location = new System.Drawing.Point(16, 32);
            this.buttonBrowserDialog.Size =  new System.Drawing.Size(70, 22);
            this.buttonBrowserDialog.Font = Res.DefaultFont;
            this.buttonBrowserDialog.AutoSize = false;
            //
            // FolderBrowserDialog
            //
            this.folderBrowserDialog.Description = "Select the directory that you want to use as the default";
            this.folderBrowserDialog.ShowNewFolderButton = true;
            this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            //
            // groupBoxWeblogPostsFolder
            //
            this.groupBoxWeblogPostsFolder.Controls.Add(this.textBoxWeblogPostsFolder);
            this.groupBoxWeblogPostsFolder.Controls.Add(this.buttonBrowserDialog);
            this.groupBoxWeblogPostsFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxWeblogPostsFolder.Location = new System.Drawing.Point(8, 154);
            this.groupBoxWeblogPostsFolder.Name = "groupBoxWeblogPostsFolder";
            this.groupBoxWeblogPostsFolder.Size = new System.Drawing.Size(345, 45);
            this.groupBoxWeblogPostsFolder.TabIndex = 4;
            this.groupBoxWeblogPostsFolder.TabStop = false;
            this.groupBoxWeblogPostsFolder.Text = "Post Folder Location";
            //
            // PostEditorPreferencesPanel
            //
            this.AccessibleName = "Preferences";
            this.Controls.Add(this.groupBoxPostWindows);
            this.Controls.Add(this.groupBoxPublishing);
            this.Controls.Add(this.groupBoxGeneral);
            this.Controls.Add(this.groupBoxWeblogPostsFolder);
            this.Name = "PostEditorPreferencesPanel";
            this.PanelName = "Preferences";
            this.Size = new System.Drawing.Size(370, 521);
            this.Controls.SetChildIndex(this.groupBoxPublishing, 0);
            this.Controls.SetChildIndex(this.groupBoxPostWindows, 0);
            this.Controls.SetChildIndex(this.groupBoxGeneral, 0);
            this.Controls.SetChildIndex(this.groupBoxWeblogPostsFolder, 0);
            this.groupBoxPublishing.ResumeLayout(false);
            this.groupBoxPostWindows.ResumeLayout(false);
            this.groupBoxGeneral.ResumeLayout(false);
            this.flowLayoutPanel.ResumeLayout(false);
            this.groupBoxWeblogPostsFolder.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

    }
}
