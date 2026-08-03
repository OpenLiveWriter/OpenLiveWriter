// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
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

        // Registry of all currently visible dropdowns in the app. A click inside
        // ANY of them counts as "inside", so nested dropdowns (e.g. a button
        // dropdown opened from inside a collapsed group popup) do not close
        // their parents and the click cannot fall through to controls behind.
        private static readonly List<ToolStripDropDown> _visibleDropDowns = new List<ToolStripDropDown>();
        private static readonly object _visibleDropDownsLock = new object();

        internal static void RegisterVisibleDropDown(ToolStripDropDown dropDown)
        {
            if (dropDown == null) return;
            lock (_visibleDropDownsLock)
            {
                if (!_visibleDropDowns.Contains(dropDown))
                    _visibleDropDowns.Add(dropDown);
            }
        }

        internal static void UnregisterVisibleDropDown(ToolStripDropDown dropDown)
        {
            lock (_visibleDropDownsLock)
            {
                _visibleDropDowns.Remove(dropDown);
            }
        }

        internal static bool IsInsideAnyVisibleDropDown(Point clickPoint)
        {
            lock (_visibleDropDownsLock)
            {
                foreach (var dropDown in _visibleDropDowns)
                {
                    if (dropDown.Visible && GetPhysicalScreenRect(dropDown).Contains(clickPoint))
                        return true;
                }
            }
            return false;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr hwnd, ref POINT lpPoint);

        /// <summary>
        /// Returns the control's screen rectangle in physical pixels, the same
        /// coordinate space as low-level mouse hook points. Control.Bounds can
        /// be in scaled (logical) units on high-DPI displays, which made every
        /// click read as "outside" and closed dropdowns under the mouse. The
        /// Win32 call is guarded so this also works off Windows.
        /// </summary>
        private static Rectangle GetPhysicalScreenRect(Control control)
        {
            if (OperatingSystem.IsWindows() && control.IsHandleCreated &&
                GetWindowRect(control.Handle, out RECT rect))
            {
                return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
            return control.Bounds;
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
        /// No-op off Windows: the hook exists to close dropdowns when clicking
        /// native child windows (WebView2/MSHTML) that WinForms message filters
        /// do not reach; managed-only platforms get auto-close from the
        /// ToolStrip's built-in behavior instead.
        /// </summary>
        public void Install()
        {
            if (!OperatingSystem.IsWindows())
                return;

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

                        // Low-level mouse hook points are physical device pixels,
                        // but WinForms bounds are in scaled (logical) units on
                        // high-DPI displays. Convert with the per-monitor API so
                        // the comparison is valid; without it every click reads
                        // as outside at 200% DPI.
                        var pt = hookStruct.pt;
                        var hwndForDpi = dropDown.IsHandleCreated ? dropDown.Handle : _owner.Handle;
                        if (hwndForDpi != IntPtr.Zero)
                            PhysicalToLogicalPointForPerMonitorDPI(hwndForDpi, ref pt);
                        var clickPoint = new Point(pt.x, pt.y);

                        // Check if the click is inside the dropdown bounds (in
                        // physical pixels, matching the hook point's space)
                        var dropDownBounds = GetPhysicalScreenRect(dropDown);
                        if (!dropDownBounds.Contains(clickPoint) && !IsInsideAnyVisibleDropDown(clickPoint))
                        {
                            // Also check if click is on the owner control itself (to allow toggling)
                            var ownerScreenBounds = GetPhysicalScreenRect(_owner);
                            if (!ownerScreenBounds.Contains(clickPoint))
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"[OLW-DEBUG] DropDownMouseHook: closing dropdown; raw={hookStruct.pt.x},{hookStruct.pt.y} click={clickPoint.X},{clickPoint.Y} dropdown={dropDownBounds} owner={ownerScreenBounds} dpi={GetDpiForWindow(hwndForDpi)}");
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
