// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Creates a photopaper-like border for an image (surrounding white border with a drop shadow).
    /// </summary>
    public class PhotoBorderDecorator : DropShadowBorderDecorator
    {
        public new readonly static string Id = "PhotoBorder";
        public PhotoBorderDecorator()
        {
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            const int BORDER_WIDTH = 8;
            const int SHADOW_WIDTH = 4;
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            context.Image = ImageUtils.ApplyDropShadowOutside(context.Image, settings.BackgroundColor, settings.ShadowColor, SHADOW_WIDTH, BORDER_WIDTH);

            //update the margin value to reflect the border added by this decorator.
            int borderWidth = BORDER_WIDTH * 2 + SHADOW_WIDTH;
            int borderHeight = borderWidth;
            context.BorderMargin =
                new ImageBorderMargin(borderWidth, borderHeight, new BorderCalculation(borderWidth, borderHeight));

            HideHtmlBorder(context);
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Photo_Paper; }
        }
    }
}
