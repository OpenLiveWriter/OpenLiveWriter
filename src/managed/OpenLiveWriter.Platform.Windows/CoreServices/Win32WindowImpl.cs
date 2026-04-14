// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Simple Implementation of IWin32Window
    /// </summary>
    public class Win32WindowImpl : IWin32Window
    {
        /// <summary>
        /// Get the IWin32Window interface for the current foreground window
        /// </summary>
        public static IWin32Window ForegroundWin32Window
        {
            get { return new Win32WindowImpl(User32.GetForegroundWindow()); }
        }

        /// <summary>
        /// Get the IWin32Window interface for the desktop window
        /// </summary>
        public static IWin32Window DesktopWin32Window
        {
            get { return new Win32WindowImpl(User32.GetDesktopWindow()); }
        }

        /// <summary>
        /// Get the IWin32Window interface for the desktop window
        /// </summary>
        public static IWin32Window ActiveWin32Window
        {
            get { return new Win32WindowImpl(User32.GetActiveWindow()); }
        }

        /// <summary>
        /// Initialize with the handle
        /// </summary>
        /// <param name="hWnd">handle</param>
        public Win32WindowImpl(IntPtr hWnd)
        {
            m_hWnd = hWnd;
        }

        /// <summary>
        /// Handle of the window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return m_hWnd;
            }
        }
        private IntPtr m_hWnd;

        public override int GetHashCode()
        {
            return m_hWnd.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            Win32WindowImpl win32WindowImpl = obj as Win32WindowImpl;
            if (win32WindowImpl == null) return false;
            if (!Equals(m_hWnd, win32WindowImpl.m_hWnd)) return false;
            return true;
        }
    }
}
