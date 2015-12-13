// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
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
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.DisplayMessages;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{

    internal class CategoryDropDownControl : ComboBox, IBlogPostEditor, IBlogCategorySettings
    {
        #region IBlogPostEditor Members

        void IBlogPostEditor.Initialize(IBlogPostEditingContext editingContext)
        {
            CategoryContext.SelectedCategories = editingContext.BlogPost.Categories ;
            toolTipCategories.SetToolTip(this, CategoryContext.FormattedCategoryList);
            _isDirty = false ;
        }

        void IBlogPostEditor.SaveChanges(BlogPost post)
        {
            post.Categories = CategoryContext.SelectedCategories;
            _isDirty = false ;
        }

        private System.Windows.Forms.ToolTip toolTipCategories;

        bool IBlogPostEditor.ValidatePublish()
        {
            // see if we need to halt publishing to specify a category
            if ( Visible )
            {
                if ( PostEditorSettings.CategoryReminder && (CategoryContext.Categories.Length > 0) && (CategoryContext.SelectedCategories.Length == 0) )
                {
                    if ( DisplayMessage.Show(typeof(CategoryReminderDisplayMessage), FindForm(), "\r\n\r\n") == DialogResult.No )
                    {
                        Focus() ;
                        return false ;
                    }
                }
            }

            // proceed
            return true ;
        }

        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {

        }

        bool IBlogPostEditor.IsDirty
        {
            get
            {
                return _isDirty ;
            }
        }
        private bool _isDirty = false ;

        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
            _targetBlog = newBlog ;

            if ( newBlog.ClientOptions.SupportsMultipleCategories )
                CategoryContext.SelectionMode = CategoryContext.SelectionModes.MultiSelect;
            else
                CategoryContext.SelectionMode = CategoryContext.SelectionModes.SingleSelect;

            CategoryContext.SetBlogCategories(_targetBlog.Categories);
            CategoryContext.SelectedCategories = new BlogPostCategory[0];
        }
        private Blog _targetBlog ;


        void IBlogPostEditor.OnBlogSettingsChanged(bool templateChanged)
        {
            // make sure we have the lastest categoires (in case the underlying target blog changed)
            CategoryContext.SetBlogCategories(_targetBlog.Categories) ;
        }

        #endregion

        public CategoryDropDownControl() : base()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // prevent editing and showing of drop down
            DropDownStyle = ComboBoxStyle.DropDownList ;

            // fully cusotm painting
            IntegralHeight = false ;
            DrawMode = DrawMode.OwnerDrawFixed ;
            Items.Add(String.Empty) ;

            _categoryContext = new CategoryContext();
        }

        public void Initialize(IWin32Window parentFrame)
        {
            _parentFrame = parentFrame ;
            _categoryContext.BlogCategorySettings = this;
            _categoryContext.Changed += new CategoryContext.CategoryChangedEventHandler(_categoryContext_Changed);
        }

        // replace standard drop down behavior with category form
        protected override void WndProc(ref Message m)
        {
            if (IsDropDownMessage(m))
            {
                DisplayCategoryForm();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        // replace standard painting behavior
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // determine the text color based on whether we have categories
            bool hasCategories = _categoryContext.SelectedCategories.Length > 0 ;
            Color textColor = hasCategories ? SystemColors.ControlText : SystemColors.GrayText;

            // draw the text
            const int HORIZONTAL_MARGIN = 2 ;
            const int VERTICAL_MARGIN = 1 ;
            Rectangle textRegion = new Rectangle(new Point(HORIZONTAL_MARGIN, VERTICAL_MARGIN), new Size(Width - 2*HORIZONTAL_MARGIN, Height - 2*VERTICAL_MARGIN) );
            using (Brush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(
                    _categoryContext.Text,
                    Font, textBrush, textRegion, DisplayFormat );
            }

            // draw focus rectangle (only if necessary)
            e.DrawFocusRectangle();
        }
        private const int MARGIN = 1;

        BlogPostCategory[] IBlogCategorySettings.RefreshCategories(bool ignoreErrors)
        {
            try
            {
                _targetBlog.RefreshCategories();
            }
            catch( BlogClientOperationCancelledException )
            {
                // show no UI for operation cancelled
                Debug.WriteLine("BlogClient operation cancelled");
            }
            catch (Exception ex)
            {
                if(!ignoreErrors)
                    DisplayableExceptionDisplayForm.Show( _parentFrame, ex );
            }
            return _targetBlog.Categories;
        }

        void IBlogCategorySettings.UpdateCategories(BlogPostCategory[] categories)
        {
            using ( BlogSettings blogSettings = BlogSettings.ForBlogId(_targetBlog.Id) )
                blogSettings.Categories = categories ;
        }

        private CategoryContext CategoryContext
        {
            get
            {
                return _categoryContext;
            }
        }

        private void DisplayCategoryForm()
        {
            // If we just dismissed the form, don't display it
            if (_recentlyClosedForm)
                return;

            Focus();

            _categoryDisplayForm = new CategoryDisplayForm(this, _categoryContext);
            //_categoryDisplayForm.MinDropDownWidth = 0;
            IMiniFormOwner miniFormOwner = FindForm() as IMiniFormOwner;
            if (miniFormOwner != null)
                _categoryDisplayForm.FloatAboveOwner(miniFormOwner);
            _categoryDisplayForm.Closed += new EventHandler(_categoryDisplayForm_Closed);
            _categoryDisplayForm.Show();
        }
        private CategoryDisplayFormBase _categoryDisplayForm = null;

        private void _categoryContext_Changed(object sender, CategoryContext.CategoryChangedEventArgs e)
        {
            _isDirty = true ;

            toolTipCategories.SetToolTip(this, CategoryContext.FormattedCategoryList);
            Invalidate();
            Update();
        }

        private void _categoryDisplayForm_Closed(object sender, EventArgs e)
        {
            // This timer makes it so when you click the control to dismiss the form,
            // we don't instantly redisplay the form (same goes for hitting enter)
            _recentlyClosedForm = true;
            Timer t = new Timer();
            t.Interval = 100;
            t.Tick += new EventHandler(t_Tick);
            t.Start();

            _categoryDisplayForm.Closed -= new EventHandler(_categoryDisplayForm_Closed);
            Invalidate();
        }

        private void t_Tick(object sender, EventArgs e)
        {
            _recentlyClosedForm = false;
            Timer t = (Timer)sender;
            t.Stop();
            t.Tick -= new EventHandler(t_Tick);
            t.Dispose();
        }

        private StringFormat DisplayFormat
        {
            get
            {
                if (_displayFormat == null)
                {
                    _displayFormat = new StringFormat();
                    _displayFormat.Trimming = StringTrimming.EllipsisCharacter;
                    _displayFormat.Alignment = StringAlignment.Near;
                    _displayFormat.LineAlignment = StringAlignment.Center;
                    _displayFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit;

                }
                return _displayFormat;
            }
        }
        private StringFormat _displayFormat;

        private bool IsDropDownMessage(Message m)
        {
            // Left Mouse Button
            if ( m.Msg == WM.LBUTTONDOWN || m.Msg == WM.LBUTTONDBLCLK )
            {
                return true ;
            }

                // F4
            else if ( m.Msg == WM.KEYDOWN )
            {
                Keys keyCombo = (Keys)(int)m.WParam & Keys.KeyCode ;

                if ( keyCombo == Keys.F4 )
                    return true ;
                else
                    return false ;
            }

                // Alt+Arrow
            else if ( m.Msg == WM.SYSKEYDOWN )
            {
                int wparam = m.WParam.ToInt32();
                int lparam = m.LParam.ToInt32();

                if ((wparam == 0x28 && (lparam == 0x21500001 || lparam == 0x20500001)) ||    //Alt+Down, Alt+NumDown
                    (wparam == 0x26 && (lparam == 0x21480001 || lparam == 0x20480001)))      //Alt+Up, Alt+NumUp
                {
                    return true ;
                }
                else
                {
                    return false ;
                }
            }

                // Not a drop-down message
            else
            {
                return false ;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTipCategories = new System.Windows.Forms.ToolTip(this.components);
            //
            // CategoryDropDownControl
            //
            this.Size = new System.Drawing.Size(138, 48);

        }
        #endregion

        private IWin32Window _parentFrame;
        private bool _recentlyClosedForm = false;
        private System.ComponentModel.IContainer components;
        private CategoryContext _categoryContext;

    }

}
