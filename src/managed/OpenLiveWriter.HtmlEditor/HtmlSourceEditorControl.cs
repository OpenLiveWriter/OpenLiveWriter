// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.SpellChecker;
namespace OpenLiveWriter.HtmlEditor
{
    public class HtmlSourceEditorControl : IHtmlEditor, IHtmlEditorCommandSource
    {
        CommandContextMenuDefinition contextMenu = new CommandContextMenuDefinition();

        private CommandManager _commandManager;
        public CommandManager CommandManager
        {
            get
            {
                return _commandManager;
            }
        }

        public HtmlSourceEditorControl(ISpellingChecker spellingChecker, CommandManager commandManager)
        {
            _commandManager = commandManager;
            _spellingChecker = spellingChecker;

            contextMenu.Entries.Add(CommandId.Cut, false, false);
            contextMenu.Entries.Add(CommandId.CopyCommand, false, false);
            contextMenu.Entries.Add(CommandId.Paste, false, false);
            contextMenu.Entries.Add(CommandId.PasteSpecial, false, false);
            contextMenu.Entries.Add(CommandId.SelectAll, true, true);
            contextMenu.Entries.Add(CommandId.InsertLink, true, true);

            // create and initialize the editor
            _textBox = new TextBoxEditorControl();
            _textBox.BorderStyle = BorderStyle.None;
            _textBox.Multiline = true;
            _textBox.HideSelection = false;
            _textBox.ScrollBars = ScrollBars.Vertical;
            _textBox.Font = new Font("Courier New", 10);
            _textBox.Dock = DockStyle.Fill;
            _textBox.MaxLength = 0;
            _textBox.AcceptsTab = true;
            _textBox.ContextMenu = new ContextMenu();
            _textBox.TextChanged += new EventHandler(_textBox_TextChanged);
            _textBox.ModifiedChanged += new EventHandler(_textBox_ModifiedChanged);
            _textBox.ContextMenuTriggered += new TextBoxEditorControl.ContextMenuTriggeredEventHandler(_textBox_ContextMenuTriggered);
            _textBox.GotFocus += new EventHandler(_textBox_GotFocus);
            _textBox.LostFocus += new EventHandler(_textBox_LostFocus);
            _textBox.KeyDown += new KeyEventHandler(_textBox_KeyDown);
            _textBox.MouseDown += new MouseEventHandler(_textBox_MouseDown);
            _textBox.MouseUp += new MouseEventHandler(_textBox_MouseUp);
            _textBox.RightToLeft = RightToLeft.No;
        }

        void _textBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OnCommandStateChanged();
        }

        void _textBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OnCommandStateChanged();
        }

        void _textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left ||
                e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Delete ||
                e.KeyCode == Keys.Back)
                OnCommandStateChanged();
        }

        ~HtmlSourceEditorControl()
        {
            Debug.Fail("HtmlSourceEditorControl should be disposed");
            Dispose(false);
        }

        private void _textBox_ContextMenuTriggered(object sender, TextBoxEditorControl.ContextMenuTriggeredEventArgs e)
        {
            OnCommandStateChanged();
            Command command = CommandContextMenu.ShowModal(CommandManager, (Control)sender, e.ContextMenuLocation, contextMenu);
            if (command != null)
                command.PerformExecute();
        }

        public bool InBody
        {
            set
            {
                _canInsertHyperlink = value;
            }
        }

        #region IHtmlEditor Members

        public Control EditorControl
        {
            get
            {
                return _textBox;

            }
        }

        public IHtmlEditorCommandSource CommandSource
        {
            get { return this; }
        }

        private bool _fullyEditableRegionActive;

        public bool FullyEditableRegionActive
        {
            get { return _fullyEditableRegionActive; }
            set { _fullyEditableRegionActive = value; }
        }

        public void LoadHtmlFile(string fileName)
        {
            // load contents of file
            using (TextReader htmlFileReader = new StreamReader(fileName, Encoding.UTF8))
            {
                // save current selection state so we can restore it
                int editorPosition = _textBox.SelectionStart;
                string html = htmlFileReader.ReadToEnd();
                _textBox.Text = NEWLINE + CleanupHtml(html, _textBox.Width - 5);
                _textBox.SelectionStart = editorPosition;

                OnCommandStateChanged();
            }
        }

        public virtual string GetEditedHtml(bool preferWellFormed)
        {
            string editedHtml = _textBox.Text;
            if (editedHtml.StartsWith(NEWLINE))
                editedHtml = editedHtml.Substring(NEWLINE.Length);

            return editedHtml;
        }

        public virtual string GetEditedHtmlFast()
        {
            return GetEditedHtml(false);
        }

        public string SelectedText
        {
            get
            {
                return _textBox.SelectedText;
            }
        }

        public string SelectedHtml
        {
            get
            {
                return _textBox.SelectedText;
            }
        }

        public void EmptySelection()
        {
            _textBox.Select(0, 0);
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            InsertHtml(content, moveSelectionRight ? HtmlInsertionOptions.MoveCursorAfter : HtmlInsertionOptions.Default);
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
            _textBox.Paste(content);
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            StringBuilder link = new StringBuilder("<a href=\"{0}\"");
            if (newWindow)
            {
                link.Append(" target=\"_blank\"");
            }
            if (String.Empty != linkTitle && null != linkTitle)
            {
                link.Append(" title=\"{2}\"");
            }
            if (String.Empty != rel && null != rel)
            {
                link.Append(" rel=\"{3}\"");
            }
            link.Append(">{1}</a>");
            InsertHtml(String.Format(CultureInfo.InvariantCulture, link.ToString(),
                                      HtmlUtils.EscapeEntities(url),
                                      HtmlUtils.EscapeEntities(linkText),
                                      HtmlUtils.EscapeEntities(linkTitle),
                                      HtmlUtils.EscapeEntities(rel)),
                true);
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                if (_isDirty && IsDirtyEvent != null)
                {
                    IsDirtyEvent(this, EventArgs.Empty);
                }
            }
        }
        private bool _isDirty;
        public event EventHandler IsDirtyEvent;

        public bool SuspendAutoSave
        {
            get { return false; }
        }

        public void Print()
        { }

        public void PrintPreview()
        { }

        public bool CanPrint
        {
            get
            {
                return false;
            }
        }

        public void Find()
        {
            using (HtmlSourceEditorFindTextForm findTextForm = new HtmlSourceEditorFindTextForm(_textBox))
            {
                // determine ownership and location of form
                Form parentForm = _textBox.FindForm();
                // No need to to manually center when we can just allow the form to center itself on the parent
                //findTextForm.StartPosition = FormStartPosition.CenterParent ;
                //findTextForm.Top = parentForm.Top + (parentForm.Height/2) - (findTextForm.Height/2) ;
                //findTextForm.Left = parentForm.Right - findTextForm.Width - 25 ;

                // show the form
                findTextForm.ShowDialog(parentForm);
            }
        }

        public bool CheckSpelling()
        {
            // check spelling
            using (SpellCheckerForm spellCheckerForm = new SpellCheckerForm(SpellingChecker, EditorControl.FindForm(), false))
            {
                //  center the spell-checking form over the document body
                spellCheckerForm.StartPosition = FormStartPosition.CenterParent;

                // create word range
                // TODO: smarter word range for html
                //TextBoxWordRange wordRange = new TextBoxWordRange(_textBox, _textBox.SelectionLength > 0);
                HtmlTextBoxWordRange wordRange = new HtmlTextBoxWordRange(_textBox);

                // check spelling
                spellCheckerForm.CheckSpelling(wordRange);

                // return completed status
                return spellCheckerForm.Completed;
            }
        }

        /// <summary>
        /// Get the spelling-checker (demand-create and cache/re-use)
        /// </summary>
        public ISpellingChecker SpellingChecker
        {
            get
            {
                return _spellingChecker;
            }
        }
        private ISpellingChecker _spellingChecker;

        #endregion

        #region IHtmlEditorCommandSource Members

        public void ViewSource()
        {
        }

        void IHtmlEditorCommandSource.ClearFormatting()
        {
            Debug.Assert(false, "This should be disabled");
        }

        bool IHtmlEditorCommandSource.CanApplyFormatting(CommandId? commandId)
        {
            if (commandId == CommandId.ClearFormatting)
                return false;

            // we always have a selection
            return true;
        }

        public string SelectionFontFamily
        {
            get { return String.Empty; }
        }

        void IHtmlEditorCommandSource.ApplyFontForeColor(int color)
        {
            string selectedText = _textBox.SelectedText;
            _textBox.Paste("<font color=\"" + ColorHelper.ColorToString(Color.FromArgb(color)) + "\">" + selectedText + "</font>");
        }

        public int SelectionBackColor
        {
            get { return 0; }
        }

        void IHtmlEditorCommandSource.ApplyFontBackColor(int? color)
        {
            string selectedText = _textBox.SelectedText;
            if (color.HasValue)
            {
                _textBox.Paste("<font style=\"background-color:" + ColorHelper.ColorToString(Color.FromArgb(color.Value)) + "\">" + selectedText + "</font>");
            }
        }

        void IHtmlEditorCommandSource.ApplyFontFamily(string fontFamily)
        {
            ApplyFormattingTag("font", "face=\"" + fontFamily + "\"");
        }

        public float SelectionFontSize
        {
            get { return 0; }
        }

        void IHtmlEditorCommandSource.ApplyFontSize(float fontSize)
        {
            ApplyFormattingTag("font", "size=\"" + HTMLElementHelper.PointFontSizeToHtmlFontSize(fontSize) + "\"");
        }

        public int SelectionForeColor
        {
            get { return 0; }
        }

        string IHtmlEditorCommandSource.SelectionStyleName
        {
            get
            {
                return null;
            }
        }

        void IHtmlEditorCommandSource.ApplyHtmlFormattingStyle(IHtmlFormattingStyle style)
        {
            ApplyFormattingTag(style.ElementName, null);
        }

        bool IHtmlEditorCommandSource.SelectionBold
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyBold()
        {
            ApplyFormattingTag("strong", null);
        }

        bool IHtmlEditorCommandSource.SelectionItalic
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyItalic()
        {
            ApplyFormattingTag("em", null);
        }

        bool IHtmlEditorCommandSource.SelectionUnderlined
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyUnderline()
        {
            ApplyFormattingTag("u", null);
        }

        bool IHtmlEditorCommandSource.SelectionStrikethrough
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyStrikethrough()
        {
            ApplyFormattingTag("strike", null);
        }

        bool IHtmlEditorCommandSource.SelectionSuperscript
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplySuperscript()
        {
            ApplyFormattingTag("sup", null);
        }

        bool IHtmlEditorCommandSource.SelectionSubscript
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplySubscript()
        {
            ApplyFormattingTag("sub", null);

        }

        bool IHtmlEditorCommandSource.SelectionIsLTR
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.InsertLTRTextBlock()
        {
            ApplyFormattingTag("p", "dir=\"ltr\"");
        }

        bool IHtmlEditorCommandSource.SelectionIsRTL
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.InsertRTLTextBlock()
        {
            ApplyFormattingTag("p", "dir=\"rtl\"");
        }

        bool IHtmlEditorCommandSource.CanPasteSpecial
        {
            get
            {
                return false;
            }
        }

        bool IHtmlEditorCommandSource.AllowPasteSpecial
        {
            get
            {
                return false;
            }
        }

        void IHtmlEditorCommandSource.PasteSpecial()
        {
            throw new NotSupportedException();
        }

        public EditorTextAlignment GetSelectionAlignment()
        {
            // we don't analyze the source to determine latched state (too difficult/expensive)
            return EditorTextAlignment.None;
        }

        void IHtmlEditorCommandSource.ApplyAlignment(EditorTextAlignment alignment)
        {
            switch (alignment)
            {
                case EditorTextAlignment.Left:
                    ApplyAlignment("left");
                    break;
                case EditorTextAlignment.Center:
                    ApplyAlignment("center");
                    break;
                case EditorTextAlignment.Right:
                    ApplyAlignment("right");
                    break;
                case EditorTextAlignment.Justify:
                    ApplyAlignment("justify");
                    break;

            }

        }

        bool IHtmlEditorCommandSource.SelectionBulleted
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyBullets()
        {
            InsertList("ul");
        }

        bool IHtmlEditorCommandSource.SelectionNumbered
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyNumbers()
        {
            InsertList("ol");
        }

        void IHtmlEditorCommandSource.ApplyBlockquote()
        {
            string selectedText = _textBox.SelectedText;
            _textBox.Paste("<blockquote>" + NEWLINE + selectedText + NEWLINE + "</blockquote>");
        }

        bool IHtmlEditorCommandSource.SelectionBlockquoted
        {
            get
            {
                // we don't analyze the source to determine latched state (too difficult/expensive)
                return false;
            }
        }

        bool IHtmlEditorCommandSource.CanIndent
        {
            get { return CommandSource.CanApplyFormatting(null); }
        }

        void IHtmlEditorCommandSource.ApplyIndent()
        {
            ((IHtmlEditorCommandSource)this).ApplyBlockquote();
        }

        bool IHtmlEditorCommandSource.CanOutdent
        {
            get { return false; }
        }

        void IHtmlEditorCommandSource.ApplyOutdent()
        {
            // not supported
        }

        bool IHtmlEditorCommandSource.CanInsertLink
        {
            get
            {
                return CommandSource.CanApplyFormatting(null);
            }
        }

        void IHtmlEditorCommandSource.InsertLink()
        {
            using (new WaitCursor())
            {
                if (!_canInsertHyperlink)
                {
                    DisplayMessage.Show(MessageId.TitleNotLinkable);
                    return;
                }
                using (HyperlinkForm hyperlinkForm = new HyperlinkForm(CommandManager, ShowAllLinkOptions))
                {
                    hyperlinkForm.LinkText = _textBox.SelectedText;
                    hyperlinkForm.EditStyle = false;
                    if (hyperlinkForm.ShowDialog(Owner) == DialogResult.OK)
                    {
                        InsertLink(hyperlinkForm.Hyperlink, hyperlinkForm.LinkText, hyperlinkForm.LinkTitle, hyperlinkForm.Rel, hyperlinkForm.NewWindow);
                    }
                }
            }
        }

        protected virtual bool ShowAllLinkOptions { get { return false; } }

        bool IHtmlEditorCommandSource.CanRemoveLink
        {
            get
            {
                // not suppported
                return false;
            }
        }

        void IHtmlEditorCommandSource.RemoveLink()
        {
            // not supported
        }

        bool IHtmlEditorCommandSource.CanFind
        {
            get
            {
                return true;
            }
        }

        void IHtmlEditorCommandSource.OpenLink()
        {
            // not supported
        }

        void IHtmlEditorCommandSource.AddToGlossary()
        {
            // not supported
        }

        LinkInfo IHtmlEditorCommandSource.DiscoverCurrentLink()
        {
            return new LinkInfo(null, null, null, null, false);
        }

        #endregion

        #region ISimpleTextEditorCommandSource Members

        public bool HasFocus
        {
            get
            {
                return _textBox.ContainsFocus;
            }
        }

        public bool CanUndo
        {
            get
            {
                return _textBox.CanUndo;
            }
        }

        public void Undo()
        {
            _textBox.Undo(); ;
        }

        public bool CanRedo
        {
            get
            {
                //not suported
                return false;
            }
        }

        public void Redo()
        {
            //not impelemented;
        }

        public bool CanCut
        {
            get
            {
                return _textBox.SelectionLength > 0;
            }
        }

        public void Cut()
        {
            _textBox.Cut();
        }

        public bool CanCopy
        {
            get
            {
                return CanCut;
            }
        }

        public void Copy()
        {
            _textBox.Copy();
        }

        public bool CanPaste
        {
            get
            {
                return Clipboard.GetDataObject() != null &&
                       Clipboard.ContainsData(DataFormats.UnicodeText) ||
                       Clipboard.ContainsData(DataFormats.Text);
            }
        }

        public void Paste()
        {
            _textBox.Paste();
        }

        // Unlike other ISimpleTextCommandSource implementations, "Clear" here
        // only refers to invoking the command from the menu or programmatically,
        // NOT hitting the delete key. The delete key is handled directly by the
        // edit control. This is made possible by the GotFocus and LostFocus event
        // handlers telling the command manager to ignore Delete.

        public bool CanClear
        {
            get
            {
                return HasFocus && _textBox.SelectionLength > 0;
            }
        }

        public void Clear()
        {
            // This is a no-op if there is no selection.
            User32.SendMessage(_textBox.Handle, WM.CLEAR, UIntPtr.Zero, IntPtr.Zero);
        }

        public void SelectAll()
        {
            _textBox.SelectAll();
        }

        public void InsertEuroSymbol()
        {
            IntPtr euro = Marshal.StringToCoTaskMemUni("&euro;");
            try
            {
                User32.SendMessage(_textBox.Handle, WM.EM_REPLACESEL, new IntPtr(1), euro);
            }
            finally
            {
                Marshal.FreeCoTaskMem(euro);
            }
        }

        bool ISimpleTextEditorCommandSource.ReadOnly
        {
            get { return _textBox.ReadOnly; }
        }

        public event EventHandler CommandStateChanged;
        protected void OnCommandStateChanged()
        {
            if (CommandStateChanged != null)
                CommandStateChanged(this, EventArgs.Empty);
        }

        public event EventHandler AggressiveCommandStateChanged;

        protected void OnAggressiveCommandStateChanged()
        {
            if (AggressiveCommandStateChanged != null)
                AggressiveCommandStateChanged(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                _textBox.Dispose();
            }
        }

        #endregion

        #region Protected Properties

        protected TextBox SourceEditor
        {
            get { return _textBox; }
        }

        #endregion

        #region Syntax Edit event handlers

        private void _textBox_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            OnCommandStateChanged();
        }

        //seems like this is only called if the modified property is changed programmatically
        private void _textBox_ModifiedChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            OnCommandStateChanged();
        }

        private void _textBox_GotFocus(object sender, EventArgs e)
        {
            // Let the textbox handle delete itself.
            CommandManager.IgnoreShortcut(Shortcut.Del);
            _canInsertHyperlink = true;
        }

        private void _textBox_LostFocus(object sender, EventArgs e)
        {
            // Back to normal delete-key behavior.
            CommandManager.UnignoreShortcut(Shortcut.Del);
        }

        #endregion

        #region HTML Formatting Helpers

        protected virtual string CleanupHtml(string html, int wrapTextAt)
        {
            // Don't do anything for now.  Add source formatting someday.
            return html;
        }

        private void ApplyAlignment(string alignment)
        {
            _textBox.Paste(String.Format(CultureInfo.InvariantCulture, "<p align=\"{0}\">{1}</p>", alignment, _textBox.SelectedText));
        }

        private void ApplyFormattingTag(string tagName, string attributes)
        {
            if (attributes != null)
                _textBox.Paste(String.Format(CultureInfo.InvariantCulture, "<{0} {1}>{2}</{0}>", tagName, attributes, _textBox.SelectedText));
            else
                _textBox.Paste(String.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", tagName, _textBox.SelectedText));
        }

        private void InsertList(string listTag)
        {
            string selectedText = _textBox.SelectedText;
            _textBox.Paste(String.Format(CultureInfo.InvariantCulture, "<{0}>", listTag) + NEWLINE + "<li>" + selectedText + "</li>" + NEWLINE + String.Format(CultureInfo.InvariantCulture, "</{0}>", listTag));
        }

        #endregion

        #region UI Management Helpers

        private IWin32Window Owner
        {
            get
            {
                if (_textBox != null)
                    return _textBox.FindForm();
                else
                    return null;
            }
        }

        #endregion

        #region Private Data

        private IContainer components = new Container();

        private TextBoxEditorControl _textBox;

        private const string NEWLINE = "\r\n";

        private bool _canInsertHyperlink = false;

        #endregion
    }
}
