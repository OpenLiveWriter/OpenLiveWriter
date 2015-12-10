// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public class SaturationDecorator : IImageDecorator
    {
        public SaturationDecorator()
        {
        }

        public readonly static string Id = "Color Pop";

        public void Decorate(ImageDecoratorContext context)
        {
            context.Image = Saturate(context.Image);
        }

        private Bitmap Saturate(Bitmap bitmap)
        {
            float a, b, c, d, e, f;
            float saturation;
            saturation = 1.65f;
            a = (0.3086f * (1 - saturation)) + saturation;
            b = (0.3086f * (1 - saturation));
            c = (0.6094f * (1 - saturation));
            d = (0.6094f * (1 - saturation)) + saturation;
            e = (0.0820f * (1 - saturation));
            f = (0.0820f * (1 - saturation)) + saturation;

            ColorMatrix cm = new ColorMatrix(new float[][]{
                                                               new float[] {a, b, b, 0, 0},
                                                               new float[] {c, d, c, 0, 0},
                                                               new float[] {e, e, f, 0, 0},
                                                               new float[] {0, 0, 0, 1, 0},
                                                               new float[] {0, 0, 0, 0, 1}
                                                           });
            return ImageHelper.ApplyColorMatrix(bitmap, cm);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
