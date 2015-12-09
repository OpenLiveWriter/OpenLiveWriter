// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Helper for dealing with editor's named image sizes.
    /// </summary>
    public class ImageSizeHelper
    {
        //Define the maximum reasonable image size.
        //Note: a 20 Megapixel image (assuming a 3x2 ratio) would be 5475x3650
        public static readonly int MAX_HEIGHT = 11000;
        public static readonly int MAX_WIDTH = 11000;

        private ImageSizeHelper()
        {
        }

        public static Size GetSizeConstraints(ImageSizeName sizeName)
        {
            switch (sizeName)
            {
                case ImageSizeName.Large:
                    return ImageEditingSettings.DefaultImageSizeLarge;
                case ImageSizeName.Medium:
                    return ImageEditingSettings.DefaultImageSizeMedium;
                case ImageSizeName.Small:
                    return ImageEditingSettings.DefaultImageSizeSmall;
                case ImageSizeName.Full:
                    return new Size(Int32.MaxValue, Int32.MaxValue);
                default:
                    Debug.Fail("Unsupported image size: " + sizeName);
                    return ImageEditingSettings.DefaultImageSizeMedium;
            }
        }
    }
    public enum ImageSizeName { Small, Medium, Large, Full, Custom };
}
