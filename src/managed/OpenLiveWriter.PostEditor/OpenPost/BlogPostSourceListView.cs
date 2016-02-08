// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.OpenPost
{

    public class BlogPostSourceListView : ListView
    {
        public BlogPostSourceListView()
        {
        }

        // separate initialize to prevent this code from executing in the designer
        public void Initialize()
        {
            View = View.LargeIcon ;
            MultiSelect = false ;
            HideSelection = false ;
            LabelEdit = false ;
            LabelWrap = true ;

            _imageList = new ImageList();
            _imageList.ImageSize = new Size(IMAGE_HEIGHT,IMAGE_WIDTH);
            _imageList.ColorDepth = ColorDepth.Depth32Bit ;
            _imageList.Images.Add( ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectDraftPostings.png") ) ;
            _imageList.Images.Add( ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectRecentPostings.png") ) ;
            _imageList.Images.Add( ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.SelectWebLogPostings.png") ) ;
            LargeImageList = _imageList ;

            // apply initial sizing
            UpdateIconSize() ;

            AddPostSource( new LocalDraftsPostSource(), DRAFT_IMAGE_INDEX ) ;
            AddPostSource( new LocalRecentPostsPostSource(), RECENT_POSTS_IMAGE_INDEX ) ;

            foreach ( string blogId in BlogSettings.GetBlogIds() )
                AddPostSource( new RemoteWeblogBlogPostSource(blogId), WEBLOG_IMAGE_INDEX ) ;

            // call again to reflect scrollbars that may now exist
            UpdateIconSize() ;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged (e);
            UpdateIconSize() ;
        }

        private void UpdateIconSize()
        {
            int width = Width - 8 ; // prevent horizontal scrollbar
            if ( ControlHelper.ControlHasVerticalScrollbar(this) )
                width -= SystemInformation.VerticalScrollBarWidth ;

            const int ICON_PADDING = 10;
            int height = Convert.ToInt32(IMAGE_HEIGHT + (Font.GetHeight() * 2)) + ICON_PADDING ;

            const uint LVM_FIRST = 0x1000 ;
            const uint LVM_SETICONSPACING = (LVM_FIRST + 53) ;
            User32.SendMessage( Handle, LVM_SETICONSPACING, UIntPtr.Zero, MessageHelper.MAKELONG(width,height) ) ;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imageList.Dispose() ;

            }
            base.Dispose (disposing);
        }

        public void SelectDrafts()
        {
            Items[0].Selected = true ;
        }

        public void SelectRecentPosts()
        {
            Items[1].Selected = true ;
        }

        public IPostEditorPostSource SelectedPostSource
        {
            get
            {
                IPostEditorPostSource postSource = null ;
                foreach ( ListViewItem item in SelectedItems )
                {
                    postSource = item.Tag as IPostEditorPostSource ;
                    break;
                }
                return postSource ;
            }
        }

        private void AddPostSource( IPostEditorPostSource postSource, int imageIndex )
        {
            ListViewItem item = new ListViewItem();
            item.Text = postSource.Name ;
            item.Tag = postSource ;
            item.ImageIndex = imageIndex ;
            Items.Add(item) ;
        }

        private ImageList _imageList ;
        private const int DRAFT_IMAGE_INDEX = 0 ;
        private const int RECENT_POSTS_IMAGE_INDEX = 1;
        private const int WEBLOG_IMAGE_INDEX = 2 ;

        private const int IMAGE_WIDTH = 32 ;
        private const int IMAGE_HEIGHT = 32 ;

    }
}
