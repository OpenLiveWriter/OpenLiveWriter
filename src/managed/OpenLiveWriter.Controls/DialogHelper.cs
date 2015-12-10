// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for DialogHelper.
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// Enumerate windows by calling the specified callback proc for each top-level
        /// window in the system. If a custom object is specified this object will
        /// also be passed to the callback proc (allows the callback proc to modify
        /// state, build a list, etc.)
        /// </summary>
        /// <param name="callbackProc">callback proc</param>
        /// <param name="customParam">custom data</param>
        public static void EnumerateChildWindows(IntPtr hWndParent, EnumerateChildWindowsDelegate callbackProc, bool visibleOnly, object customParam)
        {
            // create delegate that will be used for call to native EnumWindows function
            // (also create a GC handle to prevent the delegate from being garbage collected
            // during the call to EnumWindows!)
            EnumWindowsDelegate enumWindowsProc = new EnumWindowsDelegate(EnumWindowsProc);
            GCHandle hEnumWindowsProc = GCHandle.Alloc(enumWindowsProc);

            // Create the custom parameter that will be passed to EnumWindows and allocate
            // a GC handle for it so we can pass it as the LPARAM to the native EnumWindows
            EnumWindowsParam param = new EnumWindowsParam(callbackProc, visibleOnly, customParam);
            GCHandle hParam = GCHandle.Alloc(param);
            try
            {
                // enumerate windows
                User32.EnumChildWindows(hWndParent, enumWindowsProc, hParam);
            }
            finally
            {
                // free GC handles used for enumerating windows
                hEnumWindowsProc.Free();
                hParam.Free();
            }
        }

        /// <summary>
        /// Delegate used for EnumWindows callback (return false to stop enumerating)
        /// </summary>
        public delegate bool EnumerateChildWindowsDelegate(IntPtr hWnd, object customParam);

        /// <summary>
        /// Custom parameter passed to enum-windows
        /// </summary>
        private class EnumWindowsParam
        {
            /// <summary>
            /// Initialize
            /// </summary>
            /// <param name="enumDelegate">managed delegate to call back for each window</param>
            /// <param name="visibleOnly">only enumerate visible windows?</param>
            /// <param name="customParam">custom parameter passed to delegate</param>
            public EnumWindowsParam(EnumerateChildWindowsDelegate enumDelegate, bool visibleOnly, object customParam)
            {
                EnumDelegate = enumDelegate;
                VisibleOnly = visibleOnly;
                CustomParam = customParam;
            }
            public readonly EnumerateChildWindowsDelegate EnumDelegate;
            public readonly bool VisibleOnly;
            public readonly object CustomParam;
        }

        /// <summary>
        /// Callback for EnumWindows
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="lParam">Custom lParam (not used)</param>
        /// <returns>true to continue enumerating, false to stop enumerating</returns>
        private static bool EnumWindowsProc(IntPtr hWnd, GCHandle lParam)
        {
            // convert the lParam back into the correct managed object
            EnumWindowsParam param = (EnumWindowsParam)lParam.Target;

            // apply visible filter (if requested)
            if (param.VisibleOnly && ((User32.GetWindowLong(hWnd, GWL.STYLE) & WS.VISIBLE) == 0))
                return true;

            // call the underlying delegate
            return param.EnumDelegate(hWnd, param.CustomParam);
        }

    }
}
