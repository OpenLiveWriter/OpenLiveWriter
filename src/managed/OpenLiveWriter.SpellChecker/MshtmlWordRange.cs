// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.SpellChecker
{
    public delegate void DamageFunction(MarkupRange range);

    public delegate bool MarkupRangeFilter(MarkupRange range);

    /// <summary>
    /// Implementation of IWordRange for an MSHTML control
    /// </summary>
    public class MshtmlWordRange : IWordRange
    {
        MarkupRangeFilter filter = null;
        DamageFunction damageFunction = null;

        /// <summary>
        /// Initialize word range for the entire body of the document
        /// </summary>
        /// <param name="mshtml">mshtml control</param>
        public MshtmlWordRange(MshtmlControl mshtmlControl) :
            this(mshtmlControl.HTMLDocument, false, null, null)
        {
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        public MshtmlWordRange(IHTMLDocument document, bool useDocumentSelectionRange, MarkupRangeFilter filter, DamageFunction damageFunction)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            IHTMLDocument2 document2 = (IHTMLDocument2)document;
            MarkupRange markupRange;
            if (useDocumentSelectionRange)
            {
                markupRange = markupServices.CreateMarkupRange(document2.selection);
            }
            else
            {
                // TODO: Although this works fine, it would be better to only spellcheck inside editable regions.
                markupRange = markupServices.CreateMarkupRange(document2.body, false);
            }

            Init(document, markupServices, markupRange, filter, damageFunction, useDocumentSelectionRange);
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        public MshtmlWordRange(IHTMLDocument document, MarkupRange markupRange, MarkupRangeFilter filter, DamageFunction damageFunction)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            Init(document, markupServices, markupRange, filter, damageFunction, true);
        }

        private void Init(IHTMLDocument document, MshtmlMarkupServices markupServices, MarkupRange selectionRange, MarkupRangeFilter filter, DamageFunction damageFunction, bool expandRange)
        {
            // save references
            this.htmlDocument = document;
            this.markupServices = markupServices;
            this.selectionRange = selectionRange;
            this.filter = filter;
            this.damageFunction = damageFunction;

            // If the range is already the body, don't expand it or else it will be the whole document
            if (expandRange)
                ExpandRangeToWordBoundaries(selectionRange);

            // initialize pointer to beginning of selection range
            MarkupPointer wordStart = MarkupServices.CreateMarkupPointer(selectionRange.Start);
            MarkupPointer wordEnd = MarkupServices.CreateMarkupPointer(selectionRange.Start);

            //create the range for holding the current word.
            //Be sure to set its gravity so that it stays around text that get replaced.
            currentWordRange = MarkupServices.CreateMarkupRange(wordStart, wordEnd);
            currentWordRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            currentWordRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

            currentVirtualPosition = currentWordRange.End.Clone();
        }

        public static void ExpandRangeToWordBoundaries(MarkupRange range)
        {
            //adjust the selection so that it entirely covers the first and last words.

            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
        }

        public bool IsCurrentWordUrlPart()
        {
            return IsRangeInUrl(currentWordRange);
        }

        public bool FilterApplies()
        {
            return filter != null && filter(currentWordRange);
        }
        public bool FilterAppliesRanged(int offset, int length)
        {
            MarkupRange adjustedRange = currentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(ref stagingTextRange, adjustedRange, offset, length);
            return filter != null && filter(adjustedRange);
        }

        /// <summary>
        /// Do we have another word in our range?
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return currentWordRange.End.IsLeftOf(selectionRange.End);
        }

        /// <summary>
        /// Advance to next word
        /// </summary>
        public void Next()
        {
            currentWordRange.End.MoveToPointer(currentVirtualPosition);

            do
            {
                //advance the start pointer to the beginning of next word
                if(!currentWordRange.End.IsEqualTo(selectionRange.Start)) //avoids skipping over the first word
                {
                    //fix bug 1848 - move the start to the end pointer before advancing to the next word
                    //this ensures that the "next begin" is after the current selection.
                    currentWordRange.Start.MoveToPointer(currentWordRange.End);
                    currentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);

                    currentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDBEGIN);
                }
                else
                {
                    //Special case for putting the start pointer at the beginning of the
                    //correct word when the selection may or may not be already adjacent
                    //to the the beginning of the word.
                    //Note: theoretically, the selection adjustment in the constructor
                    //guarantees that we will be flush against the first word, so we could
                    //probably do nothing, but it works, so we'll keep it.
                    currentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                    currentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                }

                //advance the end pointer to the end of next word
                currentWordRange.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);

                if(currentWordRange.Start.IsRightOf(currentWordRange.End))
                {
                    //Note: this was a condition that caused several bugs that caused us to stop
                    //spell-checking correctly, so this logic is here in case we still have edge
                    //cases that trigger it.
                    //This should not occur anymore since we fixed several causes of this
                    //condition by setting the currentWordRange gravity, and detecting case where
                    //the selection may or may-not start at the beginning of a word.
                    Debug.Fail("word start jumped past word end");

                    //if this occured, it was probably because start was already at the beginning
                    //of the correct word before it was moved.  To resolve this situation, we
                    //move the start pointer back to the beginning of the word that the end pointer
                    //is at. Since the End pointer always advances on each iteration, this should not
                    //cause an infinite loop. The worst case scenario is that we check the same word
                    //more than once.
                    currentWordRange.Start.MoveToPointer(currentWordRange.End);
                    currentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                }

            } while( MarkupHelpers.GetRangeTextFast(currentWordRange) == null &&
                     currentWordRange.End.IsLeftOf(selectionRange.End));

            currentVirtualPosition.MoveToPointer(currentWordRange.End);

            if(currentWordRange.End.IsRightOf(selectionRange.End))
            {
                //then collapse the range so that CurrentWord returns Empty;
                currentWordRange.Start.MoveToPointer(currentWordRange.End);
            }
            else
            {
                MarkupRange testRange = currentWordRange.Clone();
                testRange.Collapse(false);
                testRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_NEXTCHAR, selectionRange.End);
                if (MarkupHelpers.GetRangeHtmlFast(testRange) == ".")
                {
                    currentWordRange.End.MoveToPointer(testRange.End);
                }
            }
        }

        private bool IsRangeInUrl(MarkupRange range)
        {
            //must have this range cloned, otherwise in some cases IsInsideURL call
            // was MOVING the current word range! if "www.foo.com" was in the editor,
            // when the final "\"" was the current word, this call MOVED the current
            // word range BACK to www.foo.com, then nextWord would get "\"" and a loop
            // would occur (bug 411528)
            range = range.Clone();
            IMarkupPointer2Raw p2StartRaw = (IMarkupPointer2Raw)range.Start.PointerRaw;
            bool insideUrl;
            p2StartRaw.IsInsideURL(range.End.PointerRaw, out insideUrl);
            return insideUrl;
        }

        /// <summary>
        /// Get the text of the current word
        /// </summary>
        public string CurrentWord
        {
            get
            {
                return currentWordRange.Text ?? "";
            }
        }

        public void PlaceCursor()
        {
            currentWordRange.Collapse(false);
            currentWordRange.ToTextRange().select();
        }

        /// <summary>
        /// Highlight the current word
        /// </summary>
        public void Highlight(int offset, int length)
        {
            // select word
            MarkupRange highlightRange = currentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(highlightRange, offset, length);

            try
            {
                highlightRange.ToTextRange().select();
            }
            catch (COMException ce)
            {
                // Fix bug 772709: This error happens when we try to select un-selectable objects.
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                    throw;
            }
        }

        /// <summary>
        /// Remove highlighting from the document
        /// </summary>
        public void RemoveHighlight()
        {
            // clear document selection
            try
            {
                ((IHTMLDocument2) (htmlDocument)).selection.empty();
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                    throw;
            }
        }

        /// <summary>
        /// Replace the text of the current word
        /// </summary>
        public void Replace(int offset, int length, string newText)
        {
            MarkupRange origRange = currentWordRange.Clone();
            // set the text
            currentWordRange.Text = StringHelper.Replace(currentWordRange.Text, offset, length, newText);
            damageFunction(origRange);
        }

        /// <summary>
        /// Markup services for mshtml control
        /// </summary>
        private MshtmlMarkupServices MarkupServices
        {
            get { return markupServices; }
        }

        /// <summary>
        /// Control we are providing a word range for
        /// </summary>
        //private MshtmlControl mshtmlControl ;
        private IHTMLDocument htmlDocument;
        private MshtmlMarkupServices markupServices;

        /// <summary>
        /// Range over which we are iterating
        /// </summary>
        private MarkupRange selectionRange;

        public MarkupRange SelectionRange
        {
            get
            {
                return selectionRange;
            }
        }

        /// <summary>
        /// In order to fix the "vs." defect (trailing periods need to
        /// be included in spell checked words) we adjust the currentWordRange.End
        /// to include trailing periods. This has the effect of triggering
        /// the "word start jumped past word end" assert in some circumstances
        /// (try typing "foo.. bar"). The currentVirtualPosition tells us how to
        /// undo the currentWordRange.End adjustment right before attempting
        /// to navigate to the next word.
        /// </summary>
        private MarkupPointer currentVirtualPosition;

        private IHTMLTxtRange stagingTextRange;

        /// <summary>
        /// Pointer to current word
        /// </summary>
        private MarkupRange currentWordRange;

        public MarkupRange CurrentWordRange
        {
            get
            {
                return currentWordRange;
            }
        }
    }
}
