// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{

    internal interface ICategorySelectorControl
    {
        BlogPostCategory Category
        {
            get;
        }

        bool Selected
        {
            get; set;
        }

        Control Control
        {
            get;
        }

        event EventHandler SelectedChanged;
    }

    internal class CategorySelectorControlFactory
    {

        private static CategorySelectorControlFactory _categorySelectionControlFactory;

        public static CategorySelectorControlFactory Instance
        {
            get
            {
                if (_categorySelectionControlFactory == null)
                    _categorySelectionControlFactory = new CategorySelectorControlFactory();
                return _categorySelectionControlFactory;
            }
        }

        public ICategorySelectorControl GetControl(BlogPostCategory category, bool multiSelect)
        {
            if (multiSelect)
                return new CategoryCheckSelectorControl(category);
            else
                return new CategoryRadioSelectorControl(category);
        }
    }

    internal class CategoryRadioSelectorControl: RadioButton, ICategorySelectorControl
    {

        public CategoryRadioSelectorControl(BlogPostCategory category) : base()
        {
            FlatStyle = FlatStyle.System;
            _category = category;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TextAlign = ContentAlignment.TopLeft ; // prevent wrapping
            string categoryText = HtmlUtils.UnEscapeEntities(Category.Name, HtmlUtils.UnEscapeMode.Default);
            Text = categoryText.Replace("&", "&&") ;
            using ( Graphics g = CreateGraphics() )
                Height = Math.Max((int)Math.Ceiling(DisplayHelper.ScaleY(18)), Convert.ToInt32(Font.GetHeight(g)));
        }

        #region ICategorySelectorControl Members

        public BlogPostCategory Category
        {
            get
            {
                return _category;
            }
        }
        private BlogPostCategory _category;

        public bool Selected
        {
            get
            {
                return Checked;
            }
            set
            {
                Checked = value;
            }
        }

        public Control Control
        {
            get
            {
                return this;
            }
        }

        public event EventHandler SelectedChanged;
        protected override void OnCheckedChanged(EventArgs e)
        {
            if (SelectedChanged != null)
                SelectedChanged(this, e);
            base.OnCheckedChanged (e);
        }

        #endregion

    }

    internal class CategoryCheckSelectorControl : CheckBox, ICategorySelectorControl
    {
        public CategoryCheckSelectorControl(BlogPostCategory category) : base()
        {
            FlatStyle = FlatStyle.System;
            _category = category;

        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TextAlign = ContentAlignment.TopLeft ; // prevent wrapping
            string categoryText = HtmlUtils.UnEscapeEntities(Category.Name, HtmlUtils.UnEscapeMode.Default);
            Text = categoryText.Replace("&", "&&") ;
            using ( Graphics g = CreateGraphics() )
                Height = Math.Max((int)Math.Ceiling(DisplayHelper.ScaleY(18)), Convert.ToInt32(Font.GetHeight(g)));
        }

        #region ICategorySelectorControl Members

        public BlogPostCategory Category
        {
            get
            {
                return _category;
            }
        }
        private BlogPostCategory _category;

        public bool Selected
        {
            get
            {
                return Checked;
            }
            set
            {
                Checked = value;
            }
        }

        public Control Control
        {
            get
            {
                return this;
            }
        }

        public event EventHandler SelectedChanged;
        protected override void OnCheckedChanged(EventArgs e)
        {
            if (SelectedChanged != null)
                SelectedChanged(this, e);
            base.OnCheckedChanged (e);
        }

        #endregion
    }

}
