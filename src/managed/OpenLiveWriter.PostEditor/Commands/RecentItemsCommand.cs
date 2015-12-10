// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class RecentItemsCommand : GalleryCommand<string>
    {
        private IBlogPostEditingSite postEditingSite;
        private List<PostInfo> postInfo = new List<PostInfo>();
        // Note: The maximum number of recent items specified here should match the RecentItem's MaxCount attribute in ribbon.xml.
        public const int MaxItems = PostListCache.MaxItems;

        public RecentItemsCommand(IBlogPostEditingSite postEditingSite)
            : base(CommandId.MRUList)
        {
            this.postEditingSite = postEditingSite;
            ExecuteWithArgs += new ExecuteEventHandler(RecentItemsCommand_ExecuteWithArgs);
        }

        void RecentItemsCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            selectedIndex = args.GetInt(CommandId.ToString());
            if (selectedIndex != INVALID_INDEX && selectedIndex < postInfo.Count)
            {
                WindowCascadeHelper.SetNextOpenedLocation(postEditingSite.FrameWindow.Location);
                postEditingSite.OpenLocalPost(postInfo[selectedIndex]);
            }
        }

        public override void LoadItems()
        {
            items.Clear();
            selectedIndex = INVALID_INDEX;

            PostInfo[] postInfoDrafts = PostListCache.Drafts;
            PostInfo[] postInfoPosts = PostListCache.RecentPosts;

            int indexDraft = 0;
            int indexPost = 0;
            postInfo.Clear();
            for (int i = 0; i < MaxItems; i++)
            {
                if (indexDraft >= postInfoDrafts.Length && indexPost < postInfoPosts.Length)
                {
                    // No more drafts, just add posts
                    postInfo.Add(postInfoPosts[indexPost++]);
                }
                else if (indexDraft < postInfoDrafts.Length && indexPost >= postInfoPosts.Length)
                {
                    // No more posts, add drafts
                    postInfo.Add(postInfoDrafts[indexDraft++]);
                }
                else if (indexDraft < postInfoDrafts.Length && indexPost < postInfoPosts.Length)
                {
                    // Both drafts and posts are available, pick the latest (most recent)
                    if (postInfoDrafts[indexDraft].DateModified > postInfoPosts[indexPost].DateModified)
                    {
                        postInfo.Add(postInfoDrafts[indexDraft++]);
                    }
                    else
                    {
                        postInfo.Add(postInfoPosts[indexPost++]);
                    }
                }
                else
                {
                    // no more posts/drafts to add, we are done
                    break;
                }
            }

            foreach (var v in postInfo)
            {
                items.Add(new TooltippedGalleryItem(v.Title, v.BlogName, null, v.BlogPostId));
            }

            base.LoadItems();
        }

        public override int UpdateProperty(ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            if (key == PropertyKeys.RecentItems)
            {
                object[] currColl = (object[])currentValue.PropVariant.Value;

                // Clear out old items
                for (int i = 0; i < currColl.Length; i++)
                {
                    currColl[i] = null;
                }

                for (int i = 0; i < Items.Count && i < MaxItems; i++)
                {
                    TooltippedGalleryItem item = (TooltippedGalleryItem)Items[i];

                    SimplePropertySet sps = new SimplePropertySet();
                    Debug.Assert(item.Label != null);

                    if (item.Label != null)
                    {
                        sps.Add(PropertyKeys.Label, new PropVariant(item.Label));
                    }

                    // This is the item tooltip
                    if (item.LabelDescription != null)
                    {
                        sps.Add(PropertyKeys.LabelDescription, new PropVariant(item.LabelDescription));
                    }

                    currColl[i] = sps;
                }

                newValue = new PropVariant();
                newValue.SetSafeArray(currColl);
                return HRESULT.S_OK;
            }

            return base.UpdateProperty(ref key, currentValue, out newValue);
        }
    }
}
