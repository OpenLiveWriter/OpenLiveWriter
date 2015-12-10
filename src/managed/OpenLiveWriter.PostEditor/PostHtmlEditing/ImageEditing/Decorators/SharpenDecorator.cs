// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for CopyrightDecorator.
    /// </summary>
    public class SharpenDecorator : IImageDecorator
    {
        public SharpenDecorator()
        {

        }

        public readonly static string Id = "Sharpen";

        public void Decorate(ImageDecoratorContext context)
        {
            context.Image = Sharpen(context.Image);
        }

        /// <summary>
        /// </summary>
        /// <param name="bitmap"></param>
        private Bitmap Sharpen(Bitmap bitmap)
        {
            TransformMatrix m = new TransformMatrix(0, -2, 11, 3, 0);
            return m.Conv3x3(bitmap);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
