// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000121-0000-0000-C000-000000000046")]
    public interface IDropSource
    {
        /// <summary>
        /// Determines whether a drag-and-drop operation should be continued, canceled, or completed. You do not call this method directly. The OLE DoDragDrop function calls this method during a drag-and-drop operation
        /// </summary>
        /// <param name="fEscapePressed">Specifies whether the Esc key has been pressed since the previous call to IDropSource::QueryContinueDrag or to DoDragDrop if this is the first call to QueryContinueDrag. A TRUE value indicates the end user has pressed the escape key; a FALSE value indicates it has not been pressed</param>
        /// <param name="grfKeyState">Current state of the keyboard modifier keys on the keyboard</param>
        /// <returns>S_OK The drag operation should continue. This result occurs if no errors are detected, the mouse button starting the drag-and-drop operation has not been released, and the Esc key has not been detected.
        ///			 DRAGDROP_S.DROP The drop operation should occur completing the drag operation. This result occurs if grfKeyState indicates that the key that started the drag-and-drop operation has been released.
        ///			 DRAGDROP_S.CANCEL   The drag operation should be canceled with no drop operation occurring. This result occurs if fEscapePressed is TRUE, indicating the Esc key has been pressed</returns>
        [PreserveSig]
        int QueryContinueDrag(
            [In] bool fEscapePressed,
            [In] uint grfKeyState);

        /// <summary>
        /// Enables a source application to give visual feedback to the end user during a drag-and-drop operation by providing the DoDragDrop function with an enumeration value specifying the visual effect
        /// </summary>
        /// <param name="dwEffect">The DROPEFFECT value returned by the most recent call to IDropTarget::DragEnter, IDropTarget::DragOver, or IDropTarget::DragLeave</param>
        /// <returns>S_OK The method completed its task successfully, using the cursor set by the source application.
        ///   	     DRAGDROP_S.USEDEFAULTCURSORS Indicates successful completion of the method, and requests OLE to update the cursor using the OLE-provided default cursors</returns>
        [PreserveSig]
        int GiveFeedback(
            [In] DROPEFFECT dwEffect);
    }

    /// <summary>
    /// Drag and Drop status HRESULT values
    /// </summary>
    public struct DRAGDROP_S
    {
        /// <summary>
        ///  Successful drop took place
        /// </summary>
        public const int DROP = unchecked((int)0x00040100);

        /// <summary>
        /// Drag-drop operation canceled
        /// </summary>
        public const int CANCEL = unchecked((int)0x00040101);

        /// <summary>
        /// Use the default cursor
        /// </summary>
        public const int USEDEFAULTCURSORS = unchecked((int)0x00040102);
    }

    /// <summary>
    /// Drag and drop error HRESULT values
    /// </summary>
    public struct DRAGDROP_E
    {
        //
        // MessageId: DRAGDROP_E_NOTREGISTERED
        //
        // MessageText:
        //
        //  Trying to revoke a drop target that has not been registered
        //
        public const int NOTREGISTERED = unchecked((int)0x80040100);

        //
        // MessageId: DRAGDROP_E_ALREADYREGISTERED
        //
        // MessageText:
        //
        //  This window has already been registered as a drop target
        //
        public const int ALREADYREGISTERED = unchecked((int)0x80040101);

        //
        // MessageId: DRAGDROP_E_INVALIDHWND
        //
        // MessageText:
        //
        //  Invalid window handle
        //
        public const int INVALIDHWND = unchecked((int)0x80040101);
    }
}
