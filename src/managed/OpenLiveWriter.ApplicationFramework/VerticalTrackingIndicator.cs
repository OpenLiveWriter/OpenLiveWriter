// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Project31.CoreServices;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// Provides a "vertical tracking indicator" for use in splitter bars.
    /// </summary>
    internal class VerticalTrackingIndicator
    {
        /// <summary>
        /// SRCCOPY ROP.
        /// </summary>
        private const int SRCCOPY = 0x00cc0020;

        /// <summary>
        /// DCX_PARENTCLIP option for GetDCEx.
        /// </summary>
        private const int DCX_PARENTCLIP = 0x00000020;

        /// <summary>
        /// Valid indicator.  Used to ensure that calls to Begin, Update and End are valid when
        /// they are made.
        /// </summary>
        private bool valid = false;

        /// <summary>
        /// Control into which the tracking indicator will be drawn.
        /// </summary>
        private Control control;

        /// <summary>
        /// Graphics object for the control into which the tracking indicator will be drawn.
        /// </summary>
        private Graphics controlGraphics;

        /// <summary>
        /// DC for the control into which the tracking indicator will be drawn.
        /// </summary>
        private IntPtr controlDC;

        /// <summary>
        /// The capture bitmap.  Used to capture the portion of the control that is being
        /// overwritten by the tracking indicator so that it can be restored when the
        /// tracking indicator is moved to a new location.
        /// </summary>
        private Bitmap captureBitmap;

        /// <summary>
        /// Last capture location where the tracking indicator was drawn.
        /// </summary>
        private Point lastCaptureLocation;

        /// <summary>
        /// The tracking indicator bitmap.
        /// </summary>
        private Bitmap trackingIndicatorBitmap;

        /// <summary>
        /// DllImport of Win32 GetDCEx.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr region, System.Int32 dw);

        /// <summary>
        /// DllImport of GDI BitBlt.
        /// </summary>
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(	IntPtr hdcDest,     // handle to destination DC (device context)
                                            int nXDest,         // x-coord of destination upper-left corner
                                            int nYDest,         // y-coord of destination upper-left corner
                                            int nWidth,         // width of destination rectangle
                                            int nHeight,        // height of destination rectangle
                                            IntPtr hdcSrc,      // handle to source DC
                                            int nXSrc,          // x-coordinate of source upper-left corner
                                            int nYSrc,          // y-coordinate of source upper-left corner
                                            System.Int32 dwRop);// raster operation code

        /// <summary>
        /// DllImport of Win32 ReleaseDC.
        /// </summary>
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr dc);

        /// <summary>
        /// Initializes a new instance of the VerticalTrackingIndicator class.
        /// </summary>
        public VerticalTrackingIndicator()
        {
        }

        /// <summary>
        /// Begin a tracking indicator in the specified control using the specified rectangle.
        /// </summary>
        ///	<param name="control">The control into which the tracking indicator will be drawn.</param>
        /// <param name="rectangle">Rectangle structure that represents the tracking indicator rectangle, relative to the upper-left corner of the control into which it will be drawn.</param>
        public void Begin(Control control, Rectangle rectangle)
        {
            //	Can't Begin twice.
            Debug.Assert(!valid, "Invalid nested Begin", "You must first call the End method of a VerticalTrackingIndicator before calling its Begin method again.");
            if (valid)
                End();

            //	Save away the control.  We need this so we can call ReleaseDC later on.
            this.control = control;

            //	Get a DC for the "visible region" the specified control.  Children are not clipped
            //	for this DC, which allows us to draw the tracking indicator over them.
            controlDC = GetDCEx(control.Handle, System.IntPtr.Zero, DCX_PARENTCLIP);

            //	Get a graphics object for the DC.
            controlGraphics = Graphics.FromHdc(controlDC);

            //	Instantiate the capture bitmap.
            captureBitmap = new Bitmap(rectangle.Width, rectangle.Height);

            //	Instantiate and paint the tracking indicator bitmap.
            trackingIndicatorBitmap = new Bitmap(rectangle.Width, rectangle.Height);
            Graphics trackingIndicatorBitmapGraphics = Graphics.FromImage(trackingIndicatorBitmap);
            HatchBrush hatchBrush = new HatchBrush(	HatchStyle.Percent50,
                                                    Color.FromArgb(128, Color.Black),
                                                    Color.FromArgb(128, Color.White));
            trackingIndicatorBitmapGraphics.FillRectangle(hatchBrush, new Rectangle(0, 0, rectangle.Width, rectangle.Height));
            hatchBrush.Dispose();
            trackingIndicatorBitmapGraphics.Dispose();

            //	Draw the new tracking indicator.
            DrawTrackingIndicator(rectangle.Location);

            //	Valid now.
            valid = true;
        }

        /// <summary>
        /// Update the location of the tracking indicator.
        /// </summary>
        /// <param name="location">The new location of the tracking indicator.</param>
        public void Update(Point location)
        {
            //	Can't Update without a Begin.
            Debug.Assert(valid, "Invalid Update", "You must first call the Begin method of a VerticalTrackingIndicator before calling its Update method.");
            if (!valid)
                return;

            //	Undraw the last tracking indicator.
            UndrawLastTrackingIndicator();

            //	Draw the new tracking indicator.
            DrawTrackingIndicator(location);
        }

        /// <summary>
        /// End the tracking indicator.
        /// </summary>
        public void End()
        {
            //	Can't Update without a Begin.
            Debug.Assert(valid, "Invalid End", "You must first call the Begin method of a VerticalTrackingIndicator before calling its End method.");
            if (!valid)
                return;

            //	Undraw the last tracking indicator.
            UndrawLastTrackingIndicator();

            //	Cleanup the capture bitmap.
            captureBitmap.Dispose();

            //	Dispose of the graphics context and DC for the control.
            controlGraphics.Dispose();
            ReleaseDC(control.Handle, controlDC);

            //	Not valid anymore.
            valid = false;
        }

        /// <summary>
        /// "Undraw" the last tracking indicator.
        /// </summary>
        private void UndrawLastTrackingIndicator()
        {
            controlGraphics.DrawImageUnscaled(captureBitmap, lastCaptureLocation);
        }

        /// <summary>
        /// Draw the tracking indicator.
        /// </summary>
        private void DrawTrackingIndicator(Point location)
        {
            //	BitBlt the contents of the specified rectangle of the control to the capture bitmap.
            Graphics captureBitmapGraphics = Graphics.FromImage(captureBitmap);
            System.IntPtr captureBitmapDC = captureBitmapGraphics.GetHdc();
            BitBlt(	captureBitmapDC,			// handle to destination DC (device context)
                    0,							// x-coord of destination upper-left corner
                    0,							// y-coord of destination upper-left corner
                    captureBitmap.Width,		// width of destination rectangle
                    captureBitmap.Height,		// height of destination rectangle
                    controlDC,					// handle to source DC
                    location.X,					// x-coordinate of source upper-left corner
                    location.Y,					// y-coordinate of source upper-left corner
                    SRCCOPY);					// raster operation code
            captureBitmapGraphics.ReleaseHdc(captureBitmapDC);
            captureBitmapGraphics.Dispose();

            //	Draw the tracking indicator bitmap.
            controlGraphics.DrawImageUnscaled(trackingIndicatorBitmap, location);

            //	Remember the last capture location.  UndrawLastTrackingIndicator uses this value
            //	to undraw this tracking indicator at this location.
            lastCaptureLocation = location;
        }
    }
}
