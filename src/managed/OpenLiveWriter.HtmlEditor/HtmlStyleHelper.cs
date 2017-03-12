// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public class HtmlStyleHelper
    {
        private MshtmlMarkupServices _markupServices;

        private HtmlStyleHelper(MshtmlMarkupServices markupServices)
        {
            _markupServices = markupServices;
        }

        public static void SplitInlineTags(MshtmlMarkupServices markupServices, MarkupPointer splitPoint)
        {
            HtmlStyleHelper htmlStyleHelper = new HtmlStyleHelper(markupServices);
            htmlStyleHelper.SplitInlineTags(splitPoint);
        }

        public static MarkupRange ApplyInlineTag(MshtmlMarkupServices markupServices, _ELEMENT_TAG_ID tagId, string attributes, MarkupRange selection, bool toggle)
        {
            HtmlStyleHelper htmlStyleHelper = new HtmlStyleHelper(markupServices);

            // Aligning the with the behavior of Word, we will make <SUP> and <SUB> mutually exclusive.
            // That is, if you are applying <SUB> to a selection that has <SUP> applied already, we will first remove the <SUP>, and vice versa.
            // Wait, if empty and we're on the very end of the already existing formatting, then we just want to jump out and apply...

            MarkupRange selectionToApply = selection.Clone();
            if (toggle)
            {
                // If already entirely inside the tag
                //     If empty and just inside of the closing tag, then jump outside the closing tag
                //     Else remove the tag
                // If already entirely outside the tag
                //     If empty, apply the tag and put selection inside
                //     If non-empty, then apply tag and reselect
                // If partially inside the tag
                //     Remove the tag

                _ELEMENT_TAG_ID mutuallyExclusiveTagId = _ELEMENT_TAG_ID.TAGID_NULL;
                switch (tagId)
                {
                    case _ELEMENT_TAG_ID.TAGID_SUP:
                        mutuallyExclusiveTagId = _ELEMENT_TAG_ID.TAGID_SUB;
                        break;
                    case _ELEMENT_TAG_ID.TAGID_SUB:
                        mutuallyExclusiveTagId = _ELEMENT_TAG_ID.TAGID_SUP;
                        break;
                    default:

                        break;
                }

                if (selection.IsEmpty())
                {
                    // If the selection is empty and we're just inside the tagId closing tag (meaning that there is no text before the closing tag),
                    // then we just hop outside of the tagId closing tag.

                    bool exitScopeMatchesTagIdToBeApplied;
                    MarkupPointer pointerOutsideTagIdScope = htmlStyleHelper.NextExitScopeWithoutInterveningText(selection,
                                                                            tagId,
                                                                            mutuallyExclusiveTagId,
                                                                            out exitScopeMatchesTagIdToBeApplied);

                    if (pointerOutsideTagIdScope != null)
                    {
                        selectionToApply = markupServices.CreateMarkupRange(pointerOutsideTagIdScope, pointerOutsideTagIdScope);
                        if (exitScopeMatchesTagIdToBeApplied)
                        {
                            return selectionToApply;
                        }
                        // else we still need to apply tagId
                    }
                }

                // If a mutually exclusive tag is applied, then remove it.
                if (selectionToApply.IsTagId(mutuallyExclusiveTagId, true))
                {
                    selectionToApply.RemoveElementsByTagId(mutuallyExclusiveTagId, false);
                }

                // If this tag is already applied, then remove it and return.
                if (selectionToApply.IsTagId(tagId, true))
                {
                    selectionToApply.RemoveElementsByTagId(tagId, false);
                    return selectionToApply;
                }
            }

            return htmlStyleHelper.ApplyInlineTag(tagId, attributes, selectionToApply);
        }

        private MarkupRange ApplyInlineTag(_ELEMENT_TAG_ID tagId, string attributes, MarkupRange selection)
        {
            MarkupRange newSelection = _markupServices.CreateMarkupRange();

            // If the selection is empty, then just insert the tag
            if (selection.IsEmpty())
            {
                newSelection.MoveToElement(WrapRangeInSpanElement(tagId, attributes, selection), false);
                return newSelection;
            }

            // Start at the beginning of the selection move forward until you hit a block start/exit context or the end of the selection

            bool keepApplying = true;
            MarkupContext contextStart = new MarkupContext();
            MarkupRange blockFreeRange = _markupServices.CreateMarkupRange(selection.Start.Clone(), selection.Start.Clone());
            MarkupPointer currentPointer = _markupServices.CreateMarkupPointer(blockFreeRange.Start);

            while (keepApplying)
            {
                // Check if moving right would be beyond the bounds of the selection.
                if (currentPointer.IsRightOfOrEqualTo(selection.End))
                {
                    // We've hit the end of the selection, so we're done.
                    keepApplying = false;
                    Debug.Assert(blockFreeRange.Start.IsLeftOfOrEqualTo(selection.End));
                    blockFreeRange.End.MoveToPointer(selection.End.IsLeftOf(currentPointer) ? selection.End : currentPointer);

                    if (ShouldApplyInlineTagToBlockFreeSelection(blockFreeRange))
                        newSelection.ExpandToInclude(ApplyInlineTagToBlockFreeSelection(tagId, attributes, blockFreeRange));
                    break;
                }

                // Check if the next context is entering or exiting a block.
                currentPointer.Right(false, contextStart);
                if (contextStart.Element != null && ElementFilters.IsBlockElement(contextStart.Element))
                {
                    switch (contextStart.Context)
                    {
                        case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope:

                        case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope:
                            {
                                blockFreeRange.End.MoveToPointer(selection.End.IsLeftOf(currentPointer) ? selection.End : currentPointer);
                                if (ShouldApplyInlineTagToBlockFreeSelection(blockFreeRange))
                                    newSelection.ExpandToInclude(ApplyInlineTagToBlockFreeSelection(tagId, attributes, blockFreeRange));
                                if (contextStart.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                                    blockFreeRange.Start.MoveAdjacentToElement(contextStart.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                                else if (contextStart.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                                    blockFreeRange.Start.MoveAdjacentToElement(contextStart.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

                                blockFreeRange.Collapse(true);
                            }
                            break;
                        case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None:
                            {
                                keepApplying = false;
                                blockFreeRange.End.MoveToPointer(selection.End.IsLeftOf(currentPointer) ? selection.End : currentPointer);
                                if (ShouldApplyInlineTagToBlockFreeSelection(blockFreeRange))
                                    newSelection.ExpandToInclude(ApplyInlineTagToBlockFreeSelection(tagId, attributes, blockFreeRange));
                            }
                            break;
                        default:
                            break;
                    }
                }

                // Finally, move our pointer
                currentPointer.Right(true);
            }

            if (newSelection.Positioned)
                newSelection.Trim();

            return newSelection;
        }

        private bool ShouldApplyInlineTagToBlockFreeSelection(MarkupRange blockFreeRange)
        {
            return !String.IsNullOrEmpty(blockFreeRange.Text) || blockFreeRange.ContainsElements(ElementFilters.INLINE_ELEMENTS);
        }

        private MarkupRange ApplyInlineTagToBlockFreeSelection(_ELEMENT_TAG_ID tagId, string attributes, MarkupRange blockFreeSelection)
        {
            // @RIBBON TODO: May want to be a bit more sophisticated and eliminate redundant tags
            return _markupServices.CreateMarkupRange(WrapRangeInSpanElement(tagId, attributes, blockFreeSelection));
        }

        private IHTMLElement WrapRangeInSpanElement(_ELEMENT_TAG_ID tagId, string attributes, MarkupRange spanRange)
        {
            MarkupRange insertionRange = _markupServices.CreateMarkupRange();
            IHTMLElement newSpanElement = _markupServices.CreateElement(tagId, attributes);

            //insert the new span element in front of the span content
            insertionRange.Start.MoveToPointer(spanRange.Start);
            insertionRange.End.MoveToPointer(spanRange.Start);
            _markupServices.InsertElement(newSpanElement, insertionRange.Start, insertionRange.End);

            //move the span content inside the new span element
            insertionRange.Start.MoveAdjacentToElement(newSpanElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            spanRange.Start.MoveAdjacentToElement(newSpanElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

            _markupServices.Move(spanRange.Start, spanRange.End, insertionRange.Start);
            return newSpanElement;
        }

        /// <summary>
        /// Returns the markup pointer to the position of the first exit scope of type tagId or type terminatingTagId which follows this markup range.
        /// Returns null if text exists between the range and such an exit scope, or if there is no such exit scope.
        /// </summary>
        /// <param name="terminatingTagId"></param>
        /// <returns></returns>
        internal MarkupPointer NextExitScopeWithoutInterveningText(MarkupRange selection, _ELEMENT_TAG_ID tagId, _ELEMENT_TAG_ID terminatingTagId, out bool primaryTagIdMatch)
        {
            MarkupContext context = new MarkupContext();

            MarkupPointer pointer = selection.End.Clone();
            primaryTagIdMatch = false;

            while (true)
            {
                pointer.Right(true, context);
                if (context.Element == null)
                    return null;

                switch (context.Context)
                {
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None:
                        return null;
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope:
                        {
                            if (_markupServices.GetElementTagId(context.Element) == tagId)
                            {
                                primaryTagIdMatch = true;
                                return pointer;
                            }

                            if (terminatingTagId != _ELEMENT_TAG_ID.TAGID_NULL && terminatingTagId == _markupServices.GetElementTagId(context.Element))
                            {
                                return pointer;
                            }
                        }

                        break;
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope:
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope:
                        break;
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text:
                        return null;
                }
            }
        }

        /// <summary>
        /// <font><span>aaa[splitPoint]bbb</span></font> --> <font><span>aaa</span></font><font>[splitPoint]<span>bbb</span></font>
        /// </summary>
        private void SplitInlineTags(MarkupPointer splitPoint)
        {
            Debug.Assert(splitPoint.Positioned);

            IHTMLElement currentElement = splitPoint.GetParentElement(ElementFilters.CreateElementPassFilter());
            while (currentElement != null)
            {
                if (!ElementFilters.IsInlineElement(currentElement))
                    return;

                IHTMLElement parentElement = currentElement.parentElement;

                MarkupRange currentElementRange = _markupServices.CreateMarkupRange(currentElement, false);

                MarkupRange leftRange = _markupServices.CreateMarkupRange();
                IHTMLElement leftElement = _markupServices.CreateElement(_markupServices.GetElementTagId(currentElement), null);
                HTMLElementHelper.CopyAttributes(currentElement, leftElement);
                leftRange.MoveToPointers(currentElementRange.Start, splitPoint);
                _markupServices.InsertElement(leftElement, leftRange.Start, leftRange.End);

                splitPoint.MoveAdjacentToElement(leftElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

                MarkupRange rightRange = _markupServices.CreateMarkupRange();
                IHTMLElement rightElement = _markupServices.CreateElement(_markupServices.GetElementTagId(currentElement), null);
                HTMLElementHelper.CopyAttributes(currentElement, rightElement);
                rightRange.MoveToPointers(splitPoint, currentElementRange.End);

#if DEBUG
                // Verify that the right range does not overlap the left *element*
                MarkupRange leftElementRange = _markupServices.CreateMarkupRange(leftElement, true);
                Debug.Assert(leftElementRange.End.IsLeftOfOrEqualTo(rightRange.Start), "Your right range overlaps the left element that you just created!");
#endif
                _markupServices.InsertElement(rightElement, rightRange.Start, rightRange.End);
                splitPoint.MoveAdjacentToElement(rightElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);

                _markupServices.RemoveElement(currentElement);

                currentElement = parentElement;
            }
        }

        public static void ChangeElementTagIds(MshtmlMarkupServices markupServices, MarkupRange selection, _ELEMENT_TAG_ID[] tagBefore, _ELEMENT_TAG_ID tagAfter)
        {
            HtmlStyleHelper htmlStyleHelper = new HtmlStyleHelper(markupServices);

            int parentStartDiff = 0;
            MarkupRange rangeToChange;
            bool selectionStartedEmpty = selection.IsEmpty();
            if (selectionStartedEmpty)
            {
                // Operate on parent block element
                rangeToChange = markupServices.CreateMarkupRange(selection.ParentBlockElement());
                parentStartDiff = selection.Start.MarkupPosition - rangeToChange.Start.MarkupPosition;
            }
            else
            {
                rangeToChange = selection;

                // If expanding the selection would not include any new text, then expand it.
                // <h1>|abc|</h1> --> |<h1>abc</h1>|
                rangeToChange.MoveOutwardIfNoText();
            }

            IHTMLElementFilter[] filters = new IHTMLElementFilter[tagBefore.Length];

            for (int i = 0; i < tagBefore.Length; i++)
                filters[i] = ElementFilters.CreateTagIdFilter(markupServices.GetNameForTagId(tagBefore[i]));

            IHTMLElement[] elements = rangeToChange.GetElements(ElementFilters.CreateCompoundElementFilter(filters), false);
            foreach (IHTMLElement element in elements)
            {
                MarkupRange elementRange = markupServices.CreateMarkupRange(element);

                int startPositionDiff = rangeToChange.Start.MarkupPosition - elementRange.Start.MarkupPosition;
                int endPositionDiff = rangeToChange.End.MarkupPosition - elementRange.End.MarkupPosition;

                // @RIBBON TODO: Appropriately preserve element attributes when changing tag ids?
                MarkupRange newElementRange = markupServices.CreateMarkupRange(htmlStyleHelper.WrapRangeInSpanElement(tagAfter, null, elementRange));
                markupServices.RemoveElement(element);

                MarkupPointer startPointer = rangeToChange.Start.Clone();
                startPointer.MoveToMarkupPosition(startPointer.Container, newElementRange.Start.MarkupPosition + startPositionDiff);
                if (startPointer.IsLeftOf(rangeToChange.Start))
                    rangeToChange.Start.MoveToPointer(startPointer);

                MarkupPointer endPointer = rangeToChange.End.Clone();
                endPointer.MoveToMarkupPosition(endPointer.Container, newElementRange.End.MarkupPosition + endPositionDiff);
                if (endPointer.IsLeftOf(elementRange.End))
                    rangeToChange.End.MoveToPointer(endPointer);
            }

            if (selectionStartedEmpty)
            {
                selection.Start.MoveToMarkupPosition(selection.Start.Container, rangeToChange.Start.MarkupPosition + parentStartDiff);
                selection.Collapse(true);
            }
        }

        public static void ClearBackgroundColor(MshtmlMarkupServices markupServices, MarkupRange selection)
        {
            HtmlStyleHelper htmlStyleHelper = new HtmlStyleHelper(markupServices);

            htmlStyleHelper.SplitInlineTags(selection.Start);
            htmlStyleHelper.SplitInlineTags(selection.End);

            IHTMLElement[] elements = selection.GetElements(ElementFilters.CreateTagIdFilter("font"), false);
            foreach (IHTMLElement element in elements)
            {
                element.style.backgroundColor = "";
            }

            // We may now be left with empty font tags, e.g. <font>blah</font>.
            // After switching between editors this becomes <font size="+0">blah</font>, which
            // causes blah to be rendered differently.
            // To avoid that we need to remove any empty-attribute font tags.
            selection.RemoveElementsByTagId(_ELEMENT_TAG_ID.TAGID_FONT, true);
        }

        public static void RemoveAttributes(MshtmlMarkupServices markupServices, MarkupRange selection, string[] attributesToRemove)
        {
            IHTMLElementFilter[] filters = new IHTMLElementFilter[attributesToRemove.Length];

            for (int i = 0; i < attributesToRemove.Length; i++)
                filters[i] = ElementFilters.CreateElementAttributeFilter(attributesToRemove[i]);

            IHTMLElement[] elements = selection.GetElements(ElementFilters.CreateCompoundElementFilter(filters), false);
            foreach (IHTMLElement element in elements)
            {
                foreach (string attribute in attributesToRemove)
                    element.removeAttribute(attribute, 0);
            }
        }

        public static IHTMLElement WrapRangeInElement(MshtmlMarkupServices services, MarkupRange range, _ELEMENT_TAG_ID tagId)
        {
            return WrapRangeInElement(services, range, tagId, string.Empty);
        }

        public static IHTMLElement WrapRangeInElement(MshtmlMarkupServices services, MarkupRange range, _ELEMENT_TAG_ID tagId, string attributes)
        {
            IHTMLElement newElement = services.CreateElement(tagId, attributes);

            services.InsertElement(newElement, range.Start, range.End);

            return newElement;
        }
    }
}
