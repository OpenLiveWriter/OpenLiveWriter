// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class MshtmlEditorDragAndDropTarget : DragAndDropTarget, OpenLiveWriter.Interop.Com.IDropTarget, IDisposable
    {

        #region Initialization/Disposal

        /// <summary>
        /// Initialize the drag and drop manager with its context. Note that this object
        /// is initialized in the 3 stages: construction, replacement of MSHTML DropTarget
        /// implementation, and finally installation of drag and drop event handlers for
        /// the drag target control. The 3-step initialization is required because MSHTML
        /// also initializes in stages and our initialization must correspond to its/
        /// </summary>
        /// <param name="context">presentation editor context</param>
        public MshtmlEditorDragAndDropTarget(IHtmlMarshallingTarget context, IDataFormatHandlerFactory dataFormats) : base(dataFormats)
        {
            // store reference to context
            htmlMarshallingTarget = context;
        }

        /// <summary>
        /// Method that is delegated to by the presentation editor's implementation of
        /// IDocHostCustomUI.GetDropTarget. Replaces the default drop target implementation
        /// with our own. Note that the main purpose of this method for us is to get a reference
        /// to the MSHTML drop target implementaiton. While in this method we do in fact replace
        /// thier implementation with our own, this is probabaly redundant as we also disable
        /// their drop target (see below comment on Initialize). We replace their implementation
        /// in any case just so that if they have any internal state or behavior that depends
        /// upon knowing whether their drop target implementation is active then it can be
        /// set correctly.
        /// </summary>
        /// <param name="pDropTarget">default implementation</param>
        /// <param name="ppDropTarget">our implemetation</param>
        /// <returns>S_OK to indicate that we replaced implementation, otherwise E_NOTIMPL</returns>
        public int GetDropTarget(OpenLiveWriter.Interop.Com.IDropTarget pDropTarget, out OpenLiveWriter.Interop.Com.IDropTarget ppDropTarget)
        {
            // Note: Drop target can be null after the user clicks on an <option> element.

            // save a reference to the mshtml drop target
            mshtmlDropTargetImpl = pDropTarget;

            // replace the mshtml implementaiton with our own (will delegate as necessary)
            ppDropTarget = this;

            // indicate that we replaced the implementation
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Disable the MSHTML drop target and enable this control as a drop target. In theory
        /// we don't need to do this as IDocHostUIHandler.GetDropTarget should allow us to
        /// fully replace their implementation with our own (delegating back to MSHTML as
        /// necessary). Alas, this works fine in most scenarios except when a .NET data object
        /// is being dragged over us from within the current process (as in the case of the
        /// MicroView) the .NET object is not marshalled to an OLE IDataObject so MSHTML
        /// doesn't see it and doesn't call us. For this case we need inbound IDataObject
        /// processing to be .NET based, thus the need to disable the MSHTML drop target
        /// </summary>
        /// <param name="dragTargetControl">Control that will be used as the target
        /// for drag and drop operations (passed to us so we can subscribe to it's
        /// drag and drop events)</param>
        public override void Initialize(Control dragTargetControl)
        {
            // disable the MSHTML drop target
            IntPtr hMshtmlWnd;
            IOleWindow oleWindow = (IOleWindow)htmlMarshallingTarget.HtmlDocument;
            oleWindow.GetWindow(out hMshtmlWnd);
            int hresult = Ole32.RevokeDragDrop(hMshtmlWnd);
            // If we reuse an mshtml editor, the window will have been revoked once already
            // thus it will not be registered if we try to revoke it a 2nd time.
            Debug.Assert(hresult == HRESULT.S_OK || hresult == DRAGDROP_E.NOTREGISTERED);

            base.Initialize(dragTargetControl);
        }

        #endregion

        #region Drag and Drop Event Handlers

        /// <summary>
        /// Handle the DragEnter event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected override void DragEnter(object sender, DragEventArgs e)
        {
            if (!htmlMarshallingTarget.IsEditable) //disable drop operations when not in edit mode.
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            DataObjectMeister dataMeister = new DataObjectMeister(e.Data);
            if (HtmlHandler.IsPasteFromSharedCanvas(dataMeister) && htmlMarshallingTarget.SelectionIsInvalid)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            base.DragEnter(sender, e);
        }

        protected override void OnBeforeBeginDrag(DragEventArgs e)
        {
            base.OnBeforeBeginDrag(e);

            // set focus to ourselves to provide good drag feedback
            (htmlMarshallingTarget.HtmlDocument.body as IHTMLElement2).focus();

            // call mshtml DragEnter (suppressing feedback) so that the
            // document will be auto-scrolled if required
            CallMshtmlDragEnter(e, false, false);
        }

        /// <summary>
        /// Handle the DragOver event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected override void DragOver(object sender, DragEventArgs e)
        {
            // provide feedback if this is our drag/drop
            if (ActiveDataFormatHandler != null)
            {
                // call mshtml DragOver (suppressing feedback) so that the
                // document will be auto-scrolled if required
                CallMshtmlDragOver(e, false, false);
            }
            base.DragOver(sender, e);
        }

        /// <summary>
        /// Handle the DragLeave event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected override void DragLeave(object sender, EventArgs e)
        {
            if (ActiveDataFormatHandler != null)
            {
                // call mshtml DragLeave to correctly maintain its internal state
                // (we also call him on DragEnter and DragOver so that he performs
                // auto-scrolling on our behalf)
                CallMshtmlDragLeave(false);
            }
            base.DragLeave(sender, e);
        }

        public override void Dispose()
        {
            if (emptyDataObject != null)
                emptyDataObject.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Handle the DragDrop event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected override void DragDrop(object sender, DragEventArgs e)
        {
            if (ActiveDataFormatHandler != null)
            {
                // call mshtml DragLeave to correctly maintain its internal state
                // this essentially notifies him that the drag is "cancelled" which
                // is true from his point of view but not from ours. (we also call
                // him on DragEnter and DragOver so that he performs auto-scrolling
                // on our behalf)
                CallMshtmlDragLeave(false);
            }
            base.DragDrop(sender, e);
        }

        #endregion

        #region Delegation of Drag and Drop Events to MSHTML

        /// <summary>
        /// Helper to cal the Mshtml DragEnter routine using .NET DragEventArgs
        /// </summary>
        /// <param name="e">event args</param>
        private void CallMshtmlDragEnter(DragEventArgs e, bool allowEffects, bool allowExceptions)
        {
            if (mshtmlDropTargetImpl == null)
                return;

            try
            {
                // extract ole data object
                IOleDataObject oleDataObject = SafeExtractOleDataObject(e.Data);

                // convert data types
                POINT pt = new POINT(); pt.x = e.X; pt.y = e.Y;
                DROPEFFECT dropEffect = ConvertDropEffect(e.AllowedEffect);
                MK keyState = ConvertKeyState(e.KeyState);

                // suppress effects if requested
                if (!allowEffects)
                    dropEffect = DROPEFFECT.NONE;

                // call mshtml
                mshtmlDropTargetImpl.DragEnter(oleDataObject, keyState, pt, ref dropEffect);

                // copy any changes to the dropEffect back into the event args
                e.Effect = ConvertDropEffect(dropEffect);
            }
            catch (Exception)
            {
                if (allowExceptions)
                    throw;
            }
        }

        /// <summary>
        /// Helper method to call the Mshtml DragOver routine using .NET DragEventArgs
        /// </summary>
        /// <param name="e"></param>
        private void CallMshtmlDragOver(DragEventArgs e, bool allowEffects, bool allowExceptions)
        {
            if (mshtmlDropTargetImpl == null)
                return;

            try
            {
                // convert data types
                POINT pt = new POINT(); pt.x = e.X; pt.y = e.Y;
                DROPEFFECT dropEffect = ConvertDropEffect(e.AllowedEffect);
                MK keyState = ConvertKeyState(e.KeyState);

                // suppress effects if requested
                if (!allowEffects)
                    dropEffect = DROPEFFECT.NONE;

                // call mshtml
                mshtmlDropTargetImpl.DragOver(keyState, pt, ref dropEffect);

                // copy any changes to the dropEffect back into the event args
                e.Effect = ConvertDropEffect(dropEffect);
            }
            catch (Exception)
            {
                if (allowExceptions)
                    throw;
            }
        }

        /// <summary>
        /// Helper method to call the Mshtml DragLeave routine using .NET DragEventArgs
        /// </summary>
        /// <param name="e"></param>
        private void CallMshtmlDragLeave(bool allowExceptions)
        {
            if (mshtmlDropTargetImpl == null)
                return;

            try
            {
                // call mshtml
                mshtmlDropTargetImpl.DragLeave();
            }
            catch (Exception)
            {
                if (allowExceptions)
                    throw;
            }
        }

        /// <summary>
        /// Helper to cal the Mshtml DragDrop routine using .NET DragEventArgs
        /// </summary>
        /// <param name="e">event args</param>
        private void CallMshtmlDragDrop(DragEventArgs e, bool allowExceptions)
        {
            if (mshtmlDropTargetImpl == null)
                return;

            try
            {
                // extract ole data object
                IOleDataObject oleDataObject = SafeExtractOleDataObject(e.Data);

                // convert data types
                POINT pt = new POINT(); pt.x = e.X; pt.y = e.Y;
                DROPEFFECT dropEffect = ConvertDropEffect(e.AllowedEffect);
                MK keyState = ConvertKeyState(e.KeyState);

                // call mshtml
                mshtmlDropTargetImpl.Drop(oleDataObject, keyState, pt, ref dropEffect);

                // copy any changes to the dropEffect back into the event args
                e.Effect = ConvertDropEffect(dropEffect);
            }
            catch (Exception)
            {
                if (allowExceptions)
                    throw;
            }
        }

        /// <summary>
        /// See if we can extract an IOleDataObject from the .NET data object
        /// if we can't we are basically screwed (no way to get the data to
        /// MSHTML since it only understands IOleDataObject). In that case
        /// return an empty data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>Either the IOleDataObject extracted from the passed
        /// IDataObject or a special 'empty' IOleDataObject</returns>
        public static IOleDataObject ExtractOleDataObject(IDataObject dataObject)
        {
            OleDataObject oleDataObject = OleDataObject.CreateFrom(dataObject);
            if (oleDataObject != null)
                return oleDataObject.IOleDataObject;
            else
            {
                return null;
            }
        }

        public IOleDataObject SafeExtractOleDataObject(IDataObject dataObject)
        {
            IOleDataObject oleDataObject = ExtractOleDataObject(dataObject);
            return oleDataObject ?? emptyDataObject;
        }

        /// <summary>
        /// Helper to provide drop feedback
        /// </summary>
        /// <param name="e">event args</param>
        /// <returns>effects allowed for drop</returns>
        private DragDropEffects ProvideDragFeedback(DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) > 0)
                return DragDropEffects.Copy;
            else if ((e.AllowedEffect & DragDropEffects.Link) > 0)
                return DragDropEffects.Link;
            else
                return DragDropEffects.None;
        }

        /// <summary>
        /// Helper to convert .NET drop-effect into Win32 drop effect
        /// </summary>
        /// <param name="dropEffect">drop effect to convert</param>
        /// <returns>converted effect</returns>
        public static DROPEFFECT ConvertDropEffect(DragDropEffects dropEffect)
        {
            DROPEFFECT targetDropEffect = DROPEFFECT.NONE;
            if ((dropEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                targetDropEffect |= DROPEFFECT.COPY;
            if ((dropEffect & DragDropEffects.Link) == DragDropEffects.Link)
                targetDropEffect |= DROPEFFECT.LINK;
            if ((dropEffect & DragDropEffects.Move) == DragDropEffects.Move)
                targetDropEffect |= DROPEFFECT.MOVE;
            if ((dropEffect & DragDropEffects.Scroll) == DragDropEffects.Scroll)
                targetDropEffect |= DROPEFFECT.SCROLL;
            return targetDropEffect;
        }

        /// <summary>
        /// Helper to convert Win32 drop effect into .NET drop effect
        /// </summary>
        /// <param name="dropEffect">drop effect to convert</param>
        /// <returns>converted effect</returns>
        public static DragDropEffects ConvertDropEffect(DROPEFFECT dropEffect)
        {
            DragDropEffects targetDropEffect = DragDropEffects.None;
            if ((dropEffect & DROPEFFECT.COPY) == DROPEFFECT.COPY)
                targetDropEffect |= DragDropEffects.Copy;
            if ((dropEffect & DROPEFFECT.LINK) == DROPEFFECT.LINK)
                targetDropEffect |= DragDropEffects.Link;
            if ((dropEffect & DROPEFFECT.MOVE) == DROPEFFECT.MOVE)
                targetDropEffect |= DragDropEffects.Move;
            if ((dropEffect & DROPEFFECT.SCROLL) == DROPEFFECT.SCROLL)
                targetDropEffect |= DragDropEffects.Scroll;
            return targetDropEffect;
        }

        /// <summary>
        /// Helper to convert .NET key state into Win32 key state
        /// </summary>
        /// <param name="keyState">.NET key state</param>
        /// <returns>Win32 key state</returns>
        public static MK ConvertKeyState(int keyState)
        {
            MK targetKeyState = 0;
            if ((keyState & 1) == 1)
                targetKeyState |= MK.LBUTTON;
            if ((keyState & 2) == 2)
                targetKeyState |= MK.RBUTTON;
            if ((keyState & 4) == 4)
                targetKeyState |= MK.SHIFT;
            if ((keyState & 8) == 8)
                targetKeyState |= MK.CONTROL;
            if ((keyState & 16) == 16)
                targetKeyState |= MK.MBUTTON;
            // NOTE: Win32 MK constants don't include the alt-key, the underlying
            // implementation will check for it using GetKeyState(VK_MENU) < 0
            return targetKeyState;
        }

        #endregion

        #region Implementation of IDropTarget

        // NOTE: while we do technically replace the MSHTML IDropTarget interface with our own,
        // this is not truly the mechanism employed to take over drag and drop (see comments
        // on initialization of this class for more). These implementations should not to our
        // knowledge be called so we simply assert and pass them through (as a failsafe) if
        // they do happen to get called.

        /// <summary>
        /// Indicates whether a drop can be accepted, and, if so, the effect of the drop
        /// </summary>
        /// <param name="pDataObj">Pointer to the IDataObject interface on the data object</param>
        /// <param name="grfKeyState">Current state of the keyboard modifier keys on the keyboard</param>
        /// <param name="pt">POINTL structure containing the current cursor coordinates in screen coordinates</param>
        /// <param name="pdwEffect">On entry, pointer to the value of the pdwEffect parameter of the DoDragDrop function. On return, must contain one of the effect flags from the DROPEFFECT enumeration, which indicates what the result of the drop operation would be</param>
        void OpenLiveWriter.Interop.Com.IDropTarget.DragEnter(IOleDataObject pDataObj, MK grfKeyState, POINT pt, ref DROPEFFECT pdwEffect)
        {
            Debug.Fail("Unexpected call to IDropTarget.DragEnter");

            if (mshtmlDropTargetImpl != null)
            {
                mshtmlDropTargetImpl.DragEnter(pDataObj, grfKeyState, pt, ref pdwEffect);
            }
        }

        /// <summary>
        /// Provides target feedback to the user and communicates the drop's effect to the DoDragDrop function so it can communicate the effect of the drop back to the source
        /// </summary>
        /// <param name="grfKeyState">Current state of the keyboard modifier keys on the keyboard</param>
        /// <param name="pt">POINTL structure containing the current cursor coordinates in screen coordinates</param>
        /// <param name="pdwEffect">Pointer to the current effect flag. Valid values are from the enumeration DROPEFFECT</param>
        void OpenLiveWriter.Interop.Com.IDropTarget.DragOver(MK grfKeyState, POINT pt, ref DROPEFFECT pdwEffect)
        {
            Debug.Fail("Unexpected call to IDropTarget.DragOver");

            if (mshtmlDropTargetImpl != null)
            {
                mshtmlDropTargetImpl.DragOver(grfKeyState, pt, ref pdwEffect);
            }
        }

        /// <summary>
        /// Removes target feedback and releases the data object
        /// </summary>
        void OpenLiveWriter.Interop.Com.IDropTarget.DragLeave()
        {
            Debug.Fail("Unexpected call to IDropTarget.DragLeave");

            if (mshtmlDropTargetImpl != null)
            {
                mshtmlDropTargetImpl.DragLeave();
            }
        }

        /// <summary>
        /// Incorporates the source data into the target window, removes target feedback, and releases the data object
        /// </summary>
        /// <param name="pDataObj">Pointer to the IDataObject interface on the data object being transferred in the drag-and-drop operation</param>
        /// <param name="grfKeyState">Current state of the keyboard modifier keys on the keyboard</param>
        /// <param name="pt">POINTL structure containing the current cursor coordinates in screen coordinates</param>
        /// <param name="pdwEffect">Pointer to the current effect flag. Valid values are from the enumeration DROPEFFECT</param>
        void OpenLiveWriter.Interop.Com.IDropTarget.Drop(IOleDataObject pDataObj, MK grfKeyState, POINT pt, ref DROPEFFECT pdwEffect)
        {
            Debug.Fail("Unexpected call to IDropTarget.Drop");

            if (mshtmlDropTargetImpl != null)
            {
                mshtmlDropTargetImpl.Drop(pDataObj, grfKeyState, pt, ref pdwEffect);
            }
        }

        #endregion

        #region Private Member Variables

        /// <summary>
        /// Interface to context/services of presentation editor we are hosted on
        /// </summary>
        private IHtmlMarshallingTarget htmlMarshallingTarget = null;

        // <summary>
        /// Default mshtml drop target implementation
        /// </summary>
        private OpenLiveWriter.Interop.Com.IDropTarget mshtmlDropTargetImpl = null;

        /// <summary>
        /// Null data object that is passed in to MSHTML when we want him
        /// to provide automatic document scrolling when the drag cursor
        /// reaches the edge of the document
        /// </summary>
        OleDataObjectImpl emptyDataObject = new OleDataObjectImpl();

        #endregion
    }
}
