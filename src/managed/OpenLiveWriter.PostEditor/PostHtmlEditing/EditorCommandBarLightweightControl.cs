// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class EditorCommandBarLightweightControl : CommandBarLightweightControl
    {
        private Bitmap _contextMenuArrowBitmap = ResourceHelper.LoadAssemblyResourceBitmap(typeof(Command).Assembly, "Images.HIG.BlackDropArrow.png");
        private Bitmap _contextMenuArrowBitmapDisabled = ImageHelper.MakeDisabled(ResourceHelper.LoadAssemblyResourceBitmap(typeof(Command).Assembly, "Images.HIG.BlackDropArrow.png"));
        private Bitmap _dropShadowTop = ResourceHelper.LoadAssemblyResourceBitmap(typeof(Command).Assembly, "Images.HIG.DropShadowTop.png");

        public EditorCommandBarLightweightControl(IContainer container) : base(container)
        {
        }

        public EditorCommandBarLightweightControl()
        {
        }

        public override int TopLayoutMargin
        {
            get { return 5; }
        }

        public override int BottomLayoutMargin
        {
            get
            {
                return 5;
            }
        }

        public override int LeftLayoutMargin
        {
            get { return 0; }
        }

        public override int RightLayoutMargin
        {
            get { return 0; }
        }

        public bool DrawVerticalLine
        {
            get
            {
                return _drawVerticalLine;
            }
            set
            {
                _drawVerticalLine = value;
                Invalidate();
            }
        }
        private bool _drawVerticalLine = false;

        public int VerticalLineX
        {
            get
            {
                return _verticalLineX;
            }
            set
            {
                _verticalLineX = value;
                Invalidate();
            }
        }
        private int _verticalLineX = -1;

        public override Bitmap ContextMenuArrowBitmap
        {
            get { return _contextMenuArrowBitmap; }
        }

        public override Bitmap ContextMenuArrowBitmapDisabled
        {
            get { return _contextMenuArrowBitmapDisabled; }
        }

        public override Color TopColor
        {
            get
            {
                return ColorizedResources.Instance.SecondaryToolbarColor;
            }
        }

        public override Color BottomColor
        {
            get
            {
                return ColorizedResources.Instance.SecondaryToolbarColor;
            }
        }

        public override Color BottomBevelFirstLineColor
        {
            get
            {
                return Color.Transparent;
            }
        }

        public override Color BottomBevelSecondLineColor
        {
            get
            {
                return Color.Transparent;
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, e.ClipRectangle);

            int shadowHeight = _dropShadowTop.Height;

            Rectangle r = VirtualClientRectangle;
            r.Y = 0;
            r.Height = shadowHeight;

            GraphicsHelper.TileFillScaledImageHorizontally(g, _dropShadowTop, r);

            shadowHeight = ColorizedResources.Instance.DropShadowBitmap.Height;

            r = VirtualClientRectangle;
            r.Y = VirtualHeight - shadowHeight;
            r.Height = shadowHeight;
            e.Graphics.FillRectangle(Brushes.White, r);

            GraphicsHelper.TileFillScaledImageHorizontally(g, ColorizedResources.Instance.DropShadowBitmap, r);

            if (_drawVerticalLine && _verticalLineX > -1)
            {
                using (Pen p = new Pen(ColorizedResources.Instance.BorderDarkColor))
                    e.Graphics.DrawLine(p, _verticalLineX, r.Y, _verticalLineX, VirtualClientRectangle.Bottom);
            }

        }
    }
}
