// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Class that interacts with the Windows API to enable the
    /// subclassing (interception of window messages) of a Win32
    /// Window.
    /// </summary>
    public class WindowSubClasser
    {

        /// <summary>
        /// Initialize the WindowSubclasser. To actually subclass the
        /// underlying control use the Install() and Remove() methods
        /// </summary>
        /// <param name="window">Control to subclass</param>
        /// <param name="wndProc">Delegate to pass window messages to</param>
        public WindowSubClasser(IWin32Window window, WndProcDelegate wndProc)
        {
            _window = window;
            m_wndProcDelegate = wndProc;
            m_baseWndProc = IntPtr.Zero;
        }

        /// <summary>
        /// Install the subclass hook so window messages are sent to the
        /// external delegate
        /// </summary>
        public void Install()
        {
            // verify that the operation is valid
            Debug.Assert(m_baseWndProc == IntPtr.Zero,
                "Invalid attempt to subclass a window twice");

            // subclass the window (x64-safe)
            m_baseWndProc = User32.SetWindowProc(_window.Handle, GWL.WNDPROC, m_wndProcDelegate);
        }

        /// <summary>
        /// Remove the subclass hook
        /// </summary>
        public void Remove()
        {
            // verify that the operation is valid
            Debug.Assert(m_baseWndProc != IntPtr.Zero,
                "Invalid call to Uninstall prior to Install");

            // uninstall the subclassing behavior (x64-safe)
            User32.SetWindowLongPtr(_window.Handle, GWL.WNDPROC, m_baseWndProc);
            m_baseWndProc = IntPtr.Zero;
        }

        /// <summary>
        /// Call the underlying window procedure of the Control that
        /// has been subclassed. Note that you must call this for
        /// every message except those that you want to hide from
        /// the underlying Control.
        /// </summary>
        public IntPtr CallBaseWindowProc(
            IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            return User32.CallWindowProc(m_baseWndProc, hWnd, uMsg, wParam, lParam);
        }

        // implementation data
        private IWin32Window _window;
        private WndProcDelegate m_wndProcDelegate;
        private IntPtr m_baseWndProc;
    }

}

