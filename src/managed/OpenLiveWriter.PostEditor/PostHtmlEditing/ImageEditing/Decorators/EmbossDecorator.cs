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
    public class EmbossDecorator : IImageDecorator
    {
        public EmbossDecorator()
        {

        }

        public readonly static string Id = "Emboss";

        public void Decorate(ImageDecoratorContext context)
        {
            context.Image = Emboss(context.Image);
        }

        /// <summary>
        /// </summary>
        /// <param name="bitmap"></param>
        private Bitmap Emboss(Bitmap bitmap)
        {
            TransformMatrix m = new TransformMatrix(-1, -1, 8, 1, 127);
            return m.Conv3x3(bitmap);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
