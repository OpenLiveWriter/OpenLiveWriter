// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    //used to store the highlight segments in a post for tracking spelling changes
    public class HighlightSegmentTracker
    {
        SortedList list;

        private class SegmentDef
        {
            public IHighlightSegmentRaw segment;
            public IMarkupPointerRaw endPtr;
            public IMarkupPointerRaw startPtr;
            public string word;
            public SegmentDef(IHighlightSegmentRaw seg, IMarkupPointerRaw start, IMarkupPointerRaw end, string wd)
            {
                segment = seg;
                startPtr = start;
                endPtr = end;
                word = wd;
            }

        }

        public class MatchingSegment
        {
            public IHighlightSegmentRaw _segment;
            public IMarkupPointerRaw _pointer;
            public MatchingSegment(IHighlightSegmentRaw seg, IMarkupPointerRaw pointer)
            {
                _segment = seg;
                _pointer = pointer;
            }
        }

        //this needs to be on a post by post basis
        public HighlightSegmentTracker()
        {
            list = new SortedList(new MarkupPointerComparer());
        }

        //adds a segment to the list
        //used when a misspelled word is found
        public void AddSegment(IHighlightSegmentRaw segment, string wordHere, IMarkupServicesRaw markupServices)
        {
            IMarkupPointerRaw start, end;
            markupServices.CreateMarkupPointer(out start);
            markupServices.CreateMarkupPointer(out end);
            segment.GetPointers(start, end);
            if (!list.ContainsKey(start))
                list.Add(start, new SegmentDef(segment, start, end, wordHere));
        }

        //find all the segments in a specific range
        //used to clear out a section when it is getting rechecked
        //need to expand selection from these bounds out around full words
        public IHighlightSegmentRaw[] GetSegments(IMarkupPointerRaw start, IMarkupPointerRaw end)
        {
            if (list.Count == 0)
                return null;
            int firstSegmentInd = -1;
            int lastSegmentInd = list.Count;
            bool test;
            //find the first segment after the start pointer
            do
            {
                firstSegmentInd++;
                //check if we have gone through the whole selection
                if (firstSegmentInd >= lastSegmentInd)
                    return null;
                SegmentDef x = (SegmentDef) list.GetByIndex(firstSegmentInd);
                start.IsRightOf(x.startPtr, out test);
            } while (test);
            do
            {
                lastSegmentInd--;
                //check if we have gone through the whole selection
                if (lastSegmentInd < firstSegmentInd)
                    return null;
                SegmentDef x = (SegmentDef) list.GetByIndex(lastSegmentInd);
                end.IsLeftOf(x.startPtr, out test);
            } while (test);
            return Subarray(firstSegmentInd, lastSegmentInd);
        }

        public IHighlightSegmentRaw[] ClearAllSegments()
        {
            return Subarray(0, list.Count - 1);
        }

        public delegate bool CheckWordSpelling(string word);

        //find all the segments with a specific misspelled word
        //used to clear for ignore all, add to dictionary
        public MatchingSegment[] GetSegments(string word, CheckWordSpelling checkSpelling)
        {
            ArrayList segments = new ArrayList();
            for (int i = 0; i < list.Count; i++)
            {
                SegmentDef x = (SegmentDef) list.GetByIndex(i);
                //TODO: Change with new cultures added!!!
                if (0 == String.Compare(word, x.word, true, CultureInfo.InvariantCulture))
                {
                    //check spelling--capitalized word may be ok, but not mixed case, etc.
                    if (!checkSpelling(x.word))
                    {
                        segments.Add(new MatchingSegment(x.segment, x.startPtr));
                    }
                }
            }
            return (MatchingSegment[])segments.ToArray(typeof (MatchingSegment));
        }

        public void RemoveSegment(IMarkupPointerRaw pointer)
        {
            list.Remove(pointer);
        }

        public MisspelledWordInfo FindSegment(MshtmlMarkupServices markupServices, IMarkupPointerRaw current)
        {
            //binary search
            int start = 0;
            int end = list.Count - 1;
            int i = Middle(start, end);
            while (-1 != i)
            {
                SegmentDef x = (SegmentDef)list.GetByIndex(i);
                bool startTest;
                current.IsRightOfOrEqualTo(x.startPtr, out startTest);
                if (startTest)
                {
                    bool endTest;
                    current.IsLeftOfOrEqualTo(x.endPtr, out endTest);
                    if (endTest)
                    {
                        MarkupPointer pStart = markupServices.CreateMarkupPointer(x.startPtr);
                        MarkupPointer pEnd = markupServices.CreateMarkupPointer(x.endPtr);
                        MarkupRange range = markupServices.CreateMarkupRange(pStart, pEnd);
                        //this could be a "phantom" range...no more content due to uncommitted damage or other reasons
                        //if it is phantom, remove it from the tracker and return null
                        if (range.Text == null)
                        {
                            list.RemoveAt(i);
                            return null;
                        }
                        return new MisspelledWordInfo(range, x.word);
                    }
                    start = i + 1;
                }
                else
                {
                    end = i - 1;
                }
                i = Middle(start, end);
            }
            return null;
        }

        private int Middle(int start, int end)
        {
            if (start <= end)
            {
                return (int)Math.Floor(Convert.ToDouble((start + end)/2));
            }
            return -1;
        }

        private IHighlightSegmentRaw[] Subarray(int start, int end)
        {
            int count = end - start + 1;
            IHighlightSegmentRaw[] segments = new IHighlightSegmentRaw[count];
            //fill in array by removing from the list starting at the end, so that
            // deleting from the list doesn't change the other indices
            for (int i = end; i >= start; i--)
            {
                segments[--count] = ((SegmentDef) list.GetByIndex(i)).segment;
                list.RemoveAt(i);
            }
            return segments;
        }

        public void Clear()
        {
            list.Clear();
        }
    }

    internal class MarkupPointerComparer : IComparer, System.Collections.Generic.IComparer<IMarkupPointerRaw>
    {
        public int Compare(object x, object y)
        {
            IMarkupPointerRaw a = (IMarkupPointerRaw) x;
            IMarkupPointerRaw b = (IMarkupPointerRaw) y;
            return Compare(a, b);
        }

        public int Compare(IMarkupPointerRaw a, IMarkupPointerRaw b)
        {
            bool test;
            a.IsEqualTo(b, out test);
            if (test) return 0;
            a.IsLeftOf(b, out test);
            if (test) return -1;
            return 1;
        }
    }
}
