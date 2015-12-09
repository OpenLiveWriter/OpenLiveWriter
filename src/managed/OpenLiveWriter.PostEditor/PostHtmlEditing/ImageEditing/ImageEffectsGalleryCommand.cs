// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageEffectsGalleryCommand : GalleryCommand<string>
    {
        public ImageDecoratorsManager DecoratorsManager { get; set; }

        public ImageEffectsGalleryCommand(CommandId commandId, string defaultItem)
            : base(commandId, defaultItem)
        {
        }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.LabelDescription)
            {
                // In order to vertically align the label title, we need to return an error for the label description.
                value.SetError();
                return;
            }

            base.GetPropVariant(key, currentValue, ref value);
        }
    }
}
