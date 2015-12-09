// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class ReflectionBorderDecorator : ImageBorderDecorator
    {
        public readonly static string Id = "ReflectionBorder";

        public override void Decorate(ImageDecoratorContext context)
        {
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            Bitmap original = context.Image;

            int reflectionHeight = original.Height / 3;

            Bitmap bitmap = new Bitmap(original.Width, original.Height + reflectionHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
#if !TRUE_TRANSPARENCY
                using (Brush b = new SolidBrush(settings.BackgroundColor))
                    g.FillRectangle(b, 0, 0, bitmap.Width, bitmap.Height);
#endif

                g.DrawImage(original, 0, 0, original.Width, original.Height);

                Rectangle reflectionRect = new Rectangle(0, original.Height, bitmap.Width, reflectionHeight);
                Rectangle clipRect = new Rectangle(reflectionRect.X, reflectionRect.Y + 1, reflectionRect.Width, reflectionRect.Height - 1);
                g.SetClip(clipRect);

                using (new QuickTimer("Draw faded reflection"))
                {
#if TRUE_TRANSPARENCY
                    float startOpacity = 0.5f;
                    float endOpacity = 0f;

                    for (int vOffset = 0; ; vOffset++)
                    {
                        int destRow = reflectionRect.Top + 1 + vOffset;
                        if (destRow >= bitmap.Height)
                            break;
                        int srcRow = original.Height - vOffset - 1;
                        float opacity =
                            Math.Min(1.0f,
                                     startOpacity +
                                     ((endOpacity - startOpacity) * (vOffset / (float)reflectionRect.Height)));

                        ImageAttributes ia = new ImageAttributes();
                        ColorMatrix cm = new ColorMatrix();
                        cm.Matrix33 = opacity;
                        ia.SetColorMatrix(cm);
                        g.DrawImage(original,
                                    new Rectangle(0, destRow, reflectionRect.Width, 1),
                                    0, srcRow, reflectionRect.Width, 1,
                                    GraphicsUnit.Pixel,
                                    ia);
                    }
#else
                    Point upperLeft = new Point(0, original.Height * 2);
                    Point upperRight = new Point(original.Width, original.Height * 2);
                    Point lowerLeft = new Point(0, original.Height);
                    g.DrawImage(original, new Point[] { upperLeft, upperRight, lowerLeft });
                    using (
                        Brush b =
                            new LinearGradientBrush(reflectionRect, Color.FromArgb(128, settings.BackgroundColor),
                                                    settings.BackgroundColor, LinearGradientMode.Vertical))
                        g.FillRectangle(b, reflectionRect);
#endif
                }
            }

            context.Image = bitmap;
            //update the margin value to reflect the border added by this decorator.
            context.BorderMargin = new ImageBorderMargin(0, reflectionHeight, new BorderCalculation(1f, 1 + (1 / 3f)));

            HideHtmlBorder(context);
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Reflection; }
        }
    }
}
