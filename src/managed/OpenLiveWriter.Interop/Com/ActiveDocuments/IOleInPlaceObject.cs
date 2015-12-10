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
    [Guid("00000113-0000-0000-C000-000000000046")]
    public interface IOleInPlaceObject
    {
        void GetWindow(
            [Out] out IntPtr phwnd);

        void ContextSensitiveHelp(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        void InPlaceDeactivate();

        void UIDeactivate();

        void SetObjectRects(
            [In] ref RECT lprcPosRect,
            [In] ref RECT lprcClipRect);

        [PreserveSig]
        int ReactivateAndUndo();
    }
}

