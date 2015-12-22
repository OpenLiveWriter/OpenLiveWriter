// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bcc6-4e68-101b-a2bc-00aa00404770")]
    public interface IOleDocumentView
    {
        void SetInPlaceSite(
            [In] IOleInPlaceSite pIPSite);

        void GetInPlaceSite(
            [Out] out IOleInPlaceSite ppIPSite);

        void GetDocument(
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        void SetRect(
            [In] ref RECT prcView);

        void GetRect(
            [Out] out RECT prcView);

        [PreserveSig]
        int SetRectComplex(
            [In] ref RECT prcView,
            [In] ref RECT prcHScroll,
            [In] ref RECT prcVScroll,
            [In] ref RECT prcSizeBox);

        void Show(
            [In, MarshalAs(UnmanagedType.Bool)] bool fShow);

        void UIActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fUIActivate);

        [PreserveSig]
        int Open();

        void Close(uint dwReserved);

        void SaveViewState(
            [In] IStream pstm);

        void ApplyViewState(
            [In] IStream pstm);

        [PreserveSig]
        int Clone(
            [In] IOleInPlaceSite pIPSiteNew,
            [Out] out IOleDocumentView ppViewNew);

    }

}
