// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000122-0000-0000-C000-000000000046")]
    public interface IDropTarget
    {
        void DragEnter(
            [In] IOleDataObject pDataObj,
            [In] MK grfKeyState,
            [In] POINT pt,
            [Out, In] ref DROPEFFECT pdwEffect);

        void DragOver(
            [In] MK grfKeyState,
            [In] POINT pt,
            [Out, In] ref DROPEFFECT pdwEffect);

        void DragLeave();

        void Drop(
            [In] IOleDataObject pDataObj,
            [In] MK grfKeyState,
            [In] POINT pt,
            [Out, In] ref DROPEFFECT pdwEffect);
    }

    /// <summary>
    /// The DoDragDrop function and many of the methods in the IDropSource and
    /// IDropTarget interfaces pass information about the effects of a drag-and-drop
    /// operation in a DROPEFFECT enumeration. Valid drop-effect values are the
    /// result of applying the OR operation to the values contained in the DROPEFFECT
    /// enumeration
    /// </summary>
    [Flags]
    public enum DROPEFFECT : uint
    {
        /// <summary>
        /// Drop target cannot accept the data.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Drop results in a copy. The original data is untouched by the drag source.
        /// </summary>
        COPY = 1,

        /// <summary>
        /// Drag source should remove the data.
        /// </summary>
        MOVE = 2,

        /// <summary>
        /// Drag source should create a link to the original data.
        /// </summary>
        LINK = 4,

        /// <summary>
        /// Scrolling is about to start or is currently occurring in the target.
        /// This value is used in addition to the other values.
        /// </summary>
        SCROLL = 0x80000000
    };
}
