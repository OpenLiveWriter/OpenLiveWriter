// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    /*
     * TODO
     * Bugs:
     - Visibility is all screwed up!!
     - Position of page-relevant labels vs. controls in dialog
     * Putting focus in a textbox that has a cue banner, causes dirty flag to be marked
     - Space should trigger View All
     - F2 during page context shows too many labels
     - Label visibility not staying in sync with control
     - Unchecking publish date in dialog doesn't set cue banner in band
     - Activate main window when dialog is dismissed
     - Some labels not localized
     - Labels do not have mnemonics
     - Clicking on View All should restore a visible but minimized dialog
     * Horizontal scrollbar sometimes flickers on
     - Trackback detailed label
     * Dropdown for Page Parent combo is not the right width
     * Each Page Parent combo makes its own delayed request
     - Tags control should share leftover space with category control
     x Properties dialog should hide and come back
     *
     * Questions:
     * Should Enter dismiss the properties dialog?
     * Should properties dialog scroll state be remembered between views?
     */
    public partial class PostPropertiesBandControl : UserControl, IBlogPostEditor, IRtlAware, INewCategoryContext
    {
        private Blog _targetBlog;
        private IBlogClientOptions _clientOptions;

        private const int COL_CATEGORY = 0;
        private const int COL_TAGS = 1;
        private const int COL_DATE = 2;
        private const int COL_PAGEPARENTLABEL = 3;
        private const int COL_PAGEPARENT = 4;
        private const int COL_PAGEORDERLABEL = 5;
        private const int COL_PAGEORDER = 6;
        private const int COL_FILLER = 7;
        private const int COL_VIEWALL = 8;

        private readonly PostPropertiesForm postPropertiesForm;
        private readonly List<PropertyField> fields = new List<PropertyField>();
        private readonly SharedPropertiesController controller;
        private readonly CategoryContext categoryContext;

        public PostPropertiesBandControl(CommandManager commandManager)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            categoryContext = new CategoryContext();

            controller = new SharedPropertiesController(this, null, categoryDropDown,
                null, textTags, labelPageOrder, textPageOrder, labelPageParent, comboPageParent, null,
                datePublishDate, fields, categoryContext);

            SimpleTextEditorCommandHelper.UseNativeBehaviors(commandManager,
                textTags, textPageOrder);

            postPropertiesForm = new PostPropertiesForm(commandManager, categoryContext);
            if (components == null)
                components = new Container();
            components.Add(postPropertiesForm);

            postPropertiesForm.Synchronize(controller);

            commandManager.Add(CommandId.PostProperties, PostProperties_Execute);
            commandManager.Add(CommandId.ShowCategoryPopup, ShowCategoryPopup_Execute);

            linkViewAll.KeyDown += (sender, args) =>
                                   {
                                       if (args.KeyValue == ' ')
                                           linkViewAll_LinkClicked(sender, new LinkLabelLinkClickedEventArgs(null));
                                   };

            // WinLive 180287: We don't want to show or use mnemonics on labels in the post properties band because
            // they can steal focus from the canvas.
            linkViewAll.Text = TextHelper.StripAmpersands(Res.Get(StringId.ViewAll));
            linkViewAll.UseMnemonic = false;
            labelPageParent.Text = TextHelper.StripAmpersands(Res.Get(StringId.PropertiesPageParent));
            labelPageParent.UseMnemonic = false;
            labelPageOrder.Text = TextHelper.StripAmpersands(Res.Get(StringId.PropertiesPageOrder));
            labelPageOrder.UseMnemonic = false;
        }

        private void ShowCategoryPopup_Execute(object sender, EventArgs e)
        {
            if (postPropertiesForm.Visible)
                postPropertiesForm.DisplayCategoryForm();
            else
                categoryDropDown.DisplayCategoryForm();
        }

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);

            FixCategoryDropDown();
        }

        private void FixCategoryDropDown()
        {
            // Exactly align the sizes of the category control and the publish datetime picker control
            int nonItemHeight = categoryDropDown.Height - categoryDropDown.ItemHeight;
            categoryDropDown.ItemHeight = datePublishDate.Height - nonItemHeight;
            categoryDropDown.Height = datePublishDate.Height;

            datePublishDate.LocationChanged += delegate
            {
                // Exactly align the vertical position of the category control and the publish datetime picker control
                categoryDropDown.Anchor = categoryDropDown.Anchor | AnchorStyles.Top;
                Padding margin = categoryDropDown.Margin;
                margin.Top = datePublishDate.Top;
                categoryDropDown.Margin = margin;
            };
        }

        private void PostProperties_Execute(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            if (postPropertiesForm.Visible)
                postPropertiesForm.Hide();
            else
                postPropertiesForm.Show(FindForm());
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Without the height/width checks, minimizing and restoring causes painting to blow up
            if (!SystemInformation.HighContrast && table.Height > 0 && table.Width > 0 && panelShadow.Height > 0 && panelShadow.Width > 0)
            {
                using (
                    Brush brush = new LinearGradientBrush(table.Bounds, Color.FromArgb(0xDC, 0xE7, 0xF5), Color.White,
                                                          LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(brush, table.Bounds);
                using (
                    Brush brush = new LinearGradientBrush(panelShadow.Bounds, Color.FromArgb(208, 208, 208), Color.White,
                                                          LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(brush, panelShadow.Bounds);
            }
            else
            {
                e.Graphics.Clear(SystemColors.Window);
            }
        }

        private bool categoryVisible = true;

        private bool CategoryVisible
        {
            set
            {
                table.ColumnStyles[COL_CATEGORY].SizeType = value ? SizeType.Percent : SizeType.AutoSize;
                categoryDropDown.Visible = categoryVisible = value;
                ManageFillerVisibility();
            }
        }

        private bool tagsVisible = true;

        private bool TagsVisible
        {
            set
            {
                table.ColumnStyles[COL_TAGS].SizeType = value ? SizeType.Percent : SizeType.AutoSize;
                textTags.Visible = tagsVisible = value;
                ManageFillerVisibility();
            }
        }

        private void ManageFillerVisibility()
        {
            bool shouldShow = !categoryVisible && !tagsVisible;
            table.ColumnStyles[COL_FILLER].SizeType = shouldShow ? SizeType.Percent : SizeType.AutoSize;
        }

        private IBlogPostEditingContext _editorContext;
        public void Initialize(IBlogPostEditingContext editorContext, IBlogClientOptions clientOptions)
        {
            _editorContext = editorContext;
            _clientOptions = clientOptions;

            controller.Initialize(editorContext, clientOptions);
            ((IBlogPostEditor)postPropertiesForm).Initialize(editorContext, clientOptions);

            ManageLayout();
        }

        private bool IsPage
        {
            get { return _editorContext != null && _editorContext.BlogPost != null ? _editorContext.BlogPost.IsPage : false; }

        }

        public void OnBlogChanged(Blog newBlog)
        {
            _clientOptions = newBlog.ClientOptions;
            _targetBlog = newBlog;

            controller.OnBlogChanged(newBlog);
            ((IBlogPostEditor)postPropertiesForm).OnBlogChanged(newBlog);

            ManageLayout();
        }

        public void OnBlogSettingsChanged(bool templateChanged)
        {
            controller.OnBlogSettingsChanged(templateChanged);
            ((IBlogPostEditor)postPropertiesForm).OnBlogSettingsChanged(templateChanged);

            ManageLayout();
        }

        private void ManageLayout()
        {
            if (IsPage)
            {
                bool showViewAll = _clientOptions.SupportsCommentPolicy
                                   || _clientOptions.SupportsPingPolicy
                                   || _clientOptions.SupportsAuthor
                                   || _clientOptions.SupportsSlug
                                   || _clientOptions.SupportsPassword;
                linkViewAll.Visible = showViewAll;
                CategoryVisible = false;
                TagsVisible = false;
                Visible = showViewAll || _clientOptions.SupportsPageParent || _clientOptions.SupportsPageOrder;
            }
            else
            {
                bool showViewAll = _clientOptions.SupportsCommentPolicy
                                   || _clientOptions.SupportsPingPolicy
                                   || _clientOptions.SupportsAuthor
                                   || _clientOptions.SupportsSlug
                                   || _clientOptions.SupportsPassword
                                   || _clientOptions.SupportsExcerpt
                                   || _clientOptions.SupportsTrackbacks;

                bool showTags = (_clientOptions.SupportsKeywords && (_clientOptions.KeywordsAsTags || _clientOptions.SupportsGetKeywords));
                Visible = showViewAll
                          || _clientOptions.SupportsCustomDate
                          || showTags
                          || _clientOptions.SupportsCategories;

                CategoryVisible = _clientOptions.SupportsCategories;
                TagsVisible = showTags;

                linkViewAll.Visible = showViewAll;
            }
        }

        public bool IsDirty
        {
            get { return controller.IsDirty || ((IBlogPostEditor)postPropertiesForm).IsDirty; }
        }

        public bool HasKeywords
        {
            get { return postPropertiesForm.HasKeywords; }
        }

        public void SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            controller.SaveChanges(post, options);
            ((IBlogPostEditor)postPropertiesForm).SaveChanges(post, options);
        }

        public bool ValidatePublish()
        {
            return controller.ValidatePublish();
        }

        public void OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            controller.OnPublishSucceeded(blogPost, postResult);
            ((IBlogPostEditor)postPropertiesForm).OnPublishSucceeded(blogPost, postResult);
        }

        public void OnClosing(CancelEventArgs e)
        {
            controller.OnClosing(e);
            ((IBlogPostEditor)postPropertiesForm).OnClosing(e);
        }

        public void OnPostClosing(CancelEventArgs e)
        {
            controller.OnPostClosing(e);
            ((IBlogPostEditor)postPropertiesForm).OnPostClosing(e);
        }

        public void OnClosed()
        {
            controller.OnClosed();
            ((IBlogPostEditor)postPropertiesForm).OnClosed();
        }

        public void OnPostClosed()
        {
            controller.OnPostClosed();
            ((IBlogPostEditor)postPropertiesForm).OnPostClosed();
        }

        private void linkViewAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!postPropertiesForm.Visible)
                postPropertiesForm.Show(FindForm());
            else
            {
                if (postPropertiesForm.WindowState == FormWindowState.Minimized)
                    postPropertiesForm.WindowState = FormWindowState.Normal;
                postPropertiesForm.Activate();
            }
        }

        void IRtlAware.Layout()
        {
        }

        #region Implementation of INewCategoryContext

        public void NewCategoryAdded(BlogPostCategory category)
        {
            controller.NewCategoryAdded(category);
        }

        #endregion
    }
}
