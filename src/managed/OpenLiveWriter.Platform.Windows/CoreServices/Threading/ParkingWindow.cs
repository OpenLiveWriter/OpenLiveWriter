// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Threading
{
    public class ParkingWindow
    {
        [ThreadStatic]
        private static Form parkingWindow;

        /// <summary>
        /// Returns a control that can be used for BeginInvoke.  The control
        /// will be owned by the current thread.
        /// </summary>
        public static Control Instance
        {
            get
            {
                Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA,
                    "Should ParkingWindow exist on non-STA threads?");

                if (parkingWindow == null)
                {
                    parkingWindow = new Form();
                    parkingWindow.Text = String.Format(CultureInfo.InvariantCulture, "{0} Task Window", ApplicationEnvironment.ProductName);
                    parkingWindow.FormBorderStyle = FormBorderStyle.None;
                    parkingWindow.MinimumSize = new Size(1, 1);
                    parkingWindow.Size = new Size(1, 1);
                    parkingWindow.Location = new Point(-32000, -32000);
                    // DO NOT set ShowInTaskbar to false. It causes the window to be destroyed when an entity is deleted. (!?!?!?!?!?)
                    //					parkingWindow.ShowInTaskbar = false;
                    //					parkingWindow.Opacity = 0.0;
                    //					parkingWindow.WindowState = FormWindowState.Minimized;
                    //					parkingWindow.Show();
                    IntPtr frcCreate = parkingWindow.Handle;
                    parkingWindow.Visible = false;
                }
                return parkingWindow;
            }
        }

        private ParkingWindow()
        {
        }
    }
}
