// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Video.YouTube;

namespace OpenLiveWriter.PostEditor.Video
{
    public partial class VideoPublishSource : MediaTab
    {
        private Video _video;
        IVideoPublisher _currentPublisher;
        private bool _firstRun = true;
        private SidebarListBox<IVideoPublisher> sideBarControl;
        private bool _isUserDirty = false;
        private string _selectedPermission;

        public VideoPublishSource(String selectedPermission)
        {
            InitializeComponent();
            _selectedPermission = selectedPermission;
            llTerms.FlatStyle = FlatStyle.System;
            llSafety.FlatStyle = FlatStyle.System;

            llSafety.LinkColor = SystemColors.HotTrack;
            llTerms.LinkColor = SystemColors.HotTrack;

            lblCategory.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Categories);
            lblDescription.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Description);
            lblPermissions.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Permissions);
            lblTags.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Tags);
            lblTitle.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Title);
            lblVideos.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Video_File);
            btnFileOpen.Text = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Video_File_Open);
            btnFileOpen.AccessibleName = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Video_Open_File);

            llTerms.LinkClicked += llTerms_LinkClicked;
            llSafety.LinkClicked += new LinkLabelLinkClickedEventHandler(llSafety_LinkClicked);

            TabText = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Tab_Title);
            TabBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.TabInsertVideoFromFile.png", false);
            BackColor = SystemColors.Control;

            // Since it is a generic control we have to create it ourself
            sideBarControl = new SidebarListBox<IVideoPublisher>();
            sideBarControl.AccessibleName = Res.Get(StringId.Plugin_Video_Provider_Select);
            sideBarControl.Name = "sideBarControl";
            sideBarControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            sideBarControl.TabIndex = 5;
            sideBarControl.Location = new Point(10, 48);
            sideBarControl.Size = new Size(101, 381);
            sideBarControl.Initialize();
            Controls.Add(sideBarControl);

            videoLoginStatusControl.Top = ContentSourceLayout.Y_MARGIN;
            videoLoginStatusControl.Left = 8;

            videoLoginStatusControl.LoginClicked += new EventHandler(videoLoginStatusControl_LoginClicked);
            videoLoginStatusControl.SwitchUserClicked += new EventHandler(videoLoginStatusControl_LoginClicked);

            ToolTip2 tip = new ToolTip2();
            tip.SetToolTip(btnFileOpen, Res.Get(StringId.BrowseForFile));
        }

        public override List<Control> GetAccessibleControls()
        {
            List<Control> controls = new List<Control>();
            if (videoLoginStatusControl.ShowLoginButton)
            {
                controls.Add(videoLoginStatusControl);
            }
            controls.Add(sideBarControl);
            controls.Add(txtFile);
            controls.Add(btnFileOpen);
            controls.Add(txtTitle);
            controls.Add(txtDescription);
            controls.Add(txtTags);
            controls.Add(comboBoxCategory);
            controls.Add(comboBoxPermissions);
            controls.Add(cbAcceptTerms);
            controls.Add(llTerms);
            controls.Add(llSafety);
            return controls;
        }

        void videoLoginStatusControl_LoginClicked(object sender, EventArgs e)
        {
            _currentPublisher.Auth.Login(true, FindForm());
        }

        void sideBarControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedPublisher();
        }

        private void UpdateSelectedPublisher()
        {
            // We get the new publisher, we always trust that _currentPublisher is not null
            _currentPublisher = sideBarControl.SelectedValue;

            videoLoginStatusControl.Auth = _currentPublisher.Auth;

            Debug.Assert(_currentPublisher != null);

            // Get the acceptance text from the publisher and link it
            llTerms.Text = _currentPublisher.AcceptanceTitle;
            llSafety.Text = _currentPublisher.SafetyTipTitle;

            txtAcceptTerms.Text = _currentPublisher.AcceptanceText;

            cbAcceptTerms.Width = txtTags.Width;
            cbAcceptTerms.Left = txtTags.Left;

            comboBoxPermissions.Items.Clear();
            if (_currentPublisher.Id != YouTubeVideoPublisher.ID)
            {
                comboBoxPermissions.Items.Add(new SecurityItem("1", Res.Get(StringId.Plugin_Video_Soapbox_Publish_Permissions_Hidden)));
                comboBoxPermissions.Items.Add(new SecurityItem("0", Res.Get(StringId.Plugin_Video_Soapbox_Publish_Permissions_Public)));

                int index = comboBoxPermissions.FindString(_selectedPermission);
                index = Math.Max(0, index);
                comboBoxPermissions.SelectedIndex = index;
            }
            else
            {
                comboBoxPermissions.Items.Add(new SecurityItem("0", Res.Get(StringId.Plugin_Video_Soapbox_Publish_Permissions_Public)));
                comboBoxPermissions.SelectedIndex = 0;
            }

            // Add the categories for the publisher
            List<CategoryItem> categoryList = _currentPublisher.Categories;
            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.Add(new CategoryItem(Guid.Empty.ToString(), String.Empty));
            if (categoryList.Count > 0)
            {
                foreach (CategoryItem c in categoryList)
                {
                    comboBoxCategory.Items.Add(c);
                }
                comboBoxCategory.SelectedIndex = 0;
                comboBoxCategory.Enabled = true;
                comboBoxCategory.DropDownWidth = DisplayHelper.AutoFitSystemComboDropDown(comboBoxCategory) + 10;
            }
            else
            {
                comboBoxCategory.Enabled = false;
            }

            cbAcceptTerms.Checked = false;
        }

        void llTerms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = _currentPublisher.AcceptanceUrl;
            LaunchUrl(link);
        }

        void llSafety_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = _currentPublisher.SafetyTipUrl;
            LaunchUrl(link);
        }

        private static void LaunchUrl(string link)
        {
            if (link != null && link != String.Empty)
            {
                try
                {
                    ShellHelper.LaunchUrl(link);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception navigating to copyright page: " + link + ", " + ex.ToString());
                }
            }
        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            ofd.Title = Res.Get(StringId.Plugin_Video_Soapbox_Publish_Video_Open_File);
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.Filter = _currentPublisher.FileFilter;
            ofd.InitialDirectory = DirectoryHelper.GetVideosFolder() ?? "";
            if (ofd.ShowDialog(ParentForm) == DialogResult.OK)
            {
                SelectedPath = ofd.FileName;
            }
        }

        public string SelectedPath
        {
            get
            {
                return txtFile.Text;
            }
            set
            {
                FileInfo fi = new FileInfo(value);
                if (fi.Exists)
                {
                    txtFile.Text = value;
                    string baseWords = Path.GetFileNameWithoutExtension(value);
                    string seperator = Res.Get(StringId.Plugin_Video_Publish_Filename_Seperator);

                    // Try to fill in as many fields as we can
                    if (!String.IsNullOrEmpty(seperator))
                    {
                        if (txtTitle.Text == String.Empty || !_isUserDirty)
                            txtTitle.Text = baseWords.Replace(seperator, " ");
                        if (txtDescription.Text == String.Empty || !_isUserDirty)
                            txtDescription.Text = baseWords.Replace(seperator, " ");
                        if (txtTags.Text == String.Empty || !_isUserDirty)
                            txtTags.Text = baseWords.Replace(seperator, " ");
                    }
                    else
                    {
                        if (txtTitle.Text == String.Empty || !_isUserDirty)
                            txtTitle.Text = baseWords;
                        if (txtDescription.Text == String.Empty || !_isUserDirty)
                            txtDescription.Text = baseWords;
                        if (txtTags.Text == String.Empty || !_isUserDirty)
                            txtTags.Text = baseWords;
                    }

                }

            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Make sure that the button is the same space away from the textbox
            // after it has been resized for localization
            int gutter = btnFileOpen.Left - txtFile.Right;
            DisplayHelper.AutoFitSystemButton(btnFileOpen);
            int newGutter = btnFileOpen.Left - txtFile.Right;
            txtFile.Width += (newGutter - gutter);
        }

        #region VideoSource Members

        public override bool ValidateSelection()
        {
            string title = txtTitle.Text.Trim();
            txtTags.Text = _currentPublisher.FormatTags(txtTags.Text);
            string tags = txtTags.Text;
            string description = txtDescription.Text.Trim();
            string filePath = txtFile.Text.Trim();

            // Make sure they filled in all the fields
            if (((CategoryItem)comboBoxCategory.SelectedItem).CategoryId == Guid.Empty.ToString() ||
                title == String.Empty ||
                description == String.Empty ||
                tags == String.Empty ||
                filePath == String.Empty)
            {
                DisplayMessage.Show(MessageId.VideoRequiredFields, ParentForm, ApplicationEnvironment.ProductNameQualified);
                return false;
            }

            // Make sure they agreed to the terms of use
            if (!cbAcceptTerms.Checked)
            {
                DisplayMessage.Show(MessageId.MustAcceptTermsOfUse, ParentForm, ApplicationEnvironment.ProductNameQualified);
                return false;
            }

            // Make sure they are logged in
            if (!_currentPublisher.Auth.IsLoggedIn)
            {
                if (_currentPublisher.Auth.Login(true, FindForm()))
                {
                    videoLoginStatusControl.UpdateStatus();
                }
                else
                {
                    return false;
                }
            }

            if (!PathHelper.IsPathVideo(filePath))
            {
                DisplayMessage.Show(MessageId.VideoPathInvalid, ParentForm, ApplicationEnvironment.ProductNameQualified);
                return false;
            }

            try
            {
                // Try to make a video
                _video = _currentPublisher.GetVideo(title, description, tags, ((CategoryItem)comboBoxCategory.SelectedItem).CategoryId, ((CategoryItem)comboBoxCategory.SelectedItem).CategoryName, ((SecurityItem)comboBoxPermissions.SelectedItem).SecurityId, ((SecurityItem)comboBoxPermissions.SelectedItem).SecurityName);

                if (_video == null)
                    return false;

            }
            catch (WebException ex)
            {
                Trace.WriteLine("Failed to start video publish: " + ex);
                DisplayMessage message = new DisplayMessage(Res.Get(StringId.VideoNetworkError), Res.Get(StringId.VideoError));
                message.Show(ParentForm, ApplicationEnvironment.ProductNameQualified);
                return false;
            }
            catch (VideoPublishException ex)
            {
                DisplayMessage message = new DisplayMessage(ex.Message, Res.Get(StringId.VideoError));
                message.Show(ParentForm, ApplicationEnvironment.ProductNameQualified);
                return false;
            }

            // Save the name of the user that published the video, we will need
            // this later when we open a post with an unpublished video
            _video.Username = _currentPublisher.Auth.Username;

            return true;
        }

        public override void SaveContent(MediaSmartContent content)
        {
            // Start to actually publish the video
            _video.StatusWatcher = _currentPublisher.Publish(txtTitle.Text, txtDescription.Text, txtTags.Text, ((CategoryItem)comboBoxCategory.SelectedItem).CategoryId, ((CategoryItem)comboBoxCategory.SelectedItem).CategoryName, ((SecurityItem)comboBoxPermissions.SelectedItem).SecurityId, ((SecurityItem)comboBoxPermissions.SelectedItem).SecurityName, txtFile.Text);
            ((VideoSmartContent)content).Initialize(_video, _blogId);

        }

        public override void TabSelected()
        {
            if (_firstRun)
            {
                sideBarControl.SelectedIndex = 0;
                _firstRun = false;
            }
            else
            {
                _currentPublisher.Auth.Login(false, FindForm());
                videoLoginStatusControl.UpdateStatus();
            }

            txtFile.Select();
        }

        public override void Init(MediaSmartContent content, string blogId)
        {
            base.Init(content, blogId);

            if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.YouTubeVideo))
            {
                YouTubeVideoPublisher youtube = new YouTubeVideoPublisher();
                youtube.Init(null, this, blogId); //FIXME
                sideBarControl.SetEntry(youtube, ResourceHelper.LoadAssemblyResourceBitmap("Video.YouTube.Images.Sidebar.png"), youtube.ToString());
            }

            sideBarControl.SelectedIndexChanged += new EventHandler(sideBarControl_SelectedIndexChanged);

        }

        #endregion

        private void textboxKeyDown(object sender, KeyEventArgs e)
        {
            _isUserDirty = true;
        }

        private void txtTags_Leave(object sender, EventArgs e)
        {
            txtTags.Text = _currentPublisher.FormatTags(txtTags.Text);
        }
    }

}
