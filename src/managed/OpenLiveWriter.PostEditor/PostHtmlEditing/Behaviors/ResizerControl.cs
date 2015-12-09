// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    public enum SizerMode { Normal, Resizing };
    public enum SizerHandle { None, TopLeft, TopRight, BottomLeft, BottomRight, Top, Left, Right, Bottom };
    public delegate void SizerModeEventHandler(SizerHandle handle, SizerMode mode);

    /// <summary>
    /// Summary description for ResizerControl.
    /// </summary>
    public class ResizerControl : BehaviorControl
    {
        public event SizerModeEventHandler SizerModeChanged;
        public event EventHandler Resized;

        //private const string IMAGE_RESOURCE_PATH = "PostHtmlEditing.Behaviors.Images." ;
        //private Bitmap activeHatchImage = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_RESOURCE_PATH + "ActiveHatch.png") ;

        internal const int SIZERS_PADDING = 10;
        private const int SIZERS_SIZE = 7;
        private const int penWidth = 1;
        private Rectangle tlCorner;
        private Rectangle trCorner;
        private Rectangle blCorner;
        private Rectangle brCorner;
        private Rectangle topSide;
        private Rectangle rightSide;
        private Rectangle bottomSide;
        private Rectangle leftSide;
        private SizerMode _mode;
        private SizerHandle _activeHandle;
        private Point _sizerModeLocation;
        private Size _resizeInitialSize;
        private bool _allowAspectRatioDistortion = true;
        private Size _aspectRatioOffset = Size.Empty;
        private ElementFocusPainter focusPainter;

        public ResizerControl()
        {
            focusPainter = new ElementFocusPainter();
        }

        public Size AspectRatioOffset
        {
            get { return _aspectRatioOffset; }
            set { _aspectRatioOffset = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            focusPainter.DrawFocusRectangle(e.Graphics);

            if (Resizable)
            {
                PaintSizer(e.Graphics, tlCorner);
                PaintSizer(e.Graphics, trCorner);
                PaintSizer(e.Graphics, blCorner);
                PaintSizer(e.Graphics, brCorner);

                if (AllowAspectRatioDistortion)
                {
                    PaintSizer(e.Graphics, topSide);
                    PaintSizer(e.Graphics, rightSide);
                    PaintSizer(e.Graphics, bottomSide);
                    PaintSizer(e.Graphics, leftSide);
                }
            }
        }

        private void PaintSizer(Graphics g, Rectangle rect)
        {
            g.FillRectangle(new SolidBrush(Color.White), rect);
            g.DrawRectangle(new Pen(Color.Black, penWidth), rect);
        }

        private void UpdateActiveHandle(Point p)
        {
            //Warning: we only want to reset the handle if we are not resizing!
            //Otherwise, we will lose our resize operation if the mouse slips outside
            //of the corner.
            if (Mode == SizerMode.Resizing)
            {
                return;
            }

            ActiveHandle = GetHandleForPoint(p);
        }

        public SizerHandle GetHandleForPoint(Point p)
        {
            if (tlCorner.Contains(p))
                return SizerHandle.TopLeft;
            else if (trCorner.Contains(p))
                return SizerHandle.TopRight;
            else if (blCorner.Contains(p))
                return SizerHandle.BottomLeft;
            else if (brCorner.Contains(p))
                return SizerHandle.BottomRight;
            else if (topSide.Contains(p))
                return SizerHandle.Top;
            else if (rightSide.Contains(p))
                return SizerHandle.Right;
            else if (bottomSide.Contains(p))
                return SizerHandle.Bottom;
            else if (leftSide.Contains(p))
                return SizerHandle.Left;
            else
                return SizerHandle.None;
        }

        public override bool HitTestPoint(Point testPoint)
        {
            return GetHandleForPoint(testPoint) != SizerHandle.None;
        }

        protected override void OnLayout(EventArgs e)
        {
            base.OnLayout(e);

            focusPainter.LayoutFocusRectangle(CalculateRelativeElementRectangle());

            if (Resizable)
            {
                Size size = new Size(SIZERS_SIZE - penWidth, SIZERS_SIZE - penWidth);
                tlCorner = new Rectangle(new Point(0, 0), size);
                trCorner = new Rectangle(new Point(VirtualWidth - SIZERS_SIZE, 0), size);
                blCorner = new Rectangle(new Point(0, VirtualHeight - SIZERS_SIZE), size);
                brCorner = new Rectangle(new Point(VirtualWidth - SIZERS_SIZE, VirtualHeight - SIZERS_SIZE), size);

                if (AllowAspectRatioDistortion)
                {
                    topSide = Utility.CenterInRectangle(size, new Rectangle(new Point(0, 0), new Size(VirtualWidth, SIZERS_SIZE)));
                    rightSide = Utility.CenterInRectangle(size, new Rectangle(new Point(VirtualWidth - SIZERS_SIZE, 0), new Size(SIZERS_SIZE, VirtualHeight)));
                    bottomSide = Utility.CenterInRectangle(size, new Rectangle(new Point(0, VirtualHeight - SIZERS_SIZE), new Size(VirtualWidth, SIZERS_SIZE)));
                    leftSide = Utility.CenterInRectangle(size, new Rectangle(new Point(0, 0), new Size(SIZERS_SIZE, VirtualHeight)));
                }
                else
                {
                    topSide = Rectangle.Empty;
                    rightSide = Rectangle.Empty;
                    bottomSide = Rectangle.Empty;
                    leftSide = Rectangle.Empty;
                }
            }
        }

        private Rectangle CalculateRelativeElementRectangle()
        {
            Point p = VirtualLocation;
            //return new Rectangle(new Point(-p.X, -p.Y), Parent.ElementRectangle.Size);
            return new Rectangle(SIZERS_PADDING, SIZERS_PADDING, VirtualWidth - SIZERS_PADDING * 2, VirtualHeight - SIZERS_PADDING * 2);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Point p = new Point(e.X, e.Y);
            UpdateActiveHandle(p);
            if (_activeHandle != SizerHandle.None)
                OnSizerModeChanged(_activeHandle, SizerMode.Resizing);

            Point pntGlobal = Parent.TransformLocalToGlobal(new Point(e.X, e.Y));
            _sizerModeLocation = pntGlobal; // new Point(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            ResetModeToNormal();
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            ActiveHandle = SizerHandle.None;
            ResetModeToNormal();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            UpdateActiveHandle(new Point(e.X, e.Y));

            if ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left)
            {
                //we don't get mouse up events when the mouse leaves the editor
                ResetModeToNormal();
            }
            else
            {
                if (Mode == SizerMode.Resizing)
                {
                    Point pntGlobal = Parent.TransformLocalToGlobal(new Point(e.X, e.Y));
                    int xDelta = pntGlobal.X - _sizerModeLocation.X;
                    int yDelta = pntGlobal.Y - _sizerModeLocation.Y;

                    switch (_activeHandle)
                    {
                        case SizerHandle.TopLeft:
                            UpdateElementSize(_resizeInitialSize.Width - xDelta, _resizeInitialSize.Height - yDelta, true);
                            break;
                        case SizerHandle.TopRight:
                            UpdateElementSize(_resizeInitialSize.Width + xDelta, _resizeInitialSize.Height - Math.Min(yDelta, 0)/*hackish but creates a better experience when resizing using the topright knob*/ , true);
                            break;
                        case SizerHandle.BottomLeft:
                            UpdateElementSize(_resizeInitialSize.Width - xDelta, _resizeInitialSize.Height + yDelta, true);
                            break;
                        case SizerHandle.BottomRight:
                            UpdateElementSize(_resizeInitialSize.Width + xDelta, _resizeInitialSize.Height + yDelta, true);
                            break;
                        case SizerHandle.Top:
                            UpdateElementSize(_resizeInitialSize.Width, _resizeInitialSize.Height - yDelta, false);
                            break;
                        case SizerHandle.Right:
                            UpdateElementSize(_resizeInitialSize.Width + xDelta, _resizeInitialSize.Height, false);
                            break;
                        case SizerHandle.Bottom:
                            UpdateElementSize(_resizeInitialSize.Width, _resizeInitialSize.Height + yDelta, false);
                            break;
                        case SizerHandle.Left:
                            UpdateElementSize(_resizeInitialSize.Width - xDelta, _resizeInitialSize.Height, false);
                            break;
                    }
                }
            }
        }

        private void UpdateElementSize(int width, int height, bool preserveAspectRatio)
        {
            Size newSize = new Size(Math.Max(0, width), Math.Max(0, height));
            if (preserveAspectRatio)
            {
                newSize = Utility.GetScaledMaxSize(_resizeInitialSize - _aspectRatioOffset, newSize - _aspectRatioOffset) + _aspectRatioOffset;
            }
            SizerSize = newSize;
        }

        public Size SizerSize
        {
            get
            {
                return new Size(VirtualWidth - SIZERS_PADDING * 2, VirtualHeight - SIZERS_PADDING * 2);
            }
            set
            {
                VirtualSize = new Size(value.Width + SIZERS_PADDING * 2, value.Height + SIZERS_PADDING * 2);
            }
        }

        public SizerHandle ActiveSizerHandle
        {
            get
            {
                return _activeHandle;
            }
        }

        protected virtual void OnSizerModeChanged(SizerHandle handle, SizerMode mode)
        {
            bool resizeStarted = _mode == SizerMode.Normal && mode == SizerMode.Resizing;
            ActiveHandle = handle;
            _mode = mode;

            if (resizeStarted)
                OnResizeStart();
            if (SizerModeChanged != null)
                SizerModeChanged(handle, mode);
        }

        private void OnResizeStart()
        {
            //save the current size
            _resizeInitialSize = SizerSize;
        }

        protected override void OnVirtualSizeChanged(EventArgs e)
        {
            base.OnVirtualSizeChanged(e);

            if (Resized != null)
            {
                Resized(this, EventArgs.Empty);
            }
        }

        public bool AllowAspectRatioDistortion
        {
            get
            {
                return _allowAspectRatioDistortion;
            }
            set
            {
                if (_allowAspectRatioDistortion != value)
                {
                    _allowAspectRatioDistortion = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public bool Resizable
        {
            get
            {
                return _resizable;
            }
            set
            {
                if (_resizable != value)
                {
                    _resizable = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }
        private bool _resizable;

        public SizerMode Mode
        {
            get
            {
                return _mode;
            }
        }

        private SizerHandle ActiveHandle
        {
            get
            {
                return _activeHandle;
            }
            set
            {
                if (_activeHandle != value)
                {
                    _activeHandle = value;
                }
            }
        }

        private void ResetModeToNormal()
        {
            if (_mode != SizerMode.Normal)
            {
                OnSizerModeChanged(_activeHandle, SizerMode.Normal);
            }
        }
    }
}
