// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{

    internal class CategoryDropDownControlM1 : ComboBox, IBlogPostEditor, IBlogCategorySettings, INewCategoryContext
    {
        #region IBlogPostEditor Members

        void IBlogPostEditor.Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions clientOptions)
        {
            // categories from blog
            CategoryContext.SetBlogCategories(_targetBlog.Categories);

            // pickup new categories from the blog-post
            CategoryContext.SetNewCategories(editingContext.BlogPost.NewCategories);

            // combine all post categories before settting
            ArrayList selectedCategories = new ArrayList();
            selectedCategories.AddRange(editingContext.BlogPost.NewCategories);
            selectedCategories.AddRange(editingContext.BlogPost.Categories);
            _lastSelectedCategories = new BlogPostCategory[0];
            CategoryContext.SelectedCategories = selectedCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];

            toolTipCategories.SetToolTip(this, CategoryContext.FormattedCategoryList);
            _isDirty = false;
        }

        void IBlogPostEditor.SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            post.Categories = CategoryContext.SelectedExistingCategories;
            post.NewCategories = CategoryContext.SelectedNewCategories;
            _isDirty = false;
        }

        private ToolTip2 toolTipCategories;

        bool IBlogPostEditor.ValidatePublish()
        {
            // see if we need to halt publishing to specify a category
            if (Visible)
            {
                if (PostEditorSettings.CategoryReminder && (CategoryContext.Categories.Length > 0) && (CategoryContext.SelectedCategories.Length == 0))
                {
                    if (DisplayMessage.Show(MessageId.CategoryReminder, FindForm()) == DialogResult.No)
                    {
                        Focus();
                        return false;
                    }
                }
            }

            // proceed
            return true;
        }

        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            _isDirty = false;
        }

        void INewCategoryContext.NewCategoryAdded(BlogPostCategory category)
        {
            _categoryContext.CommitNewCategory(category);
            _isDirty = false;
        }

        bool IBlogPostEditor.IsDirty
        {
            get
            {
                return _isDirty;
            }
        }
        private bool _isDirty = false;

        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
            // save dirty state
            bool isDirty = _isDirty;

            _targetBlog = newBlog;

            AdaptToBlogOptions();

            // a new blog always wipes out "New" categories
            CategoryContext.SetNewCategories(new BlogPostCategory[0]);

            _lastSelectedCategories = new BlogPostCategory[0];
            CategoryContext.SelectedCategories = new BlogPostCategory[0];

            // restore dirty state
            _isDirty = isDirty;
        }
        private Blog _targetBlog;

        void IBlogPostEditor.OnBlogSettingsChanged(bool templateChanged)
        {
            AdaptToBlogOptions();
        }

        private void AdaptToBlogOptions()
        {
            if (_targetBlog.ClientOptions.SupportsMultipleCategories)
                CategoryContext.SelectionMode = CategoryContext.SelectionModes.MultiSelect;
            else
                CategoryContext.SelectionMode = CategoryContext.SelectionModes.SingleSelect;

            CategoryContext.SupportsAddingCategories = _targetBlog.ClientOptions.SupportsNewCategories;
            CategoryContext.SupportsHierarchicalCategories = _targetBlog.ClientOptions.SupportsHierarchicalCategories;
            CategoryContext.MaxCategoryNameLength = _targetBlog.ClientOptions.MaxCategoryNameLength;

            // categories from blog
            CategoryContext.SetBlogCategories(_targetBlog.Categories);
        }

        public void OnClosing(CancelEventArgs e)
        {

        }

        public void OnPostClosing(CancelEventArgs e)
        {

        }

        #endregion

        public CategoryDropDownControlM1() : base()
        {
            if (DesignMode)
                _categoryContext = new CategoryContext();

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // prevent editing and showing of drop down
            DropDownStyle = ComboBoxStyle.DropDownList;

            // fully cusotm painting
            IntegralHeight = false;
            DrawMode = DrawMode.OwnerDrawFixed;
            Items.Add(String.Empty);
        }

        public void Initialize(IWin32Window parentFrame, CategoryContext categoryContext)
        {
            _categoryContext = categoryContext ?? new CategoryContext();
            _parentFrame = parentFrame;
            _categoryContext.BlogCategorySettings = this;
            _categoryContext.Changed += new CategoryContext.CategoryChangedEventHandler(_categoryContext_Changed);
        }

        // replace standard drop down behavior with category form
        protected override void WndProc(ref Message m)
        {
            if (IsDropDownMessage(m))
            {
                if (_categoryDisplayForm == null)
                    DisplayCategoryForm();
                else
                    Focus();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        // replace standard painting behavior
        protected override void OnDrawItem(DrawItemEventArgs evt)
        {
            BidiGraphics g = new BidiGraphics(evt.Graphics, ClientRectangle);

            // determine the text color based on whether we have categories
            bool hasCategories = _categoryContext.SelectedCategories.Length > 0;
            Color textColor = hasCategories ? SystemColors.ControlText : SystemColors.GrayText; //Color.FromArgb(200, SystemColors.ControlText) ;

            // margins
            const int HORIZONTAL_MARGIN = 1;

            // draw the icon (if we have one)
            const int ICON_MARGIN = 4;
            int textMargin = 0;
            if (_icon != null)
            {
                g.DrawImage(true, _icon, new Rectangle(ScaleX(ICON_MARGIN), ScaleY(2), ScaleX(_icon.Width), ScaleY(_icon.Height)));
                textMargin += ScaleX(ICON_MARGIN + _icon.Width);
            }

            // draw the text
            int leftMargin = textMargin;
            int rightMargin = ScaleX(HORIZONTAL_MARGIN);
            Rectangle textRegion = new Rectangle(new Point(leftMargin, -1), new Size(Width - leftMargin - rightMargin, Height));

            TextFormatFlags tempFormat = DisplayFormat;
            Rectangle tempRectangle = textRegion;

            g.DrawText(
                _categoryContext.Text,
                Font, tempRectangle, textColor, tempFormat);

            // draw focus rectangle (only if necessary)
            evt.DrawFocusRectangle();
        }

        BlogPostCategory[] IBlogCategorySettings.RefreshCategories(bool ignoreErrors)
        {
            try
            {
                _targetBlog.RefreshCategories();
            }
            catch (BlogClientOperationCancelledException)
            {
                // show no UI for operation cancelled
                Debug.WriteLine("BlogClient operation cancelled");
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                    DisplayableExceptionDisplayForm.Show(_parentFrame, ex);
            }
            return _targetBlog.Categories;
        }

        void IBlogCategorySettings.UpdateCategories(BlogPostCategory[] categories)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(_targetBlog.Id))
                blogSettings.Categories = categories;
        }

        private CategoryContext CategoryContext
        {
            get
            {
                return _categoryContext;
            }
        }

        public void DisplayCategoryForm()
        {
            Focus();

            /*
            _categoryDisplayForm = new CategoryDisplayFormM1(this, _categoryContext) ;
            _categoryDisplayForm.MinDropDownWidth = 0;
            IMiniFormOwner miniFormOwner = FindForm() as IMiniFormOwner;
            if (miniFormOwner != null)
                _categoryDisplayForm.FloatAboveOwner(miniFormOwner);
            _categoryDisplayForm.Closed += new EventHandler(_categoryDisplayForm_Closed);
            using ( new WaitCursor() )
                _categoryDisplayForm.Show();
            */

            Point anchor = PointToScreen(new Point(RightToLeft == RightToLeft.Yes ? 0 : ClientSize.Width, ClientSize.Height));
            _categoryDisplayForm = new CategoryDisplayFormW3M1(_categoryContext, anchor);
            _categoryDisplayForm.SelfDispose = true;
            IMiniFormOwner miniFormOwner = FindForm() as IMiniFormOwner;
            if (miniFormOwner != null)
                _categoryDisplayForm.FloatAboveOwner(miniFormOwner);
            _categoryDisplayForm.Closed += _categoryDisplayForm_Closed;
            using (new WaitCursor())
                _categoryDisplayForm.Show();
        }
        private CategoryDisplayFormW3M1 _categoryDisplayForm = null;

        private void _categoryContext_Changed(object sender, CategoryContext.CategoryChangedEventArgs e)
        {
            if (!CategoryListsAreEqual(_lastSelectedCategories, CategoryContext.SelectedCategories))
                _isDirty = true;

            // always record last selected categories
            _lastSelectedCategories = CategoryContext.SelectedCategories;

            toolTipCategories.SetToolTip(this, CategoryContext.FormattedCategoryList);
            Invalidate();
            Update();
        }
        private BlogPostCategory[] _lastSelectedCategories = new BlogPostCategory[0];

        private bool CategoryListsAreEqual(BlogPostCategory[] categories1, BlogPostCategory[] categories2)
        {
            if (categories1.Length == categories2.Length)
            {
                for (int i = 0; i < categories1.Length; i++)
                {
                    if (!categories1[i].Equals(categories2[i]))
                        return false;
                }

                // if we got this far they are equal
                return true;
            }
            else
            {
                return false;
            }
        }

        private void _categoryDisplayForm_Closed(object sender, EventArgs e)
        {
            _categoryDisplayForm = null;
            Invalidate();
        }

        private TextFormatFlags DisplayFormat
        {
            get { return TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix; }
        }

        private bool IsDropDownMessage(Message m)
        {
            // Left Mouse Button while form is not displaying
            if (m.Msg == WM.LBUTTONDOWN || m.Msg == WM.LBUTTONDBLCLK)
            {
                return true;
            }

            // F4
            else if (m.Msg == WM.KEYDOWN)
            {
                Keys keyCombo = (Keys)(int)m.WParam & Keys.KeyCode;

                if (keyCombo == Keys.F4)
                    return true;
                else
                    return false;
            }

            // Alt+Arrow
            else if (m.Msg == WM.SYSKEYDOWN)
            {
                int wparam = m.WParam.ToInt32();
                int lparam = m.LParam.ToInt32();

                if ((wparam == 0x28 && (lparam == 0x21500001 || lparam == 0x20500001)) ||    //Alt+Down, Alt+NumDown
                    (wparam == 0x26 && (lparam == 0x21480001 || lparam == 0x20480001)))      //Alt+Up, Alt+NumUp
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Not a drop-down message
            else
            {
                return false;
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
            this.components = new System.ComponentModel.Container();
            this.toolTipCategories = new ToolTip2(this.components);
            //
            // CategoryDropDownControlM1
            //
            this.Size = new System.Drawing.Size(138, 48);

        }
        #endregion

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScaleState(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }
        protected override void ScaleCore(float dx, float dy)
        {
            SaveScaleState(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScaleState(float dx, float dy)
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

        private IWin32Window _parentFrame;
        private System.ComponentModel.IContainer components;
        private CategoryContext _categoryContext;
        private Bitmap _icon = null;

        public void OnClosed() { }
        public void OnPostClosed() { }
    }
}
