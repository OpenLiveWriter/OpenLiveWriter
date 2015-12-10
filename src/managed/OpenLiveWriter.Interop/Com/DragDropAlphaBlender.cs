// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Helper class that implements the IDropSourceHelper and IDropTargetHelper
    /// interfaces (cast the DragDropAlphaBlender) to these interfaces to get access
    /// to them). Note that this class is only useful when working with native Ole
    /// data objects (which .NET does not) so by and large it won't help us unless
    /// we bypass all of .NET's data objects and work 100% with native Ole data
    /// objects.
    /// </summary>
    [ComImport]
    [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
    public class DragDropAlphaBlender {}  // implements IDropSourceHelper and IDropTargetHelper

    /// <summary>
    /// This interface allows drop targets to display a drag image while the image
    /// is over the target window. This interface is implemented by DragDropHelper.
    /// NOTE: The Data object must support IDataObject::SetData with multiple data
    /// types and GetData must implement data type cloning(Including HGLOBAL), not
    /// just aliasing. MFC does do this, not sure if .NET does....
    /// NOTE: Minimum OS requirements for this feature are Win2K and WinME.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4657278B-411B-11d2-839A-00C04FD918D0")]
    public interface IDropTargetHelper
    {
        /// <summary>
        /// Notifies the drag-image manager that the drop target's IDropTarget::DragEnter
        /// method has been called.
        /// </summary>
        /// <param name="hwndTarget">[in] Target's window handle.</param>
        /// <param name="pDataObject">[in] Pointer to the data object's IDataObject
        /// interface. </param>
        /// <param name="ppt">[in] POINT structure pointer that was received in the
        /// IDropTarget::DragEnter method's pt parameter</param>
        /// <param name="dwEffect">[in] Value pointed to by the IDropTarget::DragEnter
        /// method's pdwEffect parameter.</param>
        void DragEnter(
            [In] IntPtr hwndTarget,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDataObject,
            [In] ref POINT ppt,
            [In] DROPEFFECT dwEffect ) ;

        /// <summary>
        /// Notifies the drag-image manager that the drop target's
        /// IDropTarget::DragLeave method has been called.
        /// </summary>
        void DragLeave() ;

        /// <summary>
        /// Notifies the drag-image manager that the drop target's
        /// IDropTarget::DragOver method has been called.
        /// </summary>
        /// <param name="ppt">[in] POINT structure pointer that was received in the
        /// IDropTarget::DragOver method's pt parameter.</param>
        /// <param name="dwEffect">[in] Value pointed to by the IDropTarget::DragOver
        /// method's pdwEffect parameter.</param>
        void DragOver(
            [In] ref POINT ppt,
            [In] DROPEFFECT dwEffect );

        /// <summary>
        /// Notifies the drag-image manager that the drop target's IDropTarget::Drop
        /// method has been called.
        /// </summary>
        /// <param name="pDataObject">[in] Pointer to the data object's IDataObject
        /// interface. </param>
        /// <param name="ppt">[in] POINT structure pointer that was received in the
        /// IDropTarget::Drop method's pt parameter</param>
        /// <param name="dwEffect">[in] Value pointed to by the IDropTarget::Drop
        /// method's pdwEffect parameter.</param>
        void Drop(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDataObject,
            [In] ref POINT ppt,
            [In] DROPEFFECT dwEffect ) ;

        /// <summary>
        /// Notifies the drag-image manager to show or hide the drag image.
        /// This method is provided for showing/hiding the Drag image in low color
        /// depth video modes. When painting to a window that is currently being
        /// dragged over (i.e. For indicating a selection) you need to hide the
        /// drag image by calling this method passing FALSE. After the window is
        /// done painting, Show the image again by passing TRUE.		///
        /// </summary>
        /// <param name="fShow">[in] Boolean value that is set to TRUE to show the
        /// drag image, and FALSE to hide it.</param>
        void Show(
            [In, MarshalAs(UnmanagedType.Bool)] bool fShow ) ;
    }

    /// <summary>
    /// This interface is exposed by the Shell to allow an application to specify
    /// the image that will be displayed during a Shell drag-and-drop operation.
    /// This interface is implemented by DragDropHelper.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("DE5BF786-477A-11d2-839D-00C04FD918D0")]
    public interface IDragSourceHelper
    {
        /// <summary>
        /// Initializes the drag-image manager for a windowless control.
        /// </summary>
        /// <param name="pshdi">[in] SHDRAGIMAGE structure that contains information
        /// about the bitmap.</param>
        /// <param name="pDataObject">[in] Pointer to the data object's IDataObject
        /// interface.</param>
        void InitializeFromBitmap(
            [In] ref SHDRAGIMAGE pshdi,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDataObject );

        /// <summary>
        /// Initializes the drag-image manager for a control with a window.
        /// DragDropHelper will send a DI_GETDRAGIMAGE message to the specified
        /// window. DI_GETDRAGIMAGE is defined in DI.GETDRAGIMAGE and must be
        /// registered with RegisterWindowMessage. When the window specified
        /// by hwnd receives the DI_GETDRAGIMAGE message, the lParam value
        /// will hold a pointer to an SHDRAGIMAGE structure. The handler should
        /// fill the structure with the drag image bitmap information.
        /// </summary>
        /// <param name="hwnd">[in] Handle to the window that will receive the
        /// DI_GETDRAGIMAGE message.</param>
        /// <param name="ppt">[in] Pointer to a POINT structure that specifies
        /// the location of the cursor within the drag image. The structure should
        /// contain the offset from the upper-left corner of the drag image to the
        /// location of the cursor. </param>
        /// <param name="pDataObject">[in] Pointer to the data object's IDataObject
        /// interface</param>
        void InitializeFromWindow(
            [In] IntPtr hwnd,
            [In] ref POINT ppt,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDataObject );
    }


    /// <summary>
    /// Structure used to define a drag image
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=8)] // corresponds to #include <pshpack8.h> in ShObj.h
    public struct SHDRAGIMAGE
    {
        SIZE sizeDragImage; // OUT - The length and Width of the rendered image
        POINT ptOffset;     // OUT - The Offset from the mouse cursor to the upper left corner of the image
        IntPtr hbmpDragImage; // OUT - The Bitmap containing the rendered drag images
        uint crColorKey;  // OUT - The COLORREF that has been blitted to the background of the images
    } ;

    /// <summary>
    /// Drag Image related window messages
    /// </summary>
    public struct DI
    {
        /// <summary>
        /// This is sent to a window to get the rendered images to a bitmap (used
        /// with IDragSourceHelper.InitializeFromWindow). Call RegisterWindowMessage
        /// to get the ID
        /// </summary>
        public const string GETDRAGIMAGE = "ShellGetDragImage" ;
    }
}

