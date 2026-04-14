// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.UI
{
    public interface IVirtualTransparencyHost
    {
        void Paint(PaintEventArgs args);
    }

    public class VirtualTransparency
    {
        public static IVirtualTransparencyHost VirtualParent(Control child)
        {
            for (Control parent = child.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is IVirtualTransparencyHost)
                {
                    return (IVirtualTransparencyHost)parent;
                }
            }
            return null;
        }

        public static void VirtualPaint(Control child, PaintEventArgs args)
        {
            IVirtualTransparencyHost parent = VirtualParent(child);
            Debug.Assert(parent != null, "No virtual transparency host found in ancestor chain of " + child.Name);
            VirtualPaint(parent, child, args);
        }

        public static void VirtualPaint(IVirtualTransparencyHost host, Control child, PaintEventArgs args)
        {
            Control hostControl = (Control)host;
            bool rtl = host is Form && ((Form)host).RightToLeft == RightToLeft.Yes && ((Form)host).RightToLeftLayout;

            Point childLocation = child.PointToScreen(new Point(0, 0));
            Point hostLocation = hostControl.PointToScreen(new Point(rtl ? hostControl.ClientSize.Width : 0, 0));
            Point relativeChildLocation = new Point(childLocation.X - hostLocation.X, childLocation.Y - hostLocation.Y);

            if (relativeChildLocation == Point.Empty)
            {
                // no translation transform required
                host.Paint(args);
                return;
            }

            Rectangle relativeClipRectangle = args.ClipRectangle;
            relativeClipRectangle.Offset(relativeChildLocation);
            args.Graphics.SetClip(relativeClipRectangle, CombineMode.Replace);

            // Global transformations applied in GDI+ land don't apply to
            // GDI calls. So we need to drop down to GDI, apply a transformation
            // there, then wrap with GDI+.

            IntPtr hdc = args.Graphics.GetHdc();
            try
            {
                Gdi32.GraphicsMode oldGraphicsMode = Gdi32.SetGraphicsMode(hdc, Gdi32.GraphicsMode.Advanced);

                Gdi32.XFORM xformOrig;
                Gdi32.GetWorldTransform(hdc, out xformOrig);
                try
                {
                    Gdi32.XFORM xform = xformOrig;
                    xform.eDx -= relativeChildLocation.X;
                    xform.eDy -= relativeChildLocation.Y;
                    Gdi32.SetWorldTransform(hdc, ref xform);

                    using (Graphics g = Graphics.FromHdc(hdc))
                    {
                        host.Paint(new PaintEventArgs(g, relativeClipRectangle));
                    }
                }
                finally
                {
                    Gdi32.SetWorldTransform(hdc, ref xformOrig);
                    Gdi32.SetGraphicsMode(hdc, oldGraphicsMode);
                }
            }
            finally
            {
                args.Graphics.ReleaseHdc(hdc);
            }
        }
    }
}
