// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    public interface ICategorySelector
    {
        void LoadCategories();
        void Filter(string criteria);
        void SelectCategory(BlogPostCategory category);
        void UpArrow();
        void DownArrow();
        void Enter();
        void CtrlEnter();
    }

    internal partial class CategoryDisplayFormW3M1 : MiniForm
    {
        private readonly CategoryContext ctx;
        private ICategorySelector selector;
        private bool selfDispose = false;
        private Point? anchor;

        public CategoryDisplayFormW3M1(CategoryContext ctx, Point? anchor)
        {
            this.ctx = ctx;
            this.anchor = anchor;

            InitializeComponent();

            if (ctx.MaxCategoryNameLength > 0)
                txtNewCategory.MaxLength = ctx.MaxCategoryNameLength;

            /* TODO: Whoops, missed UI Freeze... do this later
            txtFilter.AccessibleName = Res.Get(StringId.CategoryCategoryFilter);
            cbParent.AccessibleName = Res.Get(StringId.CategoryNewCategoryParent);
             */
            txtNewCategory.AccessibleName = Res.Get(StringId.CategoryCategoryName);
            btnRefresh.AccessibleName = Res.Get(StringId.CategoryRefreshList);

            grpAdd.Enter += delegate { AcceptButton = btnDoAdd; };
            grpAdd.Leave += delegate { AcceptButton = null; };

            grpAdd.Text = Res.Get(StringId.CategoryAdd);
            btnDoAdd.Text = Res.Get(StringId.AddButton2);
            toolTip.SetToolTip(btnRefresh, Res.Get(StringId.CategoryRefreshList));
            ControlHelper.SetCueBanner(txtNewCategory, Res.Get(StringId.CategoryCategoryName));
            lblNone.Text = Res.Get(StringId.CategoryControlNoCategories2);

            RefreshParentCombo();

            btnRefresh.Image = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.RefreshPostListEnabled.png");
            btnRefresh.ImageAlign = ContentAlignment.MiddleCenter;
            pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.Search.png");
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            DismissOnDeactivate = true;

            Control selectorControl;
            if (ctx.SelectionMode == CategoryContext.SelectionModes.SingleSelect)
                selectorControl = new RadioCategorySelector(ctx);
            else if (ctx.SelectionMode == CategoryContext.SelectionModes.MultiSelect)
                selectorControl = new TreeCategorySelector(ctx);
            else
                throw new ArgumentException("Unexpected selection mode: " + ctx.SelectionMode);

            lblNone.BringToFront();
            lblNone.Visible = ctx.Categories.Length == 0;

            selector = (ICategorySelector)selectorControl;
            AdaptAddCategories();
            ctx.Changed += ctx_Changed;
            Disposed += delegate { ctx.Changed -= ctx_Changed; };
            selectorControl.Dock = DockStyle.Fill;
            selectorContainer.Controls.Add(selectorControl);

            txtFilter.AccessibleName = Res.Get(StringId.FindCategory);
            cbParent.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.CategoryParentAccessible));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Tree control has weird first-time paint issues in RTL builds
            if (BidiHelper.IsRightToLeft)
                Invalidate(true);
        }

        private void AdaptAddCategories()
        {
            if (!ctx.SupportsAddingCategories)
            {
                int deltaY = ClientSize.Height - grpAdd.Top;
                grpAdd.Visible = false;
                using (LayoutHelper.SuspendAnchoring(selectorContainer, grpAdd))
                {
                    Height -= deltaY;
                    Top += deltaY;
                }
            }
            else
            {
                if (!ctx.SupportsHierarchicalCategories)
                {
                    txtNewCategory.Width = cbParent.Width;
                    int deltaY = cbParent.Top - txtNewCategory.Top;
                    btnDoAdd.Top -= deltaY;
                    cbParent.Visible = false;
                    using (LayoutHelper.SuspendAnchoring(txtNewCategory, cbParent, btnDoAdd, selectorContainer, grpAdd))
                    {
                        grpAdd.Height -= deltaY;
                        Height -= deltaY;
                        Top += deltaY;
                    }
                }
            }
        }

        private void ctx_Changed(object sender, CategoryContext.CategoryChangedEventArgs eventArgs)
        {
            switch (eventArgs.ChangeType)
            {
                case CategoryContext.ChangeType.Category:
                    lblNone.Visible = ctx.Categories.Length == 0;
                    selector.LoadCategories();

                    // Fix bug 611888: Funny grey box in category control when adding a category to an empty category list
                    // Yes, this does need to happen in a BeginInvoke--invalidating doesn't work
                    // properly until some other (unknown) message gets consumed
                    BeginInvoke(new System.Threading.ThreadStart(delegate { ((Control)selector).Invalidate(true); }));

                    break;
                case CategoryContext.ChangeType.SelectionMode:
                    Close();
                    break;
            }
        }

        private void RefreshParentCombo()
        {
            object selectedItem = cbParent.SelectedItem;
            cbParent.Items.Clear();
            cbParent.Items.Add(new ParentCategoryComboItemNone(cbParent));
            BlogPostCategoryListItem[] categoryListItems = BlogPostCategoryListItem.BuildList(ctx.BlogCategories, true);
            foreach (BlogPostCategoryListItem categoryListItem in categoryListItems)
                cbParent.Items.Add(new ParentCategoryComboItem(cbParent, categoryListItem.Category, categoryListItem.IndentLevel));
            if (selectedItem != null && cbParent.Items.Contains(selectedItem))
                cbParent.SelectedItem = selectedItem;
            else
                cbParent.SelectedIndex = 0;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int growth = (-btnDoAdd.Width) + DisplayHelper.AutoFitSystemButton(btnDoAdd, btnDoAdd.Width, int.MaxValue);
            if (cbParent.Visible)
                growth += (-cbParent.Width) + DisplayHelper.AutoFitSystemCombo(cbParent, cbParent.Width, int.MaxValue, true);

            Width += growth;
            MinimumSize = new Size(MinimumSize.Width + growth, MinimumSize.Height);
            if (cbParent.Visible)
                cbParent.Width -= growth;
            else
                txtNewCategory.Width -= growth;

            Size preferredSize = selectorContainer.Controls[0].GetPreferredSize(Size.Empty);
            // Fix bug 611894: Category control exhibits unexpected scrolling behavior with long category names
            if (preferredSize.Width > (selectorContainer.Width + (MaximumSize.Width - Width)))
                preferredSize.Height += SystemInformation.HorizontalScrollBarHeight;
            preferredSize.Width += SystemInformation.VerticalScrollBarWidth;

            int deltaY = preferredSize.Height - selectorContainer.Height;
            int deltaX = preferredSize.Width - selectorContainer.Width;

            Bounds = new Rectangle(
                Left - deltaX,
                Top - deltaY,
                Width + deltaX,
                Height + deltaY
                );

            txtFilter.Select();
        }

        /// <summary>
        /// Indicates whether this form should dispose itself after closing.
        /// </summary>
        public bool SelfDispose
        {
            get { return selfDispose; }
            set { selfDispose = value; }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (anchor != null)
            {
                if (RightToLeft == RightToLeft.Yes)
                    Location = (Point)anchor;
                else
                    Location = (Point)anchor - new Size(Width, 0);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            // I don't know why this is necessary, but when RightToLeft and RightToLeftLayout
            // are both true, the rightmost column or two of pixels get clipped unless we
            // explicitly reset the clip.
            e.Graphics.ResetClip();

            // draw border around form
            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            // draw border around filter textbox
            Rectangle filterRect = new Rectangle(
                selectorContainer.Left,
                txtFilter.Top - ScaleY(3),
                selectorContainer.Width - btnRefresh.Width - ScaleX(4),
                txtFilter.Height + ScaleY(5)
                );

            e.Graphics.FillRectangle(SystemBrushes.Window, filterRect);
            filterRect.Inflate(1, 1);
            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, filterRect);

            // draw border around category control
            Rectangle categoryRect = selectorContainer.Bounds;
            categoryRect.Inflate(1, 1);
            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, categoryRect);
        }

        int ScaleX(int x)
        {
            return (int)Math.Round(DisplayHelper.ScaleX(x));
        }

        int ScaleY(int y)
        {
            return (int)Math.Round(DisplayHelper.ScaleY(y));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (SelfDispose)
                Dispose();
        }

        private string lastFilterValue = "";
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            selector.Filter(txtFilter.Text.TrimStart(' ').ToLower(CultureInfo.CurrentCulture));
            if (txtNewCategory.Visible)
            {
                if (txtNewCategory.ForeColor != SystemColors.WindowText
                    || txtNewCategory.Text == ""
                    || txtNewCategory.Text == lastFilterValue)
                {
                    txtNewCategory.Text = txtFilter.Text;
                }
            }
            lastFilterValue = txtFilter.Text;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            if (!grpAdd.ContainsFocus)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        selector.DownArrow();
                        return true;
                    case Keys.Up:
                        selector.UpArrow();
                        return true;
                    case Keys.Enter:
                        selector.Enter();
                        return true;
                    case Keys.Enter | Keys.Control:
                        selector.CtrlEnter();
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                ctx.Refresh();
            }
        }

        private void btnDoAdd_Click(object sender, EventArgs e)
        {
            AddCategory();
        }

        private void AddCategory()
        {
            BlogPostCategory cat = CreateNewOrFetchExistingCategory();
            if (cat != null)
            {
                txtNewCategory.Text = "";
                cbParent.SelectedIndex = 0;

                txtFilter.Text = "";
                selector.SelectCategory(cat);
                txtNewCategory.Focus();
            }
        }

        private BlogPostCategory CreateNewOrFetchExistingCategory()
        {
            BlogPostCategory newCategory = GetNewCategory();
            if (newCategory == null)
                return null;
            foreach (BlogPostCategory existingCat in ctx.Categories)
            {
                if (BlogPostCategory.Equals(existingCat, newCategory, true))
                {
                    return existingCat;
                }
            }

            // create category
            ctx.AddNewCategory(newCategory);
            return newCategory;
        }

        private BlogPostCategory GetNewCategory()
        {
            if (txtNewCategory.ForeColor == SystemColors.WindowText)
            {
                string categoryName = txtNewCategory.Text.Trim();
                if (categoryName != String.Empty)
                {
                    // see if we have a parent
                    BlogPostCategory parentCategory = cbParent.Category;
                    if (parentCategory != null)
                        return new BlogPostCategory(categoryName, categoryName, parentCategory.Id);
                    else
                        return new BlogPostCategory(categoryName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        private class ParentCategoryComboBox : ComboBox
        {
            public ParentCategoryComboBox()
            {
                // prevent editing and showing of drop down
                DropDownStyle = ComboBoxStyle.DropDownList;

                // fully custom painting
                DrawMode = DrawMode.OwnerDrawFixed;
                IntegralHeight = false;
            }

            public BlogPostCategory Category
            {
                get
                {
                    if (!Visible)
                        return null;

                    ParentCategoryComboItem parentComboItem = SelectedItem as ParentCategoryComboItem;
                    if (parentComboItem != null)
                    {
                        if (!BlogPostCategoryNone.IsCategoryNone(parentComboItem.Category))
                            return parentComboItem.Category;
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            protected override void OnDropDown(EventArgs e)
            {
                DisplayHelper.AutoFitSystemComboDropDown(this);
                base.OnDropDown(e);
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index != -1)
                {
                    ParentCategoryComboItem comboItem = Items[e.Index] as ParentCategoryComboItem;

                    e.DrawBackground();

                    // don't indent for main display of category
                    string text = comboItem != null ? comboItem.ToString() : "";
                    if (e.Bounds.Width < Width)
                        text = text.Trim();

                    Color textColor = ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                                          ? SystemColors.HighlightText
                                          : comboItem.TextColor;

                    BidiGraphics bg = new BidiGraphics(e.Graphics, e.Bounds);
                    bg.DrawText(text, e.Font, e.Bounds, textColor, TextFormatFlags.NoPrefix);

                    e.DrawFocusRectangle();
                }
            }
        }

        private class ParentCategoryComboItem
        {
            public ParentCategoryComboItem(ComboBox parentCombo, BlogPostCategory category, int indentLevel)
            {
                _parentCombo = parentCombo;
                _category = category;
                _indentLevel = indentLevel;
            }

            public BlogPostCategory Category
            {
                get { return _category; }
            }
            private BlogPostCategory _category;

            public virtual Color TextColor { get { return _parentCombo.ForeColor; } }

            public override bool Equals(object obj)
            {
                ParentCategoryComboItem item = obj as ParentCategoryComboItem;
                if (item == null)
                    return false;
                return item.Category.Equals(Category);
            }

            public override int GetHashCode()
            {
                return Category.GetHashCode();
            }

            public override string ToString()
            {
                // default padding
                string padding = new String(' ', _indentLevel * 3);

                // override if we are selected
                if (!_parentCombo.DroppedDown && (_parentCombo.SelectedItem != null) && this.Equals(_parentCombo.SelectedItem))
                    padding = String.Empty;

                string categoryName = HtmlUtils.UnEscapeEntities(Category.Name, HtmlUtils.UnEscapeMode.Default);
                string stringRepresentation = padding + categoryName;
                return stringRepresentation;
            }

            private int _indentLevel;
            private ComboBox _parentCombo;

        }

        private class ParentCategoryComboItemNone : ParentCategoryComboItem
        {
            public ParentCategoryComboItemNone(ComboBox parentCombo)
                : base(parentCombo, new BlogPostCategoryNone(), 0)
            {
            }

            public override Color TextColor { get { return SystemColors.GrayText; } }

            public override string ToString()
            {
                return Res.Get(StringId.CategoryNoParent);
            }
        }

    }
}
