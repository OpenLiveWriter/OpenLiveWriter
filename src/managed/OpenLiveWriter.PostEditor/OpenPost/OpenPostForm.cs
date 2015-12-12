// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    /// <summary>
    /// Summary description for OpenPostForm.
    /// </summary>
    public class OpenPostForm : ApplicationDialog
    {
        public enum OpenMode
        {
            Auto,
            Drafts,
            RecentPosts
        }

        public OpenPostForm()
            : this(OpenMode.Auto)
        {
        }

        private ToolTip2 toolTip;

        public OpenPostForm(OpenMode openMode)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.buttonDelete.Text = Res.Get(StringId.OpenPostDelete) + " ";
            this.labelOpenPostFrom.Text = Res.Get(StringId.OpenPostOpenFrom);
            this.labelPosts.Text = Res.Get(StringId.OpenPostItems);
            this.labelShow.Text = Res.Get(StringId.OpenPostShow);
            this.radioButtonPosts.Text = Res.Get(StringId.Posts);
            this.radioButtonPages.Text = Res.Get(StringId.Pages);
            this.Text = Res.Get(StringId.Open);

            _openMode = openMode;
            _includeDrafts = true;
            _allowDelete = true;

            listBoxPosts.AccessibleName = Res.Get(StringId.OpenPostItems);

            // Set up the timer that will be used to track when a user is done typing in the filter box
            _filterTimer = new Timer();
            _filterTimer.Interval = 100;
            _filterTimer.Tick += new EventHandler(_timer_Tick);

            // We need to know when the box is refreshing so we can disable the search box
            listBoxPosts.RefreshBegin += new EventHandler(listBoxPosts_RefreshBegin);

            // We need to know when it gains or loses focus so we can remove the default text or put it back
            textBoxFilter.Enter += new EventHandler(textBoxFilter_Enter);
            textBoxFilter.Leave += new EventHandler(textBoxFilter_Leave);

            textBoxFilter.KeyDown += new KeyEventHandler(textBoxFilter_KeyDown);
            textBoxFilter.EnabledChanged += new EventHandler(textBoxFilter_EnabledChanged);

            filterPictureBox.Image = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.Search.png");
            filterPictureBox.BackColor = SystemColors.Window;
            filterPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            filterPictureBox.BringToFront();

            comboBoxPosts.AccessibleName = Res.Get(StringId.PostCountAccessible);
        }

        void textBoxFilter_EnabledChanged(object sender, EventArgs e)
        {
            filterPictureBox.BackColor = textBoxFilter.Enabled ? SystemColors.Window : SystemColors.Control;
        }

        void textBoxFilter_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxPosts.SelectedIndex;

            if (e.KeyCode == Keys.Up)
            {
                index--;
                if (listBoxPosts.Items.Count > 0 && index >= 0)
                    listBoxPosts.SelectedIndex = index;

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Down)
            {
                index++;
                if (listBoxPosts.Items.Count > 0 && index < listBoxPosts.Items.Count)
                    listBoxPosts.SelectedIndex = index;

                e.Handled = true;
            }
        }

        void textBoxFilter_Leave(object sender, EventArgs e)
        {
            // Put back the default text
            if (string.IsNullOrEmpty(textBoxFilter.Text))
            {
                ResetFilterBox();
            }
        }

        void textBoxFilter_Enter(object sender, EventArgs e)
        {
            // If the default text is in the box, remove it
            if (!_filterDirty)
                textBoxFilter.Text = String.Empty;

            _filterDirty = true;
        }

        private void ResetFilterBox()
        {
            // Reset the textbox back to the default text with grey font
            _filterDirty = false;
            textBoxFilter.ForeColor = SystemColors.GrayText;

            string text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PostPageFilter), listBoxPosts.ShowPages ? Res.Get(StringId.PagesLower) : Res.Get(StringId.PostsLower));
            textBoxFilter.Text = text;
            textBoxFilter.AccessibleName = text;
            listBoxPosts.ClearFilterWithoutUpdate();
        }

        void listBoxPosts_RefreshBegin(object sender, EventArgs e)
        {
            // While the listbox is loading, disable the textbox and say why it is disabled
            textBoxFilter.Enabled = false;
            ResetFilterBox();
        }

        public bool IncludeDrafts
        {
            get { return _includeDrafts; }
            set { _includeDrafts = value; }
        }

        public bool AllowDelete
        {
            get { return _allowDelete; }
            set { _allowDelete = value; }
        }

        private PostInfo BlogPostInfo
        {
            get
            {
                return listBoxPosts.SelectedPost;
            }
        }

        public IBlogPostEditingContext BlogPostEditingContext
        {
            get
            {
                return _selectedPost;
            }
        }
        private IBlogPostEditingContext _selectedPost = null;

        public event ValidatePostEventHandler ValidatePost;

        public event UserDeletedPostEventHandler UserDeletedPost
        {
            add
            {
                listBoxPosts.UserDeletedPost += value;
            }
            remove
            {
                listBoxPosts.UserDeletedPost -= value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // size form (set anchor properties here because doing so in the designer causes
            // hosage of the designer)
            // listBoxPostSources.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom ;
            // listBoxPosts.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom ;
            // Size = PostEditorSettings.OpenPostFormSize ;

            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);

            DisplayHelper.AutoFitSystemLabel(labelShow, 0, int.MaxValue);
            //DisplayHelper.AutoFitSystemCombo(comboBoxPosts, 0, int.MaxValue, false);
            DisplayHelper.AutoFitSystemLabel(labelPosts, 0, int.MaxValue);
            DisplayHelper.AutoFitSystemRadioButton(radioButtonPosts, 0, int.MaxValue);
            DisplayHelper.AutoFitSystemRadioButton(radioButtonPages, 0, int.MaxValue);
            int growBy = GetMaxWidth() - listBoxPosts.Right;
            if (growBy > 0)
            {
                Width += growBy;
                listBoxPosts.Width += growBy;
            }

            // post sources list
            listBoxPostSources.Initialize(_includeDrafts);
            listBoxPostSources.SelectedIndexChanged += new EventHandler(listBoxPostSources_SelectedIndexChanged);

            // number of posts combo
            comboBoxPosts.SelectedIndexChanged += new EventHandler(comboBoxPosts_SelectedIndexChanged);

            // post list
            listBoxPosts.Initialize(this);
            listBoxPosts.SelectedIndexChanged += new EventHandler(listBoxPosts_SelectedIndexChanged);
            listBoxPosts.PostsRefreshed += new EventHandler(listBoxPosts_PostsRefreshed);
            listBoxPosts.DoubleClick += new EventHandler(listBoxPosts_DoubleClick);

            buttonDelete.Height = comboBoxPosts.Height + 2;

            textBoxFilter.Width = listBoxPosts.Width;
            int inset = Convert.ToInt32((textBoxFilter.Height - filterPictureBox.Height) / 2f);
            filterPictureBox.Top = textBoxFilter.Top + inset;
            filterPictureBox.Left = textBoxFilter.Right - 16 - inset;

            // select post source
            SelectInitialPostSource();
        }

        private void SelectInitialPostSource()
        {
            switch (_openMode)
            {
                case OpenMode.Auto:
                    AutoSelectPostSource();
                    break;

                case OpenMode.Drafts:
                    listBoxPostSources.SelectDrafts();
                    break;

                case OpenMode.RecentPosts:
                    listBoxPostSources.SelectRecentPosts();
                    break;
            }
        }

        private void AutoSelectPostSource()
        {
            if (_includeDrafts)
            {
                // select Drafts post source (will cause population of other lists)
                listBoxPostSources.SelectDrafts();

                // if this results in no posts then switch to Recent Posts
                if (listBoxPosts.Items.Count == 0)
                    listBoxPostSources.SelectRecentPosts();

                // if this results in no posts then switch back to Drafts
                if (listBoxPosts.Items.Count == 0)
                    listBoxPostSources.SelectDrafts();
            }
            else
            {
                listBoxPostSources.SelectRecentPosts();
            }
        }

        private void listBoxPostSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            IPostEditorPostSource postSource = listBoxPostSources.SelectedPostSource;
            if (postSource != null)
            {
                listBoxPostSources.Update();

                listBoxPosts.PostSource = null; // prevent reloading until we have set all values

                // update # of posts combo
                comboBoxPosts.Items.Clear();
                int indexOf50 = -1;
                RecentPostRequest[] requests = listBoxPostSources.SelectedPostSource.RecentPostCapabilities.ValidRequests;
                for (int i = 0; i < requests.Length; i++)
                {
                    comboBoxPosts.Items.Add(requests[i]);
                    if (requests[i].NumberOfPosts <= 50)
                        indexOf50 = i;
                }

                if (indexOf50 != -1)
                    comboBoxPosts.SelectedIndex = indexOf50;

                // update post list
                RestorePostListViewSettings();
                listBoxPosts.PostSource = listBoxPostSources.SelectedPostSource;

                ManageControls(true);
            }
        }

        private void comboBoxPosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxPosts.RecentPostRequest = comboBoxPosts.SelectedItem as RecentPostRequest;

            SaveNumberOfPostsSettings();

            ManageControls(true);
        }

        private void radioButtonPostsOrPages_CheckedChanged(object sender, System.EventArgs e)
        {
            listBoxPosts.ShowPages = radioButtonPages.Checked;

            SavePostsOrPagesSettings();

            ManageControls(true);
        }

        private void listBoxPosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            ManageControls(false);
        }

        private void listBoxPosts_PostsRefreshed(object sender, EventArgs e)
        {
            //Hack! - refreshing the items in listBoxPosts causes the list to steal accessibility
            //focus from the postSourceListbox. So if the listBoxPostSources is focused, we need to
            //explicitly fire a focus changed event so that accessibility focus will return to the
            //correct control.
            if (listBoxPostSources.Focused)
                (listBoxPostSources.AccessibilityObject as BlogPostSourceListBox.BlogPostSourceListBoxAccessibility).NotifySelectionChanged();

            textBoxFilter.Enabled = true;
        }

        private void listBoxPosts_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxPosts.SelectedIndex != -1)
                AcceptSelectedPost();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            AcceptSelectedPost();
        }

        private void AcceptSelectedPost()
        {
            // see if there is anyone listening to the validate event (to veto the selection)
            if (ValidateSelectedPost())
            {
                // get the post from the list box
                try
                {
                    // get the post
                    using (new WaitCursor())
                        _selectedPost = listBoxPosts.RetrieveSelectedPost();

                    // if that succeeded then allow the dialog to be dismissed
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    DisplayableExceptionDisplayForm.Show(this, ex);
                }
            }
        }

        // see if there is anyone listening to the validate event (to veto the selection)
        private bool ValidateSelectedPost()
        {
            if (BlogPostInfo == null)
                return false;

            // perform optional event based validation
            if (ValidatePost != null)
            {
                ValidatePostEventArgs ea = new ValidatePostEventArgs(this, BlogPostInfo);
                ValidatePost(this, ea);

                if (!ea.PostIsValid)
                    return false;
            }

            // if we get through validation then return true
            return true;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            listBoxPosts.RefreshPosts();
            ManageControls(true);
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            listBoxPosts.DeleteSelectedPost();
        }

        private int GetMaxWidth()
        {
            int pad = GetHpadding();
            // a
            int x = labelShow.Right + pad;
            // b
            x += /*comboBoxPosts.Width*/ (int)DisplayHelper.ScaleX(160) + pad;
            // c
            x += Math.Max(labelPosts.Width, radioButtonPosts.Width + pad + radioButtonPages.Width);
            x += pad;
            // d
            x += pad;
            // e
            x += buttonDelete.Width;

            return x;
        }

        private void RearrangeControls()
        {
            if (!Visible)
                return;

            // This width depends on the contents of the combo, which are dynamic
            DisplayHelper.AutoFitSystemCombo(comboBoxPosts, 0, int.MaxValue, false);

            int HPADDING = GetHpadding();
            int x;

            // a
            x = labelShow.Right + HPADDING;

            // b
            comboBoxPosts.Left = x;
            x = comboBoxPosts.Right + HPADDING;

            // c
            if (labelPosts.Visible)
            {
                labelPosts.Left = x;
            }
            else
            {
                panelType.Left = x;

                int x1 = 0;
                radioButtonPosts.Left = x1;
                x1 = radioButtonPosts.Right + HPADDING;

                radioButtonPages.Left = x1;
                // x1 = radioButtonPages.Right + HPADDING;

                panelType.Width = radioButtonPages.Right;

            }

            BidiHelper.RtlLayoutFixup(panelType);
            buttonDelete.Left = textBoxFilter.Right - buttonDelete.Width;
        }

        private static int GetHpadding()
        {
            return (int)Math.Ceiling(DisplayHelper.ScaleX(5));
        }

        private void ManageControls(bool fullManage)
        {
            SuspendLayout();

            // get post source
            IPostEditorPostSource selectedPostSource = listBoxPostSources.SelectedPostSource;

            // manage posts vs. pages
            if ((selectedPostSource) != null && selectedPostSource.SupportsPages)
            {
                labelPosts.Visible = false;
                panelType.Visible = true;
                if (fullManage)
                {
                    panelType.Left = labelPosts.Left + ScaleX(5);
                }
            }
            else
            {
                labelPosts.Visible = true;
                panelType.Visible = false;
            }

            // manage delete button
            bool allowDelete = _allowDelete && (selectedPostSource != null) && (selectedPostSource.SupportsDelete);
            buttonDelete.Visible = allowDelete;
            buttonDelete.Width = buttonDelete.GetPreferredSize(new Size(Int32.MaxValue, buttonDelete.Height)).Width;
            buttonDelete.Left = textBoxFilter.Right - buttonDelete.Width;

            listBoxPosts.AllowDelete = allowDelete;

            // control enabled state
            bool postSelected = listBoxPosts.SelectedIndex != -1;
            buttonDelete.Enabled = postSelected;
            buttonOK.Enabled = postSelected;

            RearrangeControls();

            ResumeLayout();

            // force immediate re-rendering of controls
            PerformLayout();
            Update();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // save settings
            PostEditorSettings.OpenPostFormSize = Size;
        }

        private void SaveNumberOfPostsSettings()
        {
            // get post data/context to save
            using (SettingsPersisterHelper postSourceSettings = _recentPostsSettings.GetSubSettings(listBoxPostSources.SelectedPostSource.UniqueId))
            {
                // number of posts
                int numberOfPosts = (comboBoxPosts.SelectedItem as RecentPostRequest).NumberOfPosts;
                postSourceSettings.SetInt32(NUMBER_OF_POSTS, numberOfPosts);
            }
        }

        private void SavePostsOrPagesSettings()
        {
            // get post data/context to save
            using (SettingsPersisterHelper postSourceSettings = _recentPostsSettings.GetSubSettings(listBoxPostSources.SelectedPostSource.UniqueId))
            {
                if (listBoxPostSources.SelectedPostSource.SupportsPages)
                    postSourceSettings.SetBoolean(SHOW_PAGES, radioButtonPages.Checked);
                else
                    postSourceSettings.SetBoolean(SHOW_PAGES, false);
            }
        }

        private void RestorePostListViewSettings()
        {
            IPostEditorPostSource postSource = listBoxPostSources.SelectedPostSource as IPostEditorPostSource;

            using (SettingsPersisterHelper postSourceSettings = _recentPostsSettings.GetSubSettings(postSource.UniqueId))
            {
                // number of posts
                RecentPostRequest recentPostRequest = new RecentPostRequest(postSourceSettings.GetInt32(NUMBER_OF_POSTS, postSource.RecentPostCapabilities.DefaultRequest.NumberOfPosts));
                if (comboBoxPosts.Items.Contains(recentPostRequest))
                    comboBoxPosts.SelectedItem = recentPostRequest;

                // pages
                if (postSource.SupportsPages)
                {
                    radioButtonPages.Checked = postSourceSettings.GetBoolean(SHOW_PAGES, false);
                }
                else
                {
                    radioButtonPages.Checked = false;
                }
                radioButtonPosts.Checked = !radioButtonPages.Checked;
            }

        }

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }
        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }
        #endregion

        private const string NUMBER_OF_POSTS = "NumberOfPosts";
        private const string SHOW_PAGES = "ShowPages";

        private TextBox textBoxFilter;
        private PictureBox filterPictureBox;
        private Label labelShow;
        private System.Windows.Forms.Panel panelType;
        private System.Windows.Forms.RadioButton radioButtonPosts;
        private System.Windows.Forms.RadioButton radioButtonPages;

        private SettingsPersisterHelper _recentPostsSettings = PostEditorSettings.SettingsKey.GetSubSettings("RecentPostDefaults");

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _recentPostsSettings.Dispose();

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenPostForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listBoxPostSources = new OpenLiveWriter.PostEditor.OpenPost.BlogPostSourceListBox();
            this.listBoxPosts = new OpenLiveWriter.PostEditor.OpenPost.BlogPostListBox();
            this.labelOpenPostFrom = new System.Windows.Forms.Label();
            this.comboBoxPosts = new System.Windows.Forms.ComboBox();
            this.labelPosts = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.labelShow = new System.Windows.Forms.Label();
            this.filterPictureBox = new System.Windows.Forms.PictureBox();
            this.panelType = new System.Windows.Forms.Panel();
            this.radioButtonPages = new System.Windows.Forms.RadioButton();
            this.radioButtonPosts = new System.Windows.Forms.RadioButton();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.toolTip = new OpenLiveWriter.Controls.ToolTip2(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.filterPictureBox)).BeginInit();
            this.panelType.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(414, 421);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 26);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(514, 421);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 26);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            //
            // listBoxPostSources
            //
            this.listBoxPostSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxPostSources.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxPostSources.IntegralHeight = false;
            this.listBoxPostSources.Location = new System.Drawing.Point(13, 40);
            this.listBoxPostSources.Name = "listBoxPostSources";
            this.listBoxPostSources.Size = new System.Drawing.Size(181, 373);
            this.listBoxPostSources.TabIndex = 1;
            //
            // listBoxPosts
            //
            this.listBoxPosts.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxPosts.Filter = "";
            this.listBoxPosts.IntegralHeight = false;
            this.listBoxPosts.Location = new System.Drawing.Point(202, 70);
            this.listBoxPosts.Name = "listBoxPosts";
            this.listBoxPosts.PostSource = null;
            this.listBoxPosts.ShowPages = false;
            this.listBoxPosts.Size = new System.Drawing.Size(400, 343);
            this.listBoxPosts.TabIndex = 3;
            //
            // labelOpenPostFrom
            //
            this.labelOpenPostFrom.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelOpenPostFrom.Location = new System.Drawing.Point(16, 12);
            this.labelOpenPostFrom.Name = "labelOpenPostFrom";
            this.labelOpenPostFrom.Size = new System.Drawing.Size(178, 16);
            this.labelOpenPostFrom.TabIndex = 0;
            this.labelOpenPostFrom.Text = "&Open from:";
            //
            // comboBoxPosts
            //
            this.comboBoxPosts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPosts.Location = new System.Drawing.Point(240, 7);
            this.comboBoxPosts.Name = "comboBoxPosts";
            this.comboBoxPosts.Size = new System.Drawing.Size(108, 23);
            this.comboBoxPosts.TabIndex = 6;
            //
            // labelPosts
            //
            this.labelPosts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPosts.Location = new System.Drawing.Point(655, 11);
            this.labelPosts.Name = "labelPosts";
            this.labelPosts.Size = new System.Drawing.Size(42, 18);
            this.labelPosts.TabIndex = 7;
            this.labelPosts.Text = "items";
            //
            // buttonDelete
            //
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.Location = new System.Drawing.Point(525, 6);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(77, 26);
            this.buttonDelete.TabIndex = 10;
            this.buttonDelete.Text = "&Delete";
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            //
            // labelShow
            //
            this.labelShow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelShow.Location = new System.Drawing.Point(203, 11);
            this.labelShow.Name = "labelShow";
            this.labelShow.Size = new System.Drawing.Size(33, 18);
            this.labelShow.TabIndex = 5;
            this.labelShow.Text = "&Show";
            //
            // filterPictureBox
            //
            this.filterPictureBox.Location = new System.Drawing.Point(204, 42);
            this.filterPictureBox.Name = "filterPictureBox";
            this.filterPictureBox.Size = new System.Drawing.Size(16, 16);
            this.filterPictureBox.TabIndex = 10;
            this.filterPictureBox.TabStop = false;
            //
            // panelType
            //
            this.panelType.Controls.Add(this.radioButtonPages);
            this.panelType.Controls.Add(this.radioButtonPosts);
            this.panelType.Location = new System.Drawing.Point(354, 10);
            this.panelType.Name = "panelType";
            this.panelType.Size = new System.Drawing.Size(168, 18);
            this.panelType.TabIndex = 8;
            //
            // radioButtonPages
            //
            this.radioButtonPages.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonPages.Location = new System.Drawing.Point(100, 0);
            this.radioButtonPages.Name = "radioButtonPages";
            this.radioButtonPages.Size = new System.Drawing.Size(65, 18);
            this.radioButtonPages.TabIndex = 1;
            this.radioButtonPages.Text = "P&ages";
            this.radioButtonPages.CheckedChanged += new System.EventHandler(this.radioButtonPostsOrPages_CheckedChanged);
            //
            // radioButtonPosts
            //
            this.radioButtonPosts.Checked = true;
            this.radioButtonPosts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonPosts.Location = new System.Drawing.Point(3, 0);
            this.radioButtonPosts.Name = "radioButtonPosts";
            this.radioButtonPosts.Size = new System.Drawing.Size(64, 18);
            this.radioButtonPosts.TabIndex = 0;
            this.radioButtonPosts.TabStop = true;
            this.radioButtonPosts.Text = "&Posts";
            this.radioButtonPosts.CheckedChanged += new System.EventHandler(this.radioButtonPostsOrPages_CheckedChanged);
            //
            // textBoxFilter
            //
            this.textBoxFilter.Location = new System.Drawing.Point(202, 40);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(400, 23);
            this.textBoxFilter.TabIndex = 2;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            //
            // OpenPostForm
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(621, 458);
            this.Controls.Add(this.panelType);
            this.Controls.Add(this.labelShow);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.labelPosts);
            this.Controls.Add(this.comboBoxPosts);
            this.Controls.Add(this.labelOpenPostFrom);
            this.Controls.Add(this.listBoxPosts);
            this.Controls.Add(this.listBoxPostSources);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxFilter);
            this.Controls.Add(this.filterPictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenPostForm";
            this.Text = "Open";
            ((System.ComponentModel.ISupportInitialize)(this.filterPictureBox)).EndInit();
            this.panelType.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        void _timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer and set the filter the user has entered
            _filterTimer.Stop();
            listBoxPosts.Filter = textBoxFilter.Text;

        }

        void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            _filterTimer.Stop();

            // Don't set the filter if the  textbox was disabled or if it is the default text
            if (!textBoxFilter.Enabled || !_filterDirty)
                return;

            // When a user enters a filter we set it black
            textBoxFilter.ForeColor = SystemColors.WindowText;
            textBoxFilter.Font = Res.GetFont(FontSize.Normal, FontStyle.Regular);

            // Mark the filter field dirty and then start the timer so we dont run the filter while they are still typing.
            _filterDirty = true;
            _filterTimer.Start();
        }
        #endregion

        private Button buttonOK;
        private Button buttonCancel;
        private BlogPostSourceListBox listBoxPostSources;
        private BlogPostListBox listBoxPosts;
        private Label labelOpenPostFrom;
        private Label labelPosts;
        private ComboBox comboBoxPosts;
        private Button buttonDelete;
        private IContainer components;

        private OpenMode _openMode;
        private bool _includeDrafts;
        private bool _allowDelete;
        private bool _filterDirty = false;
        private Timer _filterTimer;

    }

    public delegate void ValidatePostEventHandler(object sender, ValidatePostEventArgs ea);

    public class ValidatePostEventArgs : EventArgs
    {
        public ValidatePostEventArgs(IWin32Window openPostDialog, PostInfo postInfo)
        {
            _openPostDialog = openPostDialog;
            _postInfo = postInfo;
            _postIsValid = true;
        }

        public IWin32Window OpenPostDialog { get { return _openPostDialog; } }
        private IWin32Window _openPostDialog;
        public PostInfo PostInfo { get { return _postInfo; } }
        private PostInfo _postInfo;
        public bool PostIsValid { get { return _postIsValid; } set { _postIsValid = value; } }
        private bool _postIsValid;
    }
}

