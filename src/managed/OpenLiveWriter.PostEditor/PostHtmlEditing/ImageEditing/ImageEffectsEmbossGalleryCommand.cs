// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsEmbossGalleryCommand : ImageEffectsGalleryCommand
    {
        // Category indices.
        private const uint NoEmbossIndex = 0;
        private const uint EmbossIndex = 1;

        public ImageEffectsEmbossGalleryCommand(CommandId commandId)
            : base(commandId, Res.Get(StringId.DecoratorNoEmbossLabel))
        {
        }

        public override void LoadItems()
        {
            if (Items.Count == 0)
            {
                Categories.AddRange(new GalleryItem[] {
                    new GalleryItem(Res.Get(StringId.ImageEffectsNoEmbossCategory), NoEmbossIndex),
                    new GalleryItem(Res.Get(StringId.ImageEffectsEmbossCategory), EmbossIndex)
                });

                Items.AddRange(new GalleryItem[] {
                    // No emboss category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorNoEmbossLabel), Res.Get(StringId.DecoratorNoEmbossDescription), Images.Effects_Default, NoEmbossDecorator.Id, NoEmbossIndex),
                    // Emboss category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorEmbossLabel), Res.Get(StringId.DecoratorEmbossDescription), Images.Effects_Emboss, EmbossDecorator.Id, EmbossIndex)
                });

                base.LoadItems();

                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}
