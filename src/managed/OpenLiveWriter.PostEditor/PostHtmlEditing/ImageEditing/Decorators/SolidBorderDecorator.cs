// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public abstract class SolidBorderDecorator : ImageBorderDecorator
    {
        private readonly int borderWidth;

        protected SolidBorderDecorator(int borderWidth)
        {
            this.borderWidth = borderWidth;
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            Bitmap bitmap = new Bitmap(context.Image.Width + borderWidth * 2, context.Image.Height + borderWidth * 2);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Brush b = new SolidBrush(settings.ShadowColor))
                {
                    g.FillRectangle(b, 0, 0, bitmap.Width, borderWidth);
                    g.FillRectangle(b, 0, borderWidth, borderWidth, bitmap.Height);
                    g.FillRectangle(b, borderWidth, bitmap.Height - borderWidth, bitmap.Width - borderWidth, borderWidth);
                    g.FillRectangle(b, bitmap.Width - borderWidth, borderWidth, borderWidth, bitmap.Height - borderWidth * 2);
                }
                g.DrawImage(context.Image, borderWidth, borderWidth, context.Image.Width, context.Image.Height);
            }
            context.Image = bitmap;
            context.BorderMargin =
                new ImageBorderMargin(borderWidth * 2, borderWidth * 2, new BorderCalculation(borderWidth * 2, borderWidth * 2));

            HideHtmlBorder(context);
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }

    public class ThinSolidBorderDecorator : SolidBorderDecorator
    {
        public readonly static string Id = "ThinSolidBorder";

        public ThinSolidBorderDecorator() : base(1)
        {
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Solid_1_pixel; }
        }
    }

    public class ThickSolidBorderDecorator : SolidBorderDecorator
    {
        public readonly static string Id = "ThickSolidBorder";

        public ThickSolidBorderDecorator() : base(3)
        {
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Solid_3_pixel; }
        }
    }
}
