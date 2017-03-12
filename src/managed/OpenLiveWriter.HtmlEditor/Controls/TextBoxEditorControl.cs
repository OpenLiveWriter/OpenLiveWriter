// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.HtmlEditor.Controls
{

    public class TextBoxEditorControl : TextBox
    {
        public event ContextMenuTriggeredEventHandler ContextMenuTriggered;

        #region Protected Method Overrides

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows Message to process.</param>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            //	Dispatch the message.
            switch (m.Msg)
            {
                //	The WM_CONTEXTMENU message notifies a window that the user clicked the right
                //	mouse button (right clicked) in the window (or performed some other action that
                //	will display the context menu).
                case (int)WM.CONTEXTMENU:
                    {

                        //	Crack out x and y position.  This is in screen coordinates.
                        int x = MessageHelper.LOWORDToInt32(m.LParam);
                        int y = MessageHelper.HIWORDToInt32(m.LParam);
                        if (-1 == x && -1 == y)
                        {
                            Point newPoint = PointToScreen(new Point(0, 0));
                            x = newPoint.X;
                            y = newPoint.Y;
                        }
                        OnContextMenuTriggered(new ContextMenuTriggeredEventArgs(x, y));

                        //	Done!
                        return;
                    }
            }

            //	Call the base class's method.
            try
            {
                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                Trace.Fail("TextBoxEditorControl WndProc Exception", e.ToString());
            }
        }

        #endregion Protected Method Overrides

        protected void OnContextMenuTriggered(ContextMenuTriggeredEventArgs e)
        {
            if (ContextMenuTriggered != null)
                ContextMenuTriggered(this, e);
        }

        public delegate void ContextMenuTriggeredEventHandler(object sender, ContextMenuTriggeredEventArgs eventArgs);

        public class ContextMenuTriggeredEventArgs : EventArgs
        {
            private Point _location;

            public ContextMenuTriggeredEventArgs(int x, int y) : base()
            {
                _location = new Point(x, y);
            }

            public Point ContextMenuLocation
            {
                get
                {
                    return _location;
                }
            }
        }
    }
}
