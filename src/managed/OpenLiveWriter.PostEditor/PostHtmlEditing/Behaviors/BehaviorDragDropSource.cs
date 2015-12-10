// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.Emoticons;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Handles initiation of drag and drop for behaviors in the editor
    /// </summary>
    internal abstract class BehaviorDragAndDropSource : IDisposable
    {
        /// <summary>
        /// Initialize with editor context
        /// </summary>
        /// <param name="context">editor context</param>
        public BehaviorDragAndDropSource(IHtmlEditorComponentContext context)
        {
            // store reference to context
            EditorContext = context;
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// Pre-process mouse messages to detect drag-and-drop of selections
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_FALSE to continue default processing, S_OK to prevent further processing</returns>
        internal int PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            switch (inEvtDispId)
            {
                // pre-handle mouse events
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                    return PreHandleMouseDown(inEvtDispId, pIEventObj);
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                    return PreHandleMouseUp(inEvtDispId, pIEventObj);
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE:
                    return PreHandleMouseMove(inEvtDispId, pIEventObj);

                // allow all other events to pass through
                default:
                    return HRESULT.S_FALSE;
            }
        }

        // <summary>
        /// Pre-process mouse messages to detect drag-and-drop of selections
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_FALSE to continue default processing, S_OK to prevent further processing</returns>
        private int PreHandleMouseDown(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            // if this is a left mouse down over an existing selection then start
            // watching for a drag and drop
            if (CouldBeDragBegin(pIEventObj))
            {
                // set state for drag/drop detection
                watchForDragDrop = true;
                dragDropWatchStartPoint = new Point(pIEventObj.clientX, pIEventObj.clientY);

                // prevent MSHTML from even knowing about the MouseDown! (otherwise he
                // will capture the mouse, start drag/drop detection, and generally get
                // in a very confused state)
                return HRESULT.S_OK;
            }
            else
            {
                // allow default processing
                return HRESULT.S_FALSE;
            }
        }

        /// <summary>
        /// Might this be a drag begin>
        /// </summary>
        /// <param name="pIEventObj"></param>></param>
        /// <returns>true if it could be a drag begin</returns>
        protected bool CouldBeDragBegin(IHTMLEventObj pIEventObj)
        {
            // if the left mouse button is down
            if ((Control.MouseButtons & MouseButtons.Left) > 0)
            {
                return true;
            }
            else
                return false;
        }

        // <summary>
        /// Pre-process mouse messages to detect drag-and-drop of selections
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_FALSE to continue default processing, S_OK to prevent further processing</returns>
        private int PreHandleMouseMove(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (watchForDragDrop)
            {
                // compare against drag watch start point
                Point currentPoint = new Point(pIEventObj.clientX, pIEventObj.clientY);
                if (DragDropHelper.PointOutsideDragSize(currentPoint, dragDropWatchStartPoint))
                {
                    // no longer watching
                    watchForDragDrop = false;

                    DoDragDrop(pIEventObj);
                }

                // suppress event (MSHTML blocked from seeing mouse-moves while we are
                // doing drag-drop detection and processing)
                return HRESULT.S_OK;
            }
            else
            {
                // continue default processing
                return HRESULT.S_FALSE;
            }
        }

        protected abstract void DoDragDrop(IHTMLEventObj pIEventObj);

        /// <summary>
        /// Pre-process mouse messages to detect drag-and-drop of selections
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_FALSE to continue default processing, S_OK to prevent further processing</returns>
        private int PreHandleMouseUp(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            // if we are watching for a drag and drop and we didn't get one then
            // position the caret where the mouse originally went down
            if (watchForDragDrop)
            {
                // no longer watching for drag drop
                watchForDragDrop = false;

                // handled internally, don't let MSHTML see it
                return HRESULT.S_OK;
            }
            else
            {
                // do default processing
                return HRESULT.S_FALSE;
            }
        }

        protected class Undo : IDisposable
        {
            private IHtmlEditorComponentContext _editorContext;
            public Undo(IHtmlEditorComponentContext editorContext)
            {
                _editorContext = editorContext;
                editorContext.MarkupServices.BeginUndoUnit(Guid.NewGuid().ToString());
            }
            public void Dispose()
            {
                _editorContext.MarkupServices.EndUndoUnit();
            }
        }

        /// <summary>
        /// Editor context
        /// </summary>
        protected IHtmlEditorComponentContext EditorContext { get; private set; }

        /// <summary>
        /// Watch for a drag drop?
        /// </summary>
        bool watchForDragDrop = false;

        /// <summary>
        /// Start point to watch for drag drop from
        /// </summary>
        Point dragDropWatchStartPoint;
    }

    internal class SmartContentDragAndDropSource : BehaviorDragAndDropSource
    {
        public SmartContentDragAndDropSource(IHtmlEditorComponentContext context)
            : base(context)
        {
        }

        protected override void DoDragDrop(IHTMLEventObj pIEventObj)
        {
            SmartContentSelection smartSelection = EditorContext.Selection as SmartContentSelection;
            if (smartSelection == null)
                return;

            // allow each of the drop source format handlers a chance to create the
            // drop-source data object
            IDataObject dataObject = SmartContentDataObject.CreateFrom(smartSelection.HTMLElement, EditorContext.EditorId);

            // do the drag and drop
            using (new Undo(EditorContext))
            {
                EditorContext.DoDragDrop(dataObject, DragDropEffects.Move);
            }

            // If the user dragged into a restrcited area(like an edit field) the markup range is destroyed
            // when we strip out the html, so dont try to select it
            IHTMLElement e = smartSelection.HTMLElement;
            if (e.sourceIndex < 0 || e.document == null || ((IHTMLDocument2)e.document).body == null)
                return;

            //re-select the smartContent item now that the drag/dropis done
            SmartContentSelection.SelectIfSmartContentElement(EditorContext, smartSelection.HTMLElement, smartSelection.ContentState);

            // Update the area around the smart content for inline spelling
            MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(smartSelection.HTMLElement, true);
            EditorContext.DamageServices.AddDamage(range);
        }
    }

    internal class DisabledImageDragAndDropSource : BehaviorDragAndDropSource
    {
        public DisabledImageDragAndDropSource(IHtmlEditorComponentContext context)
            : base(context)
        {
        }

        protected override void DoDragDrop(IHTMLEventObj pIEventObj)
        {
            IHTMLElement element = pIEventObj.srcElement;

            // Make sure the element is an image.
            IHTMLImgElement imgElement = element as IHTMLImgElement;
            if (imgElement == null)
                return;

            // We'll need to uniquely identify this image when its inserted at a new spot.
            string oldElementId = element.id;
            element.id = Guid.NewGuid().ToString();

            IDataObject dataObject = SmartContentDataObject.CreateFrom(element, EditorContext.EditorId);

            // do the drag and drop
            using (new Undo(EditorContext))
            {
                EditorContext.DoDragDrop(dataObject, DragDropEffects.Move);
            }

            // Revert back to the old id after drag/drop is done.
            element.id = oldElementId;
        }
    }

    public class SmartContentDataObject
    {
        /// <summary>
        /// Attempt to create a smart content data object from the selection.
        /// </summary>
        /// <returns>data object (if could be created), otherwise null</returns>
        public static IDataObject CreateFrom(IHTMLElement element, string editorId)
        {
            // create a new new data object
            DataObject dataObject = new DataObject();

            //add internal format
            if (element != null)
            {
                dataObject.SetData(INTERNAL_SMART_CONTENT_DATAFORMAT, element.id);
                dataObject.SetData(INSTANCE_ID_DATAFORMAT, editorId);
            }

            // return the data object
            return dataObject;
        }

        public static bool IsContainedIn(IDataObject dataObject)
        {
            return OleDataObjectHelper.GetDataPresentSafe(dataObject, INSTANCE_ID_DATAFORMAT);
        }

        /// <summary>
        /// Name of data format used to represent the instance id of a presentation editor
        /// (used to identify an internal operation vs. a cross-process one)
        /// </summary>
        internal const string INSTANCE_ID_DATAFORMAT = "OpenLiveWriterInstanceId";

        /// <summary>
        /// Name of data format for moving around items internaly
        /// </summary>
        internal const string INTERNAL_SMART_CONTENT_DATAFORMAT = "OpenLiveWriterInternalSmartContent";
    }
}
