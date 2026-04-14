// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Bidirectional (RTL/LTR) text and graphics rendering support.
    /// </summary>
    public interface IBidiSupport
    {
        void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color, bool isRtl);
        Size MeasureText(Graphics g, string text, Font font, bool isRtl);
        void DrawIcon(Graphics g, Icon icon, Rectangle bounds, bool isRtl);
        Rectangle AdjustLayoutRect(Rectangle containerBounds, Rectangle childBounds, bool isRtl);
    }
}
