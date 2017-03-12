// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Draws a focus outline
    /// </summary>
    public class FocusControl : BehaviorControl
    {
        private ElementFocusPainter focusPainter;

        private ElementControlBehavior _parent;
        public FocusControl(ElementControlBehavior parent)
        {
            _parent = parent;
            _parent.ElementSizeChanged += new EventHandler(_parent_ElementSizeChanged);
            focusPainter = new ElementFocusPainter();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            focusPainter.DrawFocusRectangle(e.Graphics);
        }

        protected override void OnLayout(EventArgs e)
        {
            int padding = ElementFocusPainter.TOTAL_FOCUS_PADDING;
            Rectangle relativeElementRectangle = new Rectangle(padding, padding, VirtualWidth - padding * 2, VirtualHeight - padding * 2);
            focusPainter.LayoutFocusRectangle(relativeElementRectangle);
        }

        private void _parent_ElementSizeChanged(object sender, EventArgs e)
        {
            Rectangle elementRect = Parent.ElementRectangle;
            int padding = ElementFocusPainter.TOTAL_FOCUS_PADDING;
            Rectangle newVirtualRect = new Rectangle(elementRect.X - padding, elementRect.Y - padding,
                elementRect.Width + padding * 2, elementRect.Height + padding * 2);
            VirtualLocation = newVirtualRect.Location;
            VirtualSize = newVirtualRect.Size;
        }
    }
}
