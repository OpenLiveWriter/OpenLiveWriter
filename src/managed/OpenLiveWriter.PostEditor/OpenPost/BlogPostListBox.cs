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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using Timer = System.Windows.Forms.Timer;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    internal class BlogPostListBox : ListBox
    {
        #region Initialization and Cleanup

        public BlogPostListBox()
        {
            _theme = new ControlTheme(this, true);
        }

        public void Initialize(Form parentForm)
        {
            _parentForm = parentForm;
            DrawMode = DrawMode.OwnerDrawFixed;
            SelectionMode = SelectionMode.One;
            HorizontalScrollbar = false;
            IntegralHeight = false;
            ItemHeight = CalculateItemHeight();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // cancel any running operation
                CancelGetRecentPostsAsync();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Public Interface

        public event EventHandler PostsRefreshed;
        protected virtual void OnPostsRefreshed(EventArgs e)
        {
            if (PostsRefreshed != null)
                PostsRefreshed(this, e);
        }

        public IPostEditorPostSource PostSource
        {
            get
            {
                return _postSource;
            }
            set
            {
                if (_postSource != value)
                {
                    _postSource = value;
                    RefreshPosts();
                }
            }
        }
        private IPostEditorPostSource _postSource;

        public RecentPostRequest RecentPostRequest
        {
            get
            {
                return _recentPostRequest;
            }
            set
            {
                // null check prevents designer hosage
                if (_recentPostRequest != value && value != null)
                {
                    _recentPostRequest = value;
                    RefreshPosts();
                }
            }
        }
        private RecentPostRequest _recentPostRequest = new RecentPostRequest(5);

        public bool ShowPages
        {
            get
            {
                return _showPages;
            }
            set
            {
                if (_showPages != value)
                {
                    _showPages = value;
                    RefreshPosts();
                }
            }
        }
        private bool _showPages = false;

        public PostInfo SelectedPost
        {
            get
            {
                if (SelectedItem != null)
                    return (SelectedItem as PostInfoItem).PostInfo;
                else
                    return null;
            }
        }

        private string _filter = String.Empty;
        private Regex[] _filterSegments;
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                // Don't let the filter get set to null
                if (value == null)
                    value = String.Empty;

                // Only set the filter if it is different then the current one
                if (_filter != value)
                {
                    _filter = value;
                    string[] _stringSegments = _filter.ToLower(CultureInfo.CurrentCulture).Split(' ');
                    _filterSegments = new Regex[_stringSegments.Length];
                    for (int i = 0; i < _stringSegments.Length; i++)
                    {
                        string pattern = "(^|[^\\w])" + Regex.Escape(_stringSegments[i]);
                        _filterSegments[i] = new Regex(pattern, RegexOptions.ExplicitCapture);
                    }

                    UpdateListBox();
                }
            }
        }

        public event EventHandler RefreshBegin;
        private void FireRefreshBegin()
        {
            if (RefreshBegin != null)
                RefreshBegin(this, EventArgs.Empty);
        }

        public void RefreshPosts()
        {
            // cancel any running recent post fetch
            CancelGetRecentPostsAsync();

            // Let other controls know we are starting a refresh
            // most notably the filter box
            FireRefreshBegin();

            // clear out the list
            ClearList();

            // refresh posts as appropriate
            if (PostSource != null && PostSource.VerifyCredentials())
            {
                if (PostSource.IsSlow)
                {
                    BeginGetRecentPostsAsync();
                }
                else
                {
                    SetPostList(GetRecentPostsSync(ShowPages));
                }
            }
            else
            {
                ShowEmptyPostListControl();
            }
        }

        public bool IsRefreshing
        {
            get
            {
                return _pendingRecentPostsOperation != null;
            }
        }

        public event UserDeletedPostEventHandler UserDeletedPost;

        public void DeleteSelectedPost()
        {
            if (SelectedIndex == -1 || !AllowDelete)
                return;

            PostInfo selectedPost = SelectedPost;
            string type = selectedPost.IsPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower);
            MessageId messageId = PostSource is LocalDraftsPostSource ? MessageId.ConfirmDeleteDraft : MessageId.ConfirmDeletePost;
            if (DisplayMessage.Show(messageId, this, type, selectedPost.Title) == DialogResult.Yes)
            {
                try
                {
                    // delete the underlying post
                    Update();
                    using (new WaitCursor())
                    {
                        // delete the post
                        if (PostSource.DeletePost(selectedPost.Id, selectedPost.IsPage))
                        {
                            // note the selected index prior to deleting
                            int selectedIndex = SelectedIndex;

                            // remove that post from the list
                            RemoveItem(selectedPost);

                            // try to reselect intelligently
                            if (Items.Count > 0)
                            {
                                SelectedIndex = Math.Min(selectedIndex, Items.Count - 1);
                            }
                            else
                            {
                                ShowEmptyPostListControl();
                            }

                            // fire notification
                            if (UserDeletedPost != null)
                                UserDeletedPost(selectedPost);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayableExceptionDisplayForm.Show(this, ex);
                }
            }

            // This is because of a bug on XP that caused the focus to go in a weird place,
            // and the list box to draw the wrong color.
            Select();
            Focus();
            Refresh();
        }

        public IBlogPostEditingContext RetrieveSelectedPost()
        {
            if (_postSource != null)
                return _postSource.GetPost(SelectedPost.Id);
            else
                return null;
        }

        internal void ClearFilterWithoutUpdate()
        {
            _filter = String.Empty;
            _filterSegments = null;
        }

        private bool _allowDelete = false;
        public bool AllowDelete
        {
            get
            {
                return _allowDelete;
            }
            set
            {
                _allowDelete = value;
            }
        }

        #endregion

        #region Event Handling

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // delete key deletes items
            if (keyData == Keys.Delete && PostSource.SupportsDelete)
            {
                DeleteSelectedPost();
                return true;
            }
            else // delegate to base
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion

        #region Painting

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // screen invalid drawing states
            if (DesignMode || e.Index == -1)
                return;

            // get post we are rendering
            PostInfo postInfo = (Items[e.Index] as PostInfoItem).PostInfo;

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

            // setup standard string format
            TextFormatFlags ellipsesStringFormat = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis | TextFormatFlags.NoPrefix;

            // draw background
            using (SolidBrush solidBrush = new SolidBrush(backColor))
                g.FillRectangle(solidBrush, e.Bounds);

            // draw top-line if necessary
            //if ( !selected )
            {
                using (Pen pen = new Pen(_theme.topLineColor))
                    g.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - ScaleY(1));
            }

            // draw post icon
            g.DrawImage(false, _blogPostImage, new Rectangle(
                                  e.Bounds.Left + ScaleX(HORIZONTAL_INSET), e.Bounds.Top + ScaleY(TITLE_INSET),
                                  ScaleX(_blogPostImage.Width), ScaleY(_blogPostImage.Height)));

            // calculate standard text drawing metrics
            int leftMargin = ScaleX(HORIZONTAL_INSET) + ScaleX(_blogPostImage.Width) + ScaleX(HORIZONTAL_INSET);
            int topMargin = e.Bounds.Top + ScaleY(TITLE_INSET);
            // draw title line

            // post date
            string dateText = postInfo.PrettyDateDisplay;
            Size dateSize = g.MeasureText(dateText, e.Font);
            Rectangle dateRectangle = new Rectangle(
                e.Bounds.Right - ScaleX(HORIZONTAL_INSET) - dateSize.Width, topMargin, dateSize.Width, dateSize.Height);

            g.DrawText(dateText, e.Font, dateRectangle, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);

            // post title
            int titleWidth = dateRectangle.Left - leftMargin - ScaleX(DATE_PADDING);
            string titleString = String.Format(CultureInfo.CurrentCulture, "{0}", postInfo.Title);
            int fontHeight = g.MeasureText(titleString, e.Font).Height;
            Rectangle titleRectangle = new Rectangle(leftMargin, topMargin, titleWidth, fontHeight);

            g.DrawText(
                titleString,
                e.Font,
                titleRectangle, textColor, ellipsesStringFormat);

            // draw post preview

            // calculate layout rectangle
            Rectangle layoutRectangle = new Rectangle(
                leftMargin,
                topMargin + fontHeight + ScaleY(PREVIEW_INSET),
                e.Bounds.Width - leftMargin - ScaleX(HORIZONTAL_INSET),
                fontHeight * PREVIEW_LINES);

            // draw post preview
            string postPreview = postInfo.PlainTextContents;
            if (PostSource.HasMultipleWeblogs && (postInfo.BlogName != String.Empty))
                postPreview = String.Format(CultureInfo.CurrentCulture, "{0} - {1}", postInfo.BlogName, postPreview);

            g.DrawText(
                postPreview,
                e.Font, layoutRectangle, textColor, ellipsesStringFormat);

            // focus rectange if necessary
            e.DrawFocusRectangle();
        }

        private Image _blogPostImage = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.BlogPost.png");

        private class ControlTheme : ControlUITheme
        {
            internal Color backColor;
            internal Color textColor;
            internal Color backColorSelected;
            internal Color textColorSelected;
            internal Color backColorSelectedFocused;
            internal Color textColorSelectedFocused;
            internal Color previewColor;
            internal Color topLineColor;
            private BlogPostListBox _listBoxControl;

            public ControlTheme(BlogPostListBox control, bool applyTheme) : base(control, applyTheme)
            {
                _listBoxControl = control;
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
                previewColor = !highContrast ? Color.FromArgb(200, textColor) : textColor;
                topLineColor = SystemColors.ControlLight;
            }
        }
        private ControlTheme _theme;

        private int CalculateItemHeight()
        {
            float fontHeight = Font.GetHeight();
            return ScaleY(TITLE_INSET) +                                    // top-margin
                    Convert.ToInt32(fontHeight) +                   // title
                    ScaleY(PREVIEW_INSET) +                                 // air between title and preview
                    Convert.ToInt32(fontHeight * PREVIEW_LINES) +   // post-preview
                    ScaleY(BOTTOM_INSET);                                   // air at bottom
        }

        // item metrics
        private const int TITLE_INSET = 7;
        private const int PREVIEW_INSET = 5;
        private const int BOTTOM_INSET = 10;
        private const int HORIZONTAL_INSET = 3;
        private const int PREVIEW_LINES = 2;
        private const int DATE_PADDING = 10;

        #endregion

        #region Post Refreshing Implementation

        private void BeginGetRecentPostsAsync()
        {
            Debug.Assert(_pendingRecentPostsOperation == null);

            // padded wait cursor to provide tactile feedback that a fetch is starting
            PaddedWaitCursor waitCursor = new PaddedWaitCursor(250);
            waitCursor.Dispose();

            // show progress UI
            GetRecentPostsProgressControl.Start(ShowPages);

            _pendingRecentPostsOperation = new GetRecentPostsAsyncOperation(new BlogClientUIContextImpl(_parentForm), PostSource, RecentPostRequest, ShowPages);
            _pendingRecentPostsOperation.Completed += new EventHandler(_pendingRecentPostsOperation_Completed);
            _pendingRecentPostsOperation.Failed += new ThreadExceptionEventHandler(_pendingRecentPostsOperation_Failed);
            _pendingRecentPostsOperation.Start();
        }
        private GetRecentPostsAsyncOperation _pendingRecentPostsOperation = null;

        private void EndGetRecentPostsAsync()
        {
            Debug.Assert(_pendingRecentPostsOperation != null);

            // unhook from events
            _pendingRecentPostsOperation.Completed -= new EventHandler(_pendingRecentPostsOperation_Completed);
            _pendingRecentPostsOperation.Failed -= new ThreadExceptionEventHandler(_pendingRecentPostsOperation_Failed);
            _pendingRecentPostsOperation = null;

            // cancel progress UI
            GetRecentPostsProgressControl.Stop();
        }

        private class GetRecentPostsAsyncOperation : OpenLiveWriter.CoreServices.AsyncOperation
        {
            public GetRecentPostsAsyncOperation(IBlogClientUIContext uiContext, IPostEditorPostSource postSource, RecentPostRequest request, bool getPages)
                : base(uiContext)
            {
                _uiContext = uiContext;
                _postSource = postSource;
                _request = request;
                _getPages = getPages;
            }

            public PostInfo[] RecentPosts
            {
                get { return _recentPosts; }
            }
            private PostInfo[] _recentPosts;

            protected override void DoWork()
            {
                using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(_uiContext))
                {
                    if (_getPages)
                        _recentPosts = _postSource.GetPages(_request);
                    else
                        _recentPosts = _postSource.GetRecentPosts(_request);

                    if (CancelRequested)
                        AcknowledgeCancel();
                }
            }

            private IBlogClientUIContext _uiContext;
            private IPostEditorPostSource _postSource;
            private RecentPostRequest _request;
            private bool _getPages;
        }

        private void CancelGetRecentPostsAsync()
        {
            if (_pendingRecentPostsOperation != null)
            {
                _pendingRecentPostsOperation.Cancel();
                EndGetRecentPostsAsync();
            }
        }

        private void _pendingRecentPostsOperation_Completed(object sender, EventArgs e)
        {
            // get the posts
            PostInfo[] posts = _pendingRecentPostsOperation.RecentPosts;

            // end the session
            EndGetRecentPostsAsync();

            // update the list
            SetPostList(posts);
        }

        private void _pendingRecentPostsOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            // end the session
            EndGetRecentPostsAsync();

            // show the no-posts UI
            ShowEmptyPostListControl();

            // show the error
            if (!(e.Exception is BlogClientOperationCancelledException))
            {
                DisplayableExceptionDisplayForm.Show(FindForm(), e.Exception);
            }
            else
            {
                Debug.WriteLine("BlogClient operation cancelled");
            }
        }

        private void ClearList()
        {
            Items.Clear();
            HideEmptyPostListControl();
            Update(); // make sure UI reflects Clear immediately
        }

        private void RemoveItem(PostInfo item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                PostInfoItem postInfoItem = Items[i] as PostInfoItem;
                if (postInfoItem.PostInfo == item)
                    Items.Remove(postInfoItem);
            }
            Invalidate();
        }

        readonly Dictionary<string, PostInfoItem[]> _filterCache = new Dictionary<string, PostInfoItem[]>();
        private string _lastFilter;
        void SetPostList(PostInfo[] posts)
        {
            // Remove all the past searchs as they are no longer valid
            _filterCache.Clear();

            // Add all the posts/pages to the cache for the empty string search
            List<PostInfoItem> allPosts = new List<PostInfoItem>();
            foreach (PostInfo blogPostInfo in posts)
            {
                allPosts.Add(new PostInfoItem(blogPostInfo, this));
            }
            _filterCache.Add(String.Empty, allPosts.ToArray());

            // Update the listbox
            UpdateListBox();

            OnPostsRefreshed(EventArgs.Empty);
        }

        private void UpdateListBox()
        {
            ClearList();

            if (_filterCache.ContainsKey(Filter))
            {
                foreach (PostInfoItem pii in _filterCache[Filter])
                    Items.Add(pii);
            }
            else
            {
                PostInfoItem[] resultSuperSet;

                // If this filter starts with the last one, then we only need to use those results when we do this search
                if (!string.IsNullOrEmpty(_lastFilter) && Filter.StartsWith(_lastFilter) && _filterCache.ContainsKey(_lastFilter))
                    resultSuperSet = _filterCache[_lastFilter];
                else
                    resultSuperSet = _filterCache[String.Empty];

                List<PostInfoItem> items = new List<PostInfoItem>();
                foreach (PostInfoItem pii in resultSuperSet)
                {
                    if (IsFilterMatch(pii))
                    {
                        items.Add(pii);

                    }

                }
                PostInfoItem[] itemArray = items.ToArray();
                Items.AddRange(itemArray);
                _filterCache.Add(Filter, itemArray);
                _lastFilter = Filter;
            }

            // default selection
            if (Items.Count > 0)
                SelectedIndex = 0;
            else
                ShowEmptyPostListControl();
        }

        private bool IsFilterMatch(PostInfoItem pii)
        {
            if (_filterSegments == null)
                return true;

            foreach (Regex regex in _filterSegments)
            {
                if (!regex.IsMatch(pii.SearchIndex))
                    return false;

            }
            return true;
        }

        private PostInfo[] GetRecentPostsSync(bool getPages)
        {
            try
            {
                using (new WaitCursor())
                {
                    if (getPages)
                        return PostSource.GetPages(RecentPostRequest);
                    else
                        return PostSource.GetRecentPosts(RecentPostRequest);

                }
            }
            catch (Exception ex)
            {
                DisplayableExceptionDisplayForm.Show(FindForm(), ex);
                return new PostInfo[] { };
            }
        }

        private GetRecentPostsProgressControl GetRecentPostsProgressControl
        {
            get
            {
                if (_getRecentPostsProgressControl == null)
                {
                    _getRecentPostsProgressControl = new GetRecentPostsProgressControl(this);
                    _getRecentPostsProgressControl.Initialize();
                }
                return _getRecentPostsProgressControl;
            }
        }
        private GetRecentPostsProgressControl _getRecentPostsProgressControl;

        private void ShowEmptyPostListControl()
        {
            SelectedIndex = -1;
            OnSelectedIndexChanged(EventArgs.Empty);
            if (PostSource != null && PostSource.EmptyPostListMessage != null)
            {
                EmptyPostListControl.Text = PostSource.EmptyPostListMessage;
            }
            else
            {
                EmptyPostListControl.Text = ShowPages ?
                    Res.Get(StringId.OpenPostNoPagesAvailable) :
                    Res.Get(StringId.OpenPostNoPostsAvailable);
            }
            EmptyPostListControl.BringToFront();
        }

        private void HideEmptyPostListControl()
        {
            EmptyPostListControl.SendToBack();
        }

        private Control EmptyPostListControl
        {
            get
            {
                if (_emptyPostListControl == null)
                {
                    Label label = new Label();
                    label.BackColor = BackColor;
                    label.ForeColor = ForeColor;
                    label.Text = Res.Get(StringId.OpenPostNoPostsAvailable);
                    label.FlatStyle = FlatStyle.System;
                    label.AutoSize = true;
                    label.TabStop = false;
                    Parent.Controls.Add(label);
                    CenterControlInControlBehavior centerControl = new CenterControlInControlBehavior(label, this);
                    _emptyPostListControl = label;
                }
                return _emptyPostListControl;
            }
        }
        private Control _emptyPostListControl;

        private Form _parentForm;

        #endregion

        #region Accessibility
        private BlogPostListBoxAccessibility _accessibleObject;
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (_accessibleObject == null)
                _accessibleObject = new BlogPostListBoxAccessibility(this);
            return _accessibleObject;
        }

        class PostInfoItem : AccessibleObject
        {
            private PostInfo _postInfo;
            private BlogPostListBox _listbox;
            public PostInfoItem(PostInfo postInfo, BlogPostListBox listBox)
            {
                _postInfo = postInfo;
                _listbox = listBox;

                SearchIndex = HTMLDocumentHelper.HTMLToPlainText(postInfo.Title.ToLower(CultureInfo.CurrentUICulture) + " " + postInfo.Contents.ToLower(CultureInfo.CurrentUICulture), true);
            }

            public override string ToString()
            {
                return _postInfo.Title;
            }

            public override string Value
            {
                get { return Res.Get(_postInfo.IsPage ? StringId.Page : StringId.Post); }
                set { base.Value = value; }
            }

            public override string Name
            {
                get
                {
                    string prettyDateDisplay = _postInfo.PrettyDateDisplay;
                    if (prettyDateDisplay != null && prettyDateDisplay.Length > 0)
                        return String.Format(CultureInfo.CurrentCulture, "{0},{1} {2}", HTMLDocumentHelper.HTMLToPlainText(_postInfo.Title), Res.Get(StringId.Date), prettyDateDisplay);
                    else
                        return HTMLDocumentHelper.HTMLToPlainText(_postInfo.Title);
                }
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

            public PostInfo PostInfo { get { return _postInfo; } }

            public readonly string SearchIndex;
        }

        class BlogPostListBoxAccessibility : ControlAccessibleObject
        {
            BlogPostListBox _listbox;
            public BlogPostListBoxAccessibility(BlogPostListBox ownerControl) : base(ownerControl)
            {
                _listbox = ownerControl;
            }

            public override AccessibleObject GetChild(int index)
            {
                return _listbox.Items[index] as AccessibleObject;
            }

            public override int GetChildCount()
            {
                return _listbox.Items.Count;
            }

            public override AccessibleRole Role
            {
                get { return AccessibleRole.List; }
            }
        }
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

    }

    public delegate void UserDeletedPostEventHandler(PostInfo postDeleted);
}
