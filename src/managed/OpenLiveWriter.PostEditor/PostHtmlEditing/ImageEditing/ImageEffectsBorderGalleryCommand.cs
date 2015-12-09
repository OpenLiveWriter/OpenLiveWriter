// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsBorderGalleryCommand : ImageEffectsGalleryCommand
    {
        public ImageEffectsBorderGalleryCommand(CommandId commandId)
            : base(commandId, Res.Get(StringId.DecoratorNoBorder))
        {
        }

        public override void LoadItems()
        {
            Debug.Assert(DecoratorsManager != null, "DecoratorsManager must be set prior to loading items.");
            Items.Clear();

            foreach (ImageDecorator borderDecorator in DecoratorsManager.GetImageDecoratorsFromGroup(ImageDecoratorsManager.BORDER_GROUP))
            {
                if (borderDecorator.Command.Enabled && borderDecorator.Command.On)
                {
                    items.Add(new GalleryItem(borderDecorator.DecoratorName, borderDecorator.IconLarge, borderDecorator.Id));
                }
            }

            base.LoadItems();

            OnStateChanged(EventArgs.Empty);
        }
    }
}
