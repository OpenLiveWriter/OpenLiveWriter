// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class PostBodyEditingElementBehavior : EditableRegionElementBehavior
    {
        BlogPostHtmlEditorControl _editor;
        public PostBodyEditingElementBehavior(BlogPostHtmlEditorControl editor, IHtmlEditorComponentContext editorContext, IHTMLElement prevEditableRegion, IHTMLElement nextEditableRegion)
            : base(editorContext, prevEditableRegion, nextEditableRegion)
        {
            _editor = editor;
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();

            if (PostEditorSettings.AutomationMode)
            {
                //Test automation requires the element to be explicitly named in the accessibility tree, but
                //setting a title causes an annoying tooltip and focus rectangle, so we only show it in automation mode
                HTMLElement.title = "Post Body";
                HTMLElement2.tabIndex = 1;
            }

            EditorContext.PerformTemporaryFixupsToEditedHtml += new TemporaryFixupHandler(DetachExtendedEntryBehavior);
        }

        protected override void OnKeyDown(object o, HtmlEventArgs e)
        {
            // this is orthoganal to keyboard processing (never cancels) so want to make
            // sure that we always do it if requested)
            if (HtmlEditorSettings.AggressivelyInvalidate)
                Invalidate();

            if (e.htmlEvt.altKey)
            {
                Keys keys = (Keys)e.htmlEvt.keyCode;
                //alt+Left/Right is the control navigation shortcut
                if (keys == Keys.Right || keys == Keys.Left)
                {
                    bool forward = keys == Keys.Right;
                    SelectNextControlElement(forward);
                    e.htmlEvt.cancelBubble = true;
                    return;
                }
            }

            base.OnKeyDown(o, e);

            if (e.WasCancelled)
                return;
        }

        private void SelectNextControlElement(bool forward)
        {
            MarkupPointer p;
            if (forward)
                p = EditorContext.Selection.SelectedMarkupRange.End.Clone();
            else
                p = EditorContext.Selection.SelectedMarkupRange.Start.Clone();

            IHTMLElement htmlElement = GetNextElement(p, ElementRange, new IHTMLElementFilter(IsSelectableControlElement), forward);
            if (ContentSourceManager.IsSmartContent(htmlElement))
            {
                SmartContentSelection.SelectIfSmartContentElement(EditorContext, htmlElement);
            }
            else if (htmlElement is IHTMLControlElement)
            {
                _editor.SelectControlElement((IHTMLControlElement)htmlElement);
            }
            if (htmlElement != null)
                htmlElement.scrollIntoView(!forward);
        }

        private bool IsSelectableControlElement(IHTMLElement e)
        {
            return ElementFilters.IsImageElement(e) || ContentSourceManager.IsSmartContent(e);
        }

        private IHTMLElement GetNextElement(MarkupPointer start, MarkupRange boundaries, IHTMLElementFilter filter, bool forward)
        {
            start = start.Clone();
            MarkupPointer boundary = forward ? boundaries.End : boundaries.Start;
            MarkupContext moveResult = new MarkupContext();
            _MARKUP_CONTEXT_TYPE skipContext = _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope;

            //advance the pointer
            if (forward)
                start.Right(true, moveResult);
            else
                start.Left(true, moveResult);

            while (forward ? start.IsLeftOf(boundary) : start.IsRightOf(boundary))
            {
                if (moveResult.Element != null && moveResult.Context != skipContext && filter(moveResult.Element))
                {
                    return moveResult.Element;
                }
                //advance the pointer
                if (forward)
                    start.Right(true, moveResult);
                else
                    start.Left(true, moveResult);
            }
            return null;
        }

        protected override void OnCommandKey(object sender, KeyEventArgs e)
        {
            base.OnCommandKey(sender, e);

            if (e.Handled)
                return;

            if (e.KeyCode == Keys.Tab && e.Shift && ShiftTabFocusChangeSupported())
            {
                SelectPreviousRegion();
                //Cancel the event so that it doesn't trigger the blockquote command
                e.Handled = true;
            }
        }

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();

            OnEditableRegionFocusChanged(null, new EditableRegionFocusChangedEventArgs(Selected && EditorContext.EditMode));
        }

        /// <summary>
        /// Returns true if shift+tab focused region changing is supported for the current edit location
        /// </summary>
        /// <returns></returns>
        private bool ShiftTabFocusChangeSupported()
        {
            //Shift-tab is only supported if the caret is positioned at the beginning of the post
            //If the selection is currently inside a non-blockquote block element with no visible content
            //to the left of it, then focus change is supported.

            if (!EditorContext.Selection.SelectedMarkupRange.IsEmpty())
                return false;

            MarkupRange range = ElementRange.Clone();
            range.Start.MoveAdjacentToElement(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            range.End = EditorContext.Selection.SelectedMarkupRange.Start;

            //if there is any text between the caret and the beginning of the post,
            //then ShiftTab focus changing is not allowed.
            string text = range.Text;
            if (text != null && range.Text.Trim() != String.Empty)
                return false;

            MarkupContext context = new MarkupContext();
            range.Start.Right(true, context);
            int blockElementDepth = 0;
            while (range.Start.IsLeftOfOrEqualTo(range.End))
            {
                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope)
                {
                    string tagName = context.Element.tagName;
                    if (tagName.Equals("BLOCKQUOTE") || ElementFilters.IsListElement(context.Element) || ElementFilters.IsListItemElement(context.Element))
                        return false; //ShiftTab in a blockquote or list implies "un-blockquote" or "un-list"
                    else if (ElementFilters.IsBlockElement(context.Element))
                    {
                        blockElementDepth++;
                        if (blockElementDepth > 1)
                            return false; //there are multiple block elements, so this is not the beginning
                    }
                    else if (ElementFilters.IsVisibleEmptyElement(context.Element))
                        return false; //there is a visible empty element (like an image), so this is not the beginning
                }
                else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                {
                    if (ElementFilters.IsVisibleEmptyElement(context.Element))
                        return false; //there is a visible empty element (like an image), so this is not the beginning
                }
                range.Start.Right(true, context);
            }

            return true;
        }

        private static void DetachExtendedEntryBehavior(TemporaryFixupArgs args)
        {
            string html = args.Html;

            if (html.Contains(EXTENDED_ENTRY_ID))
            {
                //replace the EXTENDED_ENTRY_ID behavior div with the <!--more--> comment
                StringBuilder output = new StringBuilder(html.Length);
                SimpleHtmlParser parser = new SimpleHtmlParser(html);
                SmartPredicate splitDiv = new SmartPredicate(String.Format(CultureInfo.InvariantCulture, "<div id='{0}'>", EXTENDED_ENTRY_ID));
                for (Element el; null != (el = parser.Next());)
                {
                    if (splitDiv.IsMatch(el))
                    {
                        Element e = parser.Peek(0);
                        if (e is EndTag && ((EndTag)e).NameEquals("div"))
                        {
                            output.Append(BlogPost.ExtendedEntryBreak);
                            parser.Next();
                        }

                    }
                    else
                        output.Append(html, el.Offset, el.Length);
                }
                args.Html = output.ToString();
            }
        }

        /// <summary>
        /// Inserts the extended entry break into the editor.
        /// </summary>
        internal void InsertExtendedEntryBreak()
        {
            IHTMLDocument3 doc3 = (IHTMLDocument3)HTMLElement.document;
            IHTMLElement2 entryBreak = (IHTMLElement2)doc3.getElementById(EXTENDED_ENTRY_ID);
            if (entryBreak == null)
            {
                using (IUndoUnit undo = EditorContext.CreateUndoUnit())
                {
                    using (EditorContext.DamageServices.CreateDamageTracker(ElementRange.Clone(), true))
                    {
                        MarkupPointer insertionPoint =
                            EditorContext.MarkupServices.CreateMarkupPointer(EditorContext.Selection.SelectedMarkupRange.Start);

                        //delete the parent block element of the insertion point if it is empty (bug 421500)
                        DeleteInsertionTargetBlockIfEmpty(insertionPoint);

                        IHTMLElement extendedEntryBreak = InsertExtendedEntryBreak(insertionPoint);

                        //reselect the insertion point
                        MarkupRange selection = EditorContext.MarkupServices.CreateMarkupRange();
                        insertionPoint.MoveAdjacentToElement(extendedEntryBreak, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                        MarkupPointerMoveHelper.MoveUnitBounded(
                            insertionPoint, MarkupPointerMoveHelper.MoveDirection.RIGHT,
                            MarkupPointerAdjacency.AfterEnterBlock | MarkupPointerAdjacency.BeforeText
                            , HTMLElement);
                        selection.Start.MoveToPointer(insertionPoint);
                        selection.End.MoveToPointer(insertionPoint);
                        selection.ToTextRange().select();
                    }
                    undo.Commit();
                }
            }
        }

        /// <summary>
        /// Inserts the extended entry break into the editor at the specified location.
        /// </summary>
        internal IHTMLElement InsertExtendedEntryBreak(MarkupPointer insertionPoint)
        {
            IHTMLElement entryBreakDiv = EditorContext.MarkupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_DIV, null);
            IHTMLElement postBodyElement = HTMLElement;
            insertionPoint.PushCling(false);
            insertionPoint.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);
            try
            {
                //insert the entryBreak DIV into the beginning of the post body
                entryBreakDiv.id = EXTENDED_ENTRY_ID;
                entryBreakDiv.setAttribute("name", EXTENDED_ENTRY_ID, 0);

                MarkupRange markupRange = EditorContext.MarkupServices.CreateMarkupRange();
                markupRange.MoveToElement(postBodyElement, false);
                markupRange.End.MoveToPointer(markupRange.Start);
                EditorContext.MarkupServices.InsertElement(entryBreakDiv, markupRange.Start, markupRange.End);

                //move all content that should stay above the extended entry line, above the entryBreakDiv
                //this effectively forces all open tags to be closed, and leaves the insertion point below
                //the extended entry line (with the pre-insert parent tree still intact.
                markupRange.Start.MoveAdjacentToElement(entryBreakDiv, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                markupRange.End.MoveToPointer(insertionPoint);
                MarkupPointer target = EditorContext.MarkupServices.CreateMarkupPointer(entryBreakDiv, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                EditorContext.MarkupServices.Move(markupRange.Start, markupRange.End, target);
            }
            finally
            {
                insertionPoint.PopCling();
                insertionPoint.PopGravity();
            }

            return entryBreakDiv;
        }
        private static string EXTENDED_ENTRY_HTML = String.Format(CultureInfo.InvariantCulture, "<div id='{0}' name='{0}'></div>", EXTENDED_ENTRY_ID);
        public const string EXTENDED_ENTRY_ID = "extendedEntryBreak";

        private void DeleteInsertionTargetBlockIfEmpty(MarkupPointer insertionPoint)
        {
            //locate the parent block element (stop at post body element)
            IHTMLElement parent = insertionPoint.GetParentElement(
                ElementFilters.CreateCompoundElementFilter(ElementFilters.CreateElementEqualsFilter(HTMLElement),
                ElementFilters.BLOCK_ELEMENTS));
            if (parent != null &&
                parent.sourceIndex != HTMLElement.sourceIndex &&  //never remove the post body block
                EditorContext.MarkupServices.CreateMarkupRange(parent, false).IsEmptyOfContent())
            {
                //delete the empty parent block element
                (parent as IHTMLDOMNode).removeNode(true);
            }
        }

        internal static string ApplyExtendedEntryBehavior(string contents)
        {
            return contents.Replace(BlogPost.ExtendedEntryBreak, EXTENDED_ENTRY_HTML);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.PerformTemporaryFixupsToEditedHtml -= new TemporaryFixupHandler(DetachExtendedEntryBehavior);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;
    }
}
