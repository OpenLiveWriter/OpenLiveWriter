// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Type T is the type of the objects each entry in the list will hold.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SidebarListBox<T> : ListBox
    {
        #region Initialization and Cleanup

        public SidebarListBox()
        {
            _theme = new ControlTheme(this, true);
        }

        public void Initialize()
        {
            // configure owner draw
            DrawMode = DrawMode.OwnerDrawFixed;
            SelectionMode = SelectionMode.One;
            HorizontalScrollbar = false;
            IntegralHeight = false;
            ItemHeight = CalculateItemHeight();
        }

        public void SetEntry(T item, Bitmap image, string text)
        {
            Items.Add(new ListBoxEntryItem(text, item, image, this));
        }

        public bool SelectEntry(string text)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ListBoxEntryItem item = (ListBoxEntryItem)Items[i];
                if (item.Name == text)
                {
                    SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        public new T SelectedValue
        {
            get
            {
                return (SelectedItem as ListBoxEntryItem).Value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }

        #endregion

        #region Painting

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // screen invalid drawing states
            if (DesignMode || e.Index == -1)
                return;

            BidiGraphics g = new BidiGraphics(e.Graphics, e.Bounds);
            // get post-source we are rendering
            ListBoxEntryItem item = Items[e.Index] as ListBoxEntryItem;

            // determine image and text to use
            Bitmap postSourceImage = item.Image;

            // determine state
            bool selected = (e.State & DrawItemState.Selected) > 0;

            // calculate colors
            Color backColor, textColor;
            if (selected)
            {
                if (Focused)
                {
                    backColor = _theme.backColorSelectedFocused;
                    textColor = _theme.textColorSelectedFocused;
                }
                else
                {
                    backColor = _theme.backColorSelected;
                    textColor = _theme.textColorSelected;
                }
            }
            else
            {
                backColor = _theme.backColor;
                textColor = _theme.textColor;
            }

            // draw background
            using (SolidBrush solidBrush = new SolidBrush(backColor))
                g.FillRectangle(solidBrush, e.Bounds);

            // center the image within the list box
            int imageLeft = e.Bounds.Left + ((e.Bounds.Width / 2) - (ScaleX(postSourceImage.Width) / 2));
            int imageTop = e.Bounds.Top + ScaleY(LARGE_TOP_INSET);
            g.DrawImage(false, postSourceImage, new Rectangle(imageLeft, imageTop, ScaleX(postSourceImage.Width), ScaleY(postSourceImage.Height)));

            // calculate standard text drawing metrics
            float leftMargin = ScaleX(ELEMENT_PADDING);
            float topMargin = imageTop + ScaleY(postSourceImage.Height) + ScaleY(ELEMENT_PADDING);
            float fontHeight = e.Font.GetHeight();

            // caption
            // calculate layout rectangle
            Rectangle layoutRectangle = new Rectangle(
                (int)leftMargin,
                (int)topMargin,
                e.Bounds.Width - (2 * ScaleX(ELEMENT_PADDING)),
                (int)fontHeight * TITLE_LINES);

            // draw caption
            g.DrawText(item.Name, e.Font, layoutRectangle, textColor, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);

            // draw focus rectange if necessary
            e.DrawFocusRectangle();
        }

        private class ControlTheme : ControlUITheme
        {
            internal Color backColor;
            internal Color textColor;
            internal Color backColorSelected;
            internal Color textColorSelected;
            internal Color backColorSelectedFocused;
            internal Color textColorSelectedFocused;
            internal Color topLineColor;

            public ControlTheme(Control control, bool applyTheme) : base(control, applyTheme)
            {
            }

            protected override void ApplyTheme(bool highContrast)
            {
                base.ApplyTheme(highContrast);

                backColor = SystemColors.Window;
                textColor = SystemColors.ControlText;
                backColorSelected = !highContrast ? SystemColors.ControlLight : SystemColors.InactiveCaption;
                textColorSelected = !highContrast ? SystemColors.ControlText : SystemColors.InactiveCaptionText;
                backColorSelectedFocused = SystemColors.Highlight;
                textColorSelectedFocused = SystemColors.HighlightText;
                topLineColor = SystemColors.ControlLight;
            }
        }
        private ControlTheme _theme;

        private int CalculateItemHeight()
        {
            int textHeight = Convert.ToInt32(Font.GetHeight() * TITLE_LINES);

            return ScaleY(LARGE_TOP_INSET) + ScaleY(40) + ScaleY(ELEMENT_PADDING) + textHeight + ScaleY(LARGE_BOTTOM_INSET);
        }

        #endregion

        #region Accessibility
        internal class BlogPostSourceListBoxAccessibility : ControlAccessibleObject
        {
            private SidebarListBox<T> _listBox;
            public BlogPostSourceListBoxAccessibility(SidebarListBox<T> ownerControl) : base(ownerControl)
            {
                _listBox = ownerControl;
            }

            public override AccessibleObject GetChild(int index)
            {
                return _listBox.Items[index] as AccessibleObject;
            }

            public override int GetChildCount()
            {
                return _listBox.Items.Count;
            }

            public void NotifySelectionChanged(int index)
            {
                if (index >= 0)
                    NotifyClients(AccessibleEvents.Focus, index);
            }

            public void NotifySelectionChanged()
            {
                NotifySelectionChanged(_listBox.SelectedIndex);
            }

            public override AccessibleRole Role
            {
                get { return AccessibleRole.List; }
            }
        }

        class ListBoxEntryItem : AccessibleObject, IComparable
        {
            private string _title;
            private Bitmap _image;
            private SidebarListBox<T> _listbox;

            private T _value;

            public ListBoxEntryItem(string title, T value, Bitmap image, SidebarListBox<T> ownerControl)

            {
                _image = image;
                _title = title;

                _value = value;
                _listbox = ownerControl;
            }

            public override string Name
            {
                get { return _title; }
                set { base.Name = value; }
            }

            public override string ToString()
            {
                return _title;
            }

            public Bitmap Image
            {
                get { return _image; }
            }

            public new T Value
            {

                get { return _value; }
            }

            private bool IsSelected()
            {
                return _listbox.SelectedItem == this && _listbox.Focused;
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = AccessibleStates.Focusable;
                    if (IsSelected())
                        state = state | AccessibleStates.Focused;
                    return state;
                }
            }

            public override AccessibleRole Role
            {
                get { return AccessibleRole.ListItem; }
            }

            public override AccessibleObject Parent
            {
                get { return _listbox.AccessibilityObject; }
            }

            public int CompareTo(object obj)
            {
                try
                {
                    return string.Compare(Name, ((ListBoxEntryItem)obj).Name, true, CultureInfo.CurrentCulture);
                }
                catch (InvalidCastException)
                {
                    Debug.Fail("InvalidCastException");
                    return 0;
                }
                catch (NullReferenceException)
                {
                    Debug.Fail("NullRefException");
                    return 0;
                }
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (_accessibleObject == null)
                _accessibleObject = new BlogPostSourceListBoxAccessibility(this);
            return _accessibleObject;
        }
        BlogPostSourceListBoxAccessibility _accessibleObject;

        #endregion

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

        #region Private Data

        // item metrics
        private const int LARGE_TOP_INSET = 7;
        private const int LARGE_BOTTOM_INSET = 4;
        private const int ELEMENT_PADDING = 3;
        private const int TITLE_LINES = 3;

        #endregion
    }
}
