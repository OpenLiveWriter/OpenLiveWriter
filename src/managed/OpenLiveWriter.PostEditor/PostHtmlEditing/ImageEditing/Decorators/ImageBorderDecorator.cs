// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Base for image border decorators.  Border decorators are mutually exclusive (only 1 can
    /// be applied at a time), and are applied after all other image decorators are applied.
    /// </summary>
    public abstract class ImageBorderDecorator : IImageDecorator, IImageDecoratorIcons
    {
        public ImageBorderDecorator()
        {
        }
        public abstract void Decorate(ImageDecoratorContext context);
        public abstract ImageDecoratorEditor CreateEditor(CommandManager commandManager);

        protected void HideHtmlBorder(ImageDecoratorContext context)
        {
            //remove any image style borders
            context.ImgElement.style.border = "0px;";
            context.ImgElement.setAttribute("border", "0", 0);
            // padding and background image could also interfere with borders
            context.ImgElement.style.padding = "0px;";
            context.ImgElement.style.backgroundImage = "none";
        }

        public abstract Bitmap BitmapLarge
        {
            get;
        }
    }
}
