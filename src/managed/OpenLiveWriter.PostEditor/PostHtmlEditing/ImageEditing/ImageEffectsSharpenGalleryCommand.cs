// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsSharpenGalleryCommand : ImageEffectsGalleryCommand
    {
        // Category indices.
        private const int NoSharpenIndex = 0;
        private const int SharpenIndex = 1;

        public ImageEffectsSharpenGalleryCommand(CommandId commandId)
            : base(commandId, Res.Get(StringId.DecoratorNoSharpenLabel))
        {
        }

        public override void LoadItems()
        {
            if (Items.Count == 0)
            {
                Categories.AddRange(new GalleryItem[] {
                    new GalleryItem(Res.Get(StringId.ImageEffectsNoSharpenCategory), NoSharpenIndex),
                    new GalleryItem(Res.Get(StringId.ImageEffectsSharpenCategory), SharpenIndex)
                });

                Items.AddRange(new GalleryItem[] {
                    // No sharpen category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorNoSharpenLabel), Res.Get(StringId.DecoratorNoSharpenDescription), Images.Effects_Default, NoSharpenDecorator.Id, NoSharpenIndex),
                    // Sharpen category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorSharpenLabel), Res.Get(StringId.DecoratorSharpenDescription), Images.Effects_Sharpen, SharpenDecorator.Id, SharpenIndex)
                });

                base.LoadItems();

                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}
