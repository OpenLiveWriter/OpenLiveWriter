// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsRecolorGalleryCommand : ImageEffectsGalleryCommand
    {
        // Category indices.
        private const uint NoRecolorIndex = 0;
        private const uint ColorModesIndex = 1;
        private const uint ColorTemperatureIndex = 2;

        public ImageEffectsRecolorGalleryCommand(CommandId commandId)
            : base(commandId, Res.Get(StringId.DecoratorNoRecolorLabel))
        {
        }

        public override void LoadItems()
        {
            if (Items.Count == 0)
            {
                Categories.AddRange(new GalleryItem[] {
                    new GalleryItem(Res.Get(StringId.ImageEffectsNoRecolorCategory), NoRecolorIndex),
                    new GalleryItem(Res.Get(StringId.ImageEffectsColorModesCategory), ColorModesIndex),
                    new GalleryItem(Res.Get(StringId.ImageEffectsColorTemperatureCategory), ColorTemperatureIndex)
                });

                Items.AddRange(new GalleryItem[] {
                    // No recolor category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorNoRecolorLabel), Res.Get(StringId.DecoratorNoRecolorDescription), Images.Effects_Default, NoRecolorDecorator.Id, NoRecolorIndex),
                    // Color modes category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorBWLabel), Res.Get(StringId.DecoratorBWDescription), Images.Effects_Black_and_White, BlackandWhiteDecorator.Id, ColorModesIndex),
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorSepiaLabel), Res.Get(StringId.DecoratorSepiaDescription), Images.Effects_Sepia, SepiaToneDecorator.Id, ColorModesIndex),
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorSaturationLabel), Res.Get(StringId.DecoratorSaturationDescription), Images.Effects_Color_Pop, SaturationDecorator.Id, ColorModesIndex),
                    // Color temperature category.
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorCoolestTemperatureLabel), Res.Get(StringId.DecoratorTemperatureDescription), Images.Effects_Coolest, CoolestDecorator.Id, ColorTemperatureIndex),
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorCoolTemperatureLabel), Res.Get(StringId.DecoratorTemperatureDescription), Images.Effects_Cool, CoolDecorator.Id, ColorTemperatureIndex),
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorWarmTemperatureLabel), Res.Get(StringId.DecoratorTemperatureDescription), Images.Effects_Warm, WarmDecorator.Id, ColorTemperatureIndex),
                    new TooltippedGalleryItem(Res.Get(StringId.DecoratorWarmestTemperatureLabel), Res.Get(StringId.DecoratorTemperatureDescription), Images.Effects_Warmest, WarmestDecorator.Id, ColorTemperatureIndex)
                });

                base.LoadItems();

                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}
