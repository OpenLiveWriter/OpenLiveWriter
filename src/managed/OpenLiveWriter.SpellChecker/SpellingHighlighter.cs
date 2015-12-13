// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    public class SpellingHighlighter : IDisposable
    {

        static int TIMER_INTERVAL = 10;
        static int NUMBER_OF_WORDS_TO_CHECK = 30;

        private ISpellingChecker _spellingChecker ;

        private IHighlightRenderingServicesRaw _highlightRenderingServices;

        private IDisplayServicesRaw _displayServices;

        private IMarkupServicesRaw _markupServicesRaw;

        private MshtmlMarkupServices _markupServices;

        private IHTMLDocument4 _htmlDocument;

        private HighlightSegmentTracker _tracker;

        private Queue _workerQueue;

        private SpellingTimer _timer;

        private bool _fatalSpellingError = false ;

        public SpellingHighlighter(ISpellingChecker spellingChecker, IHighlightRenderingServicesRaw highlightRenderingServices,
                                   IDisplayServicesRaw displayServices, IMarkupServicesRaw markupServices, IHTMLDocument4 htmlDocument)
        {
            _spellingChecker = spellingChecker;
            _highlightRenderingServices = highlightRenderingServices;
            _displayServices = displayServices;
            _markupServicesRaw = markupServices;
            _markupServices = new MshtmlMarkupServices(_markupServicesRaw);
            _htmlDocument = htmlDocument;
            _tracker = new HighlightSegmentTracker();
            //the timer to handle interleaving of spell ghecking
            _timer = new SpellingTimer(TIMER_INTERVAL);
            _timer.Start();
            _timer.Tick += new EventHandler(_timer_Tick);
            _workerQueue = new Queue();
        }

        /// <summary>
        /// Check spelling--called by the damage handler
        /// </summary>
        public void CheckSpelling(MshtmlWordRange range)
        {
            if ( !_fatalSpellingError )
            {
                _timer.Enabled = true;
                _workerQueue.Enqueue(range);
                if (_workerQueue.Count == 1)
                {
                    //if the queue had been empty, process this range right away
                    DoWork();
                }
            }
        }

        /// <summary>
        /// Prevents asserts that happen within _timer_Tick from going bonkers; since
        /// they can happen reentrantly, you can end up with hundreds of assert windows.
        /// </summary>
        private bool reentrant = false;

        private void _timer_Tick(object o, EventArgs args)
        {
            if (reentrant)
                return;
            reentrant = true;

            try
            {
                if (_workerQueue.Count > 0)
                {
                    DoWork();
                }
                else
                    _timer.Enabled = false;
            }
            catch(Exception ex)
            {
                UnexpectedErrorMessage.Show(Win32WindowImpl.ForegroundWin32Window, ex, "Unexpected Error Spell Checking");

                Reset() ;

                _fatalSpellingError = true ;
            }
            finally
            {
                reentrant = false;
            }
        }

        //manages the queue during work
        private void DoWork()
        {
            {
                //start processing the first word range, and pop it if we get to the end
                if (ProcessWordRange((MshtmlWordRange)_workerQueue.Peek()))
                    _workerQueue.Dequeue();
            }
        }

        //iterates through a word range checking for spelling errors
        //return: whether the word range is finished (true) or not
        private bool ProcessWordRange(MshtmlWordRange wordRange)
        {
            if (wordRange.CurrentWordRange.Positioned)
            {
                //track where we will need to clear;
                MarkupPointer start = _markupServices.CreateMarkupPointer();
                start.MoveToPointer(wordRange.CurrentWordRange.End);
                ArrayList highlightwords = new ArrayList(NUMBER_OF_WORDS_TO_CHECK);

                int i = 0;
                //to do....the word range is losing its place when it stays in the queue
                while (wordRange.HasNext() && i < NUMBER_OF_WORDS_TO_CHECK )
                {
                    // advance to the next word
                    wordRange.Next() ;
                    // check the spelling
                    int offset, length;
                    if (ProcessWord(wordRange, out offset, out length))
                    {
                        MarkupRange highlightRange = wordRange.CurrentWordRange.Clone();
                        MarkupHelpers.AdjustMarkupRange(ref stagingTextRange, highlightRange, offset, length);

                        //note: cannot just push the current word range here, as it moves before we get to the highlighting step
                        highlightwords.Add(highlightRange);
                    }
                    i++;
                }
                MarkupPointer end = wordRange.CurrentWordRange.End;

                //got our words, clear the checked range and then add the misspellings
                ClearRange(start, end);
                foreach (MarkupRange word in highlightwords)
                {
                    HighlightWordRange(word);
                }

                return !wordRange.HasNext();
            }
            else
                return true;
        }

        //takes one the first word on the range, and checks it for spelling errors
        //***returns true if word is misspelled***
        private bool ProcessWord(MshtmlWordRange word, out int offset, out int length)
        {
            offset = 0;
            length = 0;

            string otherWord = null;
            SpellCheckResult result;
            string currentWord = word.CurrentWord;
            if (!word.IsCurrentWordUrlPart() && !WordRangeHelper.ContainsOnlySymbols(currentWord))
                result = _spellingChecker.CheckWord( currentWord, out otherWord, out offset, out length ) ;
            else
                result = SpellCheckResult.Correct;

            if (result != SpellCheckResult.Correct)
            {
                //note: currently using this to not show any errors in smart content, since the fix isn't
                // propogated to the underlying data structure
                if (!word.FilterApplies())
                {
                    return true;
                }
            }
            return false;
        }

        private bool ProcessWord(string word)
        {
            int offset, length;
            string otherWord ;
            SpellCheckResult result;
            result = _spellingChecker.CheckWord( word, out otherWord, out offset, out length ) ;
            if (result == SpellCheckResult.Correct)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Highlight the current word range
        /// </summary>
        ///
        private IHTMLRenderStyle _highlightWordStyle;
        IHTMLRenderStyle HighlightWordStyle
        {
            get
            {
                if(_highlightWordStyle == null)
                {
                    _highlightWordStyle = _htmlDocument.createRenderStyle(null);
                    _highlightWordStyle.defaultTextSelection = "false";
                    _highlightWordStyle.textDecoration = "underline";
                    _highlightWordStyle.textUnderlineStyle = "wave";
                    _highlightWordStyle.textDecorationColor = "red";
                    _highlightWordStyle.textBackgroundColor = "transparent";
                    _highlightWordStyle.textColor = "transparent";
                }
                return _highlightWordStyle;
            }
        }

        private void HighlightWordRange(MarkupRange word)
        {
            try
            {
                IHighlightSegmentRaw segment;
                IDisplayPointerRaw start;
                IDisplayPointerRaw end;
                _displayServices.CreateDisplayPointer(out start);
                _displayServices.CreateDisplayPointer(out end);
                DisplayServices.TraceMoveToMarkupPointer(start, word.Start);
                DisplayServices.TraceMoveToMarkupPointer(end, word.End);

                _highlightRenderingServices.AddSegment(start, end, HighlightWordStyle, out segment);
                _tracker.AddSegment(segment,
                    MarkupHelpers.UseStagingTextRange(ref stagingTextRange, word, rng => rng.text),
                    _markupServicesRaw);
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode == unchecked((int)0x800A025E))
                    return;
                throw;
            }
        }

        private IHTMLTxtRange stagingTextRange;

        //remove any covered segments from the tracker and clear their highlights
        public void ClearRange(MarkupPointer start, MarkupPointer end)
        {
            IHighlightSegmentRaw[] segments =
                _tracker.GetSegments(start.PointerRaw, end.PointerRaw);
            if (segments != null)
            {
                for (int i = 0; i < segments.Length; i++)
                {
                    _highlightRenderingServices.RemoveSegment(segments[i]);
                }
            }
        }

        //remove all misspellings from tracker and clear their highlights
        //used when turning spell checking on and off
        public void Reset()
        {
            _timer.Enabled = false;
            stagingTextRange = null;
            _workerQueue.Clear();
            IHighlightSegmentRaw[] allWords = _tracker.ClearAllSegments();
            for (int i = 0; i < allWords.Length; i++)
            {
                _highlightRenderingServices.RemoveSegment(allWords[i]);
            }
        }

        //used for ignore all, add to dictionary to remove highlights from new word
        public void UnhighlightWord(string word)
        {
            HighlightSegmentTracker.MatchingSegment[] relevantHighlights = _tracker.GetSegments(word, new HighlightSegmentTracker.CheckWordSpelling(ProcessWord));
            for (int i = 0; i < relevantHighlights.Length; i++)
            {
                HighlightSegmentTracker.MatchingSegment segment = relevantHighlights[i];
                _highlightRenderingServices.RemoveSegment(segment._segment);
                _tracker.RemoveSegment(segment._pointer);
            }
        }

        public MisspelledWordInfo FindMisspelling(MarkupPointer markupPointer)
        {
            return _tracker.FindSegment(_markupServices, markupPointer.PointerRaw);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _timer.Stop();
            if (_tracker != null)
                _tracker = null;
            if (_workerQueue != null)
                _workerQueue = null;
        }

        #endregion

        public void UnhighlightRange(MarkupRange range)
        {
            IHighlightSegmentRaw[] segments = _tracker.GetSegments(range.Start.PointerRaw, range.End.PointerRaw);

            if (segments == null)
            {
                // This can happen when realtime spell checking is disabled
                return;
            }

            foreach (IHighlightSegmentRaw segment in segments)
                _highlightRenderingServices.RemoveSegment(segment);
        }
    }
}
