// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{

    /// <summary>
    /// Interface implemented to allow the presentation editor to accept dropped items
    /// from a variety of inbound formats.
    /// </summary>
    public abstract class DataFormatHandler : IDisposable
    {
        /// <summary>
        /// Dispose the data format handler
        /// </summary>
        public virtual void Dispose()
        {
            // suppress finalize so we can assert in the finalizer as a signal to the
            // developer that they forgot to dispose
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer -- should never hit this if we are disposed. Fail!
        /// </summary>
        ~DataFormatHandler()
        {
            Debug.Fail("Did not call Dispose on DataFormatHandler!!!!");
        }


        /// <summary>
        /// Optional notification that we are beginning a drag operation
        /// </summary>
        public abstract void BeginDrag();

        /// <summary>
        /// Provide drop feedback
        /// </summary>
        /// <param name="screenPoint">screen-point</param>
        /// <param name="keyState">key-state</param>
        /// <param name="supportedEffects">supported effects</param>
        /// <returns>actual effect</returns>
        public abstract DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects);

        /// <summary>
        /// Optional notification that we are ending a drag operation
        /// </summary>
        public abstract void EndDrag();

        /// <summary>
        /// Notify the data format handler that data was dropped and should be inserted into
        /// the document at whatever insert location the handler has internally tracked.
        /// </summary>
        /// <param name="action"></param>
        public abstract bool DataDropped(DataAction action);

        /// <summary>
        /// Instruct the handler to insert data.
        /// </summary>
        public abstract bool InsertData(DataAction action, params object[] args);

        /// <summary>
        /// Support only copy
        /// </summary>
        /// <param name="supportedEffects">Effects supported by the drag source</param>
        /// <returns>Effets allowed by the drop target</returns>
        protected DragDropEffects ProvideCopy(int keyState, DragDropEffects supportedEffects)
        {
            // allow only copy
            if ((supportedEffects & DragDropEffects.Copy) > 0)
                return DragDropEffects.Copy;
            else
                return DragDropEffects.None;
        }

        /// <summary>
        /// Support only move
        /// </summary>
        /// <param name="supportedEffects">Effects supported by the drag source</param>
        /// <returns>Effets allowed by the drop target</returns>
        protected DragDropEffects ProvideMove(int keyState, DragDropEffects supportedEffects)
        {
            // allow only move
            if ((supportedEffects & DragDropEffects.Move) > 0)
                return DragDropEffects.Move;
            else
                return DragDropEffects.None;
        }

        /// <summary>
        /// Support copy as default with optional shift-key override to move
        /// </summary>
        /// <param name="supportedEffects">Effects supported by the drag source</param>
        /// <returns>Effets allowed by the drop target</returns>
        protected DragDropEffects ProvideCopyAsDefaultWithMoveOverride(int keyState, DragDropEffects supportedEffects)
        {
            // effects to return
            DragDropEffects effects;

            // do move if supported and the shift key is down
            if (((keyState & 4) > 0) && ((supportedEffects & DragDropEffects.Move) > 0))
                effects = DragDropEffects.Move;
            // default to copy if supported
            else if ((supportedEffects & DragDropEffects.Copy) > 0)
                effects = DragDropEffects.Copy;
            // allow move if that is all that is supported
            else if ((supportedEffects & DragDropEffects.Move) > 0)
                effects = DragDropEffects.Move;
            else
                effects = DragDropEffects.None;

            // return effects
            return effects;
        }

        /// <summary>
        /// Support move as default with optional control-key override to copy
        /// </summary>
        /// <param name="supportedEffects">Effects supported by the drag source</param>
        /// <returns>Effets allowed by the drop target</returns>
        protected DragDropEffects ProvideMoveAsDefaultWithCopyOverride(int keyState, DragDropEffects supportedEffects)
        {
            // effects to return
            DragDropEffects effects;

            // do copy if supported and the control key is down
            if (((keyState & 8) > 0) && ((supportedEffects & DragDropEffects.Copy) > 0))
                effects = DragDropEffects.Copy;
            // default to move if supported
            else if ((supportedEffects & DragDropEffects.Move) > 0)
                effects = DragDropEffects.Move;
            // allow copy if that is all that is supported
            else if ((supportedEffects & DragDropEffects.Copy) > 0)
                effects = DragDropEffects.Copy;
            else
                effects = DragDropEffects.None;

            // return effects
            return effects;
        }

        /// <summary>
        /// Data object meister representing the data that is being handled
        /// </summary>
        protected DataObjectMeister DataMeister { get { return dataMeister; } }
        private DataObjectMeister dataMeister;

        protected DataFormatHandlerContext HandlerContext { get { return handlerContext; } }
        private DataFormatHandlerContext handlerContext;

        /// <summary>
        /// Constructor for data format handlers.
        /// </summary>
        /// <param name="dataObject"></param>
        protected DataFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext)
        {
            this.dataMeister = dataObject;
            this.handlerContext = handlerContext;
        }
    }

    /// <summary>
    /// Action taken on data
    /// </summary>
    public enum DataAction
    {
        Copy,
        Move
    }

    /// <summary>
    /// Context in which the data format handler is being invoked
    /// </summary>
    public enum DataFormatHandlerContext
    {
        Insert,
        ClipboardPaste,
        DragAndDrop
    }
}
