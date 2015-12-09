// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Control that is useful for debugging element control boundaries and eventing.
    /// </summary>
    public class DebugBorderControl : BehaviorControl
    {
        private String _name;
        private Color _color;
        public DebugBorderControl(String name, Color c)
        {
            _name = name;
            _color = c;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawRectangle(new Pen(_color, 1f), new Rectangle(VirtualLocation, new Size(VirtualWidth - 1, VirtualHeight - 1)));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Trace.WriteLine(_name + ".onMouseMove: " + new Point(e.X, e.Y));
            base.OnMouseMove(e);
        }
    }
}
