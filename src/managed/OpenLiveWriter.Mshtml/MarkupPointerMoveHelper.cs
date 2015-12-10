// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.Mshtml
{
    [Flags]
    public enum MarkupPointerAdjacency : ulong
    {
        BeforeText = 0x000,
        AfterEnterBlock = 0x001,
        BeforeExitBlock = 0x002,
        BeforeVisible = 0x004,
        BeforeEnterScope = 0x008,
        //Flag6					=			0x010,
        //Flag7					=			0x020,
        //Flag8					=			0x040,
        //Flag9					=			0x080,
        //Flag10				=			0x100,
        //AllFlags				=			0xffff
    }

    public class MarkupPointerMoveHelper
    {
        private enum MoveFilterResult { CONTINUE, STOP, STOP_BACK }
        private delegate MoveFilterResult MoveContextFilter(MarkupContext mc);
        public enum MoveDirection { LEFT, RIGHT };

        private static MoveContextFilter CreateMoveContextFilter(MarkupPointerAdjacency stopRule)
        {
            ArrayList stopFilters = new ArrayList();
            if (FlagIsSet(MarkupPointerAdjacency.BeforeText, stopRule))
                stopFilters.Add(new MoveContextFilter(StopBeforeText));
            if (FlagIsSet(MarkupPointerAdjacency.AfterEnterBlock, stopRule))
                stopFilters.Add(new MoveContextFilter(StopAfterEnterBlock));
            if (FlagIsSet(MarkupPointerAdjacency.BeforeExitBlock, stopRule))
                stopFilters.Add(new MoveContextFilter(StopBeforeExitBlock));
            if (FlagIsSet(MarkupPointerAdjacency.BeforeVisible, stopRule))
                stopFilters.Add(new MoveContextFilter(StopBeforeVisible));
            if (FlagIsSet(MarkupPointerAdjacency.BeforeEnterScope, stopRule))
                stopFilters.Add(new MoveContextFilter(StopBeforeEnterScope));

            if (stopFilters.Count > 0)
                return MergeContextFilters((MoveContextFilter[])stopFilters.ToArray(typeof(MoveContextFilter)));
            else
                return new MoveContextFilter(ContinueFilter);
        }

        public static void MoveUnitBounded(MarkupPointer p, MoveDirection direction, MarkupPointerAdjacency stopRule, MarkupPointer boundary)
        {
            MoveContextFilter filter = CreateMoveContextFilter(stopRule);
            MoveUnitBounded(p, direction, filter, boundary);
        }

        public static void MoveUnitBounded(MarkupPointer p, MoveDirection direction, MarkupPointerAdjacency stopRule, IHTMLElement boundary)
        {
            MarkupPointer pBoundary = p.Clone();
            pBoundary.MoveAdjacentToElement(boundary, direction == MoveDirection.LEFT ? _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin : _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            MoveUnitBounded(p, direction, stopRule, pBoundary);
        }

        private static void MoveUnitBounded(MarkupPointer p, MoveDirection direction, MoveContextFilter continueFilter, MarkupPointer boundary)
        {
            MarkupPointer p1 = p.Clone();
            MarkupPointer lastGoodPosition = p.Clone();
            MarkupContext context = new MarkupContext();
            MoveFilterResult result = MoveFilterResult.CONTINUE;
            while (CheckMoveBoundary(p1, boundary, direction) && result == MoveFilterResult.CONTINUE)
            {
                lastGoodPosition.MoveToPointer(p1);
                MovePointer(p1, direction, context);
                result = continueFilter(context);
            }
            if (result == MoveFilterResult.CONTINUE)
            {
                //we hit the boundary, so position pointer at the boundary
                p1.MoveToPointer(boundary);
            }
            else if (result == MoveFilterResult.STOP_BACK)
            {
                p1.MoveToPointer(lastGoodPosition);
            }

            p.MoveToPointer(p1);
        }

        private static bool CheckMoveBoundary(MarkupPointer p, MarkupPointer boundary, MoveDirection d)
        {
            if (d == MoveDirection.LEFT)
                return p.IsRightOf(boundary);
            else
                return p.IsLeftOf(boundary);
        }

        private static void MovePointer(MarkupPointer p, MoveDirection d, MarkupContext context)
        {
            if (d == MoveDirection.LEFT)
                p.Left(true, context);
            else
                p.Right(true, context);
        }

        private static MoveFilterResult ContinueFilter(MarkupContext mc)
        {
            return MoveFilterResult.CONTINUE;
        }

        private static MoveFilterResult StopAfterEnterBlock(MarkupContext mc)
        {
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope && ElementFilters.IsBlockElement(mc.Element))
                return MoveFilterResult.STOP;
            return MoveFilterResult.CONTINUE;
        }

        private static MoveFilterResult StopBeforeExitBlock(MarkupContext mc)
        {
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope && ElementFilters.IsBlockElement(mc.Element))
                return MoveFilterResult.STOP_BACK;
            return MoveFilterResult.CONTINUE;
        }

        private static MoveFilterResult StopBeforeText(MarkupContext mc)
        {
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                return MoveFilterResult.STOP_BACK;
            return MoveFilterResult.CONTINUE;
        }

        private static MoveFilterResult StopBeforeVisible(MarkupContext mc)
        {
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                return MoveFilterResult.STOP_BACK;
            if (mc.Element != null && ElementFilters.IsInlineElement(mc.Element) && !ElementFilters.IsVisibleEmptyElement(mc.Element))
                return MoveFilterResult.CONTINUE;
            return MoveFilterResult.STOP_BACK;
        }

        private static MoveFilterResult StopBeforeEnterScope(MarkupContext mc)
        {
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                return MoveFilterResult.STOP_BACK;
            return MoveFilterResult.CONTINUE;
        }

        private static MoveContextFilter MergeContextFilters(params MoveContextFilter[] filters)
        {
            return new MoveContextFilter(new CompoundMoveContextFilter(filters).MergeContextFilters);
        }

        private class CompoundMoveContextFilter
        {
            private MoveContextFilter[] _filters;
            public CompoundMoveContextFilter(MoveContextFilter[] filters)
            {
                _filters = filters;
            }
            public MoveFilterResult MergeContextFilters(MarkupContext mc)
            {
                foreach (MoveContextFilter filter in _filters)
                {
                    MoveFilterResult result = filter(mc);
                    if (result != MoveFilterResult.CONTINUE)
                        return result;
                }
                return MoveFilterResult.CONTINUE;
            }
        }

        private static bool FlagNotSet(MarkupPointerAdjacency flag, MarkupPointerAdjacency flagMask)
        {
            return !FlagIsSet(flag, flagMask);
        }

        private static bool FlagIsSet(MarkupPointerAdjacency flag, MarkupPointerAdjacency flagMask)
        {
            ulong flagLong = (ulong)flag;
            ulong mask = ((ulong)flagMask) & flagLong;
            return mask == flagLong;
        }

        /// <summary>
        /// Find the most logic direction to move the cursor so that it matches the where has clicked
        /// </summary>
        /// <param name="Selection"></param>
        /// <returns></returns>
        public static Direction FindSelectionToLogicalPosition(MarkupRange Selection, IHTMLElement body, bool? forward)
        {
            // There is a selection, not just a click
            if (!Selection.Start.IsEqualTo(Selection.End))
                return Direction.None;

            Direction dir = ImageBreakout(Selection.Start);

            if (dir != Direction.None)
            {
                return dir;
            }

            MarkupContext contextRight = Selection.Start.Right(false);
            MarkupContext contextLeft = Selection.Start.Left(false);

            // there is text or some other type of content around the click
            if (!((contextLeft.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope || contextLeft.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                && (contextRight.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope || contextRight.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)))
                return Direction.None;

            // The click is not between two block elements, so it should be fine where it is
            if (!ElementFilters.IsBlockElement(contextLeft.Element) || !ElementFilters.IsBlockElement(contextRight.Element))
                return Direction.None;

            // </blockElement>|</postBody>
            if (contextLeft.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                && !IsSmartContent(contextLeft)
                && contextRight.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope
                && contextRight.Element.id == body.id)
                return Direction.Left;

            // <postBody>|<blockElement>
            if (contextRight.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                && !IsSmartContent(contextRight)
                && contextLeft.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope
                && contextLeft.Element.id == body.id)
                return Direction.Right;

            // </blockElement>|<blockElement>
            if (contextLeft.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                && !IsSmartContent(contextLeft)
                && contextRight.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope
                && !IsSmartContent(contextRight))
                return forward == null || forward == true ? Direction.Right : Direction.Left;

            return Direction.None;

        }

        /// <summary>
        /// Checks if the MarkupPointer is inside of an anchor that only contains
        /// an image, and if so, moves the pointer to be outside the anchor.
        /// This prevents cases where hyperlinked images were getting nested inside
        /// of each other as a result of drag and drop.
        /// </summary>
        public static Direction ImageBreakout(MarkupPointer p)
        {
            //   If inside <a><img></a>, then move to outside
            IHTMLElement currentScope = p.CurrentScope;
            if (currentScope is IHTMLAnchorElement)
            {
                IHTMLDOMNode anchor = (IHTMLDOMNode)currentScope;
                if (anchor.hasChildNodes() && anchor.firstChild is IHTMLImgElement && anchor.firstChild.nextSibling == null)
                {
                    // Figure out if we are positioned before or after the image; this will determine
                    // whether we want to move before or after the anchor
                    return (p.Right(false).Element is IHTMLImgElement)
                                                              ? Direction.Left
                                                              : Direction.Right;
                }
            }
            return Direction.None;
        }

        private static bool IsSmartContent(MarkupContext ctx)
        {
            return ((IHTMLElement3)ctx.Element).contentEditable.ToUpperInvariant() == "FALSE";
        }

        public static bool DriveSelectionToLogicalPosition(MarkupRange Selection, IHTMLElement body)
        {
            return DriveSelectionToLogicalPosition(Selection, body, null);
        }

        /// <summary>
        /// Find the most logic direction to move to visible location that is where it appears to the user.
        /// And then moves it to that location.  Returns true if it moved the pointer.
        /// </summary>
        /// <param name="Selection"></param>
        /// <returns></returns>
        public static bool DriveSelectionToLogicalPosition(MarkupRange Selection, IHTMLElement body, bool? forward)
        {
            Direction dir = FindSelectionToLogicalPosition(Selection, body, forward);

            if (dir == Direction.None)
                return false;

            if (dir == Direction.Right)
            {
                Selection.End.Right(true);
                Selection.Start.Right(true);
            }
            else if (dir == Direction.Left)
            {
                Selection.End.Left(true);
                Selection.Start.Left(true);
            }

            return true;
        }

        public static void PerformImageBreakout(MarkupPointer p)
        {
            Direction dir = ImageBreakout(p);

            if (dir == Direction.Right)
            {
                p.MoveAdjacentToElement(p.CurrentScope, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            }

            if (dir == Direction.Left)
            {
                p.MoveAdjacentToElement(p.CurrentScope, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            }

        }
    }

    public enum Direction
    {
        None = 0,
        Left = 1,
        Right = 2
    }
}
