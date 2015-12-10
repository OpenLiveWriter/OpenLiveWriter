// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for DisabledSmartContentElementBehavior.
    /// </summary>
    internal class DisabledSmartContentElementBehavior : ElementControlBehavior
    {
        private FocusControl _focusControl;
        private SmartContentState _contentState = SmartContentState.Disabled;
        private IContentSourceSidebarContext _contentSourceContext;
        BehaviorDragAndDropSource _dragDropController;
        public DisabledSmartContentElementBehavior(IHtmlEditorComponentContext editorContext, IContentSourceSidebarContext contentSourceContext)
            : base(editorContext)
        {
            _focusControl = new FocusControl(this);
            _focusControl.Visible = false;
            _contentSourceContext = contentSourceContext;
            Controls.Add(_focusControl);
            _dragDropController = new SmartContentDragAndDropSource(editorContext);
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();
            (HTMLElement as IHTMLElement3).contentEditable = "false";

            string sourceId;
            string contentId;
            if (HTMLElement.id != null)
            {
                try
                {
                    if (HTMLElement.className == HtmlPreserver.PRESERVE_CLASS)
                        _contentState = SmartContentState.Preserve;
                    else
                    {
                        ContentSourceManager.ParseContainingElementId(HTMLElement.id, out sourceId, out contentId);
                        if (sourceId == null || contentId == null)
                            _contentState = SmartContentState.Broken;
                        else
                        {
                            if (_contentSourceContext.FindSmartContent(contentId) == null)
                                _contentState = SmartContentState.Broken;
                        }
                    }
                }
                catch (Exception)
                {
                    _contentState = SmartContentState.Broken;
                }
            }
            else
                _contentState = SmartContentState.Broken;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    _dragDropController.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        protected override void OnSelectedChanged()
        {
            _focusControl.Visible = Selected;
            PerformLayout();
            Invalidate();
        }

        private void Select()
        {
            if (_contentState == SmartContentState.Preserve)
            {
                if (HTMLElement != null && HTMLElement.className == HtmlPreserver.PRESERVE_CLASS)
                {
                    PreserveContentSelection.SelectElement(EditorContext, HTMLElement, _contentState);
                }
            }
            else
                SmartContentSelection.SelectIfSmartContentElement(EditorContext, HTMLElement, _contentState);

        }

        protected override bool QueryElementSelected()
        {
            SmartContentSelection selection = EditorContext.Selection as SmartContentSelection;
            if (selection != null)
            {
                return selection.HTMLElement.sourceIndex == HTMLElement.sourceIndex;
            }
            else
            {
                return false;
            }
        }

        protected override int HandlePreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (ShouldProcessEvents(inEvtDispId, pIEventObj))
            {
                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN && !Selected)
                {
                    Select();

                    //notify the drag drop controller about the mouse down so that drag can be initiated
                    //on the first click that selects this element. (Fixes bug that required 2 clicks to
                    //initial drag/drop).
                    _dragDropController.PreHandleEvent(inEvtDispId, pIEventObj);

                    //cancel the event so that the editor doesn't try to do anything funny (like placing a caret at the click)
                    return HRESULT.S_OK;
                }
                int controlResult = base.HandlePreHandleEvent(inEvtDispId, pIEventObj);

                if (_dragDropController.PreHandleEvent(inEvtDispId, pIEventObj) == HRESULT.S_OK)
                {
                    return HRESULT.S_OK;
                }

                //eat the mouse events so that the editor doesn't try to
                //do anything funny (like opening a browser URL).
                switch (inEvtDispId)
                {
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                        return HRESULT.S_OK;
                }
            }

            return HRESULT.S_FALSE;
        }
    }
}
