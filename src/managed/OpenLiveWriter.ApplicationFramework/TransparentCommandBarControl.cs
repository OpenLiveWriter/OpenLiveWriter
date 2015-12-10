// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.UI;

namespace OpenLiveWriter.ApplicationFramework
{
    public class TransparentCommandBarControl : CommandBarControl
    {
        private Control _parent;

        public TransparentCommandBarControl(CommandBarLightweightControl commandBar, CommandBarDefinition commandBarDefinition) : base(commandBar, commandBarDefinition)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_parent != null && !_parent.IsDisposed)
                    _parent.Invalidated -= new InvalidateEventHandler(TransparentCommandBarControl_Invalidated);
            }
            base.Dispose(disposing);
        }

        public void ForceLayout()
        {
            CommandBarLightweightControl.PerformLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_parent == null)
            {
                _parent = (Control)VirtualTransparency.VirtualParent(this);
                _parent.Invalidated += new InvalidateEventHandler(TransparentCommandBarControl_Invalidated);
            }
            VirtualTransparency.VirtualPaint((IVirtualTransparencyHost)_parent, this, pevent);
        }

        private void TransparentCommandBarControl_Invalidated(object sender, InvalidateEventArgs e)
        {
            Point absLoc = _parent.PointToScreen(e.InvalidRect.Location);
            Point relLoc = PointToClient(absLoc);
            Rectangle relRect = new Rectangle(relLoc, e.InvalidRect.Size);
            if (ClientRectangle.IntersectsWith(relRect))
            {
                relRect.Intersect(ClientRectangle);
                Invalidate(relRect);
            }
        }
    }
}
