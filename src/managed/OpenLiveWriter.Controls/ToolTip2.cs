// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    public class ToolTip2 : ToolTip
    {
        public ToolTip2(IContainer container) : base(container)
        {
            if (BidiHelper.IsRightToLeft)
            {
                OwnerDraw = true;
                Draw += ToolTip2_Draw;
            }
        }

        public ToolTip2()
            : base()
        {
            if (BidiHelper.IsRightToLeft)
            {
                OwnerDraw = true;
                Draw += ToolTip2_Draw;
            }
        }

        static void ToolTip2_Draw(object sender, DrawToolTipEventArgs e)
        {
            Debug.Assert(BidiHelper.IsRightToLeft, "Tooltip2 should only be ownerdraw when running RTL");
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText(TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.RightToLeft | TextFormatFlags.Right);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (OwnerDraw)
                Draw -= ToolTip2_Draw;
        }
    }
}
