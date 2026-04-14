// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsBidiSupport : IBidiSupport
    {
        public void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color, bool isRtl)
        {
            using (Brush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, bounds);
            }
        }

        public Size MeasureText(Graphics g, string text, Font font, bool isRtl)
        {
            SizeF size = g.MeasureString(text, font);
            return new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
        }

        public void DrawIcon(Graphics g, Icon icon, Rectangle bounds, bool isRtl)
        {
            g.DrawIcon(icon, bounds);
        }

        public Rectangle AdjustLayoutRect(Rectangle containerBounds, Rectangle childBounds, bool isRtl)
        {
            if (!isRtl)
                return childBounds;

            int mirroredX = containerBounds.Right - (childBounds.X - containerBounds.X) - childBounds.Width;
            return new Rectangle(mirroredX, childBounds.Y, childBounds.Width, childBounds.Height);
        }
    }
}
