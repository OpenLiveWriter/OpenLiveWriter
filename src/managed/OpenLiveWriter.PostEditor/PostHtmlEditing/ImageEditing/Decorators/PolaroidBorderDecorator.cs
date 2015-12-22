// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Drawing2D;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class PolaroidBorderDecorator : ImageBorderDecorator, IImageDecoratorOriginalSizeAdjuster
    {
        public readonly static string Id = "PolaroidBorder";

        private static readonly Rectangle PORTAL_RECT = new Rectangle(40, 40, 459, 472);

        public static float PortalAspectRatio
        {
            get { return PORTAL_RECT.Width / (float)PORTAL_RECT.Height; }
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Instant_Photo; }
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            Bitmap polaroidBorder = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.PolaroidBorder2.png");

            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);

            Size originalSize = context.Image.Size;

            float scaleX = originalSize.Width / (float)PORTAL_RECT.Width;
            float scaleY = originalSize.Height / (float)PORTAL_RECT.Height;
            Size finalSize = new Size((int)(polaroidBorder.Width * scaleX), (int)(polaroidBorder.Height * scaleY));

            Bitmap output = new Bitmap(finalSize.Width, finalSize.Height);
            using (Graphics g = Graphics.FromImage(output))
            {
                using (SolidBrush b = new SolidBrush(settings.BackgroundColor))
                    g.FillRectangle(b, 0, 0, output.Width, output.Height);

                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                RectangleF realPortal = new RectangleF(
                    PORTAL_RECT.X * scaleX,
                    PORTAL_RECT.Y * scaleY,
                    PORTAL_RECT.Width * scaleX,
                    PORTAL_RECT.Height * scaleY);

                realPortal.Inflate(1, 1);
                g.DrawImage(context.Image, realPortal, new RectangleF(0, 0, originalSize.Width, originalSize.Height), GraphicsUnit.Pixel);
                g.DrawImage(polaroidBorder, 0, 0, output.Width, output.Height);
            }

            context.Image = output;
            context.BorderMargin = new ImageBorderMargin(
                output.Width - originalSize.Width, output.Height - originalSize.Height,
                new BorderCalculation(output.Width / (float)originalSize.Width, output.Height / (float)originalSize.Height));

            HideHtmlBorder(context);
        }

        public void AdjustOriginalSize(IProperties properties, ref Size size)
        {
            size = RectangleHelper.EnforceAspectRatio(new Rectangle(Point.Empty, size), PortalAspectRatio).Size;
        }
    }
}
