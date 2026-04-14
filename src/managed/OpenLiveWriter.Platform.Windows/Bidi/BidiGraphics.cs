// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization.Bidi
{
    [SupportedOSPlatform("windows")]
    public class BidiGraphics
    {
        private readonly Graphics g;
        private readonly Rectangle containerBounds;
        private readonly bool isRTL;
        private bool applyRtlFormRules;

        public BidiGraphics(Graphics g, Rectangle containerBounds, bool isRtl)
        {
            this.g = g;
            this.containerBounds = containerBounds;
            isRTL = isRtl;
        }

        public BidiGraphics(Graphics g, Rectangle containerBounds, RightToLeft rtl) : this(g, containerBounds, rtl == RightToLeft.Yes && BidiHelper.IsRightToLeft)
        {
        }

        public BidiGraphics(Graphics g, Rectangle containerBounds) : this(g, containerBounds, BidiHelper.IsRightToLeft)
        {
        }

        public BidiGraphics(Graphics g, Size containerSize) : this(g, new Rectangle(Point.Empty, containerSize))
        {
        }

        public BidiGraphics(Graphics g, Size containerSize, bool isRtl)
            : this(g, new Rectangle(Point.Empty, containerSize), isRtl)
        {
        }

        public Graphics Graphics
        {
            get { return g; }
        }

        /// <summary>
        /// Sometimes, graphics contexts that are based on Forms where RightToLeftLayout is true
        /// need to be handled differently, due to the device context having a mirrored coordinate
        /// space. Note that this is not true for child controls of such forms, in general.
        /// </summary>
        public bool ApplyRtlFormRules
        {
            get { return applyRtlFormRules; }
            set { applyRtlFormRules = value; }
        }

        private Rectangle TranslateImageRectangle(Rectangle orig, bool allowMirroring)
        {
            if (!isRTL)
                return orig;

            Rectangle rect = TranslateRectangle(orig);
            if (allowMirroring)
            {
                rect.X += rect.Width;
                rect.Width *= -1;
            }
            return rect;
        }

        public Rectangle TranslateRectangle(Rectangle orig)
        {
            if (!isRTL)
                return orig;
            int x = containerBounds.Width - (orig.Location.X + orig.Width) + (2 * containerBounds.Left);
            return new Rectangle(x, orig.Y, orig.Width, orig.Height);
        }

        private RectangleF TranslateRectangleF(RectangleF orig)
        {
            if (!isRTL)
                return orig;
            float x = containerBounds.Width - (orig.Location.X + orig.Width) + (2 * containerBounds.Left);
            return new RectangleF(x, orig.Y, orig.Width, orig.Height);
        }

        private Point TranslatePoint(Point orig)
        {
            if (!isRTL)
                return orig;
            int x = containerBounds.Width - orig.X + (2 * containerBounds.Left);
            return new Point(x, orig.Y);
        }

        public void DrawImage(bool allowMirroring, Image image, int x, int y)
        {
            //TODO: measure image to get width for high DPI
            Rectangle rect = new Rectangle(x, y, image.Width, image.Height);
            rect = TranslateImageRectangle(rect, allowMirroring);
            g.DrawImage(image, rect);
        }

        public void DrawImage(bool allowMirroring, Image image, Point point)
        {
            //TODO: measure image to get width for high DPI
            Rectangle rect = new Rectangle(point.X, point.Y, image.Width, image.Height);
            rect = TranslateImageRectangle(rect, allowMirroring);
            g.DrawImage(image, rect);
        }

        public void DrawImage(bool allowMirroring, Image image, Rectangle rect)
        {
            rect = TranslateImageRectangle(rect, allowMirroring);
            g.DrawImage(image, rect);
        }

        public void DrawImage(bool allowMirroring, Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
        {
            //TODO: source coordinates need to deal with mirroring
            Debug.Assert(srcUnit == GraphicsUnit.Pixel, "BidiGraphics does not support non-Pixel units");
            g.DrawImage(image,
                TranslateImageRectangle(destRect, allowMirroring),
                srcX, srcY, srcWidth, srcHeight, srcUnit);
        }

        public void DrawImage(bool allowMirroring, Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            //TODO: source coordinates need to deal with mirroring
            Debug.Assert(srcUnit == GraphicsUnit.Pixel, "BidiGraphics does not support non-Pixel units");
            g.DrawImage(image,
                TranslateImageRectangle(destRect, allowMirroring),
                srcRect, srcUnit);
        }

        public void DrawImage(bool allowMirroring, Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttributes)
        {
            //TODO: source coordinates need to deal with mirroring
            Debug.Assert(srcUnit == GraphicsUnit.Pixel, "BidiGraphics does not support non-Pixel units");
            g.DrawImage(image,
                TranslateImageRectangle(destRect, allowMirroring),
                srcX, srcY, srcWidth, srcHeight, srcUnit,
                imageAttributes);
        }

        public void DrawIcon(bool allowMirroring, Icon icon, int x, int y)
        {
            //TODO: measure image to get width for high DPI
            Rectangle rect = new Rectangle(x, y, icon.Width, icon.Height);
            DrawIcon(allowMirroring, icon, rect);
        }

        public void DrawIcon(bool allowMirroring, Icon icon, Rectangle rect)
        {
            if (isRTL && (applyRtlFormRules || allowMirroring))
            {
                // This is necessary because form mirroring causes
                // icons to always draw mirrored.
                using (Bitmap bitmap = icon.ToBitmap())
                    DrawImage(allowMirroring, bitmap, rect);
            }
            else
                g.DrawIcon(icon, TranslateRectangle(rect));
        }

        [Obsolete("Please use DrawText- DrawString will cause issues with localized versions of the product.")]
        public void DrawString(string caption, Font font, Brush brush, RectangleF f)
        {
            StringFormat format = new StringFormat();
            DrawString(caption, font, brush, f, format);
        }

        [Obsolete("Please use DrawText- DrawString will cause issues with localized versions of the product.")]
        public void DrawString(string caption, Font font, Brush brush, RectangleF f, StringFormat format)
        {
            RectangleF newRectF = TranslateRectangleF(f);
            if (isRTL)
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            g.DrawString(caption, font, brush, newRectF, format);
        }

        [Obsolete("Please use DrawText- DrawString will cause issues with localized versions of the product.")]
        public void DrawString(string caption, Font font, Brush brush, int x, int y)
        {
            Point newPoint = TranslatePoint(new Point(x, y));
            StringFormat format = StringFormat.GenericDefault;
            if (isRTL)
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            g.DrawString(caption, font, brush, newPoint, format);
        }

        [Obsolete("Please use DrawText- DrawString will cause issues with localized versions of the product.")]
        public void DrawString(string caption, Font font, Brush b, Point point)
        {
            DrawString(caption, font, b, point.X, point.Y);
        }

        public void FillRectangle(Brush b, int x1, int y1, int width, int height)
        {
            Rectangle rect = TranslateRectangle(new Rectangle(x1, y1, width, height));
            g.FillRectangle(b, rect);
        }

        public void FillRectangle(Brush b, Rectangle Bounds)
        {
            Rectangle rect = TranslateRectangle(Bounds);
            g.FillRectangle(b, rect);
        }

        public void FillRectangle(Brush b, RectangleF Bounds)
        {
            RectangleF rect = TranslateRectangleF(Bounds);
            g.FillRectangle(b, rect);
        }

        public void DrawRectangle(Pen p, Rectangle Bounds)
        {
            Rectangle rect = TranslateRectangle(Bounds);
            if (isRTL)
                rect.X -= (int)Math.Round(p.Width);
            g.DrawRectangle(p, rect);
        }

        public void DrawLine(Pen p, int x1, int y1, int x2, int y2)
        {
            Point start = TranslatePoint(new Point(x1, y1));
            Point end = TranslatePoint(new Point(x2, y2));
            g.DrawLine(p, start, end);
        }

        public void IntersectClip(Rectangle region)
        {
            Rectangle rect = TranslateRectangle(region);
            g.IntersectClip(rect);
        }

        public Brush CreateLinearGradientBrush(Rectangle bounds, Color color, Color color1, LinearGradientMode mode)
        {
            Debug.Assert(((mode == LinearGradientMode.Horizontal) || (mode == LinearGradientMode.Vertical)), "CreateLinearGradientBrush only supports horizontal or vertical gradients");
            if (isRTL && mode == LinearGradientMode.Horizontal)
            {
                Color temp = color;
                color = color1;
                color1 = temp;
            }
            return new LinearGradientBrush(bounds, color, color1, mode);
        }

        public void DrawFocusRectangle(Rectangle rect)
        {
            ControlPaint.DrawFocusRectangle(g, TranslateRectangle(rect));
        }

        public void DrawFocusRectangle(Rectangle rect, Color foreColor, Color backColor)
        {
            ControlPaint.DrawFocusRectangle(g, TranslateRectangle(rect), foreColor, backColor);
        }

        public IDisposable Container(int xOffset, int yOffset)
        {
            GraphicsContainer gc = g.BeginContainer();
            g.TranslateTransform(xOffset, yOffset);
            return new GraphicsContainerDisposer(g, gc);
        }

        public Size MeasureText(string text, Font font)
        {
            return TextRenderer.MeasureText(g, text, font, Size.Empty, FixupTextFormatFlags(0));
        }

        public Size MeasureText(string text, Font font, Size size, TextFormatFlags flags)
        {
            return TextRenderer.MeasureText(g, text, font, size, FixupTextFormatFlags(flags));
        }

        public void DrawText(string text, Font font, Rectangle bounds, Color color, TextFormatFlags textFormatFlags)
        {
            textFormatFlags = FixupTextFormatFlags(textFormatFlags);
            TextRenderer.DrawText(
                g,
                text,
                font,
                TranslateRectangle(bounds),
                color,
                textFormatFlags
                );
        }

        private TextFormatFlags FixupTextFormatFlags(TextFormatFlags textFormatFlags)
        {
            if (isRTL)
            {
                textFormatFlags |= TextFormatFlags.RightToLeft;
                if ((textFormatFlags & TextFormatFlags.HorizontalCenter) == 0)
                    textFormatFlags ^= TextFormatFlags.Right;
            }
            return textFormatFlags;
        }

        public void DrawText(string text, Font font, Rectangle bounds, Color color, Color backgroundColor, TextFormatFlags textFormatFlags)
        {
            textFormatFlags = FixupTextFormatFlags(textFormatFlags);
            TextRenderer.DrawText(
                g,
                text,
                font,
                TranslateRectangle(bounds),
                color,
                backgroundColor,
                textFormatFlags
                );
        }

        public void DrawText(string text, Font font, Rectangle bounds, Color textColor)
        {
            TextFormatFlags textFormatFlags = 0;
            if (isRTL)
            {
                textFormatFlags |= TextFormatFlags.Right | TextFormatFlags.RightToLeft;
            }
            TextRenderer.DrawText(
                g,
                text,
                font,
                TranslateRectangle(bounds),
                textColor,
                textFormatFlags);
        }

        public const int DI_MASK = 0x0001;
        public const int DI_IMAGE = 0x0002;
        public const int DI_NORMAL = 0x0003;
        public const int DI_COMPAT = 0x0004;
        public const int DI_DEFAULTSIZE = 0x0008;
        public const int DI_NOMIRROR = 0x0010;

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon,
            int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw,
            int diFlags);

        public void FillPolygon(Brush b, Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = TranslatePoint(points[i]);
            g.FillPolygon(b, points);
        }

        public void DrawCurve(Pen p, Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = TranslatePoint(points[i]);
            g.DrawCurve(p, points);
        }
    }

    internal class GraphicsContainerDisposer : IDisposable
    {
        private readonly Graphics g;
        private readonly GraphicsContainer gc;
        private bool disposed = false;

        public GraphicsContainerDisposer(Graphics g, GraphicsContainer gc)
        {
            this.g = g;
            this.gc = gc;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                g.EndContainer(gc);
            }
        }
    }
}
