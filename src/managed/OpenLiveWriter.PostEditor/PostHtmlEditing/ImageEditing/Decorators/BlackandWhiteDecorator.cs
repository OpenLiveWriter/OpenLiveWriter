// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public class BlackandWhiteDecorator : IImageDecorator
    {
        public BlackandWhiteDecorator()
        {
        }

        public readonly static string Id = "BlackAndWhite";

        public void Decorate(ImageDecoratorContext context)
        {
            //convert the image to black and white
            context.Image = ConvertToBlackAndWhite(context.Image);
        }

        /// <summary>
        /// Convert to a black and white image.
        /// Based on image processing algorithm from Paul Haeberli.
        /// http://www.sgi.com/misc/grafica/matrix/
        /// </summary>
        /// <param name="bitmap"></param>
        private Bitmap ConvertToBlackAndWhite(Bitmap bitmap)
        {
            float rwgt = 0.3086f;
            float gwgt = 0.6094f;
            float bwgt = 0.0820f;
            ColorMatrix cm = new ColorMatrix(new float[][]{
                                                               new float[]{rwgt, rwgt, rwgt,    0f,    0f},
                                                               new float[]{gwgt, gwgt, gwgt,    0f,    0f},
                                                               new float[]{bwgt, bwgt, bwgt,    0f,    0f},
                                                               new float[]{0f,     0f,   0f,    1f,    0f},
                                                               new float[]{0f,     0f,   0f,    0f,    1f}});

            return ImageHelper.ApplyColorMatrix(bitmap, cm);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }
}
