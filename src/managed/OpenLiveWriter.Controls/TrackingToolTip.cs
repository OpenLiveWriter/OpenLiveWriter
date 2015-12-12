// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{

    /// <summary>
    /// Implements a "tracking" ToolTip (using the native Win32 tooltip api). A
    /// tracking tooltip can be positioned anywhere on the screen irrespective
    /// of parent controls, etc. Normally you should use NOT use this class and
    /// instead shoould use the .NET ToolTip class -- this class is used for
    /// scenarios where you are hosting a non .NET window (e.g. MSHTML editor)
    /// that fully occludes your .NET control and therefore prevents the display
    /// of .NET tooltips.
    /// </summary>
    public class TrackingToolTip : NativeWindow, IDisposable
    {
        /// <summary>
        /// Initialize tracking tooltip control
        /// </summary>
        public TrackingToolTip()
        {
            // create params for a tracking tooltip window
            CreateParams cp = new CreateParams();
            cp.ExStyle = (int)WS.EX_TOPMOST;
            cp.ClassName = WINDOW_CLASS.TOOLTIPS;
            cp.Style = unchecked((int)(WS.POPUP | TTS.ALWAYSTIP | TTS.NOPREFIX));

            // create the winow
            CreateHandle(cp);
        }

        /// <summary>
        /// Set the location (in screen coordiantes) of the tooltip. This location will
        /// be reflected the next time the tooltip is shown
        /// </summary>
        public Point Location
        {
            get
            {
                return location;
            }
            set
            {
                if (location != value)
                {
                    // save value
                    location = value;

                    // update location of any displayed tool
                    if (currentTool != null)
                        currentTool.Location = location;
                }
                GC.KeepAlive(this);
            }
        }

        /// <summary>
        /// Set the current tool caption (set null to hide the tooltip)
        /// </summary>
        /// <param name="caption">caption</param>
        public void SetToolTip(string caption)
        {
            // for null case, hide any existing tool and return
            if (caption == null)
            {
                // hide existing tool
                if (currentTool != null)
                {
                    currentTool.Hide();
                    currentTool = null;
                }

                // all done!
                return;
            }

            // see if this caption has already been added as a tool. If not then
            // add a new tool for this caption
            Tool tool = tools[caption] as Tool;
            if (tool == null)
                tool = AddTool(caption);

            // if the tool is not the same as the current tool then hide the current
            // tool and show this one
            if (tool != currentTool)
            {
                // hide the existing tool (if any)
                if (currentTool != null)
                    currentTool.Hide();

                // update current tool
                currentTool = tool;

                // update location
                currentTool.Location = Location;

                // show the tool
                currentTool.Show();
            }
        }

        /// <summary>
        /// Dispose unmanaged resources
        /// </summary>
        public void Dispose()
        {
            // dispose all contained tool structures
            foreach (DictionaryEntry toolEntry in tools)
            {
                Tool tool = toolEntry.Value as Tool;
                tool.Dispose();
            }

            // destroy window
            DestroyHandle();

            // suppress finalize to help verify that dispose is always called
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add a tool to the tooltip control
        /// </summary>
        /// <param name="toolText">text for tooltip</param>
        /// <returns>tool tip tool that can be positioned, shown, hidden, etc.</returns>
        private Tool AddTool(string caption)
        {
            // create the tool
            Tool tool = new Tool(Handle, caption, new UIntPtr(Convert.ToUInt32(tools.Count)));

            // add it to our internal list and return it
            tools.Add(caption, tool);

            // return it
            return tool;
        }

        /// <summary>
        /// Verify dispose was called
        /// </summary>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        ~TrackingToolTip()
        {
            Debug.Fail("Failed to dispose TrackingToolTip!");
        }

        /// <summary>
        /// dictionary of tools (caption -> Tool) associated with this tooltip control
        /// </summary>
        private IDictionary tools = new Hashtable();

        /// <summary>
        /// Current location of the tooltip
        /// </summary>
        private Point location;

        /// <summary>
        /// Tool currently displayed by the tooltip
        /// </summary>
        private Tool currentTool;

        /// <summary>
        /// Innner class: Tool associated with a TrackingToolTip
        /// </summary>
        private class Tool : IDisposable
        {
            /// <summary>
            /// Initialize a tool with its associated tool tip control, caption, and id
            /// </summary>
            /// <param name="toolTipCtl">associated tool tip control</param>
            /// <param name="toolCaption">tool caption</param>
            /// <param name="id">tool id</param>
            public Tool(IntPtr toolTipCtl, string toolCaption, UIntPtr id)
            {
                // save reference to owning tooltip control
                toolTipControl = toolTipCtl;

                // fill out the toolinfo struct
                toolInfo = new TOOLINFO();
                toolInfo.cbSize = (uint)Marshal.SizeOf(toolInfo);
                toolInfo.uFlags = (BidiHelper.IsRightToLeft ? TTF.TRACK | TTF.ABSOLUTE | TTF_RTLREADING : TTF.TRACK | TTF.ABSOLUTE);
                toolInfo.hwnd = IntPtr.Zero;
                toolInfo.hinst = IntPtr.Zero;
                toolInfo.uId = id;
                toolInfo.lpszText = toolCaption;
                toolInfo.rect.left = 0;
                toolInfo.rect.top = 0;
                toolInfo.rect.right = 0;
                toolInfo.rect.bottom = 0;
                toolInfo.lParam = IntPtr.Zero;

                // get an unmanaged pointer to the structure (will be freed on Dispose)
                pToolInfo = Marshal.AllocHGlobal(Marshal.SizeOf(toolInfo));
                Marshal.StructureToPtr(toolInfo, pToolInfo, false);

                // add the tool to the window
                User32.SendMessage(toolTipControl, TTM.ADDTOOL, UIntPtr.Zero, pToolInfo);
            }
            private const int TTF_RTLREADING = 0x0004;

            /// <summary>
            /// Set the location (in screen coordiantes) of the tooltip for the tool
            /// </summary>
            public Point Location
            {
                get
                {
                    return location;
                }
                set
                {
                    location = value;
                    User32.SendMessage(toolTipControl, TTM.TRACKPOSITION, UIntPtr.Zero, MessageHelper.MAKELONG(location.X, location.Y));
                    GC.KeepAlive(this);
                }
            }

            /// <summary>
            /// Show the ToolTip for the tool
            /// </summary>
            public void Show()
            {
                User32.SendMessage(toolTipControl, TTM.TRACKACTIVATE, new UIntPtr(1), pToolInfo);
                GC.KeepAlive(this);
            }

            /// <summary>
            /// Hide the ToolTip for the tool
            /// </summary>
            public void Hide()
            {
                User32.SendMessage(toolTipControl, TTM.TRACKACTIVATE, UIntPtr.Zero, pToolInfo);
                GC.KeepAlive(this);
            }

            /// <summary>
            /// Dispose the tool
            /// </summary>
            public void Dispose()
            {
                // always hide if shown
                Hide();

                // free unmanaged memory
                if (pToolInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(pToolInfo);

                // used to verify that Dispose is always called
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Finalize should NEVER be called since we suppress it in Dispose
            /// </summary>
            ~Tool()
            {
                Debug.Fail("Must Dispose instances of Tool class!");
            }

            /// <summary>
            /// ToolTip control associated with this tool
            /// </summary>
            private IntPtr toolTipControl;

            /// <summary>
            /// TOOLINFO structure associated with this tool
            /// </summary>
            private TOOLINFO toolInfo;

            /// <summary>
            /// Unmanaged pointer to TOOLINFO structure
            /// </summary>
            IntPtr pToolInfo = IntPtr.Zero;

            /// <summary>
            /// Location (in screen coordinates of tool)
            /// </summary>
            private Point location;
        }
    }

}

