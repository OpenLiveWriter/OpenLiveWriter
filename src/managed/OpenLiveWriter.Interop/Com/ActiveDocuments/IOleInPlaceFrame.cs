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
    [Guid("00000116-0000-0000-C000-000000000046")]
    public interface IOleInPlaceFrame
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

        void InsertMenus(
            [In] IntPtr hmenuShared,
            [In, Out] ref OLEMENUGROUPWIDTHS lpMenuWidths);

        void SetMenu(
            [In] IntPtr hmenuShared,
            [In] IntPtr holemenu,
            [In] IntPtr hwndActiveObject);

        void RemoveMenus(
            [In] IntPtr hmenuShared);

        void SetStatusText(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszStatusText);

        void EnableModeless(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnable);

        [PreserveSig]
        int TranslateAccelerator(
            [In] ref MSG lpmsg,
            [In] UInt16 wID);
    }

    /// <summary>
    /// The OLEMENUGROUPWIDTHS structure is the mechanism for building a shared menu.
    /// It indicates the number of menu items in each of the six menu groups of a menu
    /// shared between a container and an object server during an in-place editing session.
    /// </summary>
    public struct OLEMENUGROUPWIDTHS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] width;
    }

}

