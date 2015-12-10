// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.JumpList
{
    public class WriterJumpList : JumpList
    {
        public const int MAX_ITEMS = PostListCache.MaxItems;
        private readonly JumpListCustomCategory _recentDrafts;
        private readonly JumpListCustomCategory _recentPosts;

        public static WriterJumpList CreateWriterJumpList(IntPtr ownerWindow)
        {
            return new WriterJumpList(TaskbarManager.Instance.ApplicationId, ownerWindow);
        }

        protected WriterJumpList(string appId, IntPtr ownerWindow)
            : base(appId, ownerWindow)
        {
            PostInfo[] drafts = PostListCache.Drafts;
            PostInfo[] posts = PostListCache.RecentPosts;

            int numPostsToUse;
            int numDraftsToUse;
            const int MAX_POSTS_IF_CONSTRAINED = 3;
            Debug.Assert(MAX_POSTS_IF_CONSTRAINED < MAX_ITEMS);

            if (posts.Length <= MAX_POSTS_IF_CONSTRAINED)
            {
                // We can fit all of the posts.  Fill in the rest with drafts.
                numPostsToUse = posts.Length;
                numDraftsToUse = Math.Min(drafts.Length, MAX_ITEMS - numPostsToUse);
            }
            else
            {
                if (MAX_POSTS_IF_CONSTRAINED + drafts.Length <= MAX_ITEMS)
                {
                    // There's room for all the drafts.  Fill in the rest with posts.
                    numDraftsToUse = drafts.Length;
                    numPostsToUse = MAX_ITEMS - numDraftsToUse;
                }
                else
                {
                    // Limit the number of posts.  Fill in the rest with drafts.
                    numDraftsToUse = MAX_ITEMS - MAX_POSTS_IF_CONSTRAINED;
                    numPostsToUse = MAX_POSTS_IF_CONSTRAINED;
                }
            }

            // Don't show the "Recent" or "Frequent" categories.
            KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;

            // Show custom categories instead.
            _recentDrafts = new JumpListCustomCategory(Res.Get(StringId.RecentDrafts), numDraftsToUse);
            PopulateCategory(_recentDrafts, drafts);
            _recentPosts = new JumpListCustomCategory(Res.Get(StringId.RecentPosts), numPostsToUse);
            PopulateCategory(_recentPosts, posts);
            AddCustomCategories(_recentDrafts, _recentPosts);

        }

        public static void Invalidate(IntPtr ownerWindow)
        {
            try
            {
                if (CanShowJumpList())
                {
                    WriterJumpList jumpList = CreateWriterJumpList(ownerWindow);
                    jumpList.Refresh();
                }
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                if (e is InvalidOperationException || e is UnauthorizedAccessException || e is COMException)
                    return;

                throw;
            }
        }

        private void PopulateCategory(JumpListCustomCategory category, PostInfo[] postInfo)
        {
            foreach (PostInfo post in postInfo)
            {
                if (!category.AddJumpListItem(new JumpListItem(post.Id)))
                    return;
            }
        }
    }
}
