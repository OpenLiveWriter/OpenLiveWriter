// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for BlockquoteEditingBehavior.
    /// </summary>
    public class BlockquoteEditingElementBehavior : HtmlEditorElementBehavior
    {
        public BlockquoteEditingElementBehavior(IHtmlEditorComponentContext editorContext) : base(editorContext)
        {
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();
            if (EditorContext.EditMode)
                ApplyEditingStyles();
        }

        private void ApplyEditingStyles()
        {
            //fix bug that causes extra whitespace to be inserted before/after the blockquote
            //Fix notes: if the editor detected top/bottom padding or borders, the editor assumes that it should be possible
            //to place a caret between the padding/border and the next block element.  This causes the editor to render new
            //editable lines around the first/last <p> elements in the blockquote.  To avoid the issue, we set the runtime
            //style of the element to remove the padding/border
            IHTMLElement2 e2 = (IHTMLElement2)HTMLElement;
            e2.runtimeStyle.paddingTop = "0px";
            e2.runtimeStyle.paddingBottom = "0px";
            e2.runtimeStyle.borderTop = "0px";
            e2.runtimeStyle.borderBottom = "0px";
        }

        protected override bool QueryElementSelected()
        {
            return IsInRange(EditorContext.Selection.SelectedMarkupRange);
        }

        protected override void OnSelectedChanged()
        {
            if (Selected)
            {
                EditorContext.KeyDown += new HtmlEventHandler(EditorContext_KeyDown);
            }
            else
                EditorContext.KeyDown -= new HtmlEventHandler(EditorContext_KeyDown);
        }

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
        }

        private void EditorContext_KeyDown(object o, HtmlEventArgs e)
        {
            if (Attached && e.htmlEvt.keyCode == (int)Keys.Enter)
            {
                HandleEnterKey(e);
            }
        }

        private void HandleEnterKey(HtmlEventArgs e)
        {
            //pressing the enter key on an empty line is used as a gesture for exiting the blockquote
            //If this situation is encountered, move the current empty block element outside of the blockquote
            MarkupRange selection = EditorContext.Selection.SelectedMarkupRange;
            if (selection.IsEmpty())
            {
                MarkupPointer selectionPoint = EditorContext.MarkupServices.CreateMarkupPointer(selection.Start);
                selectionPoint.Cling = true;

                IHTMLElement currBlock = selection.Start.CurrentBlockScope();
                MarkupRange currBlockRange = EditorContext.MarkupServices.CreateMarkupRange(currBlock, false);
                if (currBlockRange.IsEmptyOfContent())
                {
                    currBlockRange.MoveToElement(currBlock, true);

                    // Make sure there is no content between the end of this block range and the end of the blockquote.
                    MarkupPointer afterEndCurrBlock = EditorContext.MarkupServices.CreateMarkupPointer(currBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    MarkupPointer beforeEndBlockQuote = EditorContext.MarkupServices.CreateMarkupPointer(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    MarkupRange restOfBlockQuote = EditorContext.MarkupServices.CreateMarkupRange(afterEndCurrBlock, beforeEndBlockQuote);
                    if (!restOfBlockQuote.IsEmpty() || !restOfBlockQuote.IsEmptyOfContent())
                        return;

                    //create a pointer for the new location that the block element will be moved to.
                    MarkupPointer insertionPoint =
                        EditorContext.MarkupServices.CreateMarkupPointer(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

                    //move the current empty block to the DOM location after the blockquote
                    EditorContext.MarkupServices.Move(currBlockRange.Start, currBlockRange.End, insertionPoint);
                    currBlockRange.MoveToElement(currBlock, false);

                    //adjust the selection to the new location of the block element.
                    currBlockRange.Start.MoveToPointer(selectionPoint);
                    currBlockRange.End.MoveToPointer(selectionPoint);
                    currBlockRange.ToTextRange().select();

                    //cancel the key down event so that the editor doesn't try to handle it
                    e.Cancel();
                }
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.KeyDown -= new HtmlEventHandler(EditorContext_KeyDown);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;
    }
}
