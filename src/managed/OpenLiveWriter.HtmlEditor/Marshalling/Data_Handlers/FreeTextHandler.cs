// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    /// <summary>
    /// Data format handler for mindshare entities
    /// </summary>
    public abstract class FreeTextHandler : HtmlEditorDataFormatHandler
    {
        MarkupPointer caretPointer;
        protected FreeTextHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {
            caretPointer = editorContext.MarkupServices.CreateMarkupPointer();
        }
        /// <summary>
        /// BeginDrag notification
        /// </summary>
        public override void BeginDrag()
        {
            dragType = DragType.ExternalHtml;
        }

        /// <summary>
        /// Provide drop feedback
        /// </summary>
        /// <param name="screenPoint">screen-point</param>
        /// <param name="keyState">key-state</param>
        /// <param name="supportedEffects">supported effects</param>
        /// <returns>actual effect</returns>
        public override DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects)
        {
            // update insertion point
            try
            {
                currentCaretLocation = EditorContext.MoveCaretToScreenPoint(screenPoint);

                //move the caret markup pointer to the new caret location
                currentCaretLocation.MoveMarkupPointerToCaret(caretPointer.PointerRaw);

                //determine if this location is content editable, if not provide None feedback
                //since that is not a valid drop location
                IHTMLElement3 element = (IHTMLElement3)caretPointer.CurrentScope;
                bool visible = false;
                currentCaretLocation.IsVisible(out visible);
                if (!element.isContentEditable || !visible)
                    return DragDropEffects.None;
            }
            catch (Exception ex)
            {
                if (ex is COMException)
                {
                    int errorCode = ((COMException)ex).ErrorCode;
                    if (errorCode == IE_CTL_E.INVALIDLINE || errorCode == IE_CTL_E.UNPOSITIONEDELEMENT || errorCode == IE_CTL_E.UNPOSITIONEDPOINTER)
                        return DragDropEffects.None;
                }

                Trace.Fail("Exception thrown while providing drag feedback: " + ex);

                //bug fix 1115: eat the exception if one is thrown while placing the caret.
                //this can occur when attempting to move the caret into an HTML control (such as an image or table)
                return DragDropEffects.None;
            }

            // provide feedback depending upon what we are processing
            switch (dragType)
            {
                case DragType.ExternalHtml:
                    if (!EditorContext.CanDrop(caretPointer.CurrentScope, DataMeister))
                        return DragDropEffects.None;

                    // for external html provide move and copy (prefer move -- this allows
                    // us to smoothly handle the moving of images around the document)
                    return ProvideMoveAsDefaultWithCopyOverride(keyState, supportedEffects);
                default:
                    return DragDropEffects.None;
            }
        }

        /// <summary>
        /// Release any reference to HTML Caret
        /// </summary>
        public override void EndDrag()
        {
            currentCaretLocation = null;
        }

        /// <summary>
        /// Notify the data format handler that data was dropped and should be inserted into
        /// the document at whatever insert location the handler has internally tracked.
        /// </summary>
        /// <param name="action"></param>
        public override bool DataDropped(DataAction action)
        {
            if (currentCaretLocation == null)
                return false;

            // create two markup pointers that map to the location of the caret
            MarkupPointer begin = EditorContext.MarkupServices.CreateMarkupPointer();
            MarkupPointer end = EditorContext.MarkupServices.CreateMarkupPointer();
            EditorContext.MarkupServices.MoveMarkupPointerToCaret(currentCaretLocation, begin);
            MarkupPointerMoveHelper.PerformImageBreakout(begin);

            //optimize the drop location to keep it from being in an unexpected location (fixes bug 395224)
            if (EditorContext.ShouldMoveDropLocationRight(begin))
                begin.Right(true);

            //synchronize the end pointer with the being pointer
            end.MoveToPointer(begin);

            MarkupRange selectedRange = EditorContext.SelectedMarkupRange;
            // WinLive 91888 Photomail image drag drop loses images
            //if (!selectedRange.IsEmpty() && selectedRange.InRange(end))
            if (!selectedRange.IsEmpty() && selectedRange.InRange(end, false))
            {
                //the drop location is over the drag source location, so don't so anything.
                return false;
            }

            // Forces a SelectionChanged event so that the correct behaviors around the drop location are activated.
            // For example, one side effect of this call is that the OnEditableRegionFocusChanged event is fired, which
            // sets whether the current drop location in the canvas supports images, html and/or text.
            MarkupRange dropRange = EditorContext.MarkupServices.CreateMarkupRange(begin, end);
            dropRange.ToTextRange().select();

            try
            {
                // insert the data at the current insertion point
                return InsertData(action, begin, end);
            }
            catch (Exception e)
            {
                Trace.Fail(e.Message, e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// The type of drag we are processing
        /// </summary>
        private enum DragType { ExternalHtml };
        private DragType dragType;

        /// <summary>
        /// Track the current caret location
        /// </summary>
        private IHTMLCaretRaw currentCaretLocation = null;
    }
}
