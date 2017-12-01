// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public class RibbonHelper
    {
        public const int InGalleryImageWidth = 64;
        public const int InGalleryImageHeight = 48;
        public const int InGalleryImageHeightWithoutLabel = 36;

        public const int GalleryItemTextMaxChars = 20;
        public const int GalleryItemTextMaxWidthInPixels = 120;

        [ThreadStatic]
        private static IUIImageFromBitmap imageFromBitmap;

        public static IUIImage CreateImage(IntPtr bitmap, ImageCreationOptions options)
        {
            if (imageFromBitmap == null)
                imageFromBitmap = (IUIImageFromBitmap)new UIRibbonImageFromBitmapFactory();
            return imageFromBitmap.CreateImage(bitmap, options);
        }

        public static void CreateImagePropVariant(Image image, out PropVariant pv)
        {
            try
            {
                pv = new PropVariant();

                // Note that the missing image is a 32x32 png.
                if (image == null)
                    image = Images.Missing_LargeImage;

                pv.SetIUnknown(CreateImage(((Bitmap)image).GetHbitmap(), ImageCreationOptions.Transfer));
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to create image PropVariant: " + ex);
                pv = new PropVariant();
            }
        }

        public static bool IsStringPropertyKey(PropertyKey key)
        {
            return (key == PropertyKeys.Label ||
                    key == PropertyKeys.LabelDescription ||
                    key == PropertyKeys.Keytip ||
                    key == PropertyKeys.TooltipTitle ||
                    key == PropertyKeys.TooltipDescription);
        }

        public static Bitmap GetGalleryItemImageFromCommand(CommandManager commandManager, CommandId commandId)
        {
            // @RIBBON TODO: Deal with high contrast appropriately
            Command command = commandManager.Get(commandId);
            if (command != null)
                return command.LargeImage;

            return Images.Missing_LargeImage;
        }
    }
}
