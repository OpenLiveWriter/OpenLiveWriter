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
    [Guid("00000115-0000-0000-C000-000000000046")]
    public interface IOleInPlaceUIWindow
    {
        void GetWindow(
            [Out] out IntPtr phwnd);

        void ContextSensitiveHelp(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        int GetBorder(
            [Out] out RECT lprectBorder);

        [PreserveSig]
        int RequestBorderSpace(
            [In] ref RECT pborderwidths);

        void SetBorderSpace(
            [In] ref RECT pborderwidths);

        void SetActiveObject(
            [In] IOleInPlaceActiveObject pActiveObject,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
    }
}
