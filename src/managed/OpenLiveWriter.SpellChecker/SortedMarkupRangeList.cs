// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    public class SortedMarkupRangeList
    {
        private readonly SortedList<IMarkupPointerRaw, MarkupRange> words
            = new SortedList<IMarkupPointerRaw, MarkupRange>(new MarkupPointerComparer());

        public void Add(MarkupRange range)
        {
            MarkupRange newRange = range.Clone();
            newRange.Start.Cling = false;
            newRange.End.Cling = false;
            newRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            newRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;

            // Select just the inner-most text so that it is easier to compare to other MarkupRanges.
            newRange.SelectInner();

            try
            {
                words.Add(newRange.Start.PointerRaw, newRange);
            }
            catch (ArgumentException ex)
            {
                Trace.Fail(ex.ToString());
            }

        }

        public bool Contains(MarkupRange testRange)
        {
            // Select just the inner-most text to normalize the MarkupRange before comparing it.
            testRange.SelectInner();

            int pos = BinarySearch(words.Keys, testRange.Start.PointerRaw, new MarkupPointerComparer());
            if (pos >= 0)
            {
                if (words.Values[pos].End.IsLeftOf(testRange.End))
                {
                    Debug.Fail("testRange partially overlaps with range");
                    try
                    {
                        Debug.WriteLine("testRange: [" + testRange.Text + "]");
                        Debug.WriteLine("thisRange: [" + words.Values[pos].Text + "]");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                }
                return true;
            }

            pos = (~pos)-1;

            if (pos < 0)
                return false;

            if (words.Values[pos].End.IsRightOf(testRange.Start))
            {
                if (words.Values[pos].End.IsLeftOf(testRange.End))
                {
#if DEBUG
                    MarkupRange temp = testRange.Clone();
                    temp.Start.MoveToPointer(words.Values[pos].End);
                    if ((temp.Text ?? "").Trim().Length > 0)
                    {
                        Debug.Fail("testRange partially overlaps with range");
                        try
                        {
                            Debug.WriteLine("testRange: [" + testRange.Text + "]");
                            Debug.WriteLine("thisRange: [" + words.Values[pos].Text + "]");
                            Debug.WriteLine("overlap:   [" + temp.Text + "]");
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString());
                        }
                    }
#endif
                }
                return true;
            }

            return false;
        }

        public void ClearRange(MarkupRange clear)
        {
            if (words.Count == 0)
                return;

            // Just being defensive here
            if (!clear.Positioned)
                return;

            // Debug.WriteLine(string.Format("ClearRange:\r\n{0}\r\n{1}", clear.Start.PositionTextDetail, clear.End.PositionTextDetail));

            /*
             * Start from the first range in the list where the start is left of
             * the Clear range's end. Take a look at each range until you get to
             * one where the range's end is left of the Clear range's start.
             */

            int idx = BinarySearch(words.Keys, clear.Start.PointerRaw, new MarkupPointerComparer());
            if (idx < 0)
            {
                idx = ~idx;
                idx--;
                idx = Math.Max(0, idx);
            }

            for (; idx < words.Count; idx++)
            {
                MarkupRange range = words.Values[idx];
                // Debug.WriteLine("Testing range: " + range.Text);
                if (!range.Positioned)
                {
                    // Debug.WriteLine("ClearRange: Removing unpositioned word");
                    words.RemoveAt(idx--);
                }
                else if (clear.End.IsLeftOfOrEqualTo(range.Start))
                {
                    // Debug.WriteLine("We've gone far enough--all done");
                    return;
                }
                else if (clear.InRange(range))
                {
                    // Debug.WriteLine("ClearRange: Removing contained range");
                    words.RemoveAt(idx--);
                }
                else if (range.InRange(clear))
                {
                    // Debug.WriteLine("ClearRange: Splitting range");
                    MarkupRange trailingRange = range.Clone();
                    trailingRange.Start.MoveToPointer(clear.End);
                    range.End.MoveToPointer(clear.Start);

                    if (range.IsEmpty())
                        words.RemoveAt(idx--);

                    if (!trailingRange.IsEmpty())
                        Add(trailingRange);
                }
                else if (range.InRange(clear.End, false))
                {
                    // Debug.WriteLine("ClearRange: Partial overlap, trimming from start of range");
                    words.RemoveAt(idx--);
                    range.Start.MoveToPointer(clear.End);
                    if (!range.IsEmpty())
                        Add(range);
                }
                else if (range.InRange(clear.Start, false))
                {
                    // Debug.WriteLine("ClearRange: Partial overlap, trimming from end of range");
                    range.End.MoveToPointer(clear.End);
                    if (range.IsEmpty())
                        words.RemoveAt(idx--);
                }
            }

            // Debug.WriteLine("ClearRange: Remaining words in ignore list: " + words.Count);
        }

        /// <summary>
        /// Returns true if a word range is in this list that contains this markup pointer.
        /// </summary>
        public bool Contains(MarkupPointer p)
        {
            if (words.Count == 0)
                return false;

            // Just being defensive
            if (!p.Positioned)
                return false;

            int idx = BinarySearch(words.Keys, p.PointerRaw, new MarkupPointerComparer());
            if (idx >= 0)
                return true;

            idx = ~idx;

            // really this could be "if (idx == words.Count)"--it's to handle the case
            // where p is larger than the values in the list
            while (idx >= words.Count)
                idx--;

            for (; idx >= 0; idx--)
            {
                MarkupRange wordRange = words.Values[idx];
                if (wordRange.InRange(p))
                    return true;
                if (wordRange.End.IsLeftOf(p))
                    break;
            }
            return false;
        }

        /// <summary>
        /// Does a binary search. Very similar to System.Array.BinarySearch().
        /// </summary>
        /// <returns>
        /// The index of the specified value in the specified array, if value is found.
        /// If value is not found and value is less than one or more elements in array,
        /// a negative number which is the bitwise complement of the index of the first
        /// element that is larger than value. If value is not found and value is
        /// greater than any of the elements in array, a negative number which is the
        /// bitwise complement of (the index of the last element plus 1).
        /// </returns>
        public static int BinarySearch<T>(IList<T> list, T value, IComparer<T> comparer)
        {
            int min = 0;
            int max = list.Count;

            while (min < max)
            {
                int mid = min + ((max - min)/2);
                int result = comparer.Compare(value, list[mid]);

                // found it
                if (result == 0)
                    return mid;

                if (result < 0)
                    max = mid;
                else
                    min = mid + 1;
            }

            return ~min;
        }

        public void Clear()
        {
            words.Clear();
        }
    }
}
