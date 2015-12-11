// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    internal partial class RadioCategorySelector : UserControl, ICategorySelector, IRtlAware
    {
        private readonly CategoryContext ctx;

        public RadioCategorySelector()
        {
            Debug.Assert(DesignMode);
            InitializeComponent();
        }

        public RadioCategorySelector(CategoryContext ctx)
        {
            // We set it here because we need it set before we naturalize height, which happens in the constructor (LoadCategories)
            this.Font = Res.DefaultFont;
            this.ctx = ctx;
            InitializeComponent();

            LoadCategories(false);
        }

        public void LoadCategories()
        {
            LoadCategories(true);
        }

        private void LoadCategories(bool doRtlFixup)
        {
            Controls.Clear();
            int y = DockPadding.Top;
            List<BlogPostCategory> categories = new List<BlogPostCategory>(ctx.Categories);
            if (categories.Count == 0)
                return;

            categories.Add(new BlogPostCategoryNone());
            categories.Sort();

            BlogPostCategory[] selected = ctx.SelectedCategories;

            foreach (BlogPostCategory cat in categories)
            {
                RadioButton radio = new RadioButton();
                Controls.Add(radio);
                radio.Text = HtmlUtils.UnEscapeEntities(cat.Name, HtmlUtils.UnEscapeMode.Default);
                radio.AccessibleName = cat.Name;
                radio.UseMnemonic = false;
                radio.Tag = cat;
                radio.AutoSize = true;
                radio.Location = new Point(DockPadding.Left, y);
                y = radio.Bottom;

                if (BlogPostCategoryNone.IsCategoryNone(cat))
                    radio.Checked = selected.Length == 0;
                else
                    radio.Checked = Array.IndexOf(selected, cat) >= 0;

                radio.CheckedChanged += radio_CheckedChanged;

            }

            if (doRtlFixup)
                BidiHelper.RtlLayoutFixup(this, true);
        }

        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = ((RadioButton)sender);
            if (radio.Checked)
            {
                if (ContainsFocus)
                    radio.Focus();
                ctx.SelectedCategories = new BlogPostCategory[] { ((BlogPostCategory)radio.Tag) };
            }
            else
                ctx.SelectedCategories = new BlogPostCategory[0];
        }

        public void Filter(string criteria)
        {
            int y = DockPadding.Top;
            foreach (Control c in Controls)
            {
                if (criteria.Length == 0
                    || c.Text.ToLower(CultureInfo.CurrentCulture).StartsWith(criteria))
                {
                    c.Visible = true;
                    c.Top = y;
                    y += c.GetPreferredSize(Size.Empty).Height;
                }
                else
                    c.Visible = false;
            }
        }

        public void SelectCategory(BlogPostCategory category)
        {
            foreach (RadioButton radio in Controls)
            {
                if (category.Equals(radio.Tag))
                {
                    radio.Checked = true;
                    return;
                }
            }
        }

        private int SelectedIndex
        {
            get
            {
                for (int i = 0; i < Controls.Count; i++)
                    if (((RadioButton)Controls[i]).Checked)
                        return i;
                return -1;
            }
        }

        private List<RadioButton> VisibleControls
        {
            get
            {
                List<RadioButton> results = new List<RadioButton>();
                foreach (RadioButton radio in Controls)
                    if (radio.Visible)
                        results.Add(radio);
                return results;
            }
        }

        public void UpArrow()
        {
            MoveSelection(true);
        }

        public void DownArrow()
        {
            MoveSelection(false);
        }

        private void MoveSelection(bool up)
        {
            List<RadioButton> controls = VisibleControls;
            if (controls.Count == 0)
                return;

            int idx = SelectedIndex;
            if (idx >= 0)
                idx = controls.IndexOf((RadioButton)Controls[idx]);

            if (idx < 0)
                idx = up ? controls.Count : -1;

            if (idx == 0 && up)
                idx = controls.Count;

            idx += up ? -1 : 1;
            idx %= controls.Count;
            controls[idx].Checked = true;
            ScrollControlIntoView(controls[idx]);
            controls[idx].Focus();
        }

        void ICategorySelector.Enter()
        {
            FindForm().Close();
        }

        void ICategorySelector.CtrlEnter()
        {
            FindForm().Close();
        }

        #region IRtlAware Members

        public new void Layout()
        {
            foreach (RadioButton radio in VisibleControls)
            {
                radio.Left = Width - radio.Width - DockPadding.Left;
            }
        }

        #endregion
    }
}
