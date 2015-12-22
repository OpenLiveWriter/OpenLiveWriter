// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{
    public partial class ImageCropControl : UserControl
    {
        private const AnchorStyles ANCHOR_ALL = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

        private Bitmap bitmap;
        private DualRects crop;
        private CachedResizedBitmap crbNormal;
        private CachedResizedBitmap crbGrayed;
        private bool gridLines;

        private CropStrategy cropStrategy = new FreeCropStrategy();

        private bool fireCropChangedOnKeyUp = false;

        public ImageCropControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            InitializeComponent();

            AccessibleName = Res.Get(StringId.CropPane);
            AccessibleRole = AccessibleRole.Pane;
            TabStop = true;
        }

        public bool GridLines
        {
            get { return gridLines; }
            set
            {
                if (gridLines != value)
                {
                    gridLines = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler CropRectangleChanged;
        private void OnCropRectangleChanged()
        {
            if (CropRectangleChanged != null)
                CropRectangleChanged(this, EventArgs.Empty);
        }

        public event EventHandler AspectRatioChanged;
        private void OnAspectRatioChanged()
        {
            if (AspectRatioChanged != null)
                AspectRatioChanged(this, EventArgs.Empty);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle CropRectangle
        {
            get { return crop.Real; }
            set { crop.Real = value; }
        }

        public void Crop()
        {
            Bitmap newBitmap = new Bitmap(bitmap, crop.Real.Size.Width, crop.Real.Size.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.DrawImage(bitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), crop.Real, GraphicsUnit.Pixel);
            }
            Bitmap = newBitmap;
        }

        public double? AspectRatio
        {
            get
            {
                return cropStrategy.AspectRatio;
            }
            set
            {
                if (value == null)
                {
                    cropStrategy = new FreeCropStrategy();
                    OnAspectRatioChanged();
                    Invalidate();
                }
                else
                {
                    cropStrategy = new FixedAspectRatioCropStrategy(value.Value);
                    OnAspectRatioChanged();
                    Rectangle containerRect = crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height);
                    Rectangle cropped = ((FixedAspectRatioCropStrategy)cropStrategy).ConformCropRectangle(containerRect, crop.Virtual);

                    if (cropped.Right > containerRect.Right)
                        cropped.X -= cropped.Right - containerRect.Right;
                    if (cropped.Bottom > containerRect.Bottom)
                        cropped.Y -= cropped.Bottom - containerRect.Bottom;
                    if (cropped.Left < containerRect.Left)
                        cropped.X += containerRect.Left - cropped.Left;
                    if (cropped.Top < containerRect.Top)
                        cropped.Y += containerRect.Top - cropped.Top;

                    crop.Virtual = cropped;
                    if (!cropStrategy.IsDragging)
                    {
                        cropStrategy.BeginDrag(
                            crop.Virtual,
                            new Point(crop.Virtual.Right, crop.Virtual.Bottom),
                            AnchorStyles.Bottom | AnchorStyles.Right,
                            crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height));
                        crop.Virtual = cropStrategy.GetNewBounds(new Point(crop.Virtual.Right, crop.Virtual.Bottom));
                        Invalidate();
                        Update();
                        OnCropRectangleChanged();
                        cropStrategy.EndDrag();
                    }
                }
            }
        }

        private void NullAndDispose<T>(ref T disposable) where T : IDisposable
        {
            IDisposable tmp = disposable;
            disposable = default(T);
            if (tmp != null)
                tmp.Dispose();
        }

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set
            {
                if (value != null)
                {
                    NullAndDispose(ref crbNormal);
                    NullAndDispose(ref crbGrayed);
                }

                bitmap = value;
                if (bitmap != null)
                {
                    crbNormal = new CachedResizedBitmap(value, false);
                    crbGrayed = new CachedResizedBitmap(MakeGray(value), true);

                    crop = new DualRects(PointF.Empty, bitmap.Size);
                    crop.Real = new Rectangle(
                        bitmap.Width / 4,
                        bitmap.Height / 4,
                        Math.Max(1, bitmap.Width / 2),
                        Math.Max(1, bitmap.Height / 2));
                    OnCropRectangleChanged();
                }
                PerformLayout();
                Invalidate();
            }
        }

        private Bitmap MakeGray(Bitmap orig)
        {
            Bitmap grayed = new Bitmap(orig);
            using (Graphics g = Graphics.FromImage(grayed))
            {
                using (Brush b = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                    g.FillRectangle(b, 0, 0, grayed.Width, grayed.Height);
            }
            return grayed;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                AnchorStyles anchor = cropStrategy.HitTest(crop.Virtual, e.Location);
                if (anchor != AnchorStyles.None)
                {
                    cropStrategy.BeginDrag(
                        crop.Virtual,
                        e.Location,
                        anchor,
                        crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height));

                    Capture = true;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (cropStrategy.IsDragging && e.Button == MouseButtons.Left)
            {
                cropStrategy.EndDrag();
                Capture = false;
                OnCropRectangleChanged();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (cropStrategy.IsDragging)
            {
                Rectangle originalRect = crop.Virtual;
                crop.Virtual = cropStrategy.GetNewBounds(PointToClient(MousePosition));
                InvalidateForRectChange(originalRect, crop.Virtual);
            }
            else if (e.Button == MouseButtons.None)
            {
                Cursor = ChooseCursor(crop.Virtual, e.Location);
            }
        }

        private void InvalidateForRectChange(Rectangle oldRect, Rectangle newRect)
        {
            int borderWidth = BoundsWithHandles.HANDLE_SIZE * 2;
            InvalidateBorder(oldRect, borderWidth);
            InvalidateBorder(newRect, borderWidth);
            if (gridLines)
            {
                InvalidateGridlines(oldRect);
                InvalidateGridlines(newRect);
            }

            using (Region region = new Region(oldRect))
            {
                region.Xor(newRect);
                Invalidate(region);
            }
        }

        private void InvalidateBorder(Rectangle rect, int width)
        {
            rect.Inflate(width / 2, width / 2);
            using (Region region = new Region(rect))
            {
                rect.Inflate(-width, -width);
                region.Exclude(rect);
                Invalidate(region);
            }
        }

        private void InvalidateGridlines(Rectangle rect)
        {
            int x1, x2, y1, y2;
            CalculateGridlines(rect, out x1, out x2, out y1, out y2);
            using (Region gridRegion = new Region())
            {
                gridRegion.MakeEmpty();
                gridRegion.Union(new Rectangle(x1, rect.Top, 1, rect.Height));
                gridRegion.Union(new Rectangle(x2, rect.Top, 1, rect.Height));
                gridRegion.Union(new Rectangle(rect.Left, y1, rect.Width, 1));
                gridRegion.Union(new Rectangle(rect.Left, y2, rect.Width, 1));
                Invalidate(gridRegion);
            }
        }

        private Cursor ChooseCursor(Rectangle sizeRect, Point point)
        {
            AnchorStyles anchor = cropStrategy.HitTest(sizeRect, point);
            switch (anchor)
            {
                case AnchorStyles.Left:
                case AnchorStyles.Right:
                    return Cursors.SizeWE;
                case AnchorStyles.Top:
                case AnchorStyles.Bottom:
                    return Cursors.SizeNS;
                case AnchorStyles.Top | AnchorStyles.Left:
                case AnchorStyles.Bottom | AnchorStyles.Right:
                    return Cursors.SizeNWSE;
                case AnchorStyles.Top | AnchorStyles.Right:
                case AnchorStyles.Bottom | AnchorStyles.Left:
                    return Cursors.SizeNESW;
                case AnchorStyles.None:
                    return Cursors.Default;
                case ANCHOR_ALL:
                    return Cursors.SizeAll;
                default:
                    Debug.Fail("Unexpected anchor: " + anchor);
                    return Cursors.Default;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        public bool ProcessCommandKey(ref Message msg, Keys keyData)
        {
            return ProcessCmdKey(ref msg, keyData);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up | Keys.Control:
                case Keys.Up:
                    return AdjustCropRectangle(0, -1, 0, 0);
                case Keys.Down | Keys.Control:
                case Keys.Down:
                    return AdjustCropRectangle(0, 1, 0, 0);
                case Keys.Left | Keys.Control:
                case Keys.Left:
                    return AdjustCropRectangle(-1, 0, 0, 0);
                case Keys.Right | Keys.Control:
                case Keys.Right:
                    return AdjustCropRectangle(1, 0, 0, 0);
                case Keys.Left | Keys.Shift:
                    return AdjustCropRectangle(0, 0, -1, 0);
                case Keys.Right | Keys.Shift:
                    return AdjustCropRectangle(0, 0, 1, 0);
                case Keys.Up | Keys.Shift:
                    return AdjustCropRectangle(0, 0, 0, -1);
                case Keys.Down | Keys.Shift:
                    return AdjustCropRectangle(0, 0, 0, 1);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool AdjustCropRectangle(int xOffset, int yOffset, int xGrow, int yGrow)
        {
            Rectangle orig = crop.Virtual;
            Rectangle result = cropStrategy.AdjustRectangle(
                crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height),
                orig,
                xOffset, yOffset, xGrow, yGrow);

            if (orig != result)
            {
                crop.Virtual = result;
                fireCropChangedOnKeyUp = true;
                InvalidateForRectChange(orig, result);
            }
            else
            {
                // annoying
                // SystemSounds.Beep.Play();
            }

            return true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (fireCropChangedOnKeyUp)
            {
                fireCropChangedOnKeyUp = false;
                OnCropRectangleChanged();
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (bitmap == null)
                return;

            float scaleFactor = Math.Min(
                (float)Width / bitmap.Width,
                (float)Height / bitmap.Height);

            Rectangle realRect = crop.Real;

            SizeF scale;
            PointF offset;

            if (scaleFactor > 1.0f)
            {
                scale = new SizeF(1, 1);
                offset = new PointF((Width - bitmap.Width) / 2, (Height - bitmap.Height) / 2);
            }
            else
            {
                offset = Point.Empty;
                scale = new SizeF(scaleFactor, scaleFactor);
                offset.X = (Width - (bitmap.Width * scale.Width)) / 2;
                offset.Y = (Height - (bitmap.Height * scale.Height)) / 2;
            }

            crop = new DualRects(offset, scale);
            crop.Real = realRect;

            Size virtualBitmapSize = crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height).Size;
            crbNormal.Resize(virtualBitmapSize);
            crbGrayed.Resize(virtualBitmapSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.Default;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            using (Brush b = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(b, ClientRectangle);

            if (bitmap == null)
                return;

            Rectangle bitmapRect = crop.VirtualizeRect(0, 0, bitmap.Width, bitmap.Height);

            e.Graphics.DrawImage(crbGrayed.ResizedBitmap, bitmapRect);
            Rectangle normalSrcRect = crop.Virtual;
            normalSrcRect.Offset(-bitmapRect.X, -bitmapRect.Y);
            e.Graphics.DrawImage(crbNormal.ResizedBitmap, crop.Virtual, normalSrcRect, GraphicsUnit.Pixel);

            e.Graphics.CompositingMode = CompositingMode.SourceOver;

            Rectangle cropDrawRect = crop.Virtual;
            cropDrawRect.Width -= 1;
            cropDrawRect.Height -= 1;
            using (Brush b = new SolidBrush(Color.FromArgb(200, Color.White)))
            using (Pen p = new Pen(b, 1))
            {
                e.Graphics.DrawRectangle(p, cropDrawRect);
            }

            if (gridLines)
            {
                int x1, x2, y1, y2;
                CalculateGridlines(crop.Virtual, out x1, out x2, out y1, out y2);

                using (Brush b = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                using (Pen p = new Pen(b, 1))
                {
                    e.Graphics.DrawLine(p, cropDrawRect.Left, y1, cropDrawRect.Right, y1);
                    e.Graphics.DrawLine(p, cropDrawRect.Left, y2, cropDrawRect.Right, y2);

                    e.Graphics.DrawLine(p, x1, cropDrawRect.Top, x1, cropDrawRect.Bottom);
                    e.Graphics.DrawLine(p, x2, cropDrawRect.Top, x2, cropDrawRect.Bottom);
                }
            }

            if (Focused)
            {
                Rectangle focusRect = cropDrawRect;
                focusRect.Inflate(2, 2);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRect);
            }

            BoundsWithHandles boundsWithHandles = new BoundsWithHandles(cropStrategy.AspectRatio == null ? true : false);
            boundsWithHandles.Bounds = cropDrawRect;
            using (Pen p = new Pen(SystemColors.ControlDarkDark, 1f))
            {
                foreach (Rectangle rect in boundsWithHandles.GetHandles())
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, rect);
                    e.Graphics.DrawRectangle(p, rect);
                }
            }
        }

        private static void CalculateGridlines(Rectangle cropDrawRect, out int x1, out int x2, out int y1, out int y2)
        {
            int yThird = cropDrawRect.Height / 3;
            y1 = cropDrawRect.Top + yThird;
            y2 = cropDrawRect.Top + yThird * 2;

            int xThird = cropDrawRect.Width / 3;
            x1 = cropDrawRect.Left + xThird;
            x2 = cropDrawRect.Left + xThird * 2;
        }

        private class CachedResizedBitmap : IDisposable
        {
            private readonly Bitmap original;
            private bool originalIsOwned;
            private Bitmap resized;
            private Size? currentSize;

            public CachedResizedBitmap(Bitmap original, bool takeOwnership)
            {
                this.original = original;
                this.originalIsOwned = takeOwnership;
            }

            public void Dispose()
            {
                Reset();
                if (originalIsOwned)
                {
                    originalIsOwned = false; // allows safe multiple disposal
                    original.Dispose();
                }
            }

            public void Reset()
            {
                Bitmap tmp = resized;
                resized = null;
                currentSize = null;
                if (tmp != null)
                    tmp.Dispose();
            }

            public void Resize(Size size)
            {
                size = new Size(Math.Max(size.Width, 1), Math.Max(size.Height, 1));
                Reset();
                Bitmap newBitmap = new Bitmap(original, size);
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.DrawImage(original, 0, 0, newBitmap.Width, newBitmap.Height);
                }
                resized = newBitmap;
                currentSize = size;
            }

            public Size CurrentSize
            {
                get { return currentSize ?? original.Size; }
            }

            public Bitmap ResizedBitmap
            {
                get { return resized ?? original; }
            }
        }

        public abstract class CropStrategy
        {
            protected const int MIN_WIDTH = 5;
            protected const int MIN_HEIGHT = 5;

            private bool dragging = false;
            protected Rectangle initialBounds;
            protected Point initialLoc;
            protected AnchorStyles anchor;
            protected Rectangle? container;

            public virtual void BeginDrag(Rectangle initialBounds, Point initalLoc, AnchorStyles anchor, Rectangle? container)
            {
                dragging = true;
                this.initialBounds = initialBounds;
                this.initialLoc = initalLoc;
                this.anchor = anchor;
                this.container = container;
            }

            public virtual double? AspectRatio
            {
                get { return null; }
            }

            public virtual void EndDrag()
            {
                dragging = false;
            }

            public bool IsDragging
            {
                get { return dragging; }
            }

            public virtual Rectangle GetNewBounds(Point newLoc)
            {
                Rectangle newRect = initialBounds;

                if (anchor == ANCHOR_ALL) // move
                {
                    newRect = DoMove(newLoc);
                }
                else // resize
                {
                    newRect = DoResize(newLoc);
                }

                if (container != null)
                    newRect.Intersect(container.Value);

                return newRect;
            }

            public abstract Rectangle AdjustRectangle(Rectangle cont, Rectangle rect, int xOffset, int yOffset, int xGrow, int yGrow);

            protected static int Round(double dblVal) { return (int)Math.Round(dblVal); }
            protected static int Round(float fltVal) { return (int)Math.Round(fltVal); }

            protected virtual Rectangle DoMove(Point newLoc)
            {
                Rectangle newRect = initialBounds;

                int deltaX = newLoc.X - initialLoc.X;
                int deltaY = newLoc.Y - initialLoc.Y;

                newRect.Offset(deltaX, deltaY);
                if (container != null)
                {
                    Rectangle cont = container.Value;

                    if (cont.Left > newRect.Left)
                        newRect.Offset(cont.Left - newRect.Left, 0);
                    else if (cont.Right < newRect.Right)
                        newRect.Offset(cont.Right - newRect.Right, 0);

                    if (cont.Top > newRect.Top)
                        newRect.Offset(0, cont.Top - newRect.Top);
                    else if (cont.Bottom < newRect.Bottom)
                        newRect.Offset(0, cont.Bottom - newRect.Bottom);
                }

                return newRect;
            }

            protected abstract Rectangle DoResize(Point newLoc);

            [Flags]
            private enum HT
            {
                None = 0,
                Left = 1,
                Top = 2,
                Right = 4,
                Bottom = 8,
                AlmostLeft = 16,
                AlmostTop = 32,
                AlmostRight = 64,
                AlmostBottom = 128,
                Mask_Horizontal = Left | AlmostLeft | Right | AlmostRight,
                Mask_Vertical = Top | AlmostTop | Bottom | AlmostBottom,
            }

            public virtual AnchorStyles HitTest(Rectangle sizeRect, Point point)
            {
                sizeRect.Inflate(1, 1);

                if (!sizeRect.Contains(point))
                    return AnchorStyles.None;

                HT hitTest = HT.None;

                Test(sizeRect.Right - point.X,
                    HT.Right,
                    HT.AlmostRight,
                    ref hitTest);
                Test(sizeRect.Bottom - point.Y,
                    HT.Bottom,
                    HT.AlmostBottom,
                    ref hitTest);
                Test(point.X - sizeRect.Left,
                    HT.Left,
                    HT.AlmostLeft,
                    ref hitTest);
                Test(point.Y - sizeRect.Top,
                    HT.Top,
                    HT.AlmostTop,
                    ref hitTest);

                RemoveConflicts(ref hitTest);

                switch (hitTest)
                {
                    case HT.Left:
                        return AnchorStyles.Left;
                    case HT.Top:
                        return AnchorStyles.Top;
                    case HT.Right:
                        return AnchorStyles.Right;
                    case HT.Bottom:
                        return AnchorStyles.Bottom;
                    case HT.Left | HT.Top:
                    case HT.Left | HT.AlmostTop:
                    case HT.Top | HT.AlmostLeft:
                        return AnchorStyles.Top | AnchorStyles.Left;
                    case HT.Right | HT.Top:
                    case HT.Right | HT.AlmostTop:
                    case HT.Top | HT.AlmostRight:
                        return AnchorStyles.Top | AnchorStyles.Right;
                    case HT.Right | HT.Bottom:
                    case HT.Right | HT.AlmostBottom:
                    case HT.Bottom | HT.AlmostRight:
                        return AnchorStyles.Bottom | AnchorStyles.Right;
                    case HT.Left | HT.Bottom:
                    case HT.Left | HT.AlmostBottom:
                    case HT.Bottom | HT.AlmostLeft:
                        return AnchorStyles.Bottom | AnchorStyles.Left;
                    default:
                        return ANCHOR_ALL;
                }
            }

            private static void RemoveConflicts(ref HT hitTest)
            {
                TestClearAndSet(ref hitTest, HT.Right, HT.Mask_Horizontal);
                TestClearAndSet(ref hitTest, HT.Left, HT.Mask_Horizontal);
                TestClearAndSet(ref hitTest, HT.AlmostRight, HT.Mask_Horizontal);
                TestClearAndSet(ref hitTest, HT.AlmostLeft, HT.Mask_Horizontal);
                TestClearAndSet(ref hitTest, HT.Bottom, HT.Mask_Vertical);
                TestClearAndSet(ref hitTest, HT.Top, HT.Mask_Vertical);
                TestClearAndSet(ref hitTest, HT.AlmostBottom, HT.Mask_Vertical);
                TestClearAndSet(ref hitTest, HT.AlmostTop, HT.Mask_Vertical);
            }

            private static void TestClearAndSet(ref HT val, HT test, HT clearMask)
            {
                if (test == (test & val))
                {
                    val &= ~clearMask;
                    val |= test;
                }
            }

            private static void Test(int distance, HT exactResult, HT fuzzyResult, ref HT hitTest)
            {
                hitTest |=
                    distance < 0 ? 0 :
                    distance < 5 ? exactResult :
                    distance < 10 ? fuzzyResult :
                    0;
            }
        }

        public class FixedAspectRatioCropStrategy : FreeCropStrategy
        {
            private double aspectRatio;

            public FixedAspectRatioCropStrategy(double aspectRatio)
            {
                this.aspectRatio = aspectRatio;
            }

            public override double? AspectRatio
            {
                get { return aspectRatio; }
            }

            public override Rectangle AdjustRectangle(Rectangle cont, Rectangle rect, int xOffset, int yOffset, int xGrow, int yGrow)
            {
                Rectangle result = base.AdjustRectangle(cont, rect, xOffset, yOffset, xGrow, yGrow);

                if (xGrow != 0)
                {
                    result.Height = Math.Max(MIN_HEIGHT, Round(result.Width / aspectRatio));
                }
                else if (yGrow != 0)
                {
                    result.Width = Math.Max(MIN_WIDTH, Round(result.Height * aspectRatio));
                }

                // too far--revert!
                if (result.Bottom > cont.Bottom || result.Right > cont.Right)
                    result = rect;

                return result;
            }

            public Rectangle ConformCropRectangle(Rectangle containerRect, Rectangle cropRect)
            {
                // try to preserve the same number of pixels
                int numOfPixels = cropRect.Width * cropRect.Height;

                PointF center = new PointF(cropRect.Left + cropRect.Width / 2f, cropRect.Top + cropRect.Height / 2f);

                double height = Math.Sqrt(numOfPixels / aspectRatio);
                double width = aspectRatio * height;

                PointF newLoc = new PointF(center.X - (float)width / 2f, center.Y - (float)height / 2f);
                return new Rectangle(
                    Convert.ToInt32(newLoc.X),
                    Convert.ToInt32(newLoc.Y),
                    Convert.ToInt32(width),
                    Convert.ToInt32(height));
            }

            public override AnchorStyles HitTest(Rectangle sizeRect, Point point)
            {
                AnchorStyles hitTest = base.HitTest(sizeRect, point);
                if (AnchorStyles.None == (hitTest & (AnchorStyles.Left | AnchorStyles.Right)))
                {
                    return AnchorStyles.None;
                }
                if (AnchorStyles.None == (hitTest & (AnchorStyles.Top | AnchorStyles.Bottom)))
                {
                    return AnchorStyles.None;
                }
                return hitTest;
            }

            protected override Rectangle DoResize(Point newLoc)
            {
                bool up = AnchorStyles.Top == (anchor & AnchorStyles.Top);
                bool left = AnchorStyles.Left == (anchor & AnchorStyles.Left);

                PointF origin = new PointF(
                    initialBounds.Left + (left ? initialBounds.Width : 0),
                    initialBounds.Top + (up ? initialBounds.Height : 0));

                double desiredAngle = Math.Atan2(
                    1d * (up ? -1 : 1),
                    1d * aspectRatio * (left ? -1 : 1));

                double actualAngle = Math.Atan2(
                    newLoc.Y - origin.Y,
                    newLoc.X - origin.X);

                double angleDiff = Math.Abs(actualAngle - desiredAngle);

                if (angleDiff >= Math.PI / 2) // >=90 degrees--too much angle!
                    return base.DoResize(new Point((int)origin.X, (int)origin.Y));

                double distance = Distance(origin, newLoc);

                double resizeMagnitude = Math.Cos(angleDiff) * distance;
                double xMagnitude = resizeMagnitude * Math.Cos(desiredAngle);
                double yMagnitude = resizeMagnitude * Math.Sin(desiredAngle);

                newLoc.X = Round(origin.X + xMagnitude);
                newLoc.Y = Round(origin.Y + yMagnitude);

                Rectangle newRect = base.DoResize(newLoc);

                if (container != null)
                {
                    Rectangle newRectConstrained = newRect;
                    newRectConstrained.Intersect(container.Value);
                    if (!newRectConstrained.Equals(newRect))
                    {
                        int newWidth = Round(Math.Min(newRectConstrained.Width, newRectConstrained.Height * aspectRatio));
                        int newHeight = Round(Math.Min(newRectConstrained.Height, newRectConstrained.Width / aspectRatio));
                        newRect.Width = newWidth;
                        newRect.Height = newHeight;
                        if (left)
                            newRect.Location = new Point(initialBounds.Right - newRect.Width, newRect.Top);
                        if (up)
                            newRect.Location = new Point(newRect.Left, initialBounds.Bottom - newRect.Height);
                    }
                }
                return newRect;
            }

            private double Distance(PointF pFrom, PointF pTo)
            {
                double deltaX = Math.Abs((double)(pTo.X - pFrom.X));
                double deltaY = Math.Abs((double)(pTo.Y - pFrom.Y));
                return Math.Sqrt(
                    deltaX * deltaX + deltaY * deltaY
                    );
            }

            private PointF Rotate(PointF origin, double angleRadians, PointF point)
            {
                float x, y;

                x = point.X - origin.X;
                y = point.Y - origin.Y;

                point.X = (float)(x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians));
                point.Y = (float)(y * Math.Cos(angleRadians) - x * Math.Sin(angleRadians));

                point.X += origin.X;
                point.Y += origin.Y;

                return point;
            }
        }

        public class FreeCropStrategy : CropStrategy
        {
            protected override Rectangle DoResize(Point newLoc)
            {
                Rectangle newRect = initialBounds;

                int deltaX = newLoc.X - initialLoc.X;
                int deltaY = newLoc.Y - initialLoc.Y;

                switch (anchor & (AnchorStyles.Left | AnchorStyles.Right))
                {
                    case AnchorStyles.Left:
                        if (MIN_WIDTH >= initialBounds.Width - deltaX)
                            deltaX = initialBounds.Width - MIN_WIDTH;
                        newRect.Width -= deltaX;
                        newRect.Offset(deltaX, 0);
                        break;
                    case AnchorStyles.Right:
                        if (MIN_WIDTH >= initialBounds.Width + deltaX)
                            deltaX = -initialBounds.Width + MIN_WIDTH;
                        newRect.Width += deltaX;
                        break;
                }

                switch (anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                {
                    case AnchorStyles.Top:
                        if (MIN_HEIGHT >= initialBounds.Height - deltaY)
                            deltaY = initialBounds.Height - MIN_HEIGHT;
                        newRect.Height -= deltaY;
                        newRect.Offset(0, deltaY);
                        break;
                    case AnchorStyles.Bottom:
                        if (MIN_HEIGHT >= initialBounds.Height + deltaY)
                            deltaY = -initialBounds.Height + MIN_HEIGHT;
                        newRect.Height += deltaY;
                        break;
                }

                return newRect;
            }

            public override Rectangle AdjustRectangle(Rectangle cont, Rectangle rect, int xOffset, int yOffset, int xGrow, int yGrow)
            {
                Debug.Assert(MathHelper.Max(xOffset, yOffset, xGrow, yGrow) <= 1,
                    "AdjustRectangle doesn't work well with values larger than 1--edge cases in FixedAspectRatioCropStrategy");
                Debug.Assert(MathHelper.Min(xOffset, yOffset, xGrow, yGrow) >= -1,
                    "AdjustRectangle doesn't work well with values larger than 1--edge cases in FixedAspectRatioCropStrategy");
                Debug.Assert((xOffset == 0 && yOffset == 0) || (xGrow == 0 && yGrow == 0),
                    "Beware of changing offset and size with the same call--weird things may happen as you approach the edges of the container");

                rect.X = Math.Max(cont.X, Math.Min(cont.Right - rect.Width, rect.X + xOffset));
                rect.Y = Math.Max(cont.Y, Math.Min(cont.Bottom - rect.Height, rect.Y + yOffset));
                rect.Width = Math.Max(MIN_WIDTH, Math.Min(cont.Right - rect.Left, rect.Width + xGrow));
                rect.Height = Math.Max(MIN_HEIGHT, Math.Min(cont.Bottom - rect.Top, rect.Height + yGrow));
                return rect;
            }
        }

        private class DualRects
        {
            private Rectangle r;
            private Rectangle v;
            private readonly PointF offset;
            private readonly SizeF scale;

            public DualRects(PointF offset, SizeF scale)
            {
                this.offset = offset;
                this.scale = scale;
            }

            public Rectangle Real
            {
                get { return r; }
                set
                {
                    r = value;
                    v = VirtualizeRect(r.X, r.Y, r.Width, r.Height);
                }
            }

            public Rectangle Virtual
            {
                get { return v; }
                set
                {
                    v = value;
                    r = RealizeRect(v.X, v.Y, v.Width, v.Height);
                }
            }

            public Rectangle RealizeRect(int x, int y, int width, int height)
            {
                return new Rectangle(
                    (int)Math.Round((x - offset.X) / scale.Width),
                    (int)Math.Round((y - offset.Y) / scale.Height),
                    (int)Math.Round(width / scale.Width),
                    (int)Math.Round(height / scale.Height)
                    );
            }

            public Rectangle VirtualizeRect(int x, int y, int width, int height)
            {
                return new Rectangle(
                    (int)Math.Round(x * scale.Width + offset.X),
                    (int)Math.Round(y * scale.Height + offset.Y),
                    (int)Math.Round(width * scale.Width),
                    (int)Math.Round(height * scale.Height)
                    );
            }
        }

        private class BoundsWithHandles
        {
            public const int HANDLE_SIZE = 5;

            private readonly bool includeSideHandles;
            private Rectangle bounds;

            public BoundsWithHandles(bool includeSideHandles)
            {
                this.includeSideHandles = includeSideHandles;
            }

            public Rectangle Bounds
            {
                get { return bounds; }
                set { bounds = value; }
            }

            public Rectangle[] GetHandles()
            {
                List<Rectangle> handles = new List<Rectangle>(includeSideHandles ? 8 : 4);
                // top left
                handles.Add(MakeRect(bounds.Left, bounds.Top));
                if (includeSideHandles)
                    handles.Add(MakeRect((bounds.Left + bounds.Right) / 2, bounds.Top));
                handles.Add(MakeRect(bounds.Right, bounds.Top));
                if (includeSideHandles)
                {
                    handles.Add(MakeRect(bounds.Left, (bounds.Top + bounds.Bottom) / 2));
                    handles.Add(MakeRect(bounds.Right, (bounds.Top + bounds.Bottom) / 2));
                }
                handles.Add(MakeRect(bounds.Left, bounds.Bottom));
                if (includeSideHandles)
                    handles.Add(MakeRect((bounds.Left + bounds.Right) / 2, bounds.Bottom));
                handles.Add(MakeRect(bounds.Right, bounds.Bottom));
                return handles.ToArray();
            }

            private static Rectangle MakeRect(int x, int y)
            {
                return new Rectangle(x - (HANDLE_SIZE / 2), y - (HANDLE_SIZE / 2), HANDLE_SIZE, HANDLE_SIZE);
            }
        }
    }
}
