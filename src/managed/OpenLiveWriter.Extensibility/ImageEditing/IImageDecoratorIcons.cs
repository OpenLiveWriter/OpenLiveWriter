// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    /// <summary>
    /// Summary description for IImageDecoratorIcon.
    /// </summary>
    public interface IImageDecoratorIcons
    {
        /// <summary>
        /// The large icon for an image decorator (expected size: 40x27)
        /// </summary>
        Bitmap BitmapLarge { get; }
    }
}
