// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    /// <summary>
    /// Summary description for CategoryDisplayForm.
    /// </summary>
    internal class CategoryDisplayFormM1 : MiniForm
    {
        public CategoryDisplayFormM1(Control parentControl, CategoryContext categoryContext)
        {
            SuppressAutoRtlFixup = true;
            // save references and signup for changed event
            _parentControl = parentControl;
            _categoryContext = categoryContext;
            _categoryContext.Changed += new CategoryContext.CategoryChangedEventHandler(_categoryContext_Changed);

            // standard designer stuff
            InitializeComponent();

            this.buttonAdd.Text = Res.Get(StringId.AddButton2);

            // mini form behavior
            DismissOnDeactivate = true;

            // dynamic background colors based on theme
            BackColor = ColorizedResources.Instance.FrameGradientLight ;
            categoryRefreshControl.BackColor = ColorizedResources.Instance.FrameGradientLight ;
            categoryContainerControl.BackColor = SystemColors.Window ;

            // dynamic layout depending upon whether we support adding categories
            if ( categoryContext.SupportsAddingCategories )
            {
                textBoxAddCategory.MaxLength = categoryContext.MaxCategoryNameLength ;
            }
            else
            {
                int gap = categoryContainerControl.Top - panelAddCategory.Top ;
                panelAddCategory.Visible = false ;
                categoryContainerControl.Top = panelAddCategory.Top + 2 ;
                categoryContainerControl.Height += gap ;
            }

            // dynamic layout depending upon whether we support heirarchical categories
            if ( !categoryContext.SupportsHierarchicalCategories )
            {
                textBoxAddCategory.Width += (comboBoxParent.Right - textBoxAddCategory.Right);
                comboBoxParent.Visible = false ;
            }

            // initialize add textbox
            ClearAddTextBox() ;

            // initialize category control
            categoryRefreshControl.Initialize(_categoryContext);
        }

        public int MaxDropDownWidth
        {
            get { return _maxDropDownWidth; }
            set { _maxDropDownWidth = value; }
        }

        public int MinDropDownWidth
        {
            get { return _minDropDownWidth; }
            set { _minDropDownWidth = value; }
        }

        private int _maxDropDownWidth = 260;
        private int _minDropDownWidth = 104;


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RefreshParentCombo();


            if (panelAddCategory.Visible)
            {
                if (comboBoxParent.Visible)
                {
                    DisplayHelper.AutoFitSystemButton(buttonAdd);
                    using (new AutoGrow(this, AnchorStyles.Bottom | AnchorStyles.Right, false))
                    {
                        textBoxAddCategory.Width = buttonAdd.Right - textBoxAddCategory.Left;

                        DisplayHelper.AutoFitSystemCombo(comboBoxParent, 0, int.MaxValue, true);
                        comboBoxParent.Left = textBoxAddCategory.Left;
                        comboBoxParent.Top = textBoxAddCategory.Bottom + ScaleY(3);
                        buttonAdd.Top = comboBoxParent.Top + (comboBoxParent.Height - buttonAdd.Height) / 2;
                        buttonAdd.Left = comboBoxParent.Right + ScaleX(3);

                        if (buttonAdd.Right > textBoxAddCategory.Right)
                        {
                            int deltaX = buttonAdd.Right - textBoxAddCategory.Right;
                            textBoxAddCategory.Width += deltaX;
                            using (LayoutHelper.SuspendAnchoring(textBoxAddCategory, buttonAdd, comboBoxParent))
                                panelAddCategory.Width += deltaX;
                            categoryContainerControl.Width += deltaX;
                        }
                        else
                        {
                            buttonAdd.Left = textBoxAddCategory.Right - buttonAdd.Width;
                            comboBoxParent.Width = buttonAdd.Left - comboBoxParent.Left - ScaleX(3);
                        }

                        int deltaY = comboBoxParent.Bottom - textBoxAddCategory.Bottom;
                        panelAddCategory.Height += deltaY;
                        categoryContainerControl.Top += deltaY;
                        categoryRefreshControl.Top += deltaY;
                    }
                }
                else
                {
                    using (new AutoGrow(this, AnchorStyles.Right, false))
                    {
                        int deltaX = -buttonAdd.Width + DisplayHelper.AutoFitSystemButton(buttonAdd);
                        textBoxAddCategory.Width -= deltaX;

                        int desiredWidth = DisplayHelper.MeasureString(textBoxAddCategory, textBoxAddCategory.Text).Width + (int)DisplayHelper.ScaleX(10);
                        deltaX += desiredWidth - textBoxAddCategory.Width;
                        if (deltaX > 0)
                        {
                            panelAddCategory.Width += deltaX;
                            categoryContainerControl.Width += deltaX;
//							textBoxAddCategory.Width += deltaX;
//							buttonAdd.Left = textBoxAddCategory.Right + ScaleX(3);
//							textBoxAddCategory.Width = Math.Max(desiredWidth, textBoxAddCategory.Width);
//							textBoxAddCategory.Width = buttonAdd.Left - textBoxAddCategory.Left - ScaleX(3);
                        }
                    }
                }

                /*using (LayoutHelper.SuspendAnchoring(comboBoxParent, buttonAdd, textBoxAddCategory, panelAddCategory, categoryContainerControl))
                {
                    if (comboBoxParent.Visible)
                    {
                        deltaW = -comboBoxParent.Width + DisplayHelper.AutoFitSystemCombo(comboBoxParent, 0, int.MaxValue, true);
                        buttonAdd.Left += deltaW;
                    }

                    int oldWidth = buttonAdd.Width;
                    DisplayHelper.AutoFitSystemButton(buttonAdd);
                    buttonAdd.Width += (int)DisplayHelper.ScaleX(6); // add some more "air" in the Add button
                    deltaW += buttonAdd.Width - oldWidth;

                    panelAddCategory.Width += deltaW;
                    categoryContainerControl.Width += deltaW;
                    Width += deltaW;
                }*/
            }

            // use design time defaults to drive dynamic layout
            _topMargin = categoryContainerControl.Top ;
            _bottomMargin = Bottom - categoryContainerControl.Bottom ;

            using (new AutoGrow(this, AnchorStyles.Right, false))
                LayoutControls(_categoryContext, true, false);
            UpdateSelectedCategories(_categoryContext.SelectedCategories);

            BidiHelper.RtlLayoutFixup(this);
        }


        private System.Windows.Forms.Panel panelAddCategory;
        private System.Windows.Forms.TextBox textBoxAddCategory;
        private System.Windows.Forms.Button buttonAdd;
        private OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryRefreshControl categoryRefreshControl;
        private ParentCategoryComboBox comboBoxParent;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                // Borderless windows show in the alt+tab window, so this fakes
                // out windows into thinking its a tool window (which doesn't
                // show up in the alt+tab window).
                createParams.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW

                return createParams;
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ( keyData == Keys.Enter )
            {
                if ( textBoxAddCategory.ContainsFocus )
                {
                    AddCategory() ;
                    return true ;
                }
                else if ( categoryContainerControl.ContainsFocus )
                {
                    Close() ;
                    return true ;
                }
                else
                {
                    return false ;
                }
            }

            else if (keyData == Keys.Escape )
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            // I don't know why this is necessary, but when RightToLeft and RightToLeftLayout
            // are both true, the rightmost column or two of pixels get clipped unless we
            // explicitly reset the clip.
            e.Graphics.ResetClip();

            // draw border around form
            using ( Pen pen = new Pen(ColorizedResources.Instance.BorderDarkColor) )
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            // draw border around category control
            Color controlBorderColor = ColorHelper.GetThemeBorderColor(ColorizedResources.Instance.BorderDarkColor) ;
            using ( Pen pen = new Pen( controlBorderColor, 1 ))
            {
                Rectangle categoryRect = categoryContainerControl.Bounds ;
                categoryRect.Inflate(1,1);
                e.Graphics.DrawRectangle(pen, categoryRect);
            }
        }


        private int _topMargin ;
        private int _bottomMargin  ;
        private Panel categoryContainerControl;

        private void LayoutControls(CategoryContext categoryContext, bool sizeForm, bool performRtlLayoutFixup)
        {
            ClearForm();
            SuspendLayout();

            // calculate how much room there is above me on the parent form
            int parentControlY = _parentControl.PointToScreen(_parentControl.Location).Y ;
            Form parentForm = _parentControl.FindForm() ;
            int parentFormY = parentForm.PointToScreen(parentForm.ClientRectangle.Location).Y ;
            int maxHeight = parentControlY - parentFormY - ScaleY(_topMargin) - ScaleY(_bottomMargin);

            // enforce additional constraint (or not, let freedom reign!)
            //maxHeight = Math.Min(maxHeight, 400) ;

            using (PositionManager positionManager = new PositionManager(ScaleX(X_MARGIN), ScaleY(Y_MARGIN + 2), ScaleY(4), ScaleX(MinDropDownWidth), categoryContainerControl.Width, maxHeight, scale))
            {
                // add 'none' if we're single selecting categories
                if (categoryContext.SelectionMode != CategoryContext.SelectionModes.MultiSelect)
                    AddCategoryControl(new BlogPostCategoryListItem(new BlogPostCategoryNone(), 0), positionManager);

                // add the other categories
                BlogPostCategoryListItem[] categoryListItems = BlogPostCategoryListItem.BuildList(categoryContext.Categories, true) ;
                foreach(BlogPostCategoryListItem categoryListItem in categoryListItems)
                    AddCategoryControl(categoryListItem, positionManager) ;

                if ( sizeForm )
                    PositionAndSizeForm(positionManager);
            }
            if (performRtlLayoutFixup)
                BidiHelper.RtlLayoutFixup(categoryContainerControl);
            ResumeLayout();
        }

        private BlogPostCategory[] GetCategories(CategoryContext context)
        {
            ArrayList categories = new ArrayList(context.Categories);
            categories.Sort();


            return (BlogPostCategory[]) categories.ToArray(typeof (BlogPostCategory));
        }

        private void ClearForm()
        {
            if (_categoryControls.Count > 0)
            {
                categoryContainerControl.Controls.Clear();
                _categoryControls.Clear();
            }
        }

        private void AddCategoryControl(BlogPostCategoryListItem categoryListItem, PositionManager positionManager)
        {
            ICategorySelectorControl catSelector =
                CategorySelectorControlFactory.Instance.GetControl(categoryListItem.Category,
                                                                   (_categoryContext.SelectionMode ==
                                                                    CategoryContext.SelectionModes.MultiSelect));
            catSelector.SelectedChanged += new EventHandler(catSelector_SelectedChanged);

            catSelector.Control.Scale(new SizeF(scale.X, scale.Y));

            positionManager.PositionControl(catSelector.Control, categoryListItem.IndentLevel);

            categoryContainerControl.Controls.Add(catSelector.Control);

            _categoryControls.Add(catSelector.Control);
        }

        private void PositionAndSizeForm(PositionManager manager)
        {
            // determine height based on position manager calculations
            categoryContainerControl.Height = manager.Height;
            Height = manager.Height + ScaleY(_topMargin) + ScaleY(_bottomMargin) ;

            Point bottomRightCorner =
                _parentControl.PointToScreen(new Point(_parentControl.Width - Width, -Height));
            Location = bottomRightCorner;
        }

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
            scale = new PointF(scale.X*dx, scale.Y*dy);

            //synchronize the comboBoxParent item height so it matches the scaled control height.
            //Note: ScaleY(3) is padding to deal with mismatches in the way the ItemHeight scales versus its peer controls...
            //comboBoxParent.ItemHeight = (int) (comboBoxParent.ItemHeight*dy) + ScaleY(3);
            comboBoxParent.ItemHeight = (int) (comboBoxParent.ItemHeight*dy) + (scale.Y == 1f ? 0 : ScaleY(3));
        }

        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int) (x*scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int) (y*scale.Y);
        }
        #endregion

        private const int Y_MARGIN = 2;
        private const int X_MARGIN = 4;

        private void CommitSelection()
        {
            ArrayList selectedCategories = new ArrayList();
            foreach (ICategorySelectorControl categoryControl in _categoryControls)
            {
                if (categoryControl.Selected)
                {
                    if (BlogPostCategoryNone.IsCategoryNone(categoryControl.Category))
                    {
                        selectedCategories.Clear();
                        break;
                    }
                    else
                    {
                        selectedCategories.Add(categoryControl.Category);
                    }
                }
            }
            _categoryContext.SelectedCategories = (BlogPostCategory[]) selectedCategories.ToArray(typeof (BlogPostCategory));
        }

        private BlogPostCategory[] GetCurrentlySelectedCategories()
        {
            ArrayList selectedCategories = new ArrayList();
            foreach (ICategorySelectorControl sc in _categoryControls)
            {
                if (sc.Selected)
                {
                    if (BlogPostCategoryNone.IsCategoryNone(sc.Category))
                    {
                        selectedCategories.Clear();
                        break;
                    }
                    else
                    {
                        selectedCategories.Add(sc.Category);
                    }
                }
            }
            return (BlogPostCategory[]) selectedCategories.ToArray(typeof (BlogPostCategory));
        }

        private void UpdateSelectedCategories(BlogPostCategory[] selectedCategories)
        {
            bool focused = false;
            foreach (ICategorySelectorControl sc in _categoryControls)
            {
                if (!focused)
                {
                    sc.Control.Focus();
                    focused = true;
                }
                sc.Selected = false;
            }

            if (selectedCategories.Length > 0)
            {
                foreach (BlogPostCategory c in selectedCategories)
                    foreach (ICategorySelectorControl sc in _categoryControls)
                    {
                        if (c.Id.ToLower(CultureInfo.CurrentCulture) == sc.Category.Id.ToLower(CultureInfo.CurrentCulture) ||
                            c.Name.ToLower(CultureInfo.CurrentCulture) == sc.Category.Name.ToLower(CultureInfo.CurrentCulture) )
                        {
                            sc.Selected = true;
                            break;
                        }
                    }
            }
            else
            {
                // scan for special "None" category and select it
                foreach (ICategorySelectorControl sc in _categoryControls)
                {
                    if (BlogPostCategoryNone.IsCategoryNone((sc.Category)))
                    {
                        sc.Selected = true;
                        break;
                    }
                }
            }
        }

        private void textBoxAddCategory_Enter(object sender, System.EventArgs e)
        {
            if ( textBoxAddCategory.Text == Res.Get(StringId.CategoryAdd) )
                ActivateAddTextBoxForEdit()	 ;
        }

        private void textBoxAddCategory_Leave(object sender, System.EventArgs e)
        {
            if ( textBoxAddCategory.Text.Trim() == String.Empty )
                ClearAddTextBox() ;
        }

        private void ClearAddTextBox()
        {
            textBoxAddCategory.ForeColor = SystemColors.GrayText ;
            textBoxAddCategory.Text = Res.Get(StringId.CategoryAdd);
        }

        private void ActivateAddTextBoxForEdit()
        {
            textBoxAddCategory.ForeColor = SystemColors.ControlText;
            textBoxAddCategory.Text = String.Empty ;
        }

        private void buttonAdd_Click(object sender, System.EventArgs e)
        {
            AddCategory() ;
        }

        private void AddCategory()
        {
            // add the new category
            BlogPostCategory newCategory = GetNewCategory() ;

            // see if the "new" category is already in our list
            if ( newCategory != null )
            {
                ICategorySelectorControl selectorControl = GetCategorySelectorControl(newCategory) ;

                // if there is no existing selector control then add the category
                if ( selectorControl == null )
                    _categoryContext.AddNewCategory(newCategory);

                // now re-lookup the selector control and select it
                selectorControl = GetCategorySelectorControl(newCategory) ;
                selectorControl.Selected = true ;

                // ensure the added category is scrolled into view
                categoryContainerControl.ScrollControlIntoView(selectorControl.Control);

                // clear the parent combo and update its ui
                comboBoxParent.SelectedIndex = 0 ;
                UpdateParentComboForeColor() ;

                // setup the text box to add another category if we support multiple categories
                if ( _categoryContext.SelectionMode == CategoryContext.SelectionModes.MultiSelect )
                {
                    ActivateAddTextBoxForEdit() ;
                    textBoxAddCategory.Focus() ;
                }
                else
                {
                    ClearAddTextBox() ;
                    selectorControl.Control.Focus() ;
                }
            }
        }

        private BlogPostCategory GetNewCategory()
        {
            if ( textBoxAddCategory.ForeColor == SystemColors.ControlText )
            {
                string categoryName = this.textBoxAddCategory.Text.Trim() ;
                if ( categoryName != String.Empty )
                {
                    // see if we have a parent
                    BlogPostCategory parentCategory = GetParentCategory() ;
                    if ( parentCategory != null )
                        return new BlogPostCategory(categoryName, categoryName, parentCategory.Id);
                    else
                        return new BlogPostCategory(categoryName);
                }
                else
                {
                    return null ;
                }
            }
            else
            {
                return null ;
            }
        }

        private ICategorySelectorControl GetCategorySelectorControl(BlogPostCategory category)
        {
            // scan for control
            foreach (ICategorySelectorControl selectorControl in _categoryControls)
            {
                if ( selectorControl.Category.Id.ToLower(CultureInfo.CurrentCulture) == category.Id.ToLower(CultureInfo.CurrentCulture) ||
                     selectorControl.Category.Name.ToLower(CultureInfo.CurrentCulture) == category.Name.ToLower(CultureInfo.CurrentCulture) )
                {
                    return selectorControl ;
                }
            }

            // didn't find it
            return null ;

        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.categoryContainerControl = new System.Windows.Forms.Panel();
            this.panelAddCategory = new System.Windows.Forms.Panel();
            this.comboBoxParent = new OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryDisplayFormM1.ParentCategoryComboBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.textBoxAddCategory = new System.Windows.Forms.TextBox();
            this.categoryRefreshControl = new OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryRefreshControl();
            this.panelAddCategory.SuspendLayout();
            this.SuspendLayout();
            //
            // categoryContainerControl
            //
            this.categoryContainerControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryContainerControl.AutoScroll = true;
            this.categoryContainerControl.Location = new System.Drawing.Point(7, 33);
            this.categoryContainerControl.Name = "categoryContainerControl";
            this.categoryContainerControl.Size = new System.Drawing.Size(189, 72);
            this.categoryContainerControl.TabIndex = 0;
            //
            // panelAddCategory
            //
            this.panelAddCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAddCategory.Controls.Add(this.comboBoxParent);
            this.panelAddCategory.Controls.Add(this.buttonAdd);
            this.panelAddCategory.Controls.Add(this.textBoxAddCategory);
            this.panelAddCategory.Location = new System.Drawing.Point(6, 5);
            this.panelAddCategory.Name = "panelAddCategory";
            this.panelAddCategory.Size = new System.Drawing.Size(194, 25);
            this.panelAddCategory.TabIndex = 1;
            //
            // comboBoxParent
            //
            this.comboBoxParent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxParent.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParent.DropDownWidth = 200;
            this.comboBoxParent.IntegralHeight = false;
            this.comboBoxParent.ItemHeight = 15;
            this.comboBoxParent.Location = new System.Drawing.Point(42, 1);
            this.comboBoxParent.MaxDropDownItems = 20;
            this.comboBoxParent.Name = "comboBoxParent";
            this.comboBoxParent.Size = new System.Drawing.Size(88, 21);
            this.comboBoxParent.TabIndex = 1;
            this.comboBoxParent.Leave += new System.EventHandler(this.comboBoxParent_Leave);
            this.comboBoxParent.Enter += new System.EventHandler(this.comboBoxParent_Enter);
            //
            // buttonAdd
            //
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(134, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(59, 23);
            this.buttonAdd.TabIndex = 2;
            this.buttonAdd.Text = "&Add";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // textBoxAddCategory
            //
            this.textBoxAddCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAddCategory.AutoSize = false;
            this.textBoxAddCategory.Location = new System.Drawing.Point(0, 1);
            this.textBoxAddCategory.Name = "textBoxAddCategory";
            this.textBoxAddCategory.Size = new System.Drawing.Size(38, 21);
            this.textBoxAddCategory.TabIndex = 0;
            this.textBoxAddCategory.Text = "";
            this.textBoxAddCategory.Leave += new System.EventHandler(this.textBoxAddCategory_Leave);
            this.textBoxAddCategory.Enter += new System.EventHandler(this.textBoxAddCategory_Enter);
            //
            // categoryRefreshControl
            //
            this.categoryRefreshControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryRefreshControl.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.categoryRefreshControl.Location = new System.Drawing.Point(6, 111);
            this.categoryRefreshControl.Name = "categoryRefreshControl";
            this.categoryRefreshControl.Size = new System.Drawing.Size(193, 23);
            this.categoryRefreshControl.TabIndex = 2;
            //
            // CategoryDisplayFormM1
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.AutoScrollMargin = new System.Drawing.Size(2, 2);
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(205, 138);
            this.ControlBox = false;
            this.Controls.Add(this.categoryRefreshControl);
            this.Controls.Add(this.panelAddCategory);
            this.Controls.Add(this.categoryContainerControl);
            this.DismissOnDeactivate = true;
            this.DockPadding.All = 1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CategoryDisplayFormM1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.panelAddCategory.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _categoryContext.Changed -= new CategoryContext.CategoryChangedEventHandler(_categoryContext_Changed);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private System.ComponentModel.IContainer components = new Container();
        private ArrayList _categoryControls = new ArrayList();
        private CategoryContext _categoryContext;
        private Control _parentControl;

        private void catSelector_SelectedChanged(object sender, EventArgs e)
        {
            CommitSelection();
        }

        private void _categoryContext_Changed(object sender, CategoryContext.CategoryChangedEventArgs e)
        {
            if (e.ChangeType == CategoryContext.ChangeType.Category || e.ChangeType == CategoryContext.ChangeType.SelectionMode)
            {
                BlogPostCategory[] selectedCategories = GetCurrentlySelectedCategories();
                LayoutControls(_categoryContext, false, true);
                UpdateSelectedCategories(selectedCategories);
                RefreshParentCombo() ;
                Invalidate() ;
            }
        }

        private void RefreshParentCombo()
        {
            // populate "parent" combo
            object selectedItem = comboBoxParent.SelectedItem ;
            comboBoxParent.Items.Clear();
            comboBoxParent.Items.Add(new ParentCategoryComboItemNone(comboBoxParent)) ;
            BlogPostCategoryListItem[] categoryListItems = BlogPostCategoryListItem.BuildList(_categoryContext.BlogCategories, true) ;
            foreach(BlogPostCategoryListItem categoryListItem in categoryListItems)
                comboBoxParent.Items.Add(new ParentCategoryComboItem(comboBoxParent, categoryListItem.Category, categoryListItem.IndentLevel)) ;
            UpdateParentComboForeColor() ;
            if ( selectedItem != null && comboBoxParent.Items.Contains(selectedItem))
                comboBoxParent.SelectedItem = selectedItem ;
            else
                comboBoxParent.SelectedIndex = 0 ;
        }

        private BlogPostCategory GetParentCategory()
        {
            ParentCategoryComboItem parentComboItem = comboBoxParent.SelectedItem as ParentCategoryComboItem ;
            if ( parentComboItem != null )
            {
                if ( !BlogPostCategoryNone.IsCategoryNone(parentComboItem.Category) )
                    return parentComboItem.Category ;
                else
                    return null ;
            }
            else
            {
                return null ;
            }
        }

        private void comboBoxParent_Enter(object sender, System.EventArgs e)
        {
            comboBoxParent.ForeColor = SystemColors.ControlText;
        }

        private void comboBoxParent_Leave(object sender, System.EventArgs e)
        {
            UpdateParentComboForeColor() ;
        }

        private void UpdateParentComboForeColor()
        {
            if ( GetParentCategory() != null )
                comboBoxParent.ForeColor = SystemColors.ControlText ;
            else
                comboBoxParent.ForeColor = SystemColors.GrayText ;
        }

        private class ParentCategoryComboItem
        {
            public ParentCategoryComboItem(ComboBox parentCombo, BlogPostCategory category, int indentLevel)
            {
                _parentCombo = parentCombo ;
                _category = category ;
                _indentLevel = indentLevel ;
            }

            public BlogPostCategory Category
            {
                get { return _category; }
            }
            private BlogPostCategory _category ;

            public override bool Equals(object obj)
            {
                return (obj as ParentCategoryComboItem).Category.Equals(Category) ;
            }

            public override int GetHashCode()
            {
                return Category.GetHashCode() ;
            }

            public override string ToString()
            {
                // default padding
                string padding = new String(' ', _indentLevel * 3) ;

                // override if we are selected
                if ( !_parentCombo.DroppedDown && (_parentCombo.SelectedItem != null) && this.Equals(_parentCombo.SelectedItem) )
                    padding = String.Empty ;

                string categoryName = HtmlUtils.UnEscapeEntities(Category.Name, HtmlUtils.UnEscapeMode.Default) ;
                string stringRepresentation = String.Format(CultureInfo.InvariantCulture, "{0}{1}", padding, categoryName) ;
                return stringRepresentation ;
            }

            private int _indentLevel ;
            private ComboBox _parentCombo ;

        }

        private class ParentCategoryComboBox : ComboBox
        {
            public ParentCategoryComboBox()
            {
                // prevent editing and showing of drop down
                DropDownStyle = ComboBoxStyle.DropDownList ;

                // fully cusotm painting
                DrawMode = DrawMode.OwnerDrawFixed ;
                IntegralHeight = false ;
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index != -1)
                {
                    ParentCategoryComboItem comboItem = Items[e.Index] as ParentCategoryComboItem ;

                    e.DrawBackground();

                    // don't indent for main display of category
                    string text = comboItem.ToString();
                    if  (e.Bounds.Width < Width)
                        text = text.Trim() ;

                    using ( Brush brush = new SolidBrush(e.ForeColor) )
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X, e.Bounds.Y) ;

                    e.DrawFocusRectangle();
                }
            }
        }

        private class ParentCategoryComboItemNone : ParentCategoryComboItem
        {
            public ParentCategoryComboItemNone(ComboBox parentCombo)
                : base(parentCombo, new BlogPostCategoryNone(), 0)
            {
            }

            public override string ToString()
            {
                return Res.Get(StringId.CategoryNoParent) ;
            }
        }

        private class PositionManager : IDisposable
        {
            private int _top;
            private int _left;
            private int _width;
            private int _margin;
            private ArrayList _controls = new ArrayList();
            private int _maxWidth;
            private int _minWidth;
            private int _maxHeight ;
            private int _enforcedMaxHeight = -1 ;
            private PointF _autoScaleSize;
            private StringFormat _format;

            public PositionManager(int initialLeft, int initialTop, int scaledMargin, int minWidth, int maxWidth, int maxHeight, PointF autoScaleSize)
            {
                _left = initialLeft;
                _top = initialTop;
                _margin = scaledMargin;
                _maxWidth = maxWidth;
                _minWidth = _width = minWidth;
                _maxHeight = maxHeight ;
                _autoScaleSize = autoScaleSize;
                _format = new StringFormat();
                _format.Trimming = StringTrimming.EllipsisCharacter;
            }

            public void PositionControl(Control c, int indentLevel)
            {
                int INDENT_MARGIN = ScaleX(16) ;
                c.Top = _top;
                c.Left = _left + (INDENT_MARGIN * indentLevel);

                Graphics g = c.CreateGraphics();
                try
                {
                    int maxWidth = _maxWidth - SystemInformation.VerticalScrollBarWidth - c.Left - _left;
                    c.Width = maxWidth;
                    if (c is CheckBox)
                        DisplayHelper.AutoFitSystemCheckBox((CheckBox)c, 0, maxWidth);
                    else if (c is RadioButton)
                        DisplayHelper.AutoFitSystemRadioButton((RadioButton)c, 0, maxWidth);
                    else
                        Debug.Fail("Being asked to position a control that isn't a radiobutton or checkbox");
                    LayoutHelper.NaturalizeHeight(c);
                }
                finally
                {
                    g.Dispose();
                }
                _width = Math.Max(_width, c.Width);
                _controls.Add(c);

                _top = _top + c.Height + _margin;

                // enforce max height if necessary
                if ( _enforcedMaxHeight == -1 && _top > _maxHeight )
                    _enforcedMaxHeight = _top  ;
            }

            public int Width
            {
                get { return _width; }
            }

            public int Height
            {
                get
                {
                    if ( _enforcedMaxHeight != -1 )
                        return _enforcedMaxHeight ;
                    else
                        return _top + _margin;
                }
            }

            protected int ScaleX(int x)
            {
                return (int) (x*_autoScaleSize.X);
            }

            protected int ScaleY(int y)
            {
                return (int) (y*_autoScaleSize.Y);
            }

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion
        }

    }
}
