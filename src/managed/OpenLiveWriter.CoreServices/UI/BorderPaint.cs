// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.UI
{
    /// <summary>
    /// Paints borders made from slices of a bitmap.
    ///
    /// The following illustration shows what slice index
    /// corresponds to what part of the border.
    ///
    /// 0111112
    /// 3     4
    /// 3     4
    /// 5666667
    /// </summary>
    public class BorderPaint : IDisposable
    {
        private const int TOPLEFT = 0;
        private const int TOPCENTER = 1;
        private const int TOPRIGHT = 2;
        private const int MIDDLELEFT = 3;
        private const int MIDDLECENTER = 4;
        private const int MIDDLERIGHT = 5;
        private const int BOTTOMLEFT = 6;
        private const int BOTTOMCENTER = 7;
        private const int BOTTOMRIGHT = 8;
        private const int SLICECOUNT = BOTTOMRIGHT + 1;

        private Bitmap _bitmap;
        private bool _imageIsOwned;
        private IntPtr _hBitmap;
        private Rectangle[] _slices;
        private Bitmap[] _sliceCache;
        private BorderPaintMode _flags;
        private ImageAttributes _imageAttributes;

        public BorderPaint(Bitmap image, bool imageIsOwned, BorderPaintMode flags, int vert1, int vert2, int horiz1, int horiz2)
            : this(image, imageIsOwned, flags, GraphicsHelper.SliceCompositedImageBorder(image.Size, vert1, vert2, horiz1, horiz2))
        {
        }

        public BorderPaint(Bitmap image, bool imageIsOwned, BorderPaintMode flags, params Rectangle[] slices)
        {
            _flags = flags;
            _slices = slices;
            _bitmap = image;
            _imageIsOwned = imageIsOwned;

            if (IsFlagSet(flags, BorderPaintMode.GDI))
            {
                _hBitmap = image.GetHbitmap();
            }
            else if (IsFlagSet(flags, BorderPaintMode.Cached))
            {
                _sliceCache = new Bitmap[SLICECOUNT];
                for (int i = 0; i < SLICECOUNT; i++)
                {
                    if (slices[i].Width > 0 && slices[i].Height > 0)
                    {
                        Bitmap cachedSlice = new Bitmap(slices[i].Width, slices[i].Height, image.PixelFormat);
                        using (Graphics g = Graphics.FromImage(cachedSlice))
                            g.DrawImage(image, new Rectangle(0, 0, slices[i].Width, slices[i].Height), slices[i], GraphicsUnit.Pixel);
                        _sliceCache[i] = cachedSlice;
                    }
                }
            }
        }

        ~BorderPaint()
        {
            Dispose(false);
        }

        public ImageAttributes ImageAttributes
        {
            set
            {
                if ((IsFlagSet(_flags, BorderPaintMode.GDI) || IsFlagSet(_flags, BorderPaintMode.Cached)) && value != null)
                    throw new InvalidOperationException("Can't set ImageAttributes when BorderPaint is in GDI or Cached mode");
                _imageAttributes = value;
            }
        }

        public int MinimumHeight
        {
            get
            {
                return Math.Max(
                    GetHeight(TOPLEFT) + GetHeight(BOTTOMLEFT),
                    GetHeight(TOPRIGHT) + GetHeight(BOTTOMRIGHT));
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
            //GC.KeepAlive(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_hBitmap != IntPtr.Zero)
                Gdi32.DeleteObject(_hBitmap);

            if (disposing)
            {
                if (_sliceCache != null)
                {
                    foreach (Bitmap b in _sliceCache)
                        if (b != null)
                            b.Dispose();
                }

                if (_imageIsOwned)
                    _bitmap.Dispose();
            }
            GC.KeepAlive(this);
        }

        public void DrawBorder(Graphics g, Rectangle rect)
        {
            Rectangle rectTopCenter = Rectangle.Empty;
            bool showTopCenter = false;
            if (!IsEmpty(TOPCENTER))
            {
                rectTopCenter = new Rectangle(
                    rect.Left + GetWidth(TOPLEFT),
                    rect.Top,
                    rect.Width - GetWidth(TOPLEFT) - GetWidth(TOPRIGHT),
                    _slices[TOPCENTER].Height);
                showTopCenter = g.IsVisible(rectTopCenter);
            }

            Rectangle rectBottomCenter = Rectangle.Empty;
            bool showBottomCenter = false;
            if (!IsEmpty(BOTTOMCENTER))
            {
                rectBottomCenter = new Rectangle(
                    rect.Left + GetWidth(BOTTOMLEFT),
                    rect.Bottom - _slices[BOTTOMCENTER].Height,
                    rect.Width - GetWidth(BOTTOMLEFT) - GetWidth(BOTTOMRIGHT),
                    _slices[BOTTOMCENTER].Height);
                showBottomCenter = g.IsVisible(rectBottomCenter);
            }

            Rectangle rectMiddleLeft = Rectangle.Empty;
            bool showMiddleLeft = false;
            if (!IsEmpty(MIDDLELEFT))
            {
                rectMiddleLeft = new Rectangle(
                    rect.Left,
                    rect.Top + GetHeight(TOPLEFT),
                    _slices[MIDDLELEFT].Width,
                    rect.Height - GetHeight(TOPLEFT) - GetHeight(BOTTOMLEFT));
                showMiddleLeft = g.IsVisible(rectMiddleLeft);
            }

            Rectangle rectMiddleCenter = Rectangle.Empty;
            bool showMiddleCenter = false;
            if (!IsEmpty(MIDDLECENTER) && IsFlagSet(_flags, BorderPaintMode.PaintMiddleCenter))
            {
                rectMiddleCenter = new Rectangle(
                    rect.Left + GetWidth(MIDDLELEFT),
                    rect.Top + GetHeight(TOPCENTER),
                    rect.Width - GetWidth(MIDDLELEFT) - GetWidth(MIDDLERIGHT),
                    rect.Height - GetHeight(TOPCENTER) - GetHeight(BOTTOMCENTER));
                showMiddleCenter = g.IsVisible(rectMiddleCenter);
            }

            Rectangle rectMiddleRight = Rectangle.Empty;
            bool showMiddleRight = false;
            if (!IsEmpty(MIDDLERIGHT))
            {
                rectMiddleRight = new Rectangle(
                    rect.Right - _slices[MIDDLERIGHT].Width,
                    rect.Top + GetHeight(TOPRIGHT),
                    _slices[MIDDLERIGHT].Width,
                    rect.Height - GetHeight(TOPRIGHT) - GetHeight(BOTTOMRIGHT));
                showMiddleRight = g.IsVisible(rectMiddleRight);
            }

            bool useGdi = IsFlagSet(_flags, BorderPaintMode.GDI);
            IntPtr pTarget = !useGdi ? IntPtr.Zero : g.GetHdc();
            try
            {
                IntPtr pSource = !useGdi ? IntPtr.Zero : Gdi32.CreateCompatibleDC(pTarget);
                try
                {
                    IntPtr pOrig = !useGdi ? IntPtr.Zero : Gdi32.SelectObject(pSource, _hBitmap);
                    try
                    {

                        if (!IsEmpty(TOPLEFT))
                            DrawSlice(g, TOPLEFT, rect.Location, pSource, pTarget);
                        if (!IsEmpty(TOPRIGHT))
                            DrawSlice(g, TOPRIGHT, new Point(rect.Right - _slices[TOPRIGHT].Width, rect.Top), pSource, pTarget);
                        if (!IsEmpty(BOTTOMLEFT))
                            DrawSlice(g, BOTTOMLEFT, new Point(rect.Left, rect.Bottom - _slices[BOTTOMLEFT].Height), pSource, pTarget);
                        if (!IsEmpty(BOTTOMRIGHT))
                            DrawSlice(g, BOTTOMRIGHT, new Point(rect.Right - _slices[BOTTOMRIGHT].Width, rect.Bottom - _slices[BOTTOMRIGHT].Height), pSource, pTarget);

                        if (showTopCenter)
                            HorizontalFill(g, TOPCENTER, rectTopCenter, pSource, pTarget);
                        if (showBottomCenter)
                            HorizontalFill(g, BOTTOMCENTER, rectBottomCenter, pSource, pTarget);
                        if (showMiddleLeft)
                            VerticalFill(g, MIDDLELEFT, rectMiddleLeft, pSource, pTarget);
                        if (showMiddleRight)
                            VerticalFill(g, MIDDLERIGHT, rectMiddleRight, pSource, pTarget);
                        if (showMiddleCenter)
                            StretchFill(g, MIDDLECENTER, rectMiddleCenter, pSource, pTarget);

                    }
                    finally
                    {
                        if (pSource != IntPtr.Zero)
                            Gdi32.SelectObject(pSource, pOrig);
                    }
                }
                finally
                {
                    if (pSource != IntPtr.Zero)
                        Gdi32.DeleteDC(pSource);
                }
            }
            finally
            {
                if (pTarget != IntPtr.Zero)
                    g.ReleaseHdc(pTarget);
            }
            GC.KeepAlive(this);
        }

        private int GetWidth(int sliceIndex)
        {
            return IsEmpty(sliceIndex) ? 0 : _slices[sliceIndex].Width;
        }

        private int GetHeight(int sliceIndex)
        {
            return IsEmpty(sliceIndex) ? 0 : _slices[sliceIndex].Height;
        }

        private bool IsEmpty(int sliceIndex)
        {
            return _slices[sliceIndex].Width == 0 || _slices[sliceIndex].Height == 0;
        }

        private void DrawSlice(Graphics g, int sliceIndex, Point point, IntPtr pSource, IntPtr pTarget)
        {
            Rectangle slice = _slices[sliceIndex];
            if (IsFlagSet(_flags, BorderPaintMode.GDI))
            {
                Rectangle sliceRect = slice;
                Gdi32.BitBlt(pTarget, point.X, point.Y, sliceRect.Width, sliceRect.Height, pSource, sliceRect.X, sliceRect.Y, Gdi32.TernaryRasterOperations.SRCCOPY);
            }
            else if (IsFlagSet(_flags, BorderPaintMode.Cached))
            {
                g.DrawImageUnscaled(_sliceCache[sliceIndex], point);
            }
            else
            {
                g.DrawImage(_bitmap, new Rectangle(point, slice.Size), slice.Left, slice.Top, slice.Width, slice.Height, GraphicsUnit.Pixel, _imageAttributes);
            }
        }

        private void HorizontalFill(Graphics g, int slice, Rectangle rect, IntPtr pSource, IntPtr pTarget)
        {
            if (IsFlagSet(_flags, BorderPaintMode.StretchToFill))
                StretchFill(g, slice, rect, pSource, pTarget);
            else
                HorizontalTile(g, slice, rect, pSource, pTarget);
        }

        private void VerticalFill(Graphics g, int slice, Rectangle rect, IntPtr pSource, IntPtr pTarget)
        {
            if (IsFlagSet(_flags, BorderPaintMode.StretchToFill))
                StretchFill(g, slice, rect, pSource, pTarget);
            else
                VerticalTile(g, slice, rect, pSource, pTarget);
        }

        private void StretchFill(Graphics g, int slice, Rectangle rect, IntPtr pSource, IntPtr pTarget)
        {
            GraphicsState gs = g.Save();
            try
            {
                Debug.Assert(!IsFlagSet(_flags, BorderPaintMode.GDI));
                Rectangle sliceRect = _slices[slice];

                ImageAttributes ia = _imageAttributes ?? new ImageAttributes();
                ia.SetWrapMode(WrapMode.TileFlipXY); // prevents single-pixel darkness at edges

                PointF[] destPoints = new PointF[]
                        {
                            new PointF(rect.Left, rect.Top),
                            new PointF(rect.Right, rect.Top),
                            new PointF(rect.Left, rect.Bottom),
                };

                if (IsFlagSet(_flags, BorderPaintMode.Cached))
                    g.DrawImage(_sliceCache[slice], destPoints, new Rectangle(Point.Empty, sliceRect.Size), GraphicsUnit.Pixel, ia);
                else
                    g.DrawImage(_bitmap, destPoints, sliceRect, GraphicsUnit.Pixel, ia);

                if (ia != _imageAttributes)
                    ia.Dispose();
            }
            finally
            {
                g.Restore(gs);
            }
        }

        private void HorizontalTile(Graphics g, int slice, Rectangle rect, IntPtr pSource, IntPtr pTarget)
        {
            Rectangle sliceRect = _slices[slice];
            for (int x = rect.Left; x < rect.Right; x += sliceRect.Width)
            {
                int fillWidth = (int)Math.Min(sliceRect.Width, rect.Right - x);
                if (IsFlagSet(_flags, BorderPaintMode.GDI))
                    Gdi32.BitBlt(pTarget, x, rect.Y, fillWidth, sliceRect.Height, pSource, sliceRect.X, sliceRect.Y, Gdi32.TernaryRasterOperations.SRCCOPY);
                else if (IsFlagSet(_flags, BorderPaintMode.Cached))
                    g.DrawImage(_sliceCache[slice], new Rectangle(x, rect.Y, fillWidth, sliceRect.Height), 0, 0, fillWidth, sliceRect.Height, GraphicsUnit.Pixel, _imageAttributes);
                else
                    g.DrawImage(_bitmap, new Rectangle(x, rect.Y, fillWidth, sliceRect.Height), sliceRect.Left, sliceRect.Top, sliceRect.Width, sliceRect.Height, GraphicsUnit.Pixel, _imageAttributes);
            }
        }

        private void VerticalTile(Graphics g, int slice, Rectangle rect, IntPtr pSource, IntPtr pTarget)
        {
            Rectangle sliceRect = _slices[slice];
            for (int y = rect.Top; y < rect.Bottom; y += sliceRect.Height)
            {
                int fillHeight = (int)Math.Min(sliceRect.Height, rect.Bottom - y);
                if (IsFlagSet(_flags, BorderPaintMode.GDI))
                    Gdi32.BitBlt(pTarget, rect.X, y, sliceRect.Width, fillHeight, pSource, sliceRect.X, sliceRect.Y, Gdi32.TernaryRasterOperations.SRCCOPY);
                else if (IsFlagSet(_flags, BorderPaintMode.Cached))
                    g.DrawImage(_sliceCache[slice], new Rectangle(rect.X, y, sliceRect.Width, fillHeight), 0, 0, sliceRect.Width, fillHeight, GraphicsUnit.Pixel, _imageAttributes);
                else
                    g.DrawImage(_bitmap, new Rectangle(rect.X, y, sliceRect.Width, fillHeight), sliceRect.Left, sliceRect.Top, sliceRect.Width, sliceRect.Height, GraphicsUnit.Pixel, _imageAttributes);
            }
        }

        private static bool IsFlagSet(BorderPaintMode val, BorderPaintMode test)
        {
            return (val & test) == test;
        }
    }

    [Flags]
    public enum BorderPaintMode
    {
        /// <summary>
        /// No setup time, but slow drawing operations.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Caches slices; increased setup time but
        /// at least 2X performance improvement for
        /// each draw operation.
        /// </summary>
        Cached = 1,

        /// <summary>
        /// Uses GDI. Reasonable setup time and very
        /// fast draw performance, but no alpha support
        /// and DOESN'T RESPECT GDI+ transformations!
        /// </summary>
        GDI = 2,

        /// <summary>
        /// Use stretch instead of tile to fill repeated areas.
        /// </summary>
        StretchToFill = 4,

        /// <summary>
        /// Paints the middle center slice of the borderpaint image
        /// </summary>
        PaintMiddleCenter = 8,
    }

}
