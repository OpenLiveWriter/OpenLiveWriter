// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Undo
{
    internal abstract class OleUndoUnit : IOleUndoUnit
    {
        private MshtmlMarkupServices _markupServices;
        private int _startPosition; //the serialized position of the start pointer
        private int _endPosition; //the serialized position of the end pointer
        private IMarkupContainerRaw _markupContainer;
        private string _description;

        public OleUndoUnit(MshtmlMarkupServices markupServices, MarkupRange selection)
        {
            _markupServices = markupServices;

            //serialize the current position of the markup pointers so that they can be restored if
            //the DOM gets rolled back into the same state via an undo/redo operation.
            if (selection != null && selection.Positioned)
            {
                IMarkupPointer2Raw pointer2StartRaw = selection.Start.PointerRaw as IMarkupPointer2Raw;
                IMarkupPointer2Raw pointer2EndRaw = selection.End.PointerRaw as IMarkupPointer2Raw;
                pointer2StartRaw.GetMarkupPosition(out _startPosition);
                pointer2EndRaw.GetMarkupPosition(out _endPosition);
                pointer2StartRaw.GetContainer(out _markupContainer);
            }

            _description = "OleUndoUnit" + Guid.NewGuid().ToString();

            Undo = true;
        }

        protected bool Undo { get; set; }

        protected MarkupRange GetMarkupRange()
        {
            MarkupRange range = null;

            try
            {
                if (_markupContainer != null)
                {
                    range = _markupServices.CreateMarkupRange();
                    IMarkupPointer2Raw pointer2StartRaw = range.Start.PointerRaw as IMarkupPointer2Raw;
                    IMarkupPointer2Raw pointer2EndRaw = range.End.PointerRaw as IMarkupPointer2Raw;
                    pointer2StartRaw.MoveToMarkupPosition(_markupContainer, _startPosition);
                    pointer2EndRaw.MoveToMarkupPosition(_markupContainer, _endPosition);
                }
            }
            catch (Exception e)
            {
                Debug.Fail("Failed to get markup range", e.ToString());
            }

            return range;
        }

        protected abstract void HandleUndo();

        protected abstract void HandleRedo();

        #region IOleUndoUnit Members

        public virtual void Do(IOleUndoManager undoManager)
        {
            if (undoManager != null)
                undoManager.Add(this);

            if (Undo)
                HandleUndo();
            else
                HandleRedo();

            //invert the undo state
            Undo = !Undo;
        }

        public virtual void GetDescription(out string description)
        {
            description = _description;
        }

        public virtual void GetUnitType()
        {
        }

        public virtual void OnNextAdd()
        {
        }

        #endregion
    }
}
