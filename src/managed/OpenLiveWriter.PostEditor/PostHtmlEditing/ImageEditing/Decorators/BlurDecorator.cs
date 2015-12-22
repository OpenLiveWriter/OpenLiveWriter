// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for CopyrightDecorator.
    /// </summary>
    public class BlurDecorator : IImageDecorator
    {
        public BlurDecorator()
        {

        }

        public readonly static string Id = "Blur";

        public void Decorate(ImageDecoratorContext context)
        {
            context.Image = Blur(context.Image);
        }

        /// <summary>
        /// </summary>
        /// <param name="bitmap"></param>
        private Bitmap Blur(Bitmap bitmap)
        {
            TransformMatrix m = new TransformMatrix(1, 2, 6, 18, 0);
            return m.Conv3x3(bitmap);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
