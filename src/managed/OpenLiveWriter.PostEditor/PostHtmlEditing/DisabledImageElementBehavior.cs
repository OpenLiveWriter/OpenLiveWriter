// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class DisabledImageElementBehavior : ElementControlBehavior
    {
        private BehaviorDragAndDropSource _dragDropController;

        public DisabledImageElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
            _dragDropController = new DisabledImageDragAndDropSource(editorContext);
        }

        protected override bool QueryElementSelected()
        {
            IHTMLElement selectedImage = EditorContext.Selection.SelectedImage as IHTMLElement;
            if (selectedImage != null)
            {
                return selectedImage.sourceIndex == HTMLElement.sourceIndex;
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
                if (_dragDropController.PreHandleEvent(inEvtDispId, pIEventObj) == HRESULT.S_OK)
                    return HRESULT.S_OK;

                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN && (Control.MouseButtons & MouseButtons.Right) > 0)
                {
                    // Select the disabled image so that the context menu shows up correctly.
                    EditorContext.Selection = DisabledImageSelection.SelectElement(EditorContext, HTMLElement);
                    return HRESULT.S_OK;
                }
            }

            return HRESULT.S_FALSE;
        }
    }
}
