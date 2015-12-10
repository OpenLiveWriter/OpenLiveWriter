// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor.Marshalling;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public class BoldApplier
    {
        private MshtmlMarkupServices markupServices;
        private MarkupRange markupRange;
        private IMshtmlCommand boldCommand;

        public BoldApplier(MshtmlMarkupServices markupServices, MarkupRange markupRange, IMshtmlCommand boldCommand)
        {
            this.markupServices = markupServices;
            this.markupRange = markupRange.Clone();
            this.boldCommand = boldCommand;
        }

        public void Execute()
        {
            if (!boldCommand.Enabled)
            {
                Debug.Fail("Attempted to execute bold command when it is not enabled!");
                return;
            }

            bool turnBold = boldCommand.Latched == false;

            // First let mshtml do its thing
            boldCommand.Execute();

            // Now fix up necessary header elements with right tags
            FixupHeaders(turnBold);
        }

        /// <summary>
        /// Fixes up all the headers in the entire markupRange.
        /// </summary>
        /// <param name="turnBold">Whether or not the text should be turning bold.</param>
        private void FixupHeaders(bool turnBold)
        {
            IHTMLElement elementStartHeader = markupRange.Start.GetParentElement(ElementFilters.HEADER_ELEMENTS);
            IHTMLElement elementEndHeader = markupRange.End.GetParentElement(ElementFilters.HEADER_ELEMENTS);
            MarkupRange currentRange = markupRange.Clone();

            if (elementStartHeader != null)
            {
                // Takes care of the following cases:
                //  <h1>...|blah|...</h1>
                //  <h1>...|blah...</h1>...|...
                MarkupRange startRange = markupServices.CreateMarkupRange(elementStartHeader, false);
                startRange.Start.MoveToPointer(markupRange.Start);

                if (startRange.End.IsRightOf(markupRange.End))
                {
                    startRange.End.MoveToPointer(markupRange.End);
                }

                FixupHeaderRange(startRange, turnBold);

                currentRange.Start.MoveAdjacentToElement(elementStartHeader, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                if (currentRange.End.IsLeftOf(currentRange.Start))
                {
                    currentRange.End.MoveToPointer(currentRange.Start);
                }
            }

            if (elementEndHeader != null && !HTMLElementHelper.ElementsAreEqual(elementStartHeader, elementEndHeader))
            {
                // Takes care of the following case:
                //  ...|...<h1>...blah|...</h1>
                MarkupRange endRange = markupServices.CreateMarkupRange(elementEndHeader, false);
                endRange.End.MoveToPointer(markupRange.End);

                FixupHeaderRange(endRange, turnBold);

                currentRange.End.MoveAdjacentToElement(elementEndHeader, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                if (currentRange.Start.IsRightOf(currentRange.End))
                {
                    currentRange.Start.MoveToPointer(currentRange.End);
                }
            }

            if (!markupRange.InRange(currentRange))
            {
                return;
            }

            IHTMLElement[] headerElements = currentRange.GetElements(ElementFilters.HEADER_ELEMENTS, true);
            if (headerElements != null && headerElements.Length > 0)
            {
                foreach (IHTMLElement element in headerElements)
                {
                    MarkupRange headerRange = markupServices.CreateMarkupRange(element, false);
                    FixupHeaderRange(headerRange, turnBold);
                }
            }
        }

        /// <summary>
        /// Fixes up a range that is contained in a single header element.
        /// </summary>
        /// <param name="range">A range that is contained in a single header element.</param>
        /// <param name="turnBold">Whether or not the text should be turning bold.</param>
        private void FixupHeaderRange(MarkupRange range, bool turnBold)
        {
            IHTMLElement parentHeaderElement = range.ParentBlockElement();
            if (parentHeaderElement == null || !ElementFilters.IsHeaderElement(parentHeaderElement))
            {
                Debug.Fail("Expected entire range to be inside a single header element.");
                return;
            }

            MarkupRange expandedRange = range.Clone();

            // Make sure we expand the selection to include any <font> tags that may be wrapping us.
            MarkupPointerMoveHelper.MoveUnitBounded(expandedRange.Start, MarkupPointerMoveHelper.MoveDirection.LEFT,
                                                    MarkupPointerAdjacency.BeforeVisible, parentHeaderElement);
            MarkupPointerMoveHelper.MoveUnitBounded(expandedRange.End, MarkupPointerMoveHelper.MoveDirection.RIGHT,
                                                    MarkupPointerAdjacency.BeforeVisible, parentHeaderElement);

            // Walks in-scope elements and clears out any elements or styles that might affect the bold formatting.
            var elementsToRemove = new List<IHTMLElement>();
            expandedRange.WalkRange(
                delegate (MarkupRange currentexpandedRange, MarkupContext context, string text)
                {
                    IHTMLElement currentElement = context.Element;
                    if (currentElement != null && context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                    {
                        if (IsStrongOrBold(currentElement))
                        {
                            elementsToRemove.Add(currentElement);
                        }
                        else if (IsFontableElement(currentElement) && HTMLElementHelper.IsBold((IHTMLElement2)currentElement) != turnBold)
                        {
                            currentElement.style.fontWeight = String.Empty;
                        }
                    }

                    return true;

                }, true);

            elementsToRemove.ForEach(e => markupServices.RemoveElement(e));

            // Walks the range to find any segments of text that need to be fixed up.
            var rangesToWrap = new List<MarkupRange>();
            range.WalkRange(
                delegate (MarkupRange currentRange, MarkupContext context, string text)
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                    {
                        TextStyles currentTextStyles = new TextStyles(currentRange.Start);
                        if (currentTextStyles.Bold != turnBold)
                        {
                            rangesToWrap.Add(currentRange.Clone());
                        }
                    }

                    return true;

                }, true);

            rangesToWrap.ForEach(r => WrapRangeInFontIfNecessary(r, turnBold));
        }

        /// <summary>
        /// Inspects the range if a new font tag need to be added to apply formatting for the range.
        /// Call this only for ranges that are inside a header element
        /// </summary>
        private void WrapRangeInFontIfNecessary(MarkupRange currentRange, bool turnBold)
        {
            // Check if there is an existing font/span tag that completely wraps this range,
            // we can just use that instead of inserting a new one
            MarkupContext workingContext = new MarkupContext();
            bool wrapFont = true;
            MarkupPointer start = currentRange.Start.Clone();
            start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            start.Right(false, workingContext); // Look to the right to see what we have there
            if (workingContext.Element != null && workingContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope &&
                IsFontableElement(workingContext.Element))
            {
                start.MoveAdjacentToElement(workingContext.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                if (currentRange.End.IsEqualTo(start))
                {
                    // There is an existing <FONT>/<SPAN> enclosing the range, no need to wrap again
                    wrapFont = false;

                    // set its font-weight
                    workingContext.Element.style.fontWeight = turnBold ? "bold" : "normal";
                }
            }

            if (wrapFont)
            {
                string weightAttribute = String.Format(CultureInfo.InvariantCulture, "style=\"font-weight: {0}\"", turnBold ? "bold" : "normal");
                HtmlStyleHelper.WrapRangeInElement(markupServices, currentRange, _ELEMENT_TAG_ID.TAGID_FONT, weightAttribute);
            }
        }

        /// <summary>
        /// FONT/SPAN tags are fontable tags within header
        /// </summary>
        private static bool IsFontableElement(IHTMLElement e)
        {
            return e.tagName.Equals("FONT", StringComparison.OrdinalIgnoreCase) ||
                e.tagName.Equals("SPAN", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the given element is a STRONG/B tags
        /// </summary>
        private static bool IsStrongOrBold(IHTMLElement e)
        {
            return e.tagName.Equals("STRONG", StringComparison.OrdinalIgnoreCase) ||
                e.tagName.Equals("B", StringComparison.OrdinalIgnoreCase);
        }

    }
}
