// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using mshtml;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Summary description for HtmlBlockFormatHelper.
    /// </summary>
    public class HtmlBlockFormatHelper
    {
        private MshtmlMarkupServices _markupServices;
        private HtmlEditorControl _editor;
        public HtmlBlockFormatHelper(HtmlEditorControl editor)
        {
            _editor = editor;
            _markupServices = (_editor as IHtmlEditorComponentContext).MarkupServices;
        }
        public static void ApplyBlockStyle(HtmlEditorControl editor, _ELEMENT_TAG_ID styleTagId, MarkupRange selection, MarkupRange maximumBounds, MarkupRange postOpSelection)
        {
            new HtmlBlockFormatHelper(editor).ApplyBlockStyle(styleTagId, selection, maximumBounds, postOpSelection);
        }
        private void ApplyBlockStyle(_ELEMENT_TAG_ID styleTagId, MarkupRange selection, MarkupRange maximumBounds, MarkupRange postOpSelection)
        {
            Debug.Assert(selection != maximumBounds, "selection and maximumBounds must be distinct objects");
            SelectionPositionPreservationCookie selectionPreservationCookie = null;

            //update the range cling and gravity so it will stick with the re-arranged block content
            selection.Start.PushCling(false);
            selection.Start.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);
            selection.End.PushCling(false);
            selection.End.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);

            try
            {
                if (selection.IsEmpty())
                {
                    //nothing is selected, so expand the selection to cover the entire parent block element
                    IHTMLElementFilter stopFilter =
                        ElementFilters.CreateCompoundElementFilter(ElementFilters.BLOCK_ELEMENTS,
                                                                   new IHTMLElementFilter(IsSplitStopElement));
                    MovePointerLeftUntilRegionBreak(selection.Start, stopFilter, maximumBounds.Start);
                    MovePointerRightUntilRegionBreak(selection.End, stopFilter, maximumBounds.End);
                }

                using (IUndoUnit undo = _editor.CreateSelectionUndoUnit(selection))
                {
                    selectionPreservationCookie = SelectionPositionPreservationHelper.Save(_markupServices, postOpSelection, selection);
                    if (selection.IsEmptyOfContent())
                    {
                        ApplyBlockFormatToEmptySelection(selection, styleTagId, maximumBounds);
                    }
                    else
                    {
                        ApplyBlockFormatToContentSelection(selection, styleTagId, maximumBounds);
                    }
                    undo.Commit();
                }
            }
            finally
            {
                selection.Start.PopCling();
                selection.Start.PopGravity();
                selection.End.PopCling();
                selection.End.PopGravity();
            }

            if (!SelectionPositionPreservationHelper.Restore(selectionPreservationCookie, selection, selection.Clone()))
                selection.ToTextRange().select();
        }

        private void ApplyBlockFormatToContentSelection(MarkupRange selection, _ELEMENT_TAG_ID styleTagId, MarkupRange maximumBounds)
        {
            MarkupRange[] stylableBlockRegions = GetSelectableBlockRegions(selection);
            if (stylableBlockRegions.Length > 0)
            {
                //
                // We want to make sure that the selection reflects only the
                // blocks that were changed. Unposition the start and end
                // pointers and then make sure they cover the stylable block
                // regions, no more, no less.
                selection.Start.Unposition();
                selection.End.Unposition();

                foreach (MarkupRange range in stylableBlockRegions)
                {
                    ApplyBlockStyleToRange(styleTagId, range, maximumBounds);

                    if (!selection.Start.Positioned || range.Start.IsLeftOf(selection.Start))
                        selection.Start.MoveToPointer(range.Start);
                    if (!selection.End.Positioned || range.End.IsRightOf(selection.End))
                        selection.End.MoveToPointer(range.End);
                }
            }
        }

        private void ApplyBlockFormatToEmptySelection(MarkupRange selection, _ELEMENT_TAG_ID styleTagId, MarkupRange maximumBounds)
        {
            bool deleteParentBlock = false;

            //expand the selection to include the parent content block.  If the expansion can cover the block element
            //without exceeding the maximum bounds, then delete the parent element and wrap the selection in the
            //new block element. If the maximum bounds are exceeded, then just wrap the selection around the bounds.
            IHTMLElementFilter stopFilter =
                ElementFilters.CreateCompoundElementFilter(ElementFilters.BLOCK_ELEMENTS,
                new IHTMLElementFilter(IsSplitStopElement));
            MovePointerLeftUntilRegionBreak(selection.Start, stopFilter, maximumBounds.Start);
            MovePointerRightUntilRegionBreak(selection.End, stopFilter, maximumBounds.End);

            MarkupRange tmpRange = selection.Clone();
            tmpRange.End.MoveToPointer(selection.Start);
            IHTMLElement startStopParent = tmpRange.End.GetParentElement(stopFilter);
            if (startStopParent != null)
            {
                tmpRange.Start.MoveAdjacentToElement(startStopParent, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                if (tmpRange.IsEmptyOfContent()) //the range from the selection the the block start is empty
                {
                    tmpRange.Start.MoveToPointer(selection.End);
                    IHTMLElement endStopParent = tmpRange.Start.GetParentElement(stopFilter);
                    if (endStopParent != null && startStopParent.sourceIndex == endStopParent.sourceIndex)
                    {
                        tmpRange.Start.MoveAdjacentToElement(endStopParent, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                        if (tmpRange.IsEmptyOfContent()) //the range from the selection the the block end is empty
                        {
                            tmpRange.MoveToElement(endStopParent, true);
                            if (maximumBounds.InRange(tmpRange) && !(endStopParent is IHTMLTableCell))
                            {
                                deleteParentBlock = true; //the parent has no useful content outside the selection, so it's safe to delete
                            }
                        }
                    }
                }
            }

            //delete the block parent (if appropriate) and wrap the selection in the new block element.
            if (deleteParentBlock)
            {
                (startStopParent as IHTMLDOMNode).removeNode(false);
            }
            IHTMLElement newBlock = WrapRangeInBlockElement(selection, styleTagId);
            selection.MoveToElement(newBlock, false);
        }

        private void ApplyBlockStyleToRange(_ELEMENT_TAG_ID styleTagId, MarkupRange range, MarkupRange maximumBounds)
        {
            //update the range cling and gravity so it will stick with the re-arranged block content
            range.Start.PushCling(false);
            range.Start.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);
            range.End.PushCling(false);
            range.End.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);

            try
            {
                MarkupPointer deeperPoint = GetDeeperPoint(range.Start, range.End);
                MarkupPointer insertionPoint = _markupServices.CreateMarkupPointer(deeperPoint);
                insertionPoint.Cling = false;

                //if the insertion point parent block contains content, split the block.  If the parent
                //block is now empty, then just delete the parent block.
                IHTMLElement parentBlock =
                    insertionPoint.GetParentElement(
                    ElementFilters.CreateCompoundElementFilter(ElementFilters.BLOCK_ELEMENTS,
                    new IHTMLElementFilter(IsSplitStopElement)));

                //temporarily stage the range content at the end of the document so that the split
                //operation doesn't damage the original range content
                MarkupRange stagedBlockContent = _markupServices.CreateMarkupRange();
                stagedBlockContent.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                stagedBlockContent.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
                stagedBlockContent.Start.MoveAdjacentToElement(GetBodyElement(), _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                stagedBlockContent.End.MoveToPointer(stagedBlockContent.Start);

                MarkupPointer stagedBlockInsertionPoint = _markupServices.CreateMarkupPointer(GetBodyElement(), _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);

                stagedBlockInsertionPoint.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

                // Pass over all opening elements between parent's adj_beforeend and the start of selection (range.start)
                // Any element (_enterscope) that is not closed before the close of selection is essentially
                // containing the selection completely, and needs to be copied into the staging area.
                // ex:   <p><a href="http://msn.com">abc[selection]def</a></p>
                // Here, the <a> element encloses completely the selection and needs to be explicitly copied to the
                // staging area.
                for (MarkupPointer i = _markupServices.CreateMarkupPointer(parentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    i.IsLeftOf(range.Start); i.Right(true))
                {
                    MarkupContext iContext = i.Right(false);

                    if (iContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope && iContext.Element is IHTMLAnchorElement)
                    {
                        MarkupPointer j = _markupServices.CreateMarkupPointer(iContext.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                        if (j.IsRightOfOrEqualTo(range.End))
                        {
                            // Copy the tag at posn. i to location stagedBlockInsertionPoint
                            // This is openning tag. Closing tag will be
                            // automatically added by MSHTML.
                            MarkupPointer i1 = i.Clone();
                            i1.Right(true);
                            _markupServices.Copy(i, i1, stagedBlockInsertionPoint);
                            // Skip over the closing tag, so stagedBlockInsertionPoint points between the openning and the closing
                            stagedBlockInsertionPoint.Left(true);
                        }
                        j.Unposition();
                    }
                }

                //move the range content into the staged position
                _markupServices.Move(range.Start, range.End, stagedBlockInsertionPoint); stagedBlockInsertionPoint.Unposition();

                bool splitBlock = !RemoveEmptyParentBlock(range.Clone(), parentBlock, maximumBounds);

                // If the range endpoint NOT chosen as the insertion point lies in a different block element, and
                // that parent block element is now empty, then just delete the parent block.
                MarkupPointer shallowerPoint = (deeperPoint.IsEqualTo(range.Start)) ? range.End : range.Start;
                MarkupPointer nonInsertionPoint = _markupServices.CreateMarkupPointer(shallowerPoint);

                IHTMLElement otherParentBlock =
                    nonInsertionPoint.GetParentElement(
                    ElementFilters.CreateCompoundElementFilter(ElementFilters.BLOCK_ELEMENTS,
                    new IHTMLElementFilter(IsSplitStopElement)));
                if (otherParentBlock.sourceIndex != parentBlock.sourceIndex)
                    RemoveEmptyParentBlock(range.Clone(), otherParentBlock, maximumBounds);

                if (splitBlock)
                {
                    //split the block at the insertion point
                    SplitBlockForApplyingBlockStyles(insertionPoint, maximumBounds);
                }

                //move the staged block content back to the insertion point and setup the range pointers
                //to wrap the re-inserted block content.
                range.Start.MoveToPointer(insertionPoint);
                range.End.MoveToPointer(insertionPoint);
                _markupServices.Move(stagedBlockContent.Start, stagedBlockContent.End, insertionPoint);

                //Note: the range is now re-positioned around the same content, but all of the parent
                //elements have been closed to prepare for getting new parent block elements around the
                //range

                //convert the range's content into block regions and remove block elements that were
                //parenting the regions.
                InnerBlockRegion[] blockRegions = GetNormalizedBlockContentRegions(range);

                //update all of the block regions with the desired new block style element
                foreach (InnerBlockRegion blockRegion in blockRegions)
                {
                    IHTMLElement newParentElement = WrapRangeInBlockElement(blockRegion.ContentRange, styleTagId);

                    // The old parent element may have had an alignment set on it.
                    IHTMLElement oldParentElement = blockRegion.OldParentElement ?? parentBlock;
                    if (oldParentElement != null)
                    {
                        string oldAlignment = oldParentElement.getAttribute("align", 2) as string;
                        if (!String.IsNullOrEmpty(oldAlignment))
                            newParentElement.setAttribute("align", oldAlignment, 0);
                    }
                }
            }
            finally
            {
                range.Start.PopCling();
                range.Start.PopGravity();
                range.End.PopCling();
                range.End.PopGravity();
            }
        }

        /// <summary>
        /// Returns true if the parent element was removed and false otherwise.
        /// </summary>
        private bool RemoveEmptyParentBlock(MarkupRange range, IHTMLElement parentBlock, MarkupRange maximumBounds)
        {
            if (parentBlock != null)
            {
                range.MoveToElement(parentBlock, false);
                if (maximumBounds.InRange(range) && range.IsEmptyOfContent())
                {
                    if (!IsSplitStopElement(parentBlock))
                    {
                        //delete the parent node (only if it doesn't fall outside the maxrange (bug 465995))
                        range.MoveToElement(parentBlock, true); //expand the range around deletion area to test for maxBounds exceeded
                        if (maximumBounds.InRange(range))
                        {
                            (parentBlock as IHTMLDOMNode).removeNode(true);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the markup pointer that is most deeply placed within the DOM.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private MarkupPointer GetDeeperPoint(MarkupPointer p1, MarkupPointer p2)
        {
            IHTMLElement startElement = p1.CurrentScope;
            IHTMLElement endElement = p2.CurrentScope;
            int startSourceIndex = startElement != null ? startElement.sourceIndex : -1;
            int endSourceIndex = startElement != null ? endElement.sourceIndex : -1;
            if (startSourceIndex > endSourceIndex || (startSourceIndex == endSourceIndex && p1.IsRightOfOrEqualTo(p2)))
                return p1;
            else
                return p2;
        }

        private IHTMLElement WrapRangeInBlockElement(MarkupRange blockRegion, _ELEMENT_TAG_ID styleTagId)
        {
            MarkupRange insertionRange = _markupServices.CreateMarkupRange();
            //create the new block element
            IHTMLElement newBlockElement = _markupServices.CreateElement(styleTagId, null);

            //insert the new block element in front of the block content
            insertionRange.Start.MoveToPointer(blockRegion.Start);
            insertionRange.End.MoveToPointer(blockRegion.Start);
            _markupServices.InsertElement(newBlockElement, insertionRange.Start, insertionRange.End);

            //move the block content inside the new block element
            insertionRange.Start.MoveAdjacentToElement(newBlockElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            blockRegion.Start.MoveAdjacentToElement(newBlockElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            _markupServices.Move(blockRegion.Start, blockRegion.End, insertionRange.Start);
            return newBlockElement;
        }

        private IHTMLElement GetBodyElement()
        {
            return (_markupServices.MarkupServicesRaw as IHTMLDocument2).body;
        }

        private void SplitBlockForApplyingBlockStyles(MarkupPointer splitPoint, MarkupRange maximumBounds)
        {
            //find the split stop parent
            IHTMLElement splitStop = splitPoint.GetParentElement(new IHTMLElementFilter(IsSplitStopElement));
            if (splitStop != null)
            {
                MarkupPointer stopLocation = _markupServices.CreateMarkupPointer(splitStop, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                if (maximumBounds.InRange(stopLocation))
                {
                    stopLocation.MoveAdjacentToElement(splitStop, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    if (maximumBounds.InRange(stopLocation))
                    {
                        maximumBounds = maximumBounds.Clone();
                        maximumBounds.MoveToElement(splitStop, false);
                    }
                }
            }

            MarkupHelpers.SplitBlockForInsertionOrBreakout(_markupServices, maximumBounds, splitPoint);
        }

        private bool IsSplitStopElement(IHTMLElement e)
        {
            return
                ElementFilters.IsListItemElement(e) ||
                ElementFilters.IsBlockQuoteElement(e) ||
                ElementFilters.IsTableCellElement(e);
        }

        /// <summary>
        /// Splits the specified range into self-contained stylable block regions.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private MarkupRange[] GetSelectableBlockRegions(MarkupRange range)
        {
            ArrayList regions = new ArrayList();
            ElementBreakRegion[] breakRegions = SplitIntoElementRegions(range, new IHTMLElementFilter(IsSplitStopElement));
            //DumpBreakRegions(breakRegions);
            foreach (ElementBreakRegion breakRegion in breakRegions)
            {
                //save the closed block range and start the next range
                if (!breakRegion.ContentRange.IsEmptyOfContent())
                    regions.Add(breakRegion.ContentRange);
            }

            return (MarkupRange[])regions.ToArray(typeof(MarkupRange));
        }

        /// <summary>
        /// Splits the specified range into block regions, and removes all block elements.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private InnerBlockRegion[] GetNormalizedBlockContentRegions(MarkupRange range)
        {
            ArrayList regions = new ArrayList();
            ElementBreakRegion[] breakRegions = SplitIntoElementRegions(range, ElementFilters.BLOCK_ELEMENTS);
            //DumpBreakRegions(breakRegions);
            foreach (ElementBreakRegion breakRegion in breakRegions)
            {
                //save the closed block range and start the next range
                if (!breakRegion.ContentRange.IsEmptyOfContent())
                    regions.Add(new InnerBlockRegion(breakRegion.ContentRange, breakRegion.BreakStartElement ?? breakRegion.BreakEndElement));

                //remove the break elements if they should be deleted
                if (ShouldDeleteForBlockFormatting(breakRegion.BreakStartElement))
                    _markupServices.RemoveElement(breakRegion.BreakStartElement);
                if (ShouldDeleteForBlockFormatting(breakRegion.BreakEndElement))
                    _markupServices.RemoveElement(breakRegion.BreakEndElement);
            }

            return (InnerBlockRegion[])regions.ToArray(typeof(InnerBlockRegion));
        }

        /*private void DumpBreakRegions(params ElementBreakRegion[] breakRegions)
        {
            foreach(ElementBreakRegion breakRegion in breakRegions)
            {
                String elementStartName = breakRegion.BreakStartElement != null ? breakRegion.BreakStartElement.tagName : "";
                String elementEndName = breakRegion.BreakEndElement != null ? breakRegion.BreakEndElement.tagName : "";
                String breakContent = breakRegion.ContentRange.Text;
                if(breakContent != null)
                    breakContent = breakContent.Replace('\r', ' ').Replace('\n', ' ');
                else
                    breakContent = "";
                Trace.WriteLine(String.Format("<{0}>{1}<{2}>", elementStartName, breakContent, elementEndName));
            }
        }*/

        /// <summary>
        /// Splits the specified range into regions based on a region break filter.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private ElementBreakRegion[] SplitIntoElementRegions(MarkupRange range, IHTMLElementFilter regionBreakFilter)
        {
            ArrayList regions = new ArrayList();
            MarkupRange blockRange = _markupServices.CreateMarkupRange();
            blockRange.Start.MoveToPointer(range.Start);
            blockRange.End.MoveToPointer(range.Start);
            MarkupContext moveContext = new MarkupContext();

            ElementBreakRegion currentRegion = new ElementBreakRegion(blockRange, null, null);
            while (currentRegion.ContentRange.End.IsLeftOf(range.End))
            {
                if (moveContext.Element != null)
                {
                    if (regionBreakFilter(moveContext.Element))
                    {
                        //move the end of the region back before the break element to close this region
                        currentRegion.ContentRange.End.Left(true);

                        //save the closed region and start the next region
                        currentRegion.BreakEndElement = moveContext.Element;
                        regions.Add(currentRegion);
                        currentRegion = new ElementBreakRegion(currentRegion.ContentRange.Clone(), moveContext.Element, null);
                        currentRegion.ContentRange.Start.MoveToPointer(currentRegion.ContentRange.End);

                        //move the region start over the break element
                        currentRegion.ContentRange.Start.Right(true, moveContext);
                        currentRegion.ContentRange.End.MoveToPointer(currentRegion.ContentRange.Start);
                    }
                }
                currentRegion.ContentRange.End.Right(true, moveContext);
            }

            //save the last break region
            if (moveContext.Element != null && regionBreakFilter(moveContext.Element))
            {
                //move the end of the region back before the break element to close this region
                currentRegion.ContentRange.End.Left(true);
            }
            if (currentRegion.ContentRange.End.IsRightOf(range.End))
                currentRegion.ContentRange.End.MoveToPointer(range.End);
            regions.Add(currentRegion);

            return (ElementBreakRegion[])regions.ToArray(typeof(ElementBreakRegion));
        }

        private void MovePointerLeftUntilRegionBreak(MarkupPointer p, IHTMLElementFilter regionBreakFilter, MarkupPointer leftBoundary)
        {
            MarkupContext moveContext = new MarkupContext();
            while (p.IsRightOf(leftBoundary))
            {
                p.Left(true, moveContext);
                if (moveContext.Element != null && regionBreakFilter(moveContext.Element))
                {
                    p.Right(true);
                    return;
                }
            }
        }

        private void MovePointerRightUntilRegionBreak(MarkupPointer p, IHTMLElementFilter regionBreakFilter, MarkupPointer rightBoundary)
        {
            MarkupContext moveContext = new MarkupContext();
            while (p.IsLeftOf(rightBoundary))
            {
                p.Right(true, moveContext);
                if (moveContext.Element != null && regionBreakFilter(moveContext.Element))
                {
                    p.Left(true);
                    return;
                }
            }
        }

        private bool ShouldDeleteForBlockFormatting(IHTMLElement e)
        {
            if (e == null || e.sourceIndex == -1)
                return false;

            Debug.Assert(!ElementFilters.IsListItemElement(e) && !ElementFilters.IsBlockQuoteElement(e) && !ElementFilters.IsTableCellElement(e), "");
            return
                ElementFilters.IsBlockElement(e) && !ElementFilters.IsListItemElement(e) && !ElementFilters.IsBlockQuoteElement(e);
        }

        private class ElementBreakRegion
        {
            public ElementBreakRegion(MarkupRange contentRange, IHTMLElement breakStartElement, IHTMLElement breakEndElement)
            {
                BreakStartElement = breakStartElement;
                BreakEndElement = breakEndElement;
                ContentRange = contentRange;
            }
            public IHTMLElement BreakStartElement;
            public IHTMLElement BreakEndElement;
            public MarkupRange ContentRange;
        }

        /// <summary>
        /// Represents the inner region of a block element after the parent block element has been removed.
        /// </summary>
        private class InnerBlockRegion
        {
            public InnerBlockRegion(MarkupRange contentRange, IHTMLElement oldParentElement)
            {
                OldParentElement = oldParentElement;
                ContentRange = contentRange;
            }

            /// <summary>
            /// Be careful: this element has probably been removed from the DOM.
            /// </summary>
            public IHTMLElement OldParentElement { get; private set; }
            public MarkupRange ContentRange { get; private set; }
        }
    }

    /// <summary>
    /// As part of some editing operations, blocks of HTML can
    /// be violently changed. For example, when changing the
    /// block style (like going from p to h1) the entire block
    /// is deleted and rewritten. In these cases the selection
    /// gets lost. This class helps restore the selection to
    /// approximately where it was before.
    ///
    /// Before the change is applied, call Init so the class can
    /// save the position of the cursor relative to the visible
    /// characters in the range. After the change is applied, call
    /// Restore to put the cursor back to approximately where it
    /// was originally. Each call takes a MarkupRange that represents
    /// the boundary of the range that's being changed--at least
    /// the text contained therein should be the same for both calls.
    ///
    /// Limitations:
    /// * Doesn't preserve position relative to non-textual markup,
    ///   like images, br's, etc. This means if the initial selection
    ///   is adjacent to one of these things instead of surrounded on
    ///   both sides by text (or the edge of the bounds/block) then the
    ///   resulting placement will be approximate only.
    /// * Only works if the selection is empty.
    /// </summary>
    public class SelectionPositionPreservationHelper
    {
        public static SelectionPositionPreservationCookie Save(MshtmlMarkupServices markupServices, MarkupRange selection, MarkupRange bounds)
        {
            return new SelectionPositionPreservationCookie(markupServices, selection, bounds);
        }

        public static bool Restore(SelectionPositionPreservationCookie cookie, MarkupRange selection, MarkupRange bounds)
        {
            if (cookie == null)
                return false;
            return cookie.Restore(selection, bounds);
        }
    }
    public class SelectionPositionPreservationCookie
    {
        private readonly string initialMarkup;
        private readonly int movesRight;
        private readonly int charsLeft;

        internal SelectionPositionPreservationCookie(MshtmlMarkupServices markupServices, MarkupRange selection, MarkupRange bounds)
        {
            if (!selection.IsEmpty())
                return;

            initialMarkup = bounds.HtmlText;

            NormalizeBounds(ref bounds);

            MarkupPointer p = bounds.Start.Clone();

            movesRight = 0;
            while (p.IsLeftOf(selection.Start))
            {
                movesRight++;
                p.Right(true);
                if (p.IsRightOf(bounds.End))
                {
                    movesRight = int.MaxValue;
                    p.MoveToPointer(bounds.End);
                    break;
                }
            }

            charsLeft = 0;
            while (p.IsRightOf(selection.Start))
            {
                charsLeft++;
                p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);
            }
        }

        internal bool Restore(MarkupRange selection, MarkupRange bounds)
        {
            if (initialMarkup == null)
                return false;

            NormalizeBounds(ref bounds);
            /*
                        if (initialMarkup != bounds.HtmlText)
                        {
                            Trace.Fail("Unexpected markup");
                            Trace.WriteLine(initialMarkup);
                            Trace.WriteLine(bounds.HtmlText);
                            return false;
                        }
            */

            selection.Start.MoveToPointer(bounds.Start);

            if (movesRight == int.MaxValue)
            {
                selection.Start.MoveToPointer(bounds.End);
            }
            else
            {
                for (int i = 0; i < movesRight; i++)
                {
                    selection.Start.Right(true);
                }
            }

            for (int i = 0; i < charsLeft; i++)
            {
                selection.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);
            }

            selection.Collapse(true);
            selection.ToTextRange().select();

            Debug.Assert(bounds.InRange(selection, true), "Selection was out of bounds");

            return true;
        }

        private static void NormalizeBounds(ref MarkupRange bounds)
        {
            bool cloned = false;

            if (bounds.Start.IsRightOf(bounds.End))
            {
                if (!cloned)
                {
                    cloned = true;
                    bounds = bounds.Clone();
                }
                bounds.Normalize();
            }

            MarkupContext ctx = bounds.Start.Right(false);
            while (ctx.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                && ElementFilters.IsBlockElement(ctx.Element))
            {
                if (!cloned)
                {
                    cloned = true;
                    bounds = bounds.Clone();
                }
                bounds.Start.Right(true);
                bounds.Start.Right(false, ctx);
            }
        }
    }
}
