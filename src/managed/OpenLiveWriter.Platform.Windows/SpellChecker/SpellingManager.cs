// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    public delegate void ReplaceWord(MarkupPointer start, MarkupPointer end, string newWord);

    /// <summary>
    /// Summary description for SpellingManager.
    /// </summary>
    public class SpellingManager : IDisposable
    {
        private ISpellingChecker _spellingChecker;
        private IBlogPostSpellCheckingContext _spellingContext;
        private MshtmlControl _mshtmlControl;
        private IHTMLDocument2 _htmlDocument;
        private SpellingHighlighter _spellingHighlighter;
        private ReplaceWord _replaceWordFunction;
        private MarkupRangeFilter _filter;
        private DamageFunction _damageFunction;

        public CommandManager CommandManager
        {
            get;
            private set;
        }

        private readonly SortedMarkupRangeList _ignoredOnce = new SortedMarkupRangeList();

        public SpellingManager(CommandManager commandManager)
        {
            CommandManager = commandManager;
            InitializeCommands();
        }

        public void Initialize(ISpellingChecker spellingChecker, MshtmlControl mshtmlControl,
            IHTMLDocument2 htmlDocument, ReplaceWord replaceWordFunction, MarkupRangeFilter filter, DamageFunction damageFunction)
        {
            _spellingChecker = spellingChecker;
            _mshtmlControl = mshtmlControl;
            _htmlDocument = htmlDocument;
            _filter = filter;
            _replaceWordFunction = replaceWordFunction;
            _damageFunction = damageFunction;
        }

        public void InitializeSession(IBlogPostSpellCheckingContext spellingContext)
        {
            _ignoredOnce.Clear();
            _spellingContext = spellingContext;
            _spellingHighlighter = new SpellingHighlighter(SpellingChecker, _mshtmlControl.HighlightRenderingServices,
                _mshtmlControl.DisplayServices,
                _mshtmlControl.MarkupServicesRaw,
                (IHTMLDocument4)_htmlDocument);
            //start new highlighter
            _spellingChecker.StartChecking();
            _spellingChecker.WordIgnored += new EventHandler(_spellingChecker_WordIgnored);
            _spellingChecker.WordAdded += new EventHandler(_spellingChecker_WordAdded);

        }

        public void StartSession()
        {
            Debug.Assert(_spellingContext.CanSpellCheck, "Starting spelling session when spelling is disabled!");

            HighlightSpelling();
        }

        public void StopSession(bool hardReset)
        {
            //clear tracker
            //clear work
            _spellingHighlighter.Reset();
            if (hardReset)
            {
                _ignoredOnce.Clear();
                _spellingChecker.StopChecking();
                _spellingChecker.WordIgnored -= new EventHandler(_spellingChecker_WordIgnored);
                _spellingChecker.WordAdded -= new EventHandler(_spellingChecker_WordAdded);
            }
            //clear listeners
        }

        /// <summary>
        /// Check the spelling of the entire document
        /// </summary>
        /// <returns></returns>
        public void HighlightSpelling()
        {
            HighlightSpelling(null);
        }

        public void HighlightSpelling(MarkupRange range)
        {
            // check spelling
            MshtmlWordRange wordRange;
            if (range == null) //check the whole document.
            {
                wordRange = new MshtmlWordRange(_htmlDocument, false, _filter, _damageFunction);
            }
            else
            {
                //range is invalid for some reason--damage committed while switching views, getting it later on the timer
                if (!range.Positioned)
                    return;
                else if (range.Text == null || String.IsNullOrEmpty(range.Text.Trim()))
                {
                    //empty range--on a delete for instance, just clear
                    _spellingHighlighter.ClearRange(range.Start, range.End);
                    _ignoredOnce.ClearRange(range);
                    return;
                }
                else
                {
                    MarkupRange origRange = range.Clone();
                    //here are the words to check
                    wordRange = new MshtmlWordRange(_htmlDocument, range, _filter, _damageFunction);
                    //check for emptiness at start and end, clear those
                    _spellingHighlighter.ClearRange(origRange.Start, range.Start);
                    _spellingHighlighter.ClearRange(range.End, origRange.End);

                    _ignoredOnce.ClearRange(range);
                }
            }

            _spellingHighlighter.CheckSpelling(wordRange);
        }

        //used to remove all misspellings, when turning realtime spell checking on/off
        public void ClearAll()
        {
            _spellingHighlighter.Reset();
        }

        public MisspelledWordInfo FindMisspelling(MarkupPointer markupPointer)
        {
            return _spellingHighlighter.FindMisspelling(markupPointer);
        }

        public void UpdateSpellingContext(object sender, EventArgs e)
        {
            IBlogPostSpellCheckingContext context = sender as IBlogPostSpellCheckingContext;
            _spellingContext = context;
            StopSession(true);
            InitializeSession(_spellingContext);
        }

        private void InitializeCommands()
        {
            CommandManager.BeginUpdate();

            Command commandAddToDictionary = new Command(CommandId.AddToDictionary);
            commandAddToDictionary.Execute += new EventHandler(addToDictionary_Execute);
            CommandManager.Add(commandAddToDictionary);

            Command commandIgnoreAll = new Command(CommandId.IgnoreAll);
            commandIgnoreAll.Execute += new EventHandler(ignoreAllCommand_Execute);

            CommandManager.Add(commandIgnoreAll);
            CommandManager.Add(CommandId.IgnoreOnce, ignoreOnceCommand_Execute);

            Command commandOpenSpellingForm = new Command(CommandId.OpenSpellingForm);
            commandOpenSpellingForm.Execute += new EventHandler(openSpellingForm_Execute);
            CommandManager.Add(commandOpenSpellingForm);

            CommandManager.EndUpdate();
        }

        public ISpellingChecker SpellingChecker
        {
            get
            {
                return _spellingChecker;
            }
        }

        //return the context menu definition
        public SpellCheckingContextMenuDefinition CreateSpellCheckingContextMenu(MisspelledWordInfo word)
        {
            _currentWordInfo = word;
            return new SpellCheckingContextMenuDefinition(_currentWordInfo.Word, this);
        }

        private MisspelledWordInfo _currentWordInfo;

        //handlers for various commands
        public void fixSpellingApplyCommand_Execute(object sender, EventArgs e)
        {
            Command command = (Command)sender;
            _replaceWordFunction(_currentWordInfo.WordRange.Start, _currentWordInfo.WordRange.End, command.Tag as string);
        }

        private void ignoreOnceCommand_Execute(object sender, EventArgs e)
        {
            IgnoreCore(_currentWordInfo.WordRange);
        }

        private void IgnoreCore(MarkupRange range)
        {
            _spellingHighlighter.UnhighlightRange(range);

            // Win Live 182705: Assert when ignoring misspelled words in the album title very quickly
            // It is possible to get multiple "ignores" queue up before we've been able to process them.
            if (!_ignoredOnce.Contains(range))
            {
                _ignoredOnce.Add(range);
            }
        }

        private void ignoreAllCommand_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                _spellingChecker.IgnoreAll(_currentWordInfo.Word);
            }
        }

        private void _spellingChecker_WordIgnored(object sender, EventArgs e)
        {
            _spellingHighlighter.UnhighlightWord((string)sender);
        }

        private void addToDictionary_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                _spellingChecker.AddToUserDictionary(_currentWordInfo.Word);
            }
        }

        private void _spellingChecker_WordAdded(object sender, EventArgs e)
        {
            _spellingHighlighter.UnhighlightWord((string)sender);
        }

        private void openSpellingForm_Execute(object sender, EventArgs e)
        {
            _mshtmlControl.MarkupServices.BeginUndoUnit(Guid.NewGuid().ToString());

            bool supportsIgnoreOnce = false;
            Command cmdIgnoreOnce = CommandManager.Get(CommandId.IgnoreOnce);
            if (cmdIgnoreOnce != null && cmdIgnoreOnce.On)
                supportsIgnoreOnce = true;

            // must first force the control to lose focus so that it doesn't "lose"
            // the selection when the dialog opens
            IntPtr hPrevious = User32.SetFocus(IntPtr.Zero);

            using (SpellCheckerForm spellCheckerForm = new SpellCheckerForm(SpellingChecker, _mshtmlControl.FindForm(), supportsIgnoreOnce))
            {
                //  center the spell-checking form over the document body
                spellCheckerForm.StartPosition = FormStartPosition.CenterParent;

                // WinLive 263320: We want to check from the current word to the end of the document.
                // TODO: Although this works fine, it would be better to only spellcheck inside editable regions.
                MarkupRange rangeToSpellCheck = _currentWordInfo.WordRange.Clone();
                rangeToSpellCheck.End.MoveAdjacentToElement(_htmlDocument.body, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);

                // get the word range to check
                MshtmlWordRange wordRange = new MshtmlWordRange(_htmlDocument, rangeToSpellCheck, _filter, _damageFunction);

                spellCheckerForm.WordIgnored += (sender2, args) => IgnoreOnce(wordRange.CurrentWordRange);

                // check spelling
                spellCheckerForm.CheckSpelling(wordRange);
                _mshtmlControl.MarkupServices.EndUndoUnit();

                // restore focus to the control that had it before we spell-checked
                User32.SetFocus(hPrevious);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_spellingHighlighter != null)
            {
                _spellingChecker.StopChecking();
                _spellingChecker.WordIgnored -= new EventHandler(_spellingChecker_WordIgnored);
                _spellingChecker.WordAdded -= new EventHandler(_spellingChecker_WordAdded);
                _spellingHighlighter.Dispose();
            }
        }

        #endregion

        public bool IsWordIgnored(MarkupRange range)
        {
            return _ignoredOnce.Contains(range);
        }

        public void IgnoreOnce(MarkupRange range)
        {
            IgnoreCore(range);
        }

        public bool IsIgnoreOnceEnabled
        {
            get
            {
                Command cmdIgnoreOnce = CommandManager.Get(CommandId.IgnoreOnce);
                if (cmdIgnoreOnce != null && cmdIgnoreOnce.On && cmdIgnoreOnce.Enabled)
                    return true;

                return false;
            }
            set
            {
                Command cmdIgnoreOnce = CommandManager.Get(CommandId.IgnoreOnce);
                if (cmdIgnoreOnce != null)
                {
                    cmdIgnoreOnce.Enabled = value;
                }
            }
        }

        public bool IsInIgnoredWord(MarkupPointer p)
        {
            return _ignoredOnce.Contains(p);
        }

        public void ClearIgnoreOnce()
        {
            _ignoredOnce.Clear();
        }

        public void DamagedRange(MarkupRange range, bool doSpellCheck)
        {
            if (doSpellCheck)
            {
                HighlightSpelling(range);
            }
            else
            {
                if (range.Text != null)
                    MshtmlWordRange.ExpandRangeToWordBoundaries(range);
                _ignoredOnce.ClearRange(range);
            }
        }
    }
}
