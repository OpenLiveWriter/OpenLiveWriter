// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Mshtml
{

    /// <summary>
    /// Delegate used to filter element scanning operations. If this operation returns true, then the
    /// scanning operation will consider the element as relevant to the scan.
    /// </summary>
    public delegate bool IHTMLElementFilter(IHTMLElement e);

    /// <summary>
    /// Delegate used to walk through a MarkupRange. Return true to continue walking and false to stop.
    /// </summary>
    public delegate bool MarkupRangeWalker(MarkupRange currentRange, MarkupContext context, string text);

    /// <summary>
    /// Range of markup within a document
    /// </summary>
    public class MarkupRange
    {
        /// <summary>
        /// Initialize with begin and end pointers
        /// </summary>
        /// <param name="start">start</param>
        /// <param name="end">end</param>
        /// <param name="markupServices"></param>
        internal MarkupRange(MarkupPointer start, MarkupPointer end, MshtmlMarkupServices markupServices)
        {
            Start = start;
            End = end;
            MarkupServices = markupServices;
        }

        /// <summary>
        /// Returns the Html contained in this MarkupRange.
        /// </summary>
        public string HtmlText
        {
            get
            {
                try
                {
                    if (!Positioned)
                        return null;
                    return MarkupHelpers.UseStagingTextRange(ref stagingTxtRange, this,
                                                             rng => rng.htmlText);
                }
                catch (Exception)
                {
                    //this can occur when the pointers are not positioned
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the text (stripped of HTML elements) contained in this MarkupRange.
        /// </summary>
        public string Text
        {
            get
            {
                try
                {
                    if (!Positioned || Start.IsRightOf(End))
                        return null;
                    return MarkupHelpers.GetRangeTextFast(this);
                }
                catch (Exception)
                {
                    //this can occur when the pointers are not positioned
                    return null;
                }
            }
            set
            {
                MarkupHelpers.UseStagingTextRange(ref stagingTxtRange, this,
                                                  rng =>
                                                  {
                                                      rng.text = value;
                                                      return 0;
                                                  });
            }
        }

        /// <summary>
        /// Collapses the range to make the start/end equal.
        /// </summary>
        /// <param name="start">true if start pointer should be the anchor, else its the end pointer.</param>
        public void Collapse(bool start)
        {
            if (start)
                End.MoveToPointer(Start);
            else
                Start.MoveToPointer(End);
        }

        /// <summary>
        /// Safely removes the content within this range without leaving the document badly formed.
        /// </summary>
        public void RemoveContent()
        {
            //delete the selection by moving a delete range right (from the start).
            //Each time that a tag that does not entirely exist within this selection
            //is encountered, the range content will be deleted, the deleteRange will
            //skip over the element.
            MarkupRange deleteRange = this.Clone();

            Trace.Assert(deleteRange.Start.Positioned, "Trying to remove content from selection that contains pointers that are not positioned.");

            deleteRange.End.MoveToPointer(deleteRange.Start);
            MarkupPointer p = MarkupServices.CreateMarkupPointer();
            MarkupPointer previousPosition = MarkupServices.CreateMarkupPointer(deleteRange.End);
            MarkupContext context = new MarkupContext();
            deleteRange.End.Right(true, context);
            while (deleteRange.End.IsLeftOf(End))
            {
                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                {
                    p.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    if (p.IsRightOf(End))
                    {
                        //this element does not exist entirely in this selection, so we need to
                        //ignore it in the delete.

                        //save this position so that the delete range can be repositioned here
                        p.MoveToPointer(deleteRange.End);

                        //move the end left since we overstepped the valid delete range
                        deleteRange.End.MoveToPointer(previousPosition);

                        //delete the content in the deleteRange, and move it back to this position
                        deleteRangeContentAndMoveToPosition(deleteRange, p);
                    }
                    else
                    {
                        //this element exists entirely in this selection, so skip to its end (since
                        //we know it can be deleted)
                        deleteRange.End.MoveToPointer(p);
                    }
                }
                else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                {
                    p.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    if (p.IsLeftOf(Start))
                    {
                        //this element does not exist entirely in this selection, so we need to
                        //ignore it in the delete.

                        //save this position so that the delete range can be repositioned here
                        p.MoveToPointer(deleteRange.End);

                        //move the end left since we overstepped the valid delete range
                        deleteRange.End.MoveToPointer(previousPosition);

                        //delete the content in the deleteRange, and move it back to this position
                        deleteRangeContentAndMoveToPosition(deleteRange, p);
                    }
                    else
                    {
                        //this element exists entirely in this selection, so skip to its end (since
                        //we know it can be deleted)
                        deleteRange.End.MoveToPointer(p);
                    }
                }

                previousPosition.MoveToPointer(deleteRange.End);
                deleteRange.End.Right(true, context);
            }

            //delete the last part of the range
            deleteRange.End.MoveToPointer(End);
            if (!deleteRange.Start.Equals(deleteRange.End))
                MarkupServices.Remove(deleteRange.Start, deleteRange.End);
        }

        /// <summary>
        /// Creates a clone that spans the same range as this MarkupRange.
        /// Note: The clone can be manipulated without changing the position of this range.
        /// </summary>
        /// <returns></returns>
        public MarkupRange Clone()
        {
            MarkupRange clone = MarkupServices.CreateMarkupRange();
            clone.Start.MoveToPointer(Start);
            clone.Start.Cling = Start.Cling;
            clone.Start.Gravity = Start.Gravity;
            clone.End.MoveToPointer(End);
            clone.End.Cling = End.Cling;
            clone.End.Gravity = End.Gravity;
            return clone;
        }

        public void Normalize()
        {
            if (Start.IsRightOf(End))
            {
                MarkupPointer tmp = Start;
                Start = End;
                End = tmp;
            }
        }

        /// <summary>
        /// Returns the HTML Elements that a direct children of this range.
        /// Note: Only children that are completely contained in this range will be returned.
        /// </summary>
        /// <returns></returns>
        public IHTMLElement[] GetTopLevelElements(IHTMLElementFilter filter)
        {
            return GetTopLevelBlocksAndCells(filter, false);
        }

        //this is similar to the GetTopLevelElements except will also return table cells if correct filter
        // is set and recurse is equal to true
        public IHTMLElement[] GetTopLevelBlocksAndCells(IHTMLElementFilter filter, bool recurse)
        {
            ArrayList list = new ArrayList();
            Hashtable usedElements = new Hashtable();

            MarkupPointer p = MarkupServices.CreateMarkupPointer(Start);
            MarkupContext context = p.Right(false);
            //move p through the range to locate each of the top level elements
            while (p.IsLeftOf(End))
            {
                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope)
                {
                    p.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    if (usedElements[context.Element] == null)
                    {
                        if (p.IsLeftOfOrEqualTo(End) && (filter == null || filter(context.Element)))
                        {
                            list.Add(context.Element);
                        }
                        //special case--inside of a table element, want to get out the cells inside
                        else if (recurse && ElementFilters.TABLE_ELEMENTS(context.Element))
                        {
                            MarkupRange newRange = MarkupServices.CreateMarkupRange(context.Element);
                            newRange.Start.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                            if (newRange.Start.IsLeftOf(Start))
                            {
                                newRange.Start.MoveToPointer(Start);
                            }
                            if (newRange.End.IsRightOf(End))
                            {
                                newRange.End.MoveToPointer(End);
                            }
                            //recursively check inside table element for table cells
                            list.AddRange(newRange.GetTopLevelBlocksAndCells(filter, true));
                        }
                        //cache the fact that we've already tested this element.
                        usedElements[context.Element] = context.Element;
                    }
                }
                p.Right(true, context);
            }
            return HTMLElementHelper.ToElementArray(list);
        }

        public static bool FilterNone(IHTMLElement e)
        {
            return true;
        }

        /// <summary>
        /// Gets the elements in the range that match the filter.
        /// </summary>
        /// <param name="filter">the delegate testing each element to determine if it should be added to the list of elements to return</param>
        /// <param name="inScopeElementsOnly">if true, the only</param>
        /// <returns></returns>
        public IHTMLElement[] GetElements(IHTMLElementFilter filter, bool inScopeElementsOnly)
        {
            ArrayList list = new ArrayList();
            if (!IsEmpty())
            {
                Hashtable usedElements = new Hashtable();
                MarkupPointer p = MarkupServices.CreateMarkupPointer(Start);
                MarkupPointer end = MarkupServices.CreateMarkupPointer(End);
                MarkupContext context = p.Right(false);

                //move p through the range to locate each the elements adding elements that pass the filter
                while (p.IsLeftOfOrEqualTo(end))
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                        || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope
                        || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope)
                    {
                        if (usedElements[context.Element] == null)
                        {
                            if ((inScopeElementsOnly && isInScope(context.Element)) || !inScopeElementsOnly)
                                if (filter(context.Element))
                                {
                                    list.Add(context.Element);
                                }

                            //cache the fact that we've already tested this element.
                            usedElements[context.Element] = context.Element;
                        }
                    }
                    p.Right(true, context);
                }
            }
            return HTMLElementHelper.ToElementArray(list);
        }

        /// <summary>
        /// Returns true if the the range contains an element that matches the filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public bool ContainsElements(IHTMLElementFilter filter)
        {
            if (!IsEmpty())
            {
                Hashtable usedElements = new Hashtable();
                MarkupPointer p = MarkupServices.CreateMarkupPointer(Start);
                MarkupContext context = p.Right(false);

                //move p through the range to locate each the elements adding elements that pass the filter
                while (p.IsLeftOfOrEqualTo(End))
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                        || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope
                        || context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope)
                    {
                        if (usedElements[context.Element] == null)
                        {
                            if (filter(context.Element))
                            {
                                return true;
                            }
                        }

                        //cache the fact that we've already tested this element.
                        usedElements[context.Element] = context.Element;
                    }
                    p.Right(true, context);
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the specified pointer is in a position between, or equal to this range's Start/End points.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool InRange(MarkupPointer p)
        {
            return Start.IsLeftOfOrEqualTo(p) && End.IsRightOfOrEqualTo(p);
        }

        public bool InRange(IHTMLElement e)
        {
            Debug.Assert(e != null, "Unexpected null element.");
            return InRange(MarkupServices.CreateMarkupRange(e, true));
        }

        /// <summary>
        /// Returns true if the specified pointer is in a position between, or equal to this range's Start/End points.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool InRange(MarkupPointer p, bool allowEquals)
        {
            if (allowEquals)
                return Start.IsLeftOfOrEqualTo(p) && End.IsRightOfOrEqualTo(p);
            else
                return Start.IsLeftOf(p) && End.IsRightOf(p);
        }

        /// <summary>
        /// Returns true if the specified range is in a position between, or equal to this range's Start/End points.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool InRange(MarkupRange range)
        {
            return InRange(range, true);
        }

        /// <summary>
        /// Returns true if the specified range is in a position between, or (if allowed) equal to this range's Start/End points.
        /// </summary>
        /// <returns></returns>
        public bool InRange(MarkupRange range, bool allowEquals)
        {
            return InRange(range.Start, allowEquals) && InRange(range.End, allowEquals);
        }

        public bool Intersects(MarkupRange range)
        {
            return !(Start.IsRightOf(range.End) || End.IsLeftOf(range.Start));
        }

        /// <summary>
        /// Move this markup range to the specified element.
        /// </summary>
        /// <param name="e">The element to move to</param>
        /// <param name="outside">if true, then the range will be position around the outside of the element. If false,
        /// then the range will be position inside the begin and end tags of this element.</param>
        public void MoveToElement(IHTMLElement e, bool outside)
        {
            if (outside)
            {
                Start.MoveAdjacentToElement(e, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                End.MoveAdjacentToElement(e, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            }
            else
            {
                Start.MoveAdjacentToElement(e, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                End.MoveAdjacentToElement(e, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            }
        }

        /// <summary>
        /// Move this markup range to the specified location.
        /// Note: this replaces the start/end pointer of this range, so the range will permanently follow the new pointers.
        /// </summary>
        /// <param name="textRange"></param>
        public void MoveToPointers(MarkupPointer start, MarkupPointer end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Move this markup range to the specified location.
        /// </summary>
        /// <param name="textRange"></param>
        public void MoveToRange(MarkupRange range)
        {
            Start.MoveToPointer(range.Start);
            End.MoveToPointer(range.End);
        }

        /// <summary>
        /// Move to range.
        /// </summary>
        /// <param name="textRange"></param>
        public void MoveToTextRange(IHTMLTxtRange textRange)
        {
            MarkupServices.MovePointersToRange(textRange, Start, End);
        }

        /// <summary>
        /// Returns true if this range is currently positioned.
        /// </summary>
        public bool Positioned
        {
            get
            {
                return Start.Positioned && End.Positioned;
            }
        }

        /// <summary>
        /// Returns the parent element that the is shared by the start and end pointers.
        /// </summary>
        /// <returns></returns>
        public IHTMLElement ParentElement()
        {
            return GetSharedParent(Start, End);
        }

        /// <summary>
        /// Returns the parent element that the is shared by the start and end pointers.
        /// </summary>
        /// <returns></returns>
        public IHTMLElement ParentBlockElement()
        {
            return ParentElement(ElementFilters.IsBlockElement);
        }

        public IHTMLElement ParentElement(IHTMLElementFilter filter)
        {
            IHTMLElement parent = ParentElement();
            while (parent != null && !filter(parent))
            {
                parent = parent.parentElement;
            }
            return parent;
        }

        /// <summary>
        /// Condenses this range into the smallest well-formed state that still contains the same
        /// text markup.
        /// </summary>
        /// <returns></returns>
        public bool Trim()
        {
            MarkupPointer newStart = MarkupServices.CreateMarkupPointer(Start);
            MarkupPointer newEnd = MarkupServices.CreateMarkupPointer(End);
            MarkupContext context = new MarkupContext();

            //set newStart adjacent to the first text element to its right
            newStart.Right(true, context);
            while (!HasContentBetween(Start, newStart) && newStart.IsLeftOf(End))
                newStart.Right(true, context);
            if (HasContentBetween(Start, newStart))
                newStart.Left(true); //we overstepped the text, so back up one step

            //set newEnd adjacent to the first text element to its left
            newEnd.Left(true, context);
            while (!HasContentBetween(newEnd, End) && newEnd.IsRightOf(Start))
                newEnd.Left(true, context);
            if (HasContentBetween(newEnd, End))
                newEnd.Right(true); //we overstepped the text, so back up one step

            IHTMLElement sharedParent = GetSharedParent(newStart, newEnd);

            //span the start and end pointers as siblings by finding the parents of start and end
            //pointers that are direct children of the sharedParent
            IHTMLElement child = GetOuterMostChildOfParent(newStart, true, sharedParent);
            if (child != null)
                newStart.MoveAdjacentToElement(child, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);

            child = GetOuterMostChildOfParent(newEnd, false, sharedParent);
            if (child != null)
                newEnd.MoveAdjacentToElement(child, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

            if (!HasContentBetween(newStart, Start) && !HasContentBetween(End, newEnd)
                && !(Start.IsEqualTo(newStart) && End.IsEqualTo(newEnd)))
            {
                Start.MoveToPointer(newStart);
                End.MoveToPointer(newEnd);
                return true;
            }
            else
            {
                //the range didn't change, so return false.
                return false;
            }
        }

        public delegate bool RangeFilter(MarkupPointer start, MarkupPointer end);

        public bool MoveOutwardIfNoText()
        {
            return MoveOutwardIfNo(HasTextBetween);
        }

        // <summary>
        /// Expands this range out to the next parent shared by the start and end points
        /// if there there are no non-empty text elements between them.
        /// </summary>
        /// <returns></returns>
        public bool MoveOutwardIfNoContent()
        {
            return MoveOutwardIfNo(HasContentBetween);
        }

        /// <summary>
        /// Expands this range out to the next parent shared by the start and end points
        /// if there there are no non-empty text elements between them.
        /// </summary>
        /// <returns></returns>
        public bool MoveOutwardIfNo(RangeFilter rangeFilter)
        {
            MarkupRange newRange = MarkupServices.CreateMarkupRange();

            IHTMLElement sharedParent = GetSharedParent(Start, End);
            // If share a common parent, we will take the shared parent's parent so we can see if we want to grab
            // all the html inside of it, unless the shared parent is the body element, in which case we don't want to
            // expand outward anymore
            if (Start.CurrentScope == sharedParent && End.CurrentScope == sharedParent && !(sharedParent is IHTMLBodyElement))
            {
                sharedParent = sharedParent.parentElement;
            }

            //expand to the inside of the shared parent first.  If this matches the current placement
            //of the pointers, then expand to the outside of the parent. This allows the outter selection
            //to grow incrementally in such a way as to allow the shared parent to be tested between
            //each iteration of this operation.
            newRange.Start.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            newRange.End.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            if (newRange.IsEmpty() || newRange.Start.IsRightOf(Start) || newRange.End.IsLeftOf(End))
            {
                newRange.Start.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                newRange.End.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            }

            if (!rangeFilter(newRange.Start, Start) && !rangeFilter(End, newRange.End)
                && !(Start.IsEqualTo(newRange.Start) && End.IsEqualTo(newRange.End)))
            {
                Start.MoveToPointer(newRange.Start);
                End.MoveToPointer(newRange.End);
                return true;
            }
            else
            {
                //span the start and end pointers as siblings by finding the parents of start and end
                //pointers that are direct children of the sharedParent
                IHTMLElement child = GetOuterMostChildOfParent(Start, true, sharedParent);
                if (child != null)
                    newRange.Start.MoveAdjacentToElement(child, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                else
                    newRange.Start = Start;

                child = GetOuterMostChildOfParent(End, false, sharedParent);
                if (child != null)
                    newRange.End.MoveAdjacentToElement(child, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                else
                    newRange.End = End;

                if (!rangeFilter(newRange.Start, Start) && !rangeFilter(End, newRange.End)
                    && !(Start.IsEqualTo(newRange.Start) && End.IsEqualTo(newRange.End)))
                {
                    Start.MoveToPointer(newRange.Start);
                    End.MoveToPointer(newRange.End);
                    return true;
                }
                else
                {
                    //the range didn't change, so return false.
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a text range located at the same position as this MarkupRange.
        /// </summary>
        /// <returns></returns>
        public IHTMLTxtRange ToTextRange()
        {
            return MarkupServices.CreateTextRange(Start, End);
        }

        /// <summary>
        /// Walk through the markup range letting the walker visit each position.
        /// </summary>
        /// <param name="walker">the delegate walking navigating the the markup range</param>
        /// <returns></returns>
        public void WalkRange(MarkupRangeWalker walker)
        {
            WalkRange(walker, false);
        }

        /// <summary>
        /// Walk through the markup range letting the walker visit each position.
        /// </summary>
        /// <param name="walker">the delegate walking navigating the the markup range</param>
        /// <param name="inScopeElementsOnly">if true, enter/exit notifications about out-of-scope elements will be suppressed.</param>
        /// <returns></returns>
        public void WalkRange(MarkupRangeWalker walker, bool inScopeContextsOnly)
        {
            MarkupPointer p1 = MarkupServices.CreateMarkupPointer(Start);
            MarkupPointer p2 = MarkupServices.CreateMarkupPointer(Start);

            p1.Cling = false;
            p2.Cling = false;
            MarkupContext context = new MarkupContext();
            bool continueWalking = true;
            MarkupRange currentRange = null;

            while (continueWalking && p2.IsLeftOf(End))
            {
                string text = null;
                bool isInScope = true;

                p2.Right(true, context);
                currentRange = new MarkupRange(p1.Clone(), p2.Clone(), MarkupServices);

                if (inScopeContextsOnly)
                {
                    if (context.Element != null)
                    {
                        if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                        {
                            p1.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                            isInScope = InRange(p1);
                        }
                        else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                        {
                            p1.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                            isInScope = InRange(p1);
                        }
                    }
                    else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                    {
                        // It's possible part of the text is out of scope, so only return the in-scope text.
                        if (currentRange.End.IsRightOf(End))
                        {
                            currentRange.End.MoveToPointer(End);
                        }
                    }
                }

                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                {
                    text = currentRange.Text;
                }

                if (!inScopeContextsOnly || isInScope)
                {
                    continueWalking = walker(currentRange, context, text);
                }

                p1.MoveToPointer(p2);
            }
        }

        /// <summary>
        /// Walk through the markup range in reverse, letting the walker visit each position.
        /// </summary>
        /// <param name="walker">the delegate walking navigating the the markup range</param>
        /// <param name="inScopeElementsOnly">if true, enter/exit notifications about out-of-scope elements will be suppressed.</param>
        /// <returns></returns>
        public void WalkRangeReverse(MarkupRangeWalker walker, bool inScopeContextsOnly)
        {
            MarkupPointer p1 = MarkupServices.CreateMarkupPointer(End);
            MarkupPointer p2 = MarkupServices.CreateMarkupPointer(End);
            p1.Cling = false;
            p2.Cling = false;
            MarkupContext context = new MarkupContext();
            bool continueWalking = true;
            MarkupRange currentRange = null;

            while (continueWalking && p2.IsRightOf(Start))
            {
                string text = null;
                bool isInScope = true;

                p2.Left(true, context);
                currentRange = new MarkupRange(p2.Clone(), p1.Clone(), MarkupServices);

                if (inScopeContextsOnly)
                {
                    if (context.Element != null)
                    {
                        if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                        {
                            p1.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                            isInScope = InRange(p1);
                        }
                        else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                        {
                            p1.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                            isInScope = InRange(p1);
                        }
                    }
                    else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                    {
                        // It's possible part of the text is out of scope, so only return the in-scope text.
                        if (currentRange.Start.IsLeftOf(Start))
                        {
                            currentRange.Start.MoveToPointer(Start);
                        }
                    }
                }

                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                {
                    text = currentRange.Text;
                }

                if (!inScopeContextsOnly || isInScope)
                {
                    continueWalking = walker(currentRange, context, text);
                }

                p1.MoveToPointer(p2);
            }
        }

        /// <summary>
        /// Returns true if the start and end points of the range are equal.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Start.IsEqualTo(End);
        }

        /// <summary>
        /// Returns true if this range is composes entirely of non-visible elements.
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyOfContent()
        {
            return IsEmptyOfContent(true);
        }

        /// <summary>
        /// Returns true if this range is composes entirely of non-visible elements.
        /// </summary>
        /// <param name="inScopeContextsOnly">flag to ignore out of scope element
        /// (use false unless you absolutely want to ignore visible content in cases like
        ///  [start]&lt;p&gt;[end]&lt;/p&gt;)</param>
        /// <returns></returns>
        public bool IsEmptyOfContent(bool inScopeContextsOnly)
        {
            try
            {
                bool isEmptyOfContent = true;

                WalkRange(
                    delegate (MarkupRange currentRange, MarkupContext context, string text)
                        {
                            text = text ?? string.Empty;
                            if (!String.IsNullOrEmpty(text.Trim()))
                            {
                                isEmptyOfContent = false;
                                return false;
                            }

                            if (context.Element != null && ElementFilters.IsVisibleEmptyElement(context.Element))
                            {
                                isEmptyOfContent = false;
                                return false;
                            }

                            // Continue walking the range.
                            return true;
                        },
                    inScopeContextsOnly);

                return isEmptyOfContent;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsEmptyOfText(bool inScopeContextsOnly)
        {
            try
            {
                bool isEmptyOfText = true;

                WalkRange(
                    delegate (MarkupRange currentRange, MarkupContext context, string text)
                        {
                            text = text ?? string.Empty;
                            if (!String.IsNullOrEmpty(text.Trim()))
                            {
                                isEmptyOfText = false;
                                return false;
                            }

                            // Continue walking the range.
                            return true;
                        },
                    inScopeContextsOnly);

                return isEmptyOfText;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Beginning of range
        /// </summary>
        public MarkupPointer Start;

        /// <summary>
        /// End of range
        /// </summary>
        public MarkupPointer End;

        internal MshtmlMarkupServices MarkupServices;

        private IHTMLTxtRange stagingTxtRange;

        #region PRIVATE UTILITIES

        /// <summary>
        /// Return the parent element that 2 pointers share in common
        /// </summary>
        /// <returns></returns>
        private IHTMLElement GetSharedParent(MarkupPointer start, MarkupPointer end)
        {
            IHTMLElement startCurrentScope = start.CurrentScope;
            IHTMLElement endCurrentScope = end.CurrentScope;

            if (startCurrentScope == endCurrentScope)
            {
                //the start/end points share the same current scope, so return that element as the parent.
                return startCurrentScope;
            }
            else
            {
                //find the parent element that these 2 pointers share in common
                //by locating the first parent endtag that the rangeEnd pointer
                //is contained within.
                MarkupPointer parentStart = MarkupServices.CreateMarkupPointer();
                MarkupPointer parentEnd = MarkupServices.CreateMarkupPointer();
                IHTMLElement sharedParent = startCurrentScope;
                if (sharedParent != null)
                {
                    parentEnd.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    while (sharedParent != null && parentEnd.IsLeftOf(end))
                    {
                        sharedParent = sharedParent.parentElement;
                        parentEnd.MoveAdjacentToElement(sharedParent, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    }
                }
                return sharedParent;
            }
        }

        /// <summary>
        /// Returns true if there is non-empty content between 2 pointers.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>true if there is visible content between the pointers</returns>
        private bool HasContentBetween(MarkupPointer start, MarkupPointer end)
        {
            MarkupRange range = MarkupServices.CreateMarkupRange(start, end);
            return !range.IsEmptyOfContent(false);
        }

        private bool HasTextBetween(MarkupPointer start, MarkupPointer end)
        {
            MarkupRange range = MarkupServices.CreateMarkupRange(start, end);
            return !range.IsEmptyOfText(false);
        }

        /// <summary>
        /// Retrieve the parent of a child element that is closest to an outer parent element.
        /// </summary>
        /// <param name="from">the position to move move out from</param>
        /// <param name="lookRight">if true, look right for the inner child to start from, otherwise look left</param>
        /// <param name="outerParent">parent element to move out to</param>
        /// <returns>the direct child of the outerparent that contains the innerChild</returns>
        IHTMLElement GetOuterMostChildOfParent(MarkupPointer from, bool lookRight, IHTMLElement outerParent)
        {
            MarkupContext lookContext = new MarkupContext();
            if (lookRight)
                from.Right(false, lookContext);
            else
                from.Left(false, lookContext);

            //if there is a new element coming into scope, start the search from there,
            //otherwise, start from the currentScope.
            IHTMLElement innerChild;
            if (lookContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                innerChild = lookContext.Element;
            else
                innerChild = from.CurrentScope;

            IHTMLElement parent = innerChild;
            IHTMLElement innerParent = innerChild;
            while (parent != outerParent && parent != null)
            {
                innerParent = parent;
                parent = parent.parentElement;
            }
            Debug.Assert(innerParent != null, "Parent not found");

            if (innerParent == outerParent) //occurs when the from pointer is position directly in the parent.
            {
                return null;
            }
            return innerParent;
        }

        /// <summary>
        /// Returns true if the specified pointer is in a position between, or equal to the start/end points.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool isInRange(MarkupPointer start, MarkupPointer end, MarkupPointer p)
        {
            return start.IsLeftOfOrEqualTo(p) && end.IsRightOfOrEqualTo(p);
        }

        /// <summary>
        /// Returns true if the specified element begins and ends within the range.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool isInScope(IHTMLElement e)
        {
            MarkupPointer p = MarkupServices.CreateMarkupPointer(e, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            if (p.IsRightOfOrEqualTo(Start))
            {
                p = MarkupServices.CreateMarkupPointer(e, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                if (p.IsLeftOfOrEqualTo(End))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes (unsafely!) content within a range and repositions the range at a new position.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="newPosition"></param>
        private void deleteRangeContentAndMoveToPosition(MarkupRange range, MarkupPointer newPosition)
        {
            MarkupServices.Remove(range.Start, range.End);
            range.Start.MoveToPointer(newPosition);
            range.End.MoveToPointer(newPosition);
        }

        #endregion

        /// <summary>
        /// Returns null if ranges do not intersect
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public MarkupRange Intersect(MarkupRange range)
        {
            MarkupPointer maxStart = Start.IsRightOf(range.Start) ? Start : range.Start;
            MarkupPointer minEnd = End.IsLeftOf(range.End) ? End : range.End;

            if (minEnd.IsLeftOf(maxStart))
                return null;

            MarkupRange intersection = MarkupServices.CreateMarkupRange();
            intersection.Start.MoveToPointer(maxStart);
            intersection.End.MoveToPointer(minEnd);

            return intersection;
        }

        public void ExpandToInclude(MarkupRange range)
        {
            if (range == null)
                return;

            if (Positioned)
            {
                if (range.Start.IsLeftOf(Start))
                    Start.MoveToPointer(range.Start);

                if (range.End.IsRightOf(End))
                    End.MoveToPointer(range.End);
            }
            else
                MoveToRange(range);
        }

        /// <summary>
        /// Determines if a range has a particular _ELEMENT_TAG_ID applied.
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="partially">If true, then IsTagId will return true if any part of it is contained within a tagId element.
        ///                         If false, then IsTagId will return true only if the range is entirely contained within a tagId element.</param>
        /// <returns></returns>
        public bool IsTagId(_ELEMENT_TAG_ID tagId, bool partially)
        {
            // This first block of code will return true if the range is entirely contained within an element with the given tagId.
            IHTMLElement currentElement = ParentElement();
            while (currentElement != null)
            {
                if (MarkupServices.GetElementTagId(currentElement) == tagId)
                    return true;

                currentElement = currentElement.parentElement;
            }

            // This second block of code will return true if the range is partially contained within an element with the given tagId.
            if (partially)
            {
                IHTMLElement[] elements = GetElements(ElementFilters.CreateTagIdFilter(MarkupServices.GetNameForTagId(tagId)), false);
                return elements.Length > 0;
            }

            return false;
        }

        public void RemoveElementsByTagId(_ELEMENT_TAG_ID tagId, bool onlyIfNoAttributes)
        {
            if (tagId == _ELEMENT_TAG_ID.TAGID_NULL)
                return;

            // Remove the tagId up the parent chain
            IHTMLElement currentElement = ParentElement();
            while (currentElement != null)
            {
                if (MarkupServices.GetElementTagId(currentElement) == tagId &&
                    (!onlyIfNoAttributes || !HTMLElementHelper.HasMeaningfulAttributes(currentElement)))
                {
                    try
                    {
                        MarkupServices.RemoveElement(currentElement);
                    }
                    catch (COMException e)
                    {
                        Trace.Fail(String.Format("Failed to remove element ({0}) with error: {1}",
                            currentElement.outerHTML,   // {0}
                            e                           // {1}
                        ));
                    }
                }
                currentElement = currentElement.parentElement;
            }

            // Remove any other instances
            IHTMLElement[] elements =
                GetElements(ElementFilters.CreateTagIdFilter(MarkupServices.GetNameForTagId(tagId)), false);
            foreach (IHTMLElement e in elements)
            {
                if (MarkupServices.GetElementTagId(e) == tagId &&
                    (!onlyIfNoAttributes || !HTMLElementHelper.HasMeaningfulAttributes(e)))
                {
                    try
                    {
                        MarkupServices.RemoveElement(e);
                    }
                    catch (COMException ex)
                    {
                        Trace.Fail(String.Format("Failed to remove element ({0}) with error: {1}",
                            e.outerHTML,   // {0}
                            ex             // {1}
                        ));
                    }
                }
            }
        }

        public void EnsureStartIsBeforeEnd()
        {
            if (Start.IsRightOf(End))
            {
                MarkupPointer temp = End.Clone();
                End = Start.Clone();
                Start = temp;
            }
        }

        private MarkupPointer GetFirstTextPoint(MarkupPointer from, bool forward)
        {
            MarkupPointer firstTextPoint = from.Clone();

            MarkupContext context = new MarkupContext();
            bool keepLooking = true;
            do
            {
                if (forward)
                    firstTextPoint.Right(false, context);
                else
                    firstTextPoint.Left(false, context);

                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                    break;

                if (forward)
                {
                    firstTextPoint.Right(true, context);
                    keepLooking = context.Element != null && firstTextPoint.IsLeftOf(End);
                }
                else
                {
                    firstTextPoint.Left(true, context);
                    keepLooking = context.Element != null && firstTextPoint.IsRightOf(Start);
                }
            } while (keepLooking);

            return firstTextPoint;
        }

        /// <summary>
        /// Shrinks the range until further shrinking would exclude text that is currently in the range.
        /// </summary>
        public void SelectInner()
        {
            // Without this check, the start pointer can move outside
            // of the current container tag (e.g. ...text...|</p> => </p>|)
            if (IsEmpty())
                return;

            EnsureStartIsBeforeEnd();

            // Move the start until you hit text
            MarkupPointer innerStart = GetFirstTextPoint(Start, true);
            MarkupPointer innerEnd = GetFirstTextPoint(End, false);

            Start = innerStart;
            End = innerEnd;
        }
    }
}
