// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Low-level mouse hook to detect mouse clicks outside a dropdown and close it.
    /// This is needed because native controls (WebView2/MSHTML) don't trigger
    /// the standard ToolStripDropDown auto-close behavior or WinForms message filters.
    /// </summary>
    internal class DropDownMouseHook : IDisposable
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WH_MOUSE_LL = 14;

        private readonly Control _owner;
        private readonly Func<ToolStripDropDown> _getDropDown;
        private readonly Action _closeDropDown;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc _hookProc;
        private bool _disposed;

        // P/Invoke declarations
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Creates a new mouse hook for dropdown close detection.
        /// </summary>
        /// <param name="owner">The control that owns the dropdown.</param>
        /// <param name="getDropDown">Function to get the current dropdown (may return null if not visible).</param>
        /// <param name="closeDropDown">Action to close the dropdown.</param>
        public DropDownMouseHook(Control owner, Func<ToolStripDropDown> getDropDown, Action closeDropDown)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _getDropDown = getDropDown ?? throw new ArgumentNullException(nameof(getDropDown));
            _closeDropDown = closeDropDown ?? throw new ArgumentNullException(nameof(closeDropDown));
            // Keep a reference to prevent GC
            _hookProc = HookCallback;
        }

        /// <summary>
        /// Installs the low-level mouse hook.
        /// </summary>
        public void Install()
        {
            if (_hookId == IntPtr.Zero)
            {
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        /// <summary>
        /// Removes the low-level mouse hook.
        /// </summary>
        public void Remove()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN)
                {
                    var dropDown = _getDropDown();
                    if (dropDown != null && dropDown.Visible)
                    {
                        var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        var clickPoint = new Point(hookStruct.pt.x, hookStruct.pt.y);

                        // Check if the click is inside the dropdown bounds
                        var dropDownBounds = dropDown.Bounds;
                        if (!dropDownBounds.Contains(clickPoint))
                        {
                            // Also check if click is on the owner control itself (to allow toggling)
                            var ownerScreenBounds = _owner.RectangleToScreen(_owner.ClientRectangle);
                            if (!ownerScreenBounds.Contains(clickPoint))
                            {
                                // Use BeginInvoke to close on the UI thread
                                if (_owner.IsHandleCreated && !_owner.IsDisposed)
                                {
                                    _owner.BeginInvoke(new Action(() => _closeDropDown()));
                                }
                            }
                        }
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Remove();
                _disposed = true;
            }
        }
    }
}
