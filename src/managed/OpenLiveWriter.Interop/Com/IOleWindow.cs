// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// COM interface to a window
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000114-0000-0000-C000-000000000046")]
    public interface IOleWindow
    {
        /// <summary>
        /// Return the window handle of the object implementing the DeskBand
        /// </summary>
        /// <param name="phwnd">Out parameter for window handle</param>
        void GetWindow(out IntPtr phwnd);

        /// <summary>
        /// Activate or de-activate context-sensitive help -- this
        /// method is NOT required for DeskBand implementations
        /// </summary>
        /// <param name="fEnterMode">Enter or exit help mode</param>
        void ContextSensitiveHelp([In] bool fEnterMode);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010d-0000-0000-C000-000000000046")]
    public interface IViewObject
    {
        void Draw(
            DVASPECT dwDrawAspect, int lindex, IntPtr pvAspect,
            IntPtr ptd, IntPtr hdcTargetDev, IntPtr hdcDraw,
            [In] ref RECT lprcBounds, IntPtr lprcWBounds,
            IntPtr pfnContinue, int dwContinue);

        void GetColorSet();
        void Freeze();
        void Unfreeze();
        void SetAdvise();
        void GetAdvise();
    }

    [ComImport, TypeLibType((short)0x1010), Guid("3050F52E-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)2)]
    public interface DispHTMLEmbed
    {
        [DispId(-2147412996)]
        object readyState {[return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412996)] get; }
    }

}

