// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for ElementFocusPainter.
    /// </summary>
    public class ElementFocusPainter
    {
        private const int HATCH_WIDTH = 4;
        private const int HATCH_ALPHA = (int)(255 * .60);
        private const int BORDER_ALPHA = (int)(255 * .20);
        private const int FOCUS_PADDING = 3;
        internal const int TOTAL_FOCUS_PADDING = FOCUS_PADDING + HATCH_WIDTH;

        Rectangle hatchRectangle;
        Rectangle borderRectangle;
        public ElementFocusPainter()
        {
        }

        public Rectangle LayoutFocusRectangle(Rectangle rect)
        {
            borderRectangle = new Rectangle(rect.Left - FOCUS_PADDING, rect.Top - FOCUS_PADDING, rect.Width + FOCUS_PADDING * 2 - 1, rect.Height + FOCUS_PADDING * 2 - 1);

            int flushHatchPadding = HATCH_WIDTH - 2;
            hatchRectangle = new Rectangle(borderRectangle.Left - flushHatchPadding, borderRectangle.Top - flushHatchPadding, borderRectangle.Width + flushHatchPadding * 2 + 1, borderRectangle.Height + flushHatchPadding * 2 + 1);
            return hatchRectangle;
        }

        public void DrawFocusRectangle(Graphics g)
        {

            HatchBrush b = new HatchBrush(HatchStyle.LightUpwardDiagonal,
                Color.FromArgb(HATCH_ALPHA, Color.Black),
                Color.FromArgb(0, Color.White));
            using (b)
                g.DrawRectangle(new Pen(b, HATCH_WIDTH), hatchRectangle);

            using (Pen borderPen = new Pen(Color.FromArgb(BORDER_ALPHA, Color.Black), 1))
                g.DrawRectangle(borderPen, borderRectangle);
        }
    }
}
