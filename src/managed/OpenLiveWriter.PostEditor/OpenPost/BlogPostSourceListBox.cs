// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor;
using Timer = System.Windows.Forms.Timer;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    internal class BlogPostSourceListBox : ListBox
    {
        #region Initialization and Cleanup

        public BlogPostSourceListBox()
        {
            _theme = new ControlTheme(this, true);
        }

        public void Initialize(bool _includeDrafts)
        {
            // get the list of blogs, determine how many items we will be displaying,
            // and then have that drive the view mode
            string[] blogIds = BlogSettings.GetBlogIds();
            int itemCount = (_includeDrafts ? 1 : 0) + 1 + blogIds.Length;
            _showLargeIcons = itemCount <= 5;

            // configure owner draw
            DrawMode = DrawMode.OwnerDrawFixed;
            SelectionMode = SelectionMode.One;
            HorizontalScrollbar = false;
            IntegralHeight = false;
            ItemHeight = CalculateItemHeight(_showLargeIcons);

            // populate list
            if (_includeDrafts)
                _draftsIndex = Items.Add(new PostSourceItem(new LocalDraftsPostSource(), this));
            _recentPostsIndex = Items.Add(new PostSourceItem(new LocalRecentPostsPostSource(), this));

            ArrayList blogs = new ArrayList();
            foreach (string blogId in BlogSettings.GetBlogIds())
            {
                blogs.Add(new PostSourceItem(Res.Get(StringId.Blog), new RemoteWeblogBlogPostSource(blogId), this));
            }
            blogs.Sort();
            Items.AddRange(blogs.ToArray());
        }

        public void SelectDrafts()
        {
            SelectedIndex = _draftsIndex;
        }

        public void SelectRecentPosts()
        {
            SelectedIndex = _recentPostsIndex;
        }

        public IPostEditorPostSource SelectedPostSource
        {
            get
            {
                return (SelectedItem as PostSourceItem).PostSource;
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

            // get post-source we are rendering
            IPostEditorPostSource postSource = (Items[e.Index] as PostSourceItem).PostSource;

            // determine image and text to use
            Image postSourceImage;
            if (e.Index == _draftsIndex)
                postSourceImage = _showLargeIcons ? _draftsImageLarge : _draftsImage;
            else if (e.Index == _recentPostsIndex)
                postSourceImage = _showLargeIcons ? _recentPostsImageLarge : _recentPostsImage;
            else
                postSourceImage = _showLargeIcons ? _weblogImageLarge : _weblogImage;

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

            BidiGraphics g = new BidiGraphics(e.Graphics, e.Bounds);

            // draw background
            using (SolidBrush solidBrush = new SolidBrush(backColor))
                g.FillRectangle(solidBrush, e.Bounds);

            if (_showLargeIcons)
            {
                // center the image within the list box
                int imageLeft = e.Bounds.Left + ((e.Bounds.Width / 2) - (ScaleX(postSourceImage.Width) / 2));
                int imageTop = e.Bounds.Top + ScaleY(LARGE_TOP_INSET);
                g.DrawImage(false, postSourceImage, new Rectangle(imageLeft, imageTop, ScaleX(postSourceImage.Width), ScaleY(postSourceImage.Height)));

                // setup string format
                TextFormatFlags stringFormat = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis | TextFormatFlags.HorizontalCenter;

                // calculate standard text drawing metrics
                int leftMargin = ScaleX(ELEMENT_PADDING);
                int topMargin = imageTop + ScaleY(postSourceImage.Height) + ScaleY(ELEMENT_PADDING);
                int fontHeight = g.MeasureText(postSource.Name, e.Font).Height;

                // caption
                // calculate layout rectangle
                Rectangle layoutRectangle = new Rectangle(
                    leftMargin,
                    topMargin,
                    e.Bounds.Width - (2 * ScaleX(ELEMENT_PADDING)),
                    fontHeight * TITLE_LINES);

                // draw caption
                g.DrawText(postSource.Name, e.Font, layoutRectangle, textColor, stringFormat);

            }
            else
            {
                // draw post icon
                g.DrawImage(false, postSourceImage,
                                      new Rectangle(e.Bounds.Left + ScaleX(ELEMENT_PADDING), e.Bounds.Top + ScaleY(SMALL_TOP_INSET),
                                      ScaleX(postSourceImage.Width), ScaleY(postSourceImage.Height)));

                // setup string format
                TextFormatFlags stringFormat = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis;

                // calculate standard text drawing metrics
                int leftMargin = ScaleX(ELEMENT_PADDING) + ScaleX(postSourceImage.Width) + ScaleX(ELEMENT_PADDING);
                int topMargin = e.Bounds.Top + ScaleY(SMALL_TOP_INSET);
                int fontHeight = g.MeasureText(postSource.Name, e.Font).Height;

                // caption
                // calculate layout rectangle
                Rectangle layoutRectangle = new Rectangle(
                    leftMargin,
                    topMargin,
                    e.Bounds.Width - leftMargin - ScaleX(ELEMENT_PADDING),
                    fontHeight * TITLE_LINES);

                // draw caption
                g.DrawText(postSource.Name, e.Font, layoutRectangle, textColor, stringFormat);
            }

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

        private int CalculateItemHeight(bool showLargeIcons)
        {
            int textHeight = Convert.ToInt32(Font.GetHeight() * TITLE_LINES);

            if (showLargeIcons)
            {
                return ScaleY(LARGE_TOP_INSET) + ScaleY(_weblogImageLarge.Height) + ScaleY(ELEMENT_PADDING) + textHeight + ScaleY(LARGE_BOTTOM_INSET);
            }
            else
            {
                return ScaleY(SMALL_TOP_INSET) + textHeight + ScaleY(SMALL_BOTTOM_INSET);
            }
        }

        #endregion

        #region Accessibility
        internal class BlogPostSourceListBoxAccessibility : ControlAccessibleObject
        {
            private BlogPostSourceListBox _listBox;
            public BlogPostSourceListBoxAccessibility(BlogPostSourceListBox ownerControl) : base(ownerControl)
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

        class PostSourceItem : AccessibleObject, IComparable
        {
            private IPostEditorPostSource _source;
            private string _accName;
            private BlogPostSourceListBox _listbox;

            public PostSourceItem(IPostEditorPostSource source, BlogPostSourceListBox ownerControl)
            {
                _source = source;
                _accName = source.Name;
                _listbox = ownerControl;
            }
            public PostSourceItem(string accName, IPostEditorPostSource source, BlogPostSourceListBox ownerControl)
            {
                _source = source;
                _accName = accName;
                _listbox = ownerControl;
            }

            public override string ToString()
            {
                return Name;
            }

            public override string Value
            {
                get { return _accName; }
                set { base.Value = value; }
            }

            public override string Name
            {
                get { return _source.Name; }
                set { base.Name = value; }
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

            public IPostEditorPostSource PostSource { get { return _source; } }

            public int CompareTo(object obj)
            {
                try
                {
                    return string.Compare(_source.Name, ((PostSourceItem)obj)._source.Name, true, CultureInfo.CurrentCulture);
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
        private const int SMALL_TOP_INSET = 7;
        private const int SMALL_BOTTOM_INSET = 7;
        private const int LARGE_TOP_INSET = 7;
        private const int LARGE_BOTTOM_INSET = 4;
        private const int ELEMENT_PADDING = 3;
        private const int TITLE_LINES = 2;

        // view mode
        private bool _showLargeIcons;

        // special list values
        private int _draftsIndex;
        private int _recentPostsIndex;

        // images
        private Image _draftsImage = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectDraftPostings.png");
        private Image _recentPostsImage = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectRecentPostings.png");
        private Image _weblogImage = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectWebLogPostings.png");
        private Image _draftsImageLarge = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectDraftPostingsLarge.png");
        private Image _recentPostsImageLarge = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectRecentPostingsLarge.png");
        private Image _weblogImageLarge = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectWebLogPostingsLarge.png");

        #endregion
    }
}
