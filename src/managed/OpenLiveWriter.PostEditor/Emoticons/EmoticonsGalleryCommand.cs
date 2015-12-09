// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor.Emoticons
{
    public class EmoticonsGalleryCommand : GalleryCommand<Emoticon>
    {
        // Category indices.
        private const int RecentIndex = 0;
        private const int PopularIndex = 1;

        private IBlogPostImageEditingContext _imageEditingContext;
        private List<GalleryItem> recentEmoticonsGalleryItems = new List<GalleryItem>();

        public EmoticonsGalleryCommand(CommandId commandId, IBlogPostImageEditingContext imageEditingContext)
            : base(commandId, false)
        {
            AllowSelection = false;
            _imageEditingContext = imageEditingContext;
        }

        public override void LoadItems()
        {
            if (Categories.Count == 0)
            {
                Categories.AddRange(new GalleryItem[] {
                    new GalleryItem(Res.Get(StringId.EmoticonsGalleryRecent), RecentIndex),
                    new GalleryItem(Res.Get(StringId.EmoticonsGalleryPopular), PopularIndex)
                });
            }

            if (Items.Count == 0)
            {
                foreach (Emoticon emoticon in _imageEditingContext.EmoticonsManager.PopularEmoticons)
                {
                    Items.Add(new GalleryItem(emoticon.Label, emoticon.Bitmap, emoticon, PopularIndex));

                    // WinLive 122017: Build up a cache of gallery items we can use to represent recently used emoticons.
                    recentEmoticonsGalleryItems.Add(new GalleryItem(emoticon.Label, (Bitmap)emoticon.Bitmap.Clone(), emoticon, RecentIndex));
                }
            }

            // Always refresh the Recent emoticons.
            Items.RemoveAll(item => item.CategoryIndex == RecentIndex);
            foreach (Emoticon emoticon in _imageEditingContext.EmoticonsManager.RecentEmoticons)
            {
                Emoticon recentEmoticon = emoticon;
                Items.Add(recentEmoticonsGalleryItems.Find(galleryItem => galleryItem.Cookie.Id == recentEmoticon.Id));
            }

            base.LoadItems();

            OnStateChanged(EventArgs.Empty);
        }
    }

}
