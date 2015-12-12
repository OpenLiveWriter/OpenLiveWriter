// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor
{

    public sealed class PostListCache
    {
        public static void Update()
        {
            lock (_lock)
            {
                // Cache our recent drafts/post list
                _draftList = PostEditorFile.GetRecentPosts(PostEditorFile.DraftsFolder, new RecentPostRequest(MaxItems));
                _postList = PostEditorFile.GetRecentPosts(PostEditorFile.RecentPostsFolder, new RecentPostRequest(MaxItems));
                _refresh = false;
            }
        }

        public static PostInfo[] Drafts
        {
            get
            {
                lock (_lock)
                {
                    if (_refresh)
                    {
                        Update();
                    }

                    PostInfo[] draftList = new PostInfo[_draftList.Length];
                    _draftList.CopyTo(draftList, 0);
                    return draftList;
                }
            }
        }

        public static PostInfo[] RecentPosts
        {
            get
            {
                lock (_lock)
                {
                    if (_refresh)
                    {
                        Update();
                    }

                    PostInfo[] postList = new PostInfo[_postList.Length];
                    _postList.CopyTo(postList, 0);
                    return postList;
                }
            }
        }

        private static readonly object _lock = new object();
        private static bool _refresh = true;
        private static PostInfo[] _postList;
        private static PostInfo[] _draftList;

        // Note: The maximum number of recent items specified here should match the RecentItem's MaxCount attribute in ribbon.xml.
        // This is the same count used by RecentItemsCommand, JumpList and DrafPostItemsGalleryCommand for other recent draft/post
        // surfaced in the product. If you change this value, make sure the new value works with other components using it.
        public const int MaxItems = 10;
    }
}
