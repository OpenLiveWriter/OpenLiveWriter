// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.BlogProviderButtons;
using OpenLiveWriter.PostEditor.PhotoAlbums;
using OpenLiveWriter.PostEditor.ImageInsertion.WebImages;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public class DefaultSidebarControl : SidebarControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public DefaultSidebarControl(ISidebarContext sidebarContext, IBlogPostEditingSite postEditingSite)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // record the post management context and subscribe to the post list changed event
            _postEditingSite = postEditingSite ;

            // subscribe to changes that require us to re-layout the sidebar
            _postEditingSite.WeblogChanged +=new WeblogHandler(_postEditingSite_WeblogChanged);
            _postEditingSite.WeblogSettingsChanged +=new WeblogSettingsChangedHandler(_postEditingSite_WeblogSettingsChanged);
            _postEditingSite.FrameWindow.Layout +=new LayoutEventHandler(FrameWindow_Layout);
            _postEditingSite.PostListChanged +=new EventHandler(_postEditingSite_PostListChanged);
            ContentSourceManager.GlobalContentSourceListChanged +=new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);

            // add the tooltip
            _toolTip = new ToolTip2(components);
            _toolTip.InitialDelay = 750 ;

            // set the caption
            Text = ApplicationEnvironment.ProductName;
            AccessibleName = Res.Get(StringId.SidebarPanel) ;

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // initialize components
            //_weblogPanelHeader = new PanelHeader(this, WEBLOG_PANEL_CAPTION);
            // _weblogPanelBody = new PanelBody();
            // _weblogHeader = new WeblogHeader(this, _weblogPanelBody) ;
            // _viewWeblogCommand = new LinkCommand(this, _toolTip, CommandId.ViewWeblog.ToString());
            // _viewWeblogCommand.Name = "ViewWeblog";
            // _viewWeblogAdminCommand = new LinkCommand(this, _toolTip, CommandId.ViewWeblogAdmin.ToString());
            // _viewWeblogAdminCommand.Name = "ViewWeblogAdmin";

            _headerControl = new SidebarHeaderControl();

            _openPanelHeader = new PanelHeader(this, OPEN_PANEL_CAPTION) ;
            _openPanelBody = new PanelBody();
            _draftsSectionHeader = new SectionHeader(this, DRAFTS_SECTION_CAPTION, SidebarColors.FirstSectionBottomColor );
            _draftsPostList = new DraftPostList(this, _toolTip, DRAFTS_SECTION_CAPTION);
            _draftsPostList.PostSelected +=new PostEventHandler(_draftsPostList_PostSelected);
            _draftsPostList.PostDeleteRequested +=new PostEventHandler(_draftsPostList_PostDeleteRequested);
            _openDraftCommand = new LinkCommand(this, "PostHtmlEditing.Sidebar.Images.OpenPost.png", MORE_DRAFTS_CAPTION, MORE_DRAFTS_ACCNAME, _toolTip, MORE_DRAFTS_TOOLTIP, CommandId.OpenDrafts.ToString());
            _recentPostsSectionHeader = new SectionHeader(this, RECENT_POSTS_SECTION_CAPTION, SidebarColors.SecondSectionBottomColor );
            _recentPostList = new RecentPostList(this, _toolTip, RECENT_POSTS_SECTION_CAPTION);
            _recentPostList.PostSelected +=new PostEventHandler(_recentPostList_PostSelected);
            _openPostCommand = new LinkCommand(this, "PostHtmlEditing.Sidebar.Images.OpenPost.png", MORE_POSTS_CAPTION, MORE_POSTS_ACCNAME, _toolTip, MORE_POSTS_TOOLTIP, CommandId.OpenRecentPosts.ToString());
            _insertPanelHeader = new PanelHeader(this, INSERT_PANEL_CAPTION);
            _insertPanelBody = new PanelBody();
            _insertLinkCommand = new LinkCommand(this, "PostHtmlEditing.Sidebar.Images.InsertLink.png", INSERT_LINK_CAPTION, INSERT_LINK_ACCNAME, _toolTip, INSERT_LINK_TOOLTIP, CommandId.InsertLink.ToString());
            _insertPictureCommand = new LinkCommand(this, "BlogThis.ItemTypes.Images.ImageItem.png", INSERT_PICTURE_CAPTION, INSERT_PICTURE_ACCNAME, _toolTip, INSERT_PICTURE_TOOLTIP, CommandId.InsertPictureFromFile.ToString());

            ContentSourceInfo csi = ContentSourceManager.GetContentSourceInfoById(PhotoAlbumContentSource.ID);
            if (csi != null)
            {
                _insertPhotoAlbumCommand = new LinkCommand(this, csi.Image,
                    String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), csi.InsertableContentSourceSidebarText),
                    String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), csi.InsertableContentSourceSidebarText),
                    _toolTip,
                    null,
                    PhotoAlbumContentSource.ID);
            }

            ContentSourceInfo csi2 = ContentSourceManager.GetContentSourceInfoById(WebImageContentSource.ID);
            if (csi2 != null)
            {
                _insertWebImage = new LinkCommand(this, csi2.Image,
                    String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), csi2.InsertableContentSourceSidebarText),
                    String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), csi2.InsertableContentSourceSidebarText),
                    _toolTip,
                    null,
                    WebImageContentSource.ID);
            }

            _insertTableCommand = new LinkCommand(this, "Tables.Commands.Images.CommandInsertTableCommandBarButtonBitmapEnabled.png", INSERT_TABLE_CAPTION, INSERT_TABLE_ACCNAME, _toolTip, INSERT_TABLE_TOOLTIP, CommandId.InsertTable.ToString());
            _contentInsertCommands = new ContentInsertCommands(this, _toolTip, LINK_COMMAND_PADDING, int.MaxValue);
            _separator2 = new SeparatorControl();

            Controls.Add(_separator2);
            Controls.Add(_headerControl);

            UpdatePostLists() ;
        }

        public override bool HasStatusBar
        {
            get
            {
                return false ;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            RefreshLayout(false);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                RefreshLayout();
        }

        private void RefreshLayout(bool blogChanged)
        {
            // no-op if we do not have an active weblog
            // (solves order of initialization problem)
            if (_postEditingSite.CurrentAccountId == null)
                return;

            UpdatePostLists();

            // compute text height
            int textHeight = (int) Math.Ceiling(Font.GetHeight());

            Point openPanelPoint = new Point(PANEL_HORIZONTAL_INSET, PANEL_TOP_INSET);

            _headerControl.Width = Width - 2*PANEL_HORIZONTAL_INSET;
            if (blogChanged)
            {
                using (Blog blog = new Blog(_postEditingSite.CurrentAccountId))
                {
                    _headerControl.HeaderText = blog.Name;

                    if (!string.IsNullOrEmpty(blog.ClientOptions.HomepageLinkText))
                        _headerControl.LinkText = blog.ClientOptions.HomepageLinkText;
                    else
                        _headerControl.LinkText = Res.Get(StringId.ViewWeblog);
                    _headerControl.LinkUrl = blog.HomepageUrl;

                    if (!string.IsNullOrEmpty(blog.AdminUrl))
                    {
                        if (!string.IsNullOrEmpty(blog.ClientOptions.AdminLinkText))
                            _headerControl.SecondLinkText = blog.ClientOptions.AdminLinkText;
                        else
                            _headerControl.SecondLinkText = Res.Get(StringId.ManageWeblog);
                        _headerControl.SecondLinkUrl = blog.AdminUrl;
                    }
                    else
                    {
                        _headerControl.SecondLinkText = "";
                        _headerControl.SecondLinkUrl = "";
                    }
                    _headerControl.RefreshLayout();
                }
            }
            _headerControl.Location = openPanelPoint;

            openPanelPoint.Y += _headerControl.Height;
            openPanelPoint.X -= SECONDARY_HEADER_INSET;

            int tertiaryLeft = openPanelPoint.X + TERTIARY_INSET;

            // open panel
            _openPanelHeader.Layout(openPanelPoint);

            // drafts
            _draftsSectionHeader.Layout(new Point(tertiaryLeft, _openPanelHeader.Bounds.Bottom));
            _draftsPostList.Layout(new Point(tertiaryLeft, _draftsSectionHeader.CaptionLocation.Y + textHeight));

            _openDraftCommand.Visible = ShouldShowMoreDrafts;
            _openDraftCommand.Layout(
                new Point(_draftsSectionHeader.CaptionLocation.X + OPEN_COMMAND_LEFT_OFFSET,
                          _draftsPostList.Bounds.Bottom + OPEN_COMMAND_PADDING));

            // recent posts
            _recentPostsSectionHeader.Layout(
                new Point(tertiaryLeft, _openDraftCommand.Bounds.Bottom + PANEL_SECTION_PADDING));
            _recentPostList.Layout(new Point(tertiaryLeft, _recentPostsSectionHeader.CaptionLocation.Y + textHeight));

            // commands
            _openPostCommand.Visible = ShouldShowMoreRecentPosts;
            _openPostCommand.Layout(
                new Point(_recentPostsSectionHeader.CaptionLocation.X + OPEN_COMMAND_LEFT_OFFSET,
                          _recentPostList.Bounds.Bottom + OPEN_COMMAND_PADDING));

            // body
            _openPanelBody.Layout(_openPanelHeader.Bounds, _openPostCommand.Bounds.Bottom);

            // insert panel
            Point insertHeaderPoint =
                new Point(openPanelPoint.X, _openPanelBody.Bounds.Bottom + PANEL_SECTION_PADDING);

            _separator2.Top = insertHeaderPoint.Y;
            _separator2.Left = Width/2 - _separator2.Width/2;

            insertHeaderPoint.Y += _separator2.Height + PANEL_SECTION_PADDING;

            _insertPanelHeader.Layout(insertHeaderPoint);
            Point p = new Point(insertHeaderPoint.X + LINK_COMMAND_PADDING,
                                _insertPanelHeader.Bounds.Bottom + PANEL_SECTION_PADDING_FIRST);
            foreach (LinkCommand linkCommand in new LinkCommand[] { _insertLinkCommand, _insertPictureCommand, _insertPhotoAlbumCommand, _insertWebImage, _insertTableCommand })
            {
                if (linkCommand != null)
                {
                    linkCommand.Layout(p);
                    p.Y = linkCommand.Bounds.Bottom + LINK_COMMAND_PADDING;
                }
            }
            _contentInsertCommands.Layout(p);

            Rectangle bounds = _insertPanelHeader.Bounds;
            bounds.Height += SECTION_SPACING;
            _insertPanelBody.Layout(bounds, _contentInsertCommands.Bounds.Bottom + INSERT_EXTRA_BOTTOM_PAD);

            BidiHelper.RtlLayoutFixup(this);
            MinimumSize = new Size(0, _insertPanelBody.Bounds.Bottom + INSERT_EXTRA_BOTTOM_PAD);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // alias graphics object
                BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle) ;

                _openPanelHeader.Paint(g);
                _openPanelBody.Paint(g);

                _draftsSectionHeader.Paint(g);
                _draftsPostList.Paint(g);
                if (ShouldShowMoreDrafts)
                    _openDraftCommand.Paint(g);

                _recentPostsSectionHeader.Paint(g);
                _recentPostList.Paint(g);
                if (ShouldShowMoreRecentPosts)
                    _openPostCommand.Paint(g);

                _insertPanelHeader.Paint(g);
                _insertPanelBody.Paint(g);

                _insertLinkCommand.Paint(g);
                _insertPictureCommand.Paint(g);
                if (_insertPhotoAlbumCommand != null)
                    _insertPhotoAlbumCommand.Paint(g);
                if (_insertWebImage != null)
                    _insertWebImage.Paint(g);
                _insertTableCommand.Paint(g);
                _contentInsertCommands.Paint(g);

            }
            catch(Exception ex)
            {
                Trace.Fail("Unexpected exception: " + ex.ToString()) ;
            }
        }

        private void _postEditingSite_WeblogChanged(string blogId)
        {
            RefreshLayout(true);
        }

        private void _postEditingSite_WeblogSettingsChanged(string blogId, bool templateChanged)
        {
            RefreshLayout(true);
        }

        private void _postEditingSite_PostListChanged(object sender, EventArgs e)
        {
            RefreshLayout();
        }

        private void ContentSourceManager_GlobalContentSourceListChanged(object sender, EventArgs e)
        {
            if ( ControlHelper.ControlCanHandleInvoke(this) )
                BeginInvoke(new InvokeInUIThreadDelegate(HandleContentSourceListChanged)) ;
        }

        private void HandleContentSourceListChanged()
        {
            _contentInsertCommands.RefreshContentSourceCommands();
            RefreshLayout() ;
        }

        private void _draftsPostList_PostSelected(PostInfo post)
        {
            WindowCascadeHelper.SetNextOpenedLocation(_postEditingSite.FrameWindow.Location);
            _postEditingSite.OpenLocalPost(post);
        }

        private void _draftsPostList_PostDeleteRequested(PostInfo post)
        {
            _postEditingSite.DeleteLocalPost(post) ;
            UpdatePostLists() ;
        }

        private void FrameWindow_Layout(object sender, EventArgs e)
        {
        }

        private void _recentPostList_PostSelected(PostInfo post)
        {
            WindowCascadeHelper.SetNextOpenedLocation(_postEditingSite.FrameWindow.Location);
            _postEditingSite.OpenLocalPost(post);
        }

        private void UpdatePostLists()
        {
            PostInfo[] drafts = PostListCache.Drafts;
            PostInfo[] recentPosts = PostListCache.RecentPosts;
            _draftsPostList.SetPosts(drafts);
            _recentPostList.SetPosts(recentPosts);
            _shouldShowMoreDrafts = drafts.Length > PostList.MAX_POSTS;
            _shouldShowMoreRecentPosts = recentPosts.Length > PostList.MAX_POSTS;
        }

        private bool _shouldShowMoreDrafts;
        private bool _shouldShowMoreRecentPosts;

        private bool ShouldShowMoreRecentPosts
        {
            get { return _shouldShowMoreRecentPosts; }
        }

        private bool ShouldShowMoreDrafts
        {
            get { return _shouldShowMoreDrafts; }
        }

//		private PanelHeader _weblogPanelHeader;
        // private WeblogHeader _weblogHeader ;
        // private PanelBody _weblogPanelBody ;
        // private LinkCommand _viewWeblogCommand ;
        // private LinkCommand _viewWeblogAdminCommand ;
        // private BlogProviderButtonCommandBarControl _blogProviderButtonCommandBar ;
        private SidebarHeaderControl _headerControl;
        private PanelHeader _openPanelHeader;
        private PanelBody _openPanelBody;
        private SectionHeader _draftsSectionHeader ;
        private PostList _draftsPostList ;
        private SectionHeader _recentPostsSectionHeader ;
        private PostList _recentPostList ;
        private LinkCommand _openDraftCommand ;
        private LinkCommand _openPostCommand ;
        private PanelHeader _insertPanelHeader ;
        private PanelBody _insertPanelBody;
        private LinkCommand _insertLinkCommand ;
        private LinkCommand  _insertPictureCommand ;
        private LinkCommand  _insertPhotoAlbumCommand ;
        private LinkCommand _insertTableCommand ;
        private LinkCommand _insertWebImage;
        private ContentInsertCommands _contentInsertCommands ;
        private SeparatorControl _separator2;

        private ToolTip2 _toolTip ;
        private IBlogPostEditingSite _postEditingSite ;

        private const int PANEL_TOP_INSET = 2 ;
        internal const int PANEL_HORIZONTAL_INSET = 10 ;
        private const int PANEL_SECTION_PADDING = 10;
        private const int PANEL_SECTION_PADDING_FIRST = 1;
        internal const int LINK_COMMAND_PADDING = 5 ;
        private const int INSERT_EXTRA_BOTTOM_PAD = 0 ;
        private const int OPEN_COMMAND_PADDING = 3 ;
        private const int OPEN_COMMAND_LEFT_OFFSET = 20 ;
        private const int SECTION_SPACING = 8;
        private const int SECONDARY_HEADER_INSET = 3;
        private const int TERTIARY_INSET = 3;

        private string WEBLOG_PANEL_CAPTION = Res.Get(StringId.Weblog) ;
        private string OPEN_PANEL_CAPTION = Res.Get(StringId.Open) ;
        private string DRAFTS_SECTION_CAPTION = Res.Get(StringId.Drafts) ;
        private string RECENT_POSTS_SECTION_CAPTION = Res.Get(StringId.RecentlyPosted) ;
        private string MORE_DRAFTS_CAPTION = Res.Get(StringId.MoreDotDotDot) ;
        private string MORE_DRAFTS_ACCNAME = Res.Get(StringId.MoreDrafts) ;
        private string MORE_DRAFTS_TOOLTIP = Res.Get(StringId.MoreDraftsTooltip) ;
        private string MORE_POSTS_CAPTION = Res.Get(StringId.MoreDotDotDot) ;
        private string MORE_POSTS_ACCNAME = Res.Get(StringId.MorePosts) ;
        private string MORE_POSTS_TOOLTIP = Res.Get(StringId.MorePostsTooltip) ;
        private string INSERT_PANEL_CAPTION = Res.Get(StringId.Insert) ;
        private string INSERT_LINK_CAPTION = Res.Get(StringId.InsertLinkDotDotDot) ;
        private string INSERT_LINK_ACCNAME = Res.Get(StringId.InsertLink) ;
        private string INSERT_LINK_TOOLTIP = Res.Get(StringId.InsertLinkTooltip) ;
        private string INSERT_PICTURE_CAPTION = Res.Get(StringId.InsertPictureDotDotDot) ;
        private string INSERT_PICTURE_ACCNAME = Res.Get(StringId.InsertPicture) ;
        private string INSERT_PICTURE_TOOLTIP = Res.Get(StringId.InsertPictureTooltip) ;
        private string INSERT_TABLE_CAPTION = Res.Get(StringId.InsertTableDotDotDot) ;
        private string INSERT_TABLE_ACCNAME = Res.Get(StringId.InsertTable) ;
        private string INSERT_TABLE_TOOLTIP = Res.Get(StringId.InsertTableTooltip) ;

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

                _postEditingSite.WeblogChanged -= new WeblogHandler(_postEditingSite_WeblogChanged);
                _postEditingSite.WeblogSettingsChanged -= new WeblogSettingsChangedHandler(_postEditingSite_WeblogSettingsChanged);
                _postEditingSite.FrameWindow.Layout -= new LayoutEventHandler(FrameWindow_Layout);
                _postEditingSite.PostListChanged -= new EventHandler(_postEditingSite_PostListChanged);
                ContentSourceManager.GlobalContentSourceListChanged -= new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);

                _openPanelHeader.Dispose();
                _insertPanelHeader.Dispose();
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
            components = new System.ComponentModel.Container();
        }
        #endregion

    }

    internal class ContentInsertCommands
    {

        public ContentInsertCommands(Control parent, ToolTip parentToolTip, int linkCommandPadding, int maxCommands)
        {
            // save reference to parent and command padding
            _parent = parent ;
            _linkCommandPadding = linkCommandPadding ;
            _maxCommands = maxCommands ;
            _parentToolTip = parentToolTip ;

            // update list of content-source commands
            RefreshContentSourceCommands() ;
        }

        public void RefreshContentSourceCommands()
        {
            _parent.SuspendLayout() ;
            try
            {
                // clear existing commands
                foreach ( LinkCommand linkCommand in _contentSourceCommands )
                    linkCommand.Dispose() ;
                _contentSourceCommands.Clear();

                if ( _addPluginCommand != null )
                {
                    _addPluginCommand.Dispose();
                    _addPluginCommand = null ;
                }

                // get the commands
                ContentSourceInfo[] contentSources = GetSidebarContentSources(_maxCommands) ;

                // add a link comment for each source
                foreach (ContentSourceInfo contentSourceInfo in contentSources)
                {
                    if (contentSourceInfo.Id == PhotoAlbumContentSource.ID || contentSourceInfo.Id == WebImageContentSource.ID)
                        continue;

                    LinkCommand linkCommand = new LinkCommand(
                        _parent,
                        contentSourceInfo.Image,
                        String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), contentSourceInfo.InsertableContentSourceSidebarText),
                        String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarText), contentSourceInfo.InsertableContentSourceSidebarText),
                        _parentToolTip,
                        null, //String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PluginInsertSidebarTooltip), contentSourceInfo.InsertableContentSourceMenuText),
                        contentSourceInfo.Id ) ;

                    _contentSourceCommands.Add(linkCommand) ;
                }

                // create add-plugin command
                if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.WLGallery))
                {
                    Command addPluginCommand = ApplicationManager.CommandManager.Get(CommandId.AddPlugin) ;
                    _addPluginCommand = new LinkCommand(_parent, _parentToolTip, addPluginCommand.Identifier);
                }
            }
            finally
            {
                _parent.ResumeLayout();
            }
        }

        public Rectangle Bounds = new Rectangle();

        public void Layout(Point startLocation)
        {
            Bounds.Location = startLocation ;
            Bounds.Height = startLocation.Y - Bounds.Location.Y;

            int floor = int.MaxValue; //_parent.ClientSize.Height - 50;

            for (int i = 0; i < _contentSourceCommands.Count; i++)
            {
                LinkCommand linkCommand = (LinkCommand) _contentSourceCommands[i];

                if (startLocation.Y > floor)
                {
                    for (int j = i; j < _contentSourceCommands.Count; j++)
                        ((LinkCommand)_contentSourceCommands[j]).Visible = false;
                    break;
                }

                _laidOutContentCommands = true ;

                linkCommand.Visible = true;
                // layout the link command
                linkCommand.Layout(startLocation);

                // update start location for next iteration
                startLocation = new Point(startLocation.X,  linkCommand.Bounds.Bottom + _linkCommandPadding);

                // update bounds
                Bounds.Width = linkCommand.Bounds.Width ;
                Bounds.Height = startLocation.Y - Bounds.Location.Y ;
            }

            if ( _laidOutContentCommands && MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.WLGallery))
            {
                // add the "add a plugin" command and update height
                _addPluginCommand.Layout(new Point(startLocation.X, startLocation.Y + ADD_PLUGIN_PAD));
                Bounds.Height = _addPluginCommand.Bounds.Bottom - Bounds.Location.Y - _linkCommandPadding + 2;
            }
        }
        private const int ADD_PLUGIN_PAD = 3 ;
        private bool _laidOutContentCommands = false ;

        public void Paint(BidiGraphics g)
        {
            foreach ( LinkCommand linkCommand in _contentSourceCommands )
                linkCommand.Paint(g);

            if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.WLGallery))
            {
                int y = _addPluginCommand.Bounds.Y - ADD_PLUGIN_PAD ;
                using ( Pen pen = PanelBody.CreateBorderPen() )
                    g.DrawLine(pen, _addPluginCommand.Bounds.X, y, _parent.Right - _addPluginCommand.Bounds.X, y);

                _addPluginCommand.Paint(g);
            }
        }

        private ContentSourceInfo[] GetSidebarContentSources(int maxSources)
        {
            ArrayList sidebarContentSources = new ArrayList();
            if ( maxSources <= ContentSourceManager.BuiltInInsertableContentSources.Length )
            {
                // clip the list of standard spources if necessary
                for( int i=0; i<maxSources; i++ )
                    sidebarContentSources.Add(ContentSourceManager.BuiltInInsertableContentSources[i]);
            }
            else
            {
                // add all of the standard sources
                sidebarContentSources.AddRange(ContentSourceManager.BuiltInInsertableContentSources);

                // if we have to clip the list of plugin sources then do it based
                // on which sources have been most recently used
                int sourcesLeft = maxSources - sidebarContentSources.Count ;
                if ( sourcesLeft < ContentSourceManager.PluginInsertableContentSources.Length )
                {
                    // get the list that we actually want to include
                    ArrayList filteredPluginContentSources = new ArrayList(ContentSourceManager.PluginInsertableContentSources);
                    filteredPluginContentSources.Sort(new ContentSourceInfo.LastUseComparer());
                    filteredPluginContentSources.RemoveRange(sourcesLeft, filteredPluginContentSources.Count-sourcesLeft);

                    // iterate through the parent list and add the ones that made the cut
                    foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.PluginInsertableContentSources)
                        if ( filteredPluginContentSources.Contains(contentSourceInfo))
                            sidebarContentSources.Add(contentSourceInfo);
                }
                else
                {
                    sidebarContentSources.AddRange(ContentSourceManager.PluginInsertableContentSources);
                }
            }

            return sidebarContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[] ;
        }

        private Control _parent ;
        private int _linkCommandPadding ;
        private ToolTip _parentToolTip ;
        private int _maxCommands ;
        private ArrayList _contentSourceCommands = new ArrayList();
        private LinkCommand _addPluginCommand ;

    }

    internal class SidebarLinkLabel : LinkLabel
    {
        UITheme _uiTheme;
        public SidebarLinkLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent ;
            FlatStyle = FlatStyle.System ;
            Cursor = Cursors.Hand ;
            UseMnemonic = false ;
            AutoEllipsis = true ;
            LinkColor = SystemColors.HotTrack;
            ActiveLinkColor = SystemColors.HotTrack;

            _uiTheme = new UITheme(this);
        }

        class UITheme : ControlUITheme
        {
            SidebarLinkLabel _linkLabel;
            public UITheme(SidebarLinkLabel control) : base(control, false)
            {
                _linkLabel = control;
                ApplyTheme();
            }

            protected override void ApplyTheme(bool highContrast)
            {
                _linkLabel.LinkColor = !highContrast ? SystemColors.HotTrack : SystemColors.ControlText;
                _linkLabel.LinkBehavior = !highContrast ? LinkBehavior.HoverUnderline : LinkBehavior.SystemDefault;
                base.ApplyTheme(highContrast);
            }
        }
    }

    internal class Header
    {
        protected Header(Control parent)
        {
            _parent = parent;
        }

        public Rectangle Bounds ;
        public string Caption  ;
        public Rectangle CaptionBounds ;

        public Point CaptionLocation { get { return CaptionBounds.Location; } }
        protected Control Parent { get { return _parent; }}
        private Control _parent ;
    }

    internal class WeblogHeader : Header
    {
        public WeblogHeader(Control parent, PanelBody weblogPanelBody)
            : base(parent)
        {
            Caption = String.Empty ;
            _weblogPanelBody = weblogPanelBody ;
        }

        public Icon Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
        private Icon _icon = null ;

        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }
        private Image _image = null ;

        public Image WatermarkImage
        {
            get { return _watermarkImage; }
            set { _watermarkImage = value; }
        }
        private Image _watermarkImage = null ;

        public void Layout(Point startLocation)
        {
            InitFont();

            using ( Graphics g = Parent.CreateGraphics() )
            {
                // deterine room occcupied by icon
                const int ICON_PADDING = 2 ;
                int iconWidth = 0 ;
                if ( Image != null )
                    iconWidth = Image.Width + ICON_PADDING ;
                else if ( Icon != null )
                    iconWidth = Icon.Width + ICON_PADDING ;

                // compute widths
                int width = Parent.Width - (2*startLocation.X) - 1 ;
                int textWidth = width - iconWidth ;

                // compute height
                BidiGraphics bg = new BidiGraphics(g, Size.Empty);
                Size captionSize = bg.MeasureText(Caption, _captionFont, new Size(textWidth, 0), 0);
                int textHeight = Convert.ToInt32(Math.Min(captionSize.Height, _captionFont.GetHeight() * 3)) ;

                Point captionLocation = new Point(startLocation.X + iconWidth, startLocation.Y+1) ;

                CaptionBounds = new Rectangle(captionLocation, new Size(textWidth, textHeight));

                Bounds = new Rectangle(startLocation, new Size(width, CaptionBounds.Height + (SectionHeader.SECTION_CAPTION_TOP_INSET+SectionHeader.SECTION_CAPTION_BOTTOM_INSET)));

            }
        }

        public void Paint(BidiGraphics g)
        {
            // draw icon
            if ( Image != null )
                g.DrawImage(false, Image, Bounds.X, Bounds.Y) ;
            else if ( Icon != null )
                g.DrawIcon(false, Icon, Bounds.X, Bounds.Y);

            // draw text
            g.DrawText(Caption, _captionFont, new Rectangle(CaptionBounds.X, CaptionBounds.Y, CaptionBounds.Width, CaptionBounds.Height), SystemColors.ControlText, TextFormatFlags.WordEllipsis );
        }

        public void PaintBackground(BidiGraphics g)
        {
            // draw watermark if we have one
            if ( WatermarkImage != null && !SystemInformation.HighContrast)
            {

                Point watermarkLocation = new Point(_weblogPanelBody.Bounds.Right - WatermarkImage.Width  , _weblogPanelBody.Bounds.Top  ) ;

                GraphicsContainer gc = g.Graphics.BeginContainer() ;
                try
                {
                    Rectangle clipRegion = _weblogPanelBody.Bounds ;
                    clipRegion.Width -= 1 ;
                    clipRegion.Height -= 1 ;
                    g.IntersectClip(clipRegion);
                    g.DrawImage(false, WatermarkImage, watermarkLocation.X, watermarkLocation.Y);
                }
                finally
                {
                    g.Graphics.EndContainer(gc);
                }

            }
        }

        public void Dispose()
        {
            if ( _captionFont != null )
                _captionFont.Dispose();
        }

        private void InitFont()
        {
            if ( _captionFont == null )
                _captionFont = new Font(Parent.Font.FontFamily, Parent.Font.SizeInPoints+1);
        }

        private Font _captionFont ;
        private PanelBody _weblogPanelBody ;

    }

    internal class PanelHeader : Header, IDisposable
    {
        public PanelHeader(Control parent, string caption)
            : base(parent)
        {
            Caption = caption ;
        }

        public void Layout(Point startLocation)
        {
            InitFont() ;
            using ( Graphics g = Parent.CreateGraphics() )
            {
                BidiGraphics bg = new BidiGraphics(g, Size.Empty);
                Size stringSize = bg.MeasureText(Caption, _captionFont);
                int textHeight = stringSize.Height;
                Point captionLocation = startLocation ;
                captionLocation.Offset(PANEL_CAPTION_LEFT_INSET, PANEL_CAPTION_TOP_INSET);
                CaptionBounds = new Rectangle(captionLocation, stringSize);
                Bounds = new Rectangle(startLocation, new Size(Parent.Width-startLocation.X*2-1, textHeight + (PANEL_CAPTION_TOP_INSET+PANEL_CAPTION_BOTTOM_INSET))) ;
            }
        }

        public void Paint(BidiGraphics g)
        {
            ColorizedResources colRes = ColorizedResources.Instance;
            g.DrawText(Caption, _captionFont, CaptionBounds, colRes.SidebarHeaderTextColor);
        }

        public void Dispose()
        {
            if ( _captionFont != null )
                _captionFont.Dispose();
        }

        private void InitFont()
        {
            if ( _captionFont == null )
                _captionFont = Res.GetFont(FontSize.Large, FontStyle.Regular);
        }

        private Font _captionFont ;

        internal const int PANEL_CAPTION_TOP_INSET = 1 ;
        internal const int PANEL_CAPTION_BOTTOM_INSET = 2 ;
        internal const int PANEL_CAPTION_LEFT_INSET = 0 ;
        internal const int PANEL_CAPTION_BORDER_Y_OFFSET = 8;
    }

    internal class PanelBody
    {
        public Rectangle Bounds ;

        public void Layout(Rectangle headerRect, int bottom)
        {
            Bounds = new Rectangle(headerRect.Left, headerRect.Bottom, headerRect.Width, (bottom+BODY_BOTTOM_PAD) - headerRect.Bottom) ;
        }

        public void Paint(BidiGraphics g)
        {
            ColorizedResources colRes = ColorizedResources.Instance;

            if(!ColorizedResources.UseSystemColors)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(64, Color.White)))
                    g.FillRectangle(b, Bounds );

                using (Pen p = CreateBorderPen())
                {
                    g.DrawLine(p, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1); // left
                    g.DrawLine(p, Bounds.Right, Bounds.Top, Bounds.Right, Bounds.Bottom - 1); // left
                    g.DrawLine(p, Bounds.Left + 1, Bounds.Bottom, Bounds.Right - 1, Bounds.Bottom);
                }
            }
        }

        public static Pen CreateBorderPen()
        {
            return new Pen(Color.FromArgb(128, ColorizedResources.Instance.SidebarHeaderBackgroundColor), 1f);
        }

        private const int BODY_BOTTOM_PAD = 8 ;
    }

    internal class SectionHeader : Header
    {
        public SectionHeader(Control parent, string caption, Color endColor)
            : base(parent)
        {
            Caption = caption ;
            _endColor = endColor ;
            _uiTheme = new UITheme(parent, endColor);
        }

        public void Layout(Point startLocation)
        {
            _font = Parent.Font ;
            using ( Graphics g = Parent.CreateGraphics() )
            {
                SizeF captionSize = TextRenderer.MeasureText(g, Caption, _font);
                int textHeight = Convert.ToInt32(captionSize.Height) ;
                Point captionLocation = startLocation ;
                captionLocation.Offset( SECTION_CAPTION_LEFT_INSET, SECTION_CAPTION_TOP_INSET) ;
                CaptionBounds = new Rectangle(captionLocation, new Size((int) captionSize.Width, (int) captionSize.Height));
                Bounds = new Rectangle(startLocation, new Size(Parent.Width - 20, textHeight + (SECTION_CAPTION_TOP_INSET+SECTION_CAPTION_BOTTOM_INSET)));
            }
        }

        public void Paint(BidiGraphics g)
        {
            Rectangle rectangle = CaptionBounds;
            g.DrawText(Caption, _font, rectangle, SystemColors.ControlText, TextFormatFlags.VerticalCenter);
        }

        private class UITheme : ControlUITheme
        {
            private Color _defaultEndColor;
            private Color _endColor;
            public Color TextColor;
            public bool DrawGradient;
            public UITheme(Control control, Color endColor) : base(control, true)
            {
                _defaultEndColor = endColor;
            }

            protected override void ApplyTheme(bool highContrast)
            {
                DrawGradient = !highContrast;
                if(highContrast)
                {
                    _endColor = SystemColors.Control;
                    TextColor = SystemColors.ControlText;
                }
                else
                {
                    _endColor = _defaultEndColor;
                    TextColor = Color.White;
                }
                base.ApplyTheme(highContrast);
            }

            public Color EndColor
            {
                get { return _endColor; }
            }
        }

        private UITheme _uiTheme;
        private Font _font ;
        private Color _endColor ;

        internal const int SECTION_CAPTION_LEFT_INSET = -1 ;
        internal const int SECTION_CAPTION_TOP_INSET = 0 ;
        internal const int SECTION_CAPTION_BOTTOM_INSET = 0 ;
    }


    internal class LinkCommand
    {
        public LinkCommand(Control parent, string imageName, string caption, string accName, ToolTip parentToolTip, string toolTipText, string commandIdentifier)
            : this(parent, ResourceHelper.LoadAssemblyResourceBitmap(imageName), caption, accName, parentToolTip, toolTipText, commandIdentifier)
        {
        }

        public LinkCommand(Control parent, Image image, string caption, string accName, ToolTip parentToolTip, string toolTipText, string commandIdentifier)
        {
            _parent = parent ;
            _parentToolTip = parentToolTip ;

            Image = image ;
            Caption = caption ;
            TooltipText = toolTipText ;

            _linkLabel.LinkClicked +=new LinkLabelLinkClickedEventHandler(_linkLabel_LinkClicked);
            _commandIdentifier = commandIdentifier ;
            if (accName != null)
            {
                _linkLabel.AccessibleName = accName;
            }
            if(toolTipText != null )
                _linkLabel.AccessibleDescription = toolTipText;

            _linkLabel.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
        }

        public LinkCommand(Control parent, ToolTip parentToolTip, string commandIdentifier)
            : this(parent, (Image)null, null, null, parentToolTip, null, commandIdentifier)
        {
        }

        public void Dispose()
        {
            _linkLabel.AccessibleName = null ;
            _linkLabel.AccessibleDescription = null ;
            _linkLabel.LinkClicked -=new LinkLabelLinkClickedEventHandler(_linkLabel_LinkClicked);
            TooltipText = null ;
            Caption = null ;
            if ( _parent.Controls.Contains(_linkLabel) )
                _parent.Controls.Remove(_linkLabel);
            Visible = false ;
            _linkLabel.Dispose();
        }

        public Rectangle Bounds = new Rectangle();

        public string Name
        {
            get { return _linkLabel.Name; }
            set { _linkLabel.Name = value; }
        }

        public bool ShowImage
        {
            get { return _showImage; }
            set { _showImage = value; }
        }
        private bool _showImage = true ;

        public Image Image
        {
            get { return _image ; }
            set { _image = value; }
        }

        public string Caption
        {
            get { return _linkLabel.Text; }
            set { _linkLabel.Text = value; }
        }

        public void UseCommandText()
        {
            Caption = Command.Text ;
            _linkLabel.AccessibleName = Command.Text ;
            _linkLabel.AccessibleDescription = Command.Text ;
            TooltipText = Command.Text ;
        }

        public string TooltipText
        {
            set { _parentToolTip.SetToolTip(_linkLabel, value);}
        }

        public void Layout(Point startLocation)
        {
            if (!_visible)
            {
                Bounds.Location = startLocation ;
                Bounds.Size = Size.Empty;
                return;
            }

            // ensure the link label is added to the parent
            if ( _linkLabel.Parent == null )
            {
                _parent.Controls.Add(_linkLabel);
            }

            // take defaults from command if necessary
            if ( Image == null )
                Image = Command.CommandBarButtonBitmapEnabled ;
            if ( Caption == String.Empty )
                UseCommandText() ;

            int xOffset = ShowImage ? IMAGE_WIDTH + LINK_LEFT_INSET : 0 ;

            // see how wide the label needs to be
            int labelWidth ;
            int labelHeight;
            using ( Graphics g = _parent.CreateGraphics() )
            {
                Size textSize = TextRenderer.MeasureText(g, Caption, _linkLabel.Font);
                labelWidth = textSize.Width + 5;
                labelWidth = Math.Min(labelWidth, _parent.Width - startLocation.X - xOffset - LINK_LABEL_RIGHT_INSET ) ;
                labelHeight = textSize.Height;
            }

            labelHeight = Math.Max(IMAGE_HEIGHT, labelHeight);

            // calculate the bounds
            Bounds.Location = startLocation ;
            Bounds.Size = new Size(xOffset + labelWidth, labelHeight) ;

            // position the link-label
            _linkLabel.Location = new Point(startLocation.X + xOffset, startLocation.Y + LINK_TOP_INSET);
            _linkLabel.Height = labelHeight ;
            _linkLabel.Width = labelWidth;
        }

        public void Paint(BidiGraphics g)
        {
            if (_visible && ShowImage)
            {
                if (Image != null)
                {
                    int offset = (Bounds.Height - _image.Height)/2;
                    int y = (_linkLabel.Location.Y + _linkLabel.Height/2) - (_image.Height/2);
                    g.DrawImage(false, _image, Bounds.Location.X, y );
                }

            }
        }

        private bool _visible = true;
        public bool Visible
        {
            get
            {
                return _visible ;
            }
            set
            {
                _visible = value;
                _linkLabel.Visible = _visible;

            }
        }

        private void _linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ( Command.Enabled )
                Command.PerformExecute();
        }

        private Command Command
        {
            get
            {
                if ( _command == null )
                {
                    _command = ApplicationManager.CommandManager.Get(_commandIdentifier) ;
                    if ( _command == null )
                    {
                        Trace.Fail("Unexpected null Command for " + _commandIdentifier);
                    }
                }
                return _command ;
            }
        }

        private Control _parent ;
        private ToolTip _parentToolTip ;
        private Image _image ;
        private Command _command ;
        private string _commandIdentifier ;
        private SidebarLinkLabel _linkLabel = new SidebarLinkLabel();
        private const int LINK_LEFT_INSET = 1 ;
        private const int LINK_TOP_INSET = 2 ;
        private const int LINK_LABEL_RIGHT_INSET = 10 ;
        private const int IMAGE_WIDTH = 16 ;
        private const int IMAGE_HEIGHT = 16 ;
    }

    internal delegate void PostEventHandler(PostInfo post) ;

    internal abstract class PostList
    {
        public PostList(Control parent, ToolTip toolTip, string title)
        {
            _parent = parent ;
            _toolTip = toolTip ;
            _title = title ;
        }

        public void SetPosts(PostInfo[] posts)
        {
            // copy posts into link labels
            for ( int i=0; i<MAX_POSTS; i++)
            {
                if ( posts.Length > i )
                {
                    string postType = posts[i].IsPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post);
                    LinkLabels[i].AccessibleName = string.Format(CultureInfo.CurrentCulture, AccessibilityNameFormat, postType);
                    DeleteButtons[i].AccessibleName = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DeleteSomething), LinkLabels[i].AccessibleName);

                    LinkLabels[i].Text = posts[i].Title ;
                    string tooltipText = FormatToolTipText(posts[i]);
                    LinkLabels[i].AccessibleDescription = tooltipText;
                    LinkLabels[i].LinkColor = ColorizedResources.Instance.SidebarLinkColor;
                    _toolTip.SetToolTip(LinkLabels[i], tooltipText);
                    LinkLabels[i].Tag = posts[i] ;
                    LinkLabels[i].Visible = true ;
                    _toolTip.SetToolTip(DeleteButtons[i], DeleteButtonToolTip);
                    DeleteButtons[i].Tag = posts[i] ;
                    DeleteButtons[i].Visible = true ;
                    DeleteButtons[i].AccessibleDescription = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DeleteSomething), posts[i].Title);
                }
                else
                {
                    LinkLabels[i].Text = String.Empty ;
                    LinkLabels[i].Tag = null ;
                    LinkLabels[i].Visible = false ;
                    DeleteButtons[i].Tag = null ;
                    DeleteButtons[i].Visible = false ;
                }
            }

            // note whether we have posts to display
            _havePostsToDisplay = posts.Length > 0 ;
        }

        public event PostEventHandler PostSelected ;

        public event PostEventHandler PostDeleteRequested ;

        public Rectangle Bounds = new Rectangle();

        private const int POSTLIST_INDENT = 15;

        public void Layout(Point startLocation)
        {
            // make sure the link labels have a parent
            Init() ;

            // initialize the bounds
            Bounds.Location = startLocation ;
            Bounds.Size = new Size( _parent.Width - startLocation.X, 0);

            if ( _havePostsToDisplay )
            {
                // position the visible link labels
                int currentLocationY = startLocation.Y + LINK_TEXT_PADDING;
                for (int i=0; i<MAX_POSTS; i++)
                {
                    SidebarLinkLabel linkLabel = LinkLabels[i] ;
                    if ( linkLabel.Visible )
                    {
                        // position the label
                        linkLabel.Location = new Point(startLocation.X + POSTLIST_INDENT, currentLocationY);
                        linkLabel.Width = _parent.Width - linkLabel.Left - LinkLabelRightInset ;

                        // see how much space the text will take up and size the link-label
                        // accordingly
                        using ( Graphics g = _parent.CreateGraphics() )
                        {
                            Size labelTextSize = TextRenderer.MeasureText(g, linkLabel.Text, linkLabel.Font, new Size(linkLabel.Width, 0), TextFormatFlags.WordBreak) ;
                            linkLabel.Height = Math.Min(labelTextSize.Height, _textHeight*2);
                            linkLabel.Width = Math.Min(labelTextSize.Width + 5, linkLabel.Width)  ;
                        }

                        // position the delete button if necessary
                        if ( SupportsDelete )
                        {
                            DeleteButton deleteButton = DeleteButtons[i] ;
                            deleteButton.Top = linkLabel.Top + DELETE_TOP_PADDING ;
                            deleteButton.Left = _parent.Width - LinkLabelRightInset ;
                        }

                        // increment Y-axis
                        currentLocationY += (linkLabel.Height+LINK_TEXT_PADDING) ;

                        // update the bounds
                        Bounds.Size = new Size(Bounds.Size.Width, linkLabel.Bottom - startLocation.Y );
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                // will display the "no posts" message
                Bounds.Size = new Size(Bounds.Size.Width,LINK_TEXT_PADDING + _textHeight) ;
            }
        }

        private const int DELETE_TOP_PADDING = 2;

        public void Paint(BidiGraphics g)
        {
            if ( !_havePostsToDisplay )
            {
                string noPosts = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.NoThings), _title ) ;
                Size size = g.MeasureText(noPosts, _font);
                g.DrawText(noPosts, _font,
                    new Rectangle(Bounds.Location.X + POSTLIST_INDENT, Bounds.Location.Y + LINK_TEXT_PADDING, size.Width, size.Height),
                    ColorizedResources.Instance.SidebarDisabledTextColor);
            }
        }

        private void Init()
        {
            // save the font
            _font = Res.DefaultFont;

            // compute text height
            _textHeight = (int) Math.Ceiling(_font.GetHeight());

            // make sure link labels and buttons are parented
            for ( int i=0; i<MAX_POSTS; i++)
            {
                if ( LinkLabels[i].Parent == null )
                    _parent.Controls.Add(LinkLabels[i]);

                if (SupportsDelete)
                {
                    if ( DeleteButtons[i].Parent == null )
                        _parent.Controls.Add(DeleteButtons[i]);
                }
            }

        }

        private string FormatToolTipText(PostInfo post)
        {
            if (post.BlogName != string.Empty)
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PostLinkTooltipFormat), post.Title, post.BlogName, FormatDateString(post)) ;
            else
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PostLinkTooltipFormatNoBlogTitle), post.Title, FormatDateString(post)) ;
        }

        private string FormatDateString(PostInfo post)
        {
            if ( post.DateModifiedSpecified )
            {
                return post.PrettyDateDisplay ;
            }
            else
            {
                return String.Empty ;
            }

        }

        protected virtual bool SupportsDelete { get { return false; } }

        protected virtual string DeleteButtonToolTip { get { return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DeleteSomething), Res.Get(StringId.Post)); } }

        protected abstract string AccessibilityNameFormat { get; }

        private SidebarLinkLabel[] LinkLabels
        {
            get
            {
                if ( _linkLabels == null )
                {
                    _linkLabels = new SidebarLinkLabel[MAX_POSTS];
                    for ( int i=0; i<MAX_POSTS; i++)
                    {
                        _linkLabels[i] = new SidebarLinkLabel();
                        _linkLabels[i].Visible = false ;
                        _linkLabels[i].LinkClicked +=new LinkLabelLinkClickedEventHandler(PostList_LinkClicked);
                    }
                }
                return _linkLabels ;
            }
        }

        private DeleteButton[] DeleteButtons
        {
            get
            {
                if ( _deleteButtons == null )
                {
                    _deleteButtons = new DeleteButton[MAX_POSTS];
                    for ( int i=0; i<MAX_POSTS; i++)
                    {
                        _deleteButtons[i] = new DeleteButton();
                        _deleteButtons[i].Visible = false ;
                        _deleteButtons[i].Click +=new EventHandler(DeleteButton_Click);
                    }

                }
                return _deleteButtons ;
            }
        }

        private void PostList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PostInfo postInfo = (sender as SidebarLinkLabel).Tag as PostInfo ;
            if ( postInfo != null )
            {
                if ( PostSelected != null )
                    PostSelected(postInfo) ;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            PostInfo postInfo = (sender as DeleteButton).Tag as PostInfo ;
            if ( postInfo != null )
            {
                if ( PostDeleteRequested != null )
                    PostDeleteRequested(postInfo) ;
            }
        }

        private int LinkLabelRightInset
        {
            get
            {
                if ( SupportsDelete )
                    return DeleteButtons[0].Width + LINK_LABEL_RIGHT_INSET ;
                else
                    return LINK_LABEL_RIGHT_INSET ;
            }
        }

        private Control _parent ;
        private ToolTip _toolTip ;
        private int _textHeight ;
        private Font _font ;
        private string _title ;
        private bool _havePostsToDisplay ;
        private SidebarLinkLabel[] _linkLabels ;
        private DeleteButton[] _deleteButtons ;
        internal const int MAX_POSTS = 3 ;
        internal const int LINK_TEXT_PADDING = 4 ;
        private const int LINK_LABEL_RIGHT_INSET = 10 ;

        private class DeleteButton : BitmapButton
        {
            public DeleteButton()
            {
                BackColor = Color.Transparent ;
                ButtonStyle = ButtonStyle.Bitmap ;
                BitmapEnabled = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "PostHtmlEditing.Sidebar.Images.{0}Enabled.png", DELETE_DRAFT)) ;
                BitmapDisabled = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "PostHtmlEditing.Sidebar.Images.{0}Disabled.png", DELETE_DRAFT)) ;
                BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "PostHtmlEditing.Sidebar.Images.{0}Pushed.png", DELETE_DRAFT)) ;
                BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "PostHtmlEditing.Sidebar.Images.{0}Selected.png", DELETE_DRAFT)) ;
                Width = BitmapEnabled.Width ;
                Height = BitmapEnabled.Height ;
                AutoSizeHeight = true ;
                AutoSizeWidth = true ;
            }

            protected override bool IsPushKey(Keys key)
            {
                return base.IsPushKey (key) || (key == Keys.Enter) ;
            }

            private const string DELETE_DRAFT = "DeleteDraft" ;
        }

    }


    internal class DraftPostList : PostList
    {
        public DraftPostList(Control parent, ToolTip toolTip, string title)
            : base(parent, toolTip, title)
        {
        }

        protected override bool SupportsDelete
        {
            get
            {
                return true ;
            }
        }

        protected override string DeleteButtonToolTip
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DeleteSomething), Res.Get(StringId.UpdateWeblogDraft)) ;
            }
        }

        protected override string AccessibilityNameFormat
        {
            get { return Res.Get(StringId.DraftNameFormat); }
        }
    }

    internal class RecentPostList : PostList
    {
        public RecentPostList(Control parent, ToolTip toolTip, string title)
            : base(parent, toolTip, title)
        {
        }

        protected override string AccessibilityNameFormat
        {
            get { return Res.Get(StringId.PublishedNameFormat); }
        }
    }

    internal class SidebarColors
    {
        [Obsolete("Use ColorizedResources.Instance.SidebarTextColor", true)]
        public static readonly Color TextColor = Color.Red;

        public static readonly Color BorderLineColor = Color.FromArgb(216, 227, 254);
        public static readonly Color PanelHeaderTopColor = Color.FromArgb(199,217,254);
        public static readonly Color PanelHeaderBottomColor = Color.FromArgb(110, 149, 216) ;
        public static readonly Color PanelBodyTopColor = Color.FromArgb(208, 222,251);
        public static readonly Color PanelBodyBottomColor = Color.FromArgb(183, 203, 245);
        public static readonly Color SectionTopColor = Color.FromArgb(165, 189, 232);
        public static readonly Color FirstSectionBottomColor = Color.FromArgb(208, 222, 251) ;
        public static readonly Color SecondSectionBottomColor = Color.FromArgb(198, 214, 249);
    }

    internal class DefaultSidebar : ISidebar
    {
        public DefaultSidebar(IBlogPostEditingSite postEditingSite)
        {
            _postEditingSite = postEditingSite ;
        }

        public bool AppliesToSelection( object htmlSelection )
        {
            return true ;
        }

        public SidebarControl CreateSidebarControl(ISidebarContext sidebarContext)
        {
            return new DefaultSidebarControl(sidebarContext, _postEditingSite) ;
        }

        private IBlogPostEditingSite _postEditingSite ;
    }
}
