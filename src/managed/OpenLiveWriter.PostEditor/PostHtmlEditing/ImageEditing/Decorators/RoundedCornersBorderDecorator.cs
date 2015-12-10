// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class RoundedCornersBorderDecorator : ImageBorderDecorator
    {
        public readonly static string Id = "RoundedCornersBorder";

        public override void Decorate(ImageDecoratorContext context)
        {
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            Bitmap bitmap = new Bitmap(context.Image.Width, context.Image.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(context.Image, 0, 0, context.Image.Width, context.Image.Height);

                using (ImageAttributes ia = new ImageAttributes())
                {
                    // The color matrix causes all color information to
                    // be replaced by the background color, while preserving
                    // the alpha of the source image.
                    ia.SetColorMatrix(ImageHelper.GetColorOverrideImageMatrix(settings.BackgroundColor));

                    Bitmap corner =
                        ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.Corner16.png");

                    g.DrawImage(corner,
                                new Rectangle(0, 0, corner.Width, corner.Height),
                                0, 0, corner.Width, corner.Height,
                                GraphicsUnit.Pixel, ia);
                    g.DrawImage(corner,
                                MakePoints(bitmap.Width, 0, bitmap.Width, corner.Width, bitmap.Width - corner.Height, 0),
                                new Rectangle(0, 0, corner.Width, corner.Height),
                                GraphicsUnit.Pixel, ia);
                    g.DrawImage(corner,
                                MakePoints(bitmap.Width, bitmap.Height, bitmap.Width - corner.Width, bitmap.Height, bitmap.Width, bitmap.Height - corner.Height),
                                new Rectangle(0, 0, corner.Width, corner.Height),
                                GraphicsUnit.Pixel, ia);
                    g.DrawImage(corner,
                                MakePoints(0, bitmap.Height, 0, bitmap.Height - corner.Width, corner.Width, bitmap.Height),
                                new Rectangle(0, 0, corner.Width, corner.Height),
                                GraphicsUnit.Pixel, ia);
                }
            }
            context.Image = bitmap;
            context.BorderMargin = ImageBorderMargin.Empty;
            HideHtmlBorder(context);
        }

        private static Point[] MakePoints(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            return new Point[]
                {
                    new Point(x1, y1),
                    new Point(x2, y2),
                    new Point(x3, y3)
                };
        }
        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Rounded_Corners; }
        }
    }
}
