// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml.Mshtml_Interop
{
    /// <summary>
    /// Interface used for customizing the UI of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6d0-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IDocHostUIHandler2
    {
        [PreserveSig]
        int ShowContextMenu(
            [In] int dwID,
            [In] ref POINT ppt,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pcmdtReserved,
            [In, MarshalAs(UnmanagedType.IDispatch)] object pdispReserved);

        void GetHostInfo(
            [Out][In] ref DOCHOSTUIINFO pInfo);

        [PreserveSig]
        int ShowUI(
            [In] DOCHOSTUITYPE dwID,
            [In] IOleInPlaceActiveObject pActiveObject,
            [In] IOleCommandTarget pCommandTarget,
            [In] IOleInPlaceFrame pFrame,
            [In] IOleInPlaceUIWindow pDoc);

        void HideUI();

        void UpdateUI();

        void EnableModeless(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnable);

        void OnDocWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void OnFrameWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void ResizeBorder(
            [In] ref RECT prcBorder,
            [In] IOleInPlaceUIWindow pUIWindow,
            [In, MarshalAs(UnmanagedType.Bool)] bool frameWindow);

        [PreserveSig]
        int TranslateAccelerator(
            [In] ref MSG lpMsg,
            [In] ref Guid pguidCmdGroup,
            [In] uint nCmdID);

        void GetOptionKeyPath(
            [Out] out IntPtr pchKey,
            [In] uint dwReserved);

        [PreserveSig]
        int GetDropTarget(
            [In] IDropTarget pDropTarget,
            [Out] out IDropTarget ppDropTarget);

        void GetExternal(
            [Out] out IntPtr ppDispatch);

        [PreserveSig]
        int TranslateUrl(
            [In] uint dwReserved,
            [In] IntPtr pchURLIn,
            [Out] out IntPtr ppchURLOut);

        [PreserveSig]
        int FilterDataObject(
            [In] IOleDataObject pDO,
            [Out] out IOleDataObject ppDORet);

        void GetOverrideKeyPath(
            [Out] out IntPtr pchKey,
            [In] uint dwReserved);
    }
}
