// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public enum SmartContentState { Enabled, Disabled, Broken, Preserve }
    /// <summary>
    /// The editor selection for a structured content block.
    /// </summary>
    ///
    ///

    public class PreserveContentSelection : ContentSelection
    {
        protected PreserveContentSelection(IHtmlEditorComponentContext editorComponentContext, IHTMLElement element, SmartContentState contentState)
            : base(editorComponentContext, element, contentState)
        {
        }

        public static PreserveContentSelection SelectElement(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e, SmartContentState contentState)
        {
            PreserveContentSelection selection = (PreserveContentSelection)SelectElementCore(editorComponentContext, e, new PreserveContentSelection(editorComponentContext, e, contentState));
            return selection;
        }
    }

    public class SmartContentSelection : ContentSelection
    {
        protected SmartContentSelection(IHtmlEditorComponentContext editorComponentContext, IHTMLElement element, SmartContentState contentState)
            : base(editorComponentContext, element, contentState)
        {
        }

        public static SmartContentSelection SelectIfSmartContentElement(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e)
        {
            return SelectIfSmartContentElement(editorComponentContext, e, SmartContentState.Enabled);
        }

        public static SmartContentSelection SelectIfSmartContentElement(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e, SmartContentState contentState)
        {
            if (e != null)
            {
                IHTMLElement smartContent = ContentSourceManager.GetContainingSmartContent(e);
                if (smartContent == null)
                    return null;

                return SelectElement(editorComponentContext, smartContent, contentState);
            }
            return null;
        }

        public static SmartContentSelection SelectElement(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e, SmartContentState contentState)
        {
            SmartContentSelection selection = (SmartContentSelection)SelectElementCore(editorComponentContext, e, new SmartContentSelection(editorComponentContext, e, contentState));
            return selection;
        }

    }

    public class DisabledImageSelection : ContentSelection
    {
        protected DisabledImageSelection(IHtmlEditorComponentContext editorComponentContext, IHTMLElement element)
            : base(editorComponentContext, element, SmartContentState.Disabled)
        {
        }

        public static DisabledImageSelection SelectElement(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e)
        {
            return (DisabledImageSelection)SelectElementCore(editorComponentContext, e, new DisabledImageSelection(editorComponentContext, e));
        }
    }

    public class ContentSelection : IHtmlEditorSelection
    {
        private IHTMLElement _element;
        private SmartContentState _contentState;
        private MshtmlMarkupServices _markupServices;
        private MarkupRange _markupRange;
        private IHtmlEditorComponentContext _editorComponentContext;

        protected ContentSelection(IHtmlEditorComponentContext editorComponentContext, IHTMLElement element, SmartContentState contentState)
        {
            _editorComponentContext = editorComponentContext;
            _markupServices = editorComponentContext.MarkupServices;
            _element = element;
            _contentState = contentState;

            _markupRange = _markupServices.CreateMarkupRange(_element, true);
            _markupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            _markupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            _markupRange.Start.Cling = false;
            _markupRange.End.Cling = false;
        }

        public IHTMLSelectionObject HTMLSelectionObject
        {
            get { return ((IHTMLDocument2)_element.document).selection; }
        }

        public bool IsValid
        {
            get
            {
                return _markupRange.Start.Positioned && _markupRange.Start.Positioned;
            }
        }

        public SmartContentState ContentState
        {
            get { return _contentState; }
            set { _contentState = value; }
        }

        public bool HasContiguousSelection
        {
            get
            {
                return true;
            }
        }

        public bool IsEditField
        {
            get
            {
                return SmartContentElementBehavior.IsChildEditFieldSelected(HTMLElement, SelectedMarkupRange);
            }
        }

        public void ExecuteSelectionOperation(HtmlEditorSelectionOperation op)
        {
            //suspend selection change events while the real HTML selection is temporarily adjusted
            //to include the smart content element while the selection operation executes.
            _editorComponentContext.BeginSelectionChange();
            try
            {
                IHTMLDocument2 document = (IHTMLDocument2)HTMLElement.document;
                MarkupRange elementRange = CreateElementClingMarkupRange();
                MarkupRange insertionRange = CreateSelectionBoundaryMarkupRange();
                elementRange.ToTextRange().select();
                op(this);

                //reset the selection
                if (elementRange.Start.Positioned && elementRange.End.Positioned)
                {
                    document.selection.empty();
                    _editorComponentContext.Selection = this;
                }
                else
                {
                    insertionRange.ToTextRange().select();
                }
            }
            finally
            {
                _editorComponentContext.EndSelectionChange();
            }
        }

        /// <summary>
        /// Creates a markup range that will cling to the smart content element.
        /// </summary>
        /// <returns></returns>
        private MarkupRange CreateElementClingMarkupRange()
        {
            MarkupRange markupRange = _markupServices.CreateMarkupRange(HTMLElement);
            markupRange.Start.Cling = true;
            markupRange.End.Cling = true;
            markupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            markupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            return markupRange;
        }

        /// <summary>
        /// Creates a markup range that will cling to the smart content element's
        /// virtual selection boundaries.
        /// </summary>
        /// <returns></returns>
        private MarkupRange CreateSelectionBoundaryMarkupRange()
        {
            MarkupRange markupRange = _markupServices.CreateMarkupRange(HTMLElement);
            markupRange.Start.Cling = false;
            markupRange.End.Cling = false;
            markupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            markupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            return markupRange;
        }

        public IHTMLElement HTMLElement
        {
            get
            {
                return _element;
            }
        }

        public bool Resizable
        {
            get
            {
                return true;
            }
        }

        public MarkupRange SelectedMarkupRange
        {
            get
            {
                return _markupRange;
            }
        }

        public IHTMLImgElement SelectedImage
        {
            get
            {
                return null;
            }
        }

        public IHTMLElement SelectedControl
        {
            get { return null; }
        }

        public IHTMLTable SelectedTable
        {
            get
            {
                return null;
            }
        }

        protected static ContentSelection SelectElementCore(IHtmlEditorComponentContext editorComponentContext, IHTMLElement e, ContentSelection smartContentSelection)
        {
            Debug.Assert(e.sourceIndex > -1, "Cannot select an unpositioned element");
            if (e.sourceIndex > -1) //avoid unhandled exception reported by bug 291968
            {
                //suspend selection change events while the selection object is replaced
                editorComponentContext.BeginSelectionChange();
                try
                {
                    //clear the DOM selection so that whatever is currently selected gets unselected.
                    editorComponentContext.EmptySelection();

                    //select the newly smart content element
                    editorComponentContext.Selection = smartContentSelection;
                    return smartContentSelection;
                }
                finally
                {
                    editorComponentContext.EndSelectionChange();
                }
            }
            return null;
        }

    }
}
