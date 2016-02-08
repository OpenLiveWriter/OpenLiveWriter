// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{

    /// <summary>
    /// Summary description for SpellCheckerForm.
    /// </summary>
    public class SpellCheckerForm : BaseForm
    {
        private readonly IWin32Window owner;
        private Label labelChangeTo;
        private TextBox textBoxChangeTo;
        private Label labelSuggestions;
        private ListBox listBoxSuggestions;
        private Button buttonIgnore;
        private Button buttonIgnoreAll;
        private Button buttonChange;
        private Button buttonAdd;
        private Button buttonChangeAll;
        private Label labelNotInDictionary;
        private Button buttonCancel;
        private Label labelWord;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public event EventHandler WordIgnored;

        /// <summary>
        /// Initialize spell-checker form
        /// </summary>
        public SpellCheckerForm(ISpellingChecker spellingChecker, IWin32Window owner)
            : this(spellingChecker, owner, false)
        {
        }

        /// <summary>
        /// Initialize spell-checker form
        /// </summary>
        public SpellCheckerForm(ISpellingChecker spellingChecker, IWin32Window owner, bool provideIgnoreOnce)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            buttonAdd.Text = Res.Get(StringId.SpellAdd);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            labelNotInDictionary.Text = Res.Get(StringId.SpellNotInDict);
            labelChangeTo.Text = Res.Get(StringId.SpellChange);
            labelSuggestions.Text = Res.Get(StringId.SpellOptions);
            buttonIgnore.Text = Res.Get(StringId.SpellIgnore);
            buttonIgnoreAll.Text = Res.Get(StringId.SpellIgnoreAll);
            buttonChangeAll.Text = Res.Get(StringId.SpellChangeAll);
            buttonChange.Text = Res.Get(StringId.SpellChangeWord);
            Text = Res.Get(StringId.SpellText);

            // if we aren't providing an ignore-once option then Ignore now means
            // Ignore All (simpler for users). To effect this we need to eliminate
            // the "Ignore" button, rename the "Ignore All" button to "Ignore", and
            // move around other controls as appropriate
            if (!provideIgnoreOnce)
            {
                // hide Ignore button and replace it with Ignore All button
                // renamed as generic "Ignore"
                buttonIgnore.Visible = false;

                // fixup UI by moving around buttons
                buttonIgnoreAll.Top = buttonIgnore.Top;
                int offset = buttonChange.Top - textBoxChangeTo.Top + 1;
                buttonChange.Top -= offset;
                buttonChangeAll.Top -= offset;
                buttonAdd.Top -= offset;
            }

            // keep reference to spell-checking interface
            this.spellingChecker = spellingChecker;
            this.owner = owner;
        }

        /// <summary>
        /// Check spelling
        /// </summary>
        /// <param name="range">word range to check</param>
        public void CheckSpelling(IWordRange range)
        {
            // save reference to word-range
            wordRange = range;

            // initialize flags
            completed = false;
            wordRangeHighlightPending = false;

            // enter the spell-checking loop (if there are no mispelled words
            // then the form will never show)
            ContinueSpellCheck();
        }

        /// <summary>
        /// Was the spell check completed?
        /// </summary>
        public bool Completed
        {
            get
            {
                return completed;
            }
        }

        private int offset;
        private int length;

        /// <summary>
        /// Continue the spell-checking loop
        /// </summary>
        private void ContinueSpellCheck()
        {
            // provide feedback (pump events so underlying control has an
            // opportunity to update its display)
            RemoveHighlight();
            Application.DoEvents();

            if (!spellingChecker.IsInitialized)
            {
                Trace.Fail("Spellchecker was uninitialized in the middle of spellchecking after removing highlight.");
                return;
            }

            using (new WaitCursor())
            {
                // loop through all of the words in the word-range
                bool currentWordMisspelled = false;
                while (wordRange.HasNext())
                {
                    // advance to the next word
                    wordRange.Next();

                    // check the spelling
                    string otherWord = null;
                    offset = 0;
                    length = wordRange.CurrentWord.Length;

                    SpellCheckResult result;
                    string CurrentWord = wordRange.CurrentWord;
                    if (!wordRange.IsCurrentWordUrlPart() && !WordRangeHelper.ContainsOnlySymbols(CurrentWord))
                        result = spellingChecker.CheckWord(CurrentWord, out otherWord, out offset, out length);
                    else
                        result = SpellCheckResult.Correct;

                    //note: currently using this to not show any errors in smart content, since the fix isn't
                    // propogated to the underlying data structure
                    if (result != SpellCheckResult.Correct && !wordRange.FilterAppliesRanged(offset, length))
                    {
                        // auto-replace
                        if (result == SpellCheckResult.AutoReplace)
                        {
                            // replace word and continue loop (pump events so the document
                            // is updated w/ the new word)
                            wordRange.Replace(offset, length, otherWord);
                            Application.DoEvents();

                            if (!spellingChecker.IsInitialized)
                            {
                                Trace.Fail("Spellchecker was uninitialized in the middle of spellchecking after auto-replace.");
                                return;
                            }
                        }
                        // some other incorrect word
                        else if (result != SpellCheckResult.Correct)
                        {
                            string misspelledWord = wordRange.CurrentWord;
                            if (offset > 0 && offset <= misspelledWord.Length)
                                misspelledWord = misspelledWord.Substring(offset);
                            if (length < misspelledWord.Length)
                                misspelledWord = misspelledWord.Substring(0, length);

                            // highlight the misspelled word
                            HighlightWordRange();

                            // set current misspelled word
                            labelWord.Text = misspelledWord;

                            // misspelling or incorrect capitalization
                            if (result == SpellCheckResult.Misspelled)
                            {
                                labelNotInDictionary.Text = NOT_IN_DICTIONARY;
                                ProvideSuggestions(misspelledWord);
                            }
                            else if (result == SpellCheckResult.Capitalization)
                            {
                                labelNotInDictionary.Text = CAPITALIZATION;
                                ProvideSuggestions(misspelledWord, 1);
                            }
                            // conditional replace
                            else if (result == SpellCheckResult.ConditionalReplace)
                            {
                                labelNotInDictionary.Text = NOT_IN_DICTIONARY;
                                ProvideConditionalReplaceSuggestion(otherWord);
                            }

                            // update state and break out of the loop
                            currentWordMisspelled = true;
                            break;
                        }
                    }
                }

                // there is a pending misspelling, make sure the form is visible
                if (currentWordMisspelled)
                {
                    EnsureFormVisible();
                }
                // current word not misspelled and no more words, spell check is finished
                else if (!wordRange.HasNext())
                {
                    completed = true;
                    EndSpellCheck();
                }
            }
        }

        /// <summary>
        /// Fina!
        /// </summary>
        private void EndSpellCheck()
        {
            if (Visible)
            {
                // close the form
                Close();
            }
            else
            {
                // if form never became visible make sure we reset
                ResetSpellingState();
            }
        }

        /// <summary>
        /// Cleanup when the form closes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // call base
            base.OnClosed(e);

            // reset spelling state
            ResetSpellingState();
            wordRange.PlaceCursor();
        }

        void RemoveHighlight()
        {
            try
            {
                wordRange.RemoveHighlight();
            }
            catch (COMException e)
            {
                // WinLive 263776: Mail returns E_FAIL if the SubjectEdit element cannot be found.
                // We should be able to recover from that.
                if (e.ErrorCode != HRESULT.E_FAILED)
                    throw;
            }
        }

        /// <summary>
        /// Reset the spelling state
        /// </summary>
        private void ResetSpellingState()
        {
            // remove feedback from the document
            RemoveHighlight();
        }

        /// <summary>
        /// Provide suggestions for the current misspelled word
        /// </summary>
        /// <param name="word">word to provide suggestions for</param>
        private void ProvideSuggestions(string word)
        {
            ProvideSuggestions(word, DEFAULT_MAX_SUGGESTIONS);
        }

        /// <summary>
        /// Provide suggestions for the current misspelled word
        /// </summary>
        /// <param name="word">word to provide suggestions for</param>
        /// <param name="maxSuggestions">maximum number of suggestions to provide</param>
        private void ProvideSuggestions(string word, short maxSuggestions)
        {
            // clear the existing suggestions
            listBoxSuggestions.Items.Clear();
            textBoxChangeTo.Clear();

            // retrieve suggestions
            SpellingSuggestion[] suggestions = spellingChecker.Suggest(word, maxSuggestions, SUGGESTION_DEPTH);

            // provide suggestions
            if (suggestions.Length > 0)
            {
                // add suggestions to list (stop adding when the quality of scores
                // declines precipitously)
                short lastScore = suggestions[0].Score;
                foreach (SpellingSuggestion suggestion in suggestions)
                {
                    if ((lastScore - suggestion.Score) < SCORE_GAP_FILTER && (suggestion.Suggestion != null))
                        listBoxSuggestions.Items.Add(suggestion.Suggestion);
                    else
                        break;

                    // update last score
                    lastScore = suggestion.Score;
                }
            }

            if (listBoxSuggestions.Items.Count == 0)
            {
                listBoxSuggestions.Items.Add(Res.Get(StringId.SpellNoSuggest));
                listBoxSuggestions.Enabled = false;
                buttonChange.Enabled = false;
                buttonChangeAll.Enabled = false;
            }
            else
            {
                // select first item
                listBoxSuggestions.SelectedIndex = 0;
                listBoxSuggestions.Enabled = true;
                buttonChange.Enabled = true;
                buttonChangeAll.Enabled = true;

            }

            // select and focus change-to
            textBoxChangeTo.SelectAll();
            textBoxChangeTo.Focus();
        }

        /// <summary>
        /// Provide a suggestion containing a single word
        /// </summary>
        /// <param name="suggestedWord">suggested word</param>
        private void ProvideConditionalReplaceSuggestion(string suggestedWord)
        {
            // set contents of list box to the specified word
            listBoxSuggestions.Items.Clear();
            listBoxSuggestions.Items.Add(suggestedWord);
            listBoxSuggestions.SelectedIndex = 0;

            // select and focus change-to
            textBoxChangeTo.SelectAll();
            textBoxChangeTo.Focus();
        }

        /// <summary>
        /// Handle Ignore button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            if (WordIgnored != null)
                WordIgnored(this, EventArgs.Empty);
            // continue spell checking
            ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Ignore All button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonIgnoreAll_Click(object sender, EventArgs e)
        {
            // notify engine that we want to ignore all instances of this word
            spellingChecker.IgnoreAll(labelWord.Text);

            // continue spell checking
            ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Change button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonChange_Click(object sender, EventArgs e)
        {
            DoChange();
        }

        /// <summary>
        /// Handle Change All button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonChangeAll_Click(object sender, EventArgs e)
        {
            // replace the word
            wordRange.Replace(offset, length, textBoxChangeTo.Text);

            // notify spell checker that we want to replace all instances of this word
            spellingChecker.ReplaceAll(labelWord.Text, textBoxChangeTo.Text);

            // continue spell checking
            ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Add button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // add this word to the user dictioanry
            spellingChecker.AddToUserDictionary(labelWord.Text);

            // continue spell checking
            ContinueSpellCheck();
        }

        /// <summary>
        /// Handle TextChanged event to update state of buttons
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void textBoxChangeTo_TextChanged(object sender, EventArgs e)
        {
            if (textBoxChangeTo.Text != String.Empty)
            {
                buttonChange.Enabled = true;
                buttonChangeAll.Enabled = true;
            }
            else // no text, can't change to
            {
                buttonChange.Enabled = false;
                buttonChangeAll.Enabled = false;
            }
        }

        /// <summary>
        /// Update ChangeTo text box when the selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxSuggestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            // update contents of change to text box
            if (listBoxSuggestions.SelectedIndex != -1)
                textBoxChangeTo.Text = listBoxSuggestions.SelectedItem as string;
            else
                textBoxChangeTo.Text = String.Empty;
        }

        /// <summary>
        /// Double-click of a word in suggestions results in auto-replacement
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void listBoxSuggestions_DoubleClick(object sender, EventArgs e)
        {
            // update change-to
            textBoxChangeTo.Text = listBoxSuggestions.SelectedItem as string;

            // execute the change
            DoChange();
        }

        /// <summary>
        /// Handle Done button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            EndSpellCheck();
        }

        /// <summary>
        /// Handle Options button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonOptions_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Execute the change
        /// </summary>
        private void DoChange()
        {
            // replace the word
            wordRange.Replace(offset, length, textBoxChangeTo.Text);

            // continue spell checking
            ContinueSpellCheck();
        }

        /// <summary>
        /// Highlight the current word range (delays the highlight if the form is not yet visible)
        /// </summary>
        private void HighlightWordRange()
        {
            if (Visible)
                wordRange.Highlight(offset, length);
            else
                wordRangeHighlightPending = true;
        }

        /// <summary>
        /// Ensure the form is loaded
        /// </summary>
        private void EnsureFormVisible()
        {
            if (!Visible)
                ShowDialog(owner);
        }

        /// <summary>
        /// Override on-load
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnLoad(EventArgs e)
        {
            /*
                        int originalWidth = buttonIgnore.Width;
                        int maxWidth = originalWidth;
                        foreach (Button button in buttons)
                            if (button.Visible)
                                maxWidth = Math.Max(maxWidth, DisplayHelper.AutoFitSystemButton(button));
                        foreach (Button button in buttons)
                            if (button.Visible)
                                button.Width = maxWidth;
                        Width += maxWidth - originalWidth;
            */

            // call base
            base.OnLoad(e);

            if (wordRangeHighlightPending)
            {
                wordRange.Highlight(offset, length);
                wordRangeHighlightPending = false;
            }

            Button[] buttons = {
                                   buttonIgnore,
                                   buttonIgnoreAll,
                                   buttonChange,
                                   buttonChangeAll,
                                   buttonAdd,
                                   buttonCancel
                               };

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                listBoxSuggestions.Height = buttonCancel.Bottom - listBoxSuggestions.Top;

                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left,
                    buttonIgnore.Width,
                    int.MaxValue,
                    buttons);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Word range to check
        /// </summary>
        private IWordRange wordRange;

        /// <summary>
        /// Spelling checker used by the form
        /// </summary>
        private ISpellingChecker spellingChecker;

        /// <summary>
        /// Is there a word-range highlight pending the showing of the form?
        /// </summary>
        private bool wordRangeHighlightPending = false;

        /// <summary>
        /// Indicates whether the spell-check was completed
        /// </summary>
        private bool completed = false;

        /// <summary>
        /// Default maximum suggestions to return
        /// </summary>
        private const short DEFAULT_MAX_SUGGESTIONS = 10;

        /// <summary>
        /// If we detect a gap between scores of this value or greater then
        /// we drop the score and all remaining
        /// </summary>
        private const short SCORE_GAP_FILTER = 20;

        /// <summary>
        /// Suggestion depth for searching (100 is the maximum)
        /// </summary>
        private const short SUGGESTION_DEPTH = 80;

        /// <summary>
        /// Spelling prompts
        /// </summary>
        private readonly string NOT_IN_DICTIONARY = Res.Get(StringId.SpellNotInDict);
        private readonly string CAPITALIZATION = Res.Get(StringId.SpellCaps);

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelNotInDictionary = new System.Windows.Forms.Label();
            this.labelChangeTo = new System.Windows.Forms.Label();
            this.textBoxChangeTo = new System.Windows.Forms.TextBox();
            this.labelSuggestions = new System.Windows.Forms.Label();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.buttonIgnore = new System.Windows.Forms.Button();
            this.buttonIgnoreAll = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labelWord = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // labelNotInDictionary
            //
            this.labelNotInDictionary.AutoSize = true;
            this.labelNotInDictionary.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelNotInDictionary.Location = new System.Drawing.Point(8, 12);
            this.labelNotInDictionary.Name = "labelNotInDictionary";
            this.labelNotInDictionary.Size = new System.Drawing.Size(90, 17);
            this.labelNotInDictionary.TabIndex = 0;
            this.labelNotInDictionary.Text = "&Not in dictionary:";
            //
            // labelChangeTo
            //
            this.labelChangeTo.AutoSize = true;
            this.labelChangeTo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelChangeTo.Location = new System.Drawing.Point(8, 55);
            this.labelChangeTo.Name = "labelChangeTo";
            this.labelChangeTo.Size = new System.Drawing.Size(59, 17);
            this.labelChangeTo.TabIndex = 2;
            this.labelChangeTo.Text = "C&hange to:";
            //
            // textBoxChangeTo
            //
            this.textBoxChangeTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChangeTo.Location = new System.Drawing.Point(8, 71);
            this.textBoxChangeTo.MaxLength = 100;
            this.textBoxChangeTo.Name = "textBoxChangeTo";
            this.textBoxChangeTo.Size = new System.Drawing.Size(282, 21);
            this.textBoxChangeTo.TabIndex = 3;
            this.textBoxChangeTo.Text = "";
            this.textBoxChangeTo.TextChanged += new System.EventHandler(this.textBoxChangeTo_TextChanged);
            //
            // labelSuggestions
            //
            this.labelSuggestions.AutoSize = true;
            this.labelSuggestions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSuggestions.Location = new System.Drawing.Point(9, 99);
            this.labelSuggestions.Name = "labelSuggestions";
            this.labelSuggestions.Size = new System.Drawing.Size(68, 17);
            this.labelSuggestions.TabIndex = 4;
            this.labelSuggestions.Text = "S&uggestions:";
            //
            // listBoxSuggestions
            //
            this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSuggestions.IntegralHeight = false;
            this.listBoxSuggestions.Location = new System.Drawing.Point(8, 115);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(282, 95);
            this.listBoxSuggestions.TabIndex = 5;
            this.listBoxSuggestions.DoubleClick += new System.EventHandler(this.listBoxSuggestions_DoubleClick);
            this.listBoxSuggestions.SelectedIndexChanged += new System.EventHandler(this.listBoxSuggestions_SelectedIndexChanged);
            //
            // buttonIgnore
            //
            this.buttonIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIgnore.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonIgnore.Location = new System.Drawing.Point(298, 27);
            this.buttonIgnore.Name = "buttonIgnore";
            this.buttonIgnore.TabIndex = 6;
            this.buttonIgnore.Text = "I&gnore";
            this.buttonIgnore.Click += new System.EventHandler(this.buttonIgnore_Click);
            //
            // buttonIgnoreAll
            //
            this.buttonIgnoreAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIgnoreAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonIgnoreAll.Location = new System.Drawing.Point(298, 54);
            this.buttonIgnoreAll.Name = "buttonIgnoreAll";
            this.buttonIgnoreAll.TabIndex = 7;
            this.buttonIgnoreAll.Text = "&Ignore All";
            this.buttonIgnoreAll.Click += new System.EventHandler(this.buttonIgnoreAll_Click);
            //
            // buttonChangeAll
            //
            this.buttonChangeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangeAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonChangeAll.Location = new System.Drawing.Point(298, 115);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.TabIndex = 9;
            this.buttonChangeAll.Text = "Change A&ll";
            this.buttonChangeAll.Click += new System.EventHandler(this.buttonChangeAll_Click);
            //
            // buttonChange
            //
            this.buttonChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChange.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonChange.Location = new System.Drawing.Point(298, 88);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.TabIndex = 8;
            this.buttonChange.Text = "&Change";
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(298, 237);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonAdd
            //
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(298, 149);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.TabIndex = 10;
            this.buttonAdd.Text = "&Add";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // labelWord
            //
            this.labelWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelWord.Location = new System.Drawing.Point(8, 28);
            this.labelWord.Name = "labelWord";
            this.labelWord.Size = new System.Drawing.Size(282, 21);
            this.labelWord.TabIndex = 1;
            this.labelWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // SpellCheckerForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(383, 271);
            this.Controls.Add(this.labelWord);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonChangeAll);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.buttonIgnoreAll);
            this.Controls.Add(this.buttonIgnore);
            this.Controls.Add(this.listBoxSuggestions);
            this.Controls.Add(this.labelSuggestions);
            this.Controls.Add(this.textBoxChangeTo);
            this.Controls.Add(this.labelChangeTo);
            this.Controls.Add(this.labelNotInDictionary);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpellCheckerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Check Spelling";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
