// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization.Bidi
{
    /// <summary>
    /// BidiLayout is sort of like BidiGraphics but for laying out
    /// controls relative to their parents (or other bounding box).
    ///
    /// Usually we just lay things out as left-to-right and then
    /// if we're in RTL, recursively flip everything around using
    /// BidiHelper.RtlLayoutFixup. However, when laying out controls
    /// that are already on the screen, this can lead to jarring
    /// and latent layouts which are visible to the user. In those
    /// cases you can wrap the control in BidiLayout in your layout
    /// code, and then do your usual LTR layout, and BidiLayout will
    /// take care of translating the control position.
    /// </summary>
    /// <typeparam name="TControl">The specific type of the control
    /// that is being wrapped.</typeparam>
    public struct BidiLayout<TControl> where TControl : Control
    {
        private readonly TControl c;
        private readonly Rectangle? bounds;

        public BidiLayout(TControl c, Rectangle? bounds)
        {
            this.c = c;
            this.bounds = bounds;
        }

        public BidiLayout(TControl c)
        {
            this.c = c;
            this.bounds = null;
        }

        public static implicit operator TControl(BidiLayout<TControl> o)
        {
            return o.c;
        }

        public Rectangle Bounds
        {
            get
            {
                if (!RTL)
                    return c.Bounds;

                Rectangle b = c.Bounds;
                b.X = TranslateX(b.Right);
                return b;
            }
            set
            {
                if (!RTL)
                    c.Bounds = value;
                else
                {
                    value.X = TranslateX(value.Right);
                    c.Bounds = value;
                }
            }
        }

        public Point Location
        {
            get { return Bounds.Location; }
            set { Bounds = new Rectangle(value, Size); }
        }

        public Size Size
        {
            get { return c.Size; }
            set
            {
                if (!RTL)
                    c.Size = value;
                else
                {
                    int deltaX = value.Width - c.Width;
                    c.Size = value;
                    c.Left -= deltaX;
                }
            }
        }

        public int Left
        {
            get { return Bounds.Left; }
            set { Bounds = new Rectangle(value, Top, Width, Height); }
        }

        public int Top
        {
            get { return c.Top; }
            set { c.Top = value; }
        }

        public int Right
        {
            get { return Bounds.Right; }
        }

        public int Bottom
        {
            get { return c.Bottom; }
        }

        public bool Visible
        {
            get { return c.Visible; }
            set { c.Visible = value; }
        }

        public int Width
        {
            get { return c.Width; }
            set { Size = new Size(value, c.Height); }
        }

        public int Height
        {
            get { return c.Height; }
            set { c.Height = value; }
        }

        public AnchorStyles Anchor
        {
            get { return FlipAnchor(c.Anchor); }
            set { c.Anchor = FlipAnchor(value); }
        }

        private AnchorStyles FlipAnchor(AnchorStyles value)
        {
            if (!RTL)
                return value;

            if ((value & AnchorStyles.Left) == AnchorStyles.Left
                ^ (value & AnchorStyles.Right) == AnchorStyles.Right)
            {
                value ^= AnchorStyles.Left | AnchorStyles.Right;
            }

            return value;
        }

        private int TranslateX(int x)
        {
            if (!RTL)
                return x;

            Rectangle context = bounds ?? c.Parent.ClientRectangle;
            x -= context.Left;
            x = context.Width - x;
            x += context.Left;
            return x;
        }

        private bool RTL
        {
            get { return BidiHelper.IsRightToLeft; }
        }
    }
}
