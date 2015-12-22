// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000117-0000-0000-C000-000000000046")]
    public interface IOleInPlaceActiveObject
    {
        void GetWindow(
            [Out] out IntPtr phwnd);

        void ContextSensitiveHelp(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        int TranslateAccelerator(
            [In] ref MSG lpmsg);

        void OnFrameWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void OnDocWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void ResizeBorder(
            [In] ref RECT prcBorder,
            [In] IOleInPlaceUIWindow pUIWindow,
            [In, MarshalAs(UnmanagedType.Bool)] bool fFrameWindow);

        void EnableModeless(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnable);
    }
}

