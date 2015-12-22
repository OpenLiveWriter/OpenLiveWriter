// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public abstract class ImageSliceBorderDecorator : ImageBorderDecorator
    {
        public override void Decorate(ImageDecoratorContext context)
        {
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            Rectangle adjustment = AdjustImagePositionAndSize(context);
            Bitmap bitmap = new Bitmap(
                context.Image.Width + adjustment.Width,
                context.Image.Height + adjustment.Height,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Brush b = new SolidBrush(settings.BackgroundColor))
                    g.FillRectangle(b, 0, 0, bitmap.Width, bitmap.Height);

                int[] sliceLines = SliceLines;
                BorderPaint bp = new BorderPaint(BorderImage, false, BorderPaintMode.Default, sliceLines[0], sliceLines[1], sliceLines[2], sliceLines[3]);
                if (DiscardColors)
                {
                    using (ImageAttributes ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(ImageHelper.GetColorOverrideImageMatrix(settings.ShadowColor));
                        bp.ImageAttributes = ia;
                        bp.DrawBorder(g, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }
                }
                else
                    bp.DrawBorder(g, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                g.DrawImage(context.Image,
                            adjustment.Left,
                            adjustment.Top,
                            context.Image.Width,
                            context.Image.Height);

                OnBorderDecorationComplete(g, context);
            }

            context.Image = bitmap;
            context.BorderMargin =
                new ImageBorderMargin(adjustment.Width, adjustment.Height,
                                            new BorderCalculation(adjustment.Width, adjustment.Height));
            HideHtmlBorder(context);
        }

        protected virtual void OnBorderDecorationComplete(Graphics g, ImageDecoratorContext context)
        {
        }

        protected virtual bool DiscardColors { get { return false; } }
        protected abstract Bitmap BorderImage { get; }
        protected abstract int[] SliceLines { get; }

        /// <summary>
        /// The size of this rectangle will be added to the size of the
        /// original image to determine the size of the new image.
        ///
        /// The position of this rectangle will be the position in the new
        /// image where the old image will be drawn.
        /// </summary>
        protected abstract Rectangle AdjustImagePositionAndSize(ImageDecoratorContext context);
    }
}
