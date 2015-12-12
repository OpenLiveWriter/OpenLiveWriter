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
    [Guid("00000119-0000-0000-C000-000000000046")]
    public interface IOleInPlaceSite
    {
        void GetWindow(
            [Out] out IntPtr phwnd);

        void ContextSensitiveHelp(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        int CanInPlaceActivate();

        void OnInPlaceActivate();

        void OnUIActivate();

        void GetWindowContext(
            [Out] out IOleInPlaceFrame ppFrame,
            [Out] out IOleInPlaceUIWindow ppDoc,
            [Out] out RECT lprcPosRect,
            [Out] out RECT lprcClipRect,
            [Out, In] ref OLEINPLACEFRAMEINFO lpFrameInfo);

        void Scroll(
            [In] SIZE scrollExtant);

        void OnUIDeactivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fUndoable);

        void OnInPlaceDeactivate();

        void DiscardUndoState();

        void DeactivateAndUndo();

        void OnPosRectChange(
            [In] ref RECT lprcPosRect);
    }

    public struct OLEINPLACEFRAMEINFO
    {
        public uint cb;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fMDIApp;
        public IntPtr hwndFrame;
        public IntPtr haccel;
        public uint cAccelEntries;
    }
}
