// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsBlurGalleryCommand : ImageEffectsGalleryCommand
    {
        // Category indices.
        private const int NoBlurIndex = 0;
        private const int BlurIndex = 1;

        public ImageEffectsBlurGalleryCommand(CommandId commandId)
            : base(commandId, Res.Get(StringId.DecoratorNoBlurLabel))
        {
        }

        public override void LoadItems()
        {
            if (Items.Count == 0)
            {
                Categories.AddRange(new GalleryItem[] {
                    new GalleryItem(Res.Get(StringId.ImageEffectsNoBlurCategory), NoBlurIndex),
                    new GalleryItem(Res.Get(StringId.ImageEffectsGaussianCategory), BlurIndex)
                });

                Items.AddRange(new GalleryItem[] {
                    // No gaussian category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorNoBlurLabel), Res.Get(StringId.DecoratorNoBlurDescription), Images.Effects_Default, NoBlurDecorator.Id, NoBlurIndex),
                    // Gaussian category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorGaussianBlurLabel), Res.Get(StringId.DecoratorGaussianBlurDescription), Images.Effects_Gaussian, BlurDecorator.Id, BlurIndex)
                });

                base.LoadItems();

                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}
