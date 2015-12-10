// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    public class MarkupHelpers
    {
        public static bool AdjustMarkupRange(MarkupRange range, int offset, int length)
        {
            IHTMLTxtRange stagingTextRange = range.ToTextRange();
            return AdjustMarkupRange(ref stagingTextRange, range, offset, length);
        }

        /// <summary>
        /// Adjust the start and end of the range to match the offset/length, in characters.
        /// if the offset/length adjustment fails to produce the expected value,
        /// then the adjustment is cancelled and false is returned.
        /// </summary>
        public static bool AdjustMarkupRange(ref IHTMLTxtRange stagingTextRange, MarkupRange range, int offset, int length)
        {
            string currentText = GetRangeTextFast(range) ?? "";

            if (offset == 0 && length == currentText.Length)
                return true;

            string expectedText;
            try
            {
                expectedText = currentText.Substring(offset, length);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            MarkupRange testRange = range.Clone();
            AdjustMarkupRangeCore(testRange, offset, length, currentText);
            if (GetRangeTextFast(testRange) != expectedText)
                return false;

            range.MoveToRange(testRange);
            return true;
        }

        private static void AdjustMarkupRangeCore(MarkupRange range, int offset, int length, string currentText)
        {
            MarkupPointer start = range.Start;
            MarkupPointer end = range.End;

            if (offset > 0)
                start.MoveToMarkupPosition(start.Container, start.MarkupPosition + offset);
            if (length < (offset + currentText.Length))
                end.MoveToMarkupPosition(start.Container, start.MarkupPosition + length);
        }

        public static MarkupRange GetEditableRange(IHTMLElement e, MshtmlMarkupServices markupServices)
        {
            IHTMLElement3 editableElement = null;
            while (e != null)
            {
                if (((IHTMLElement3)e).isContentEditable)
                {
                    editableElement = (IHTMLElement3)e;
                    if (ElementFilters.IsBlockElement(e))
                        break;
                }
                else
                    break;
                e = e.parentElement;
            }

            if (editableElement != null)
            {
                return markupServices.CreateMarkupRange((IHTMLElement)editableElement, false);
            }
            else
                return null;
        }

        public static void SplitBlockForInsertionOrBreakout(MshtmlMarkupServices markupServices, MarkupRange bounds, MarkupPointer insertAt)
        {
            IHTMLElement currentBlock = insertAt.GetParentElement(ElementFilters.BLOCK_OR_TABLE_CELL_ELEMENTS);
            if (currentBlock == null)
                return;

            if (ElementFilters.IsBlockQuoteElement(currentBlock) || ElementFilters.IsTableCellElement(currentBlock))
                return;

            MarkupPointer blockStart = markupServices.CreateMarkupPointer(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            MarkupPointer blockEnd = markupServices.CreateMarkupPointer(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

            if (bounds != null && (blockStart.IsLeftOf(bounds.Start) || blockEnd.IsRightOf(bounds.End)))
                return;

            // Don't split if at the beginning or end of the visible content in the block.
            // Instead just move the insertion point outside the block.
            MarkupRange testRange = markupServices.CreateMarkupRange();
            testRange.Start.MoveToPointer(insertAt);
            testRange.End.MoveAdjacentToElement(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            if (testRange.IsEmptyOfContent())
            {
                insertAt.MoveAdjacentToElement(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                return;
            }
            testRange.Start.MoveAdjacentToElement(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            testRange.End.MoveToPointer(insertAt);
            if (testRange.IsEmptyOfContent())
            {
                insertAt.MoveAdjacentToElement(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                return;
            }

            MarkupPointer moveTarget = markupServices.CreateMarkupPointer(blockEnd);
            markupServices.Move(insertAt, blockEnd, moveTarget);
            insertAt.MoveAdjacentToElement(currentBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
        }

        public delegate TResult TextRangeFunc<TResult>(IHTMLTxtRange rng);

        /// <summary>
        /// Text ranges are extremely expensive to create, and readily reusable. UseStagingTextRange
        /// is a higher order function that makes it slightly easier to reuse text ranges, which can
        /// give much better performance if the lifetime of your stagingTextRange reference spans
        /// lots of calls.
        /// </summary>
        /// <typeparam name="TResult">The type of the result from the function we'll execute.</typeparam>
        /// <param name="stagingTextRange">A reference to a stagingTextRange that we can use, it can be null at first and we'll create on demand if necessary.</param>
        /// <param name="range">The markup range to move the stagingTextRange to.</param>
        /// <param name="func">The function to pass the stagingTextRange to after it's been created/positioned.</param>
        /// <returns>The value returned from func.</returns>
        public static TResult UseStagingTextRange<TResult>(ref IHTMLTxtRange stagingTextRange, MarkupRange range, TextRangeFunc<TResult> func)
        {
            Debug.Assert(range != null, "Range must not be null!");
            Debug.Assert(range.Positioned, "Range must be positioned!");
            Debug.Assert(range.Start.IsLeftOfOrEqualTo(range.End), "Range start must be left of or equal to range end!");

            if (stagingTextRange == null)
                stagingTextRange = range.MarkupServices.CreateTextRange(range.Start, range.End);
            else
                range.MarkupServices.MoveRangeToPointers(range.Start, range.End, stagingTextRange);

            return func(stagingTextRange);
        }

        public static string GetRangeTextFast(MarkupRange range)
        {
            IHTMLTxtRange stagingTextRange = IHTMLTxtRangePool.AquireTxtRange(range);
            string returnValue = UseStagingTextRange(ref stagingTextRange, range, rng => rng.text);
            IHTMLTxtRangePool.RelinquishTxtRange(stagingTextRange, range);
            return returnValue;
        }

        public static string GetRangeHtmlFast(MarkupRange range)
        {
            IHTMLTxtRange stagingTextRange = IHTMLTxtRangePool.AquireTxtRange(range);
            string returnValue = UseStagingTextRange(ref stagingTextRange, range, rng => rng.htmlText);
            IHTMLTxtRangePool.RelinquishTxtRange(stagingTextRange, range);
            return returnValue;
        }
    }

    internal static class IHTMLTxtRangePool
    {
        private static Hashtable cache = null;

        static IHTMLTxtRangePool()
        {
            cache = Hashtable.Synchronized(new Hashtable());
        }

        public static IHTMLTxtRange AquireTxtRange(MarkupRange range)
        {
            try
            {
                if (cache != null)
                {
                    int documentId = GetDocumentKey(range.MarkupServices.MarkupServicesRaw);

                    Queue queue = null;
                    IHTMLTxtRange returnRange = null;

                    lock (cache.SyncRoot)
                    {
                        queue = (Queue)cache[documentId];

                        if (queue == null)
                        {
                            queue = Queue.Synchronized(new Queue());
                            cache.Add(documentId, queue);
                        }

                        if (queue.Count > 0)
                        {
                            lock (queue.SyncRoot)
                            {
                                returnRange = (IHTMLTxtRange)queue.Dequeue();
                            }
                        }
                        else
                        {
                            returnRange = range.ToTextRange();
                        }

                    }

                    return returnRange;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failure in IHTMLTxtRangePool: " + ex);
                cache = null;
            }

            try
            {
                return range.ToTextRange();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failure in IHTMLTxtRangePool: " + ex);
                return null;
            }
        }

        public static void RelinquishTxtRange(IHTMLTxtRange txtRange, MarkupRange range)
        {
            if (cache == null)
                return;

            try
            {
                lock (cache.SyncRoot)
                {
                    Queue queue = (Queue)cache[GetDocumentKey(range.MarkupServices.MarkupServicesRaw)];

                    if (queue != null)
                    {
                        lock (queue.SyncRoot)
                        {
                            queue.Enqueue(txtRange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failure in IHTMLTxtRangePool: " + ex);
                cache = null;
            }

        }

        internal static void Clear(IMarkupServicesRaw markupServices)
        {
            if (cache == null)
                return;

            try
            {
                lock (cache.SyncRoot)
                {
                    cache.Remove(GetDocumentKey(markupServices));
                }
            }
            catch (Exception)
            {
                cache = null;
            }
        }

        private static int GetDocumentKey(IMarkupServicesRaw markupServices)
        {
            return markupServices.GetHashCode();
        }
    }
}
