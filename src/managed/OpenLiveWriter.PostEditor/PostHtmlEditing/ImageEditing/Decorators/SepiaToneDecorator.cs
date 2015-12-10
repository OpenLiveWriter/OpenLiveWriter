// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public class SepiaToneDecorator : IImageDecorator
    {
        public SepiaToneDecorator()
        {
        }

        public readonly static string Id = "SepiaTone";

        public void Decorate(ImageDecoratorContext context)
        {
            //convert the image to sepia
            context.Image = ConvertToSepia(context.Image);
        }

        private Bitmap ConvertToSepia(Bitmap bitmap)
        {
            ColorMatrix cm = new ColorMatrix(new float[][]{
                                                               new float[] {0.393f, 0.349f, 0.272f, 0, 0},
                                                               new float[] {0.769f, 0.686f, 0.534f, 0, 0},
                                                               new float[] {0.189f, 0.168f, 0.131f, 0, 0},
                                                               new float[] {     0,      0,      0, 1, 0},
                                                               new float[] {     0,      0,      0, 0, 1}
                                                           });
            return ImageHelper.ApplyColorMatrix(bitmap, cm);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
