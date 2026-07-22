// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing.Drafts;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.App.Avalonia
{
    public partial class MainWindow : Window
    {
        private AvaloniaRibbonControl _ribbon;
        private DraftSession _draftSession;
        private OpenLiveWriter.Publishing.Accounts.BlogAccountService _accountService;
        private TextBox _titleEditor;
        private bool _suppressDirty;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRibbon();
            InitializeEditor();
            InitializeDraftSession();
            InitializeAccounts();
            InitializeSpelling();
            InitializePreferences();
            InitializeWindowLayout();
            InitializeMenuBar();
            InitializeAutosave();
        }

        private void InitializeRibbon()
        {
            var config = DefaultRibbonConfiguration.Create();
            var ribbon = new AvaloniaRibbonControl();
            _ribbon = ribbon;
            // Disable buttons for commands the shell does not route (set before load).
            ribbon.CommandFilter = HandledCommands.IsHandled;
            ribbon.LoadConfiguration(config);

            // Wire ribbon commands — the same entry point the macOS menu bar uses.
            ribbon.CommandExecuted += async (sender, commandId) => await ExecuteCommandAsync(commandId);

            ribbon.ComboSelectionChanged += async (sender, args) =>
            {
                // Blog selector is independent of the editor — handle it first.
                if (args.CommandId == CommandId.SelectBlog)
                {
                    OnBlogSelectorChanged(args.Value);
                    return;
                }

                var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
                var editor = editorPanel?.WebViewEditor;
                if (editor == null || string.IsNullOrEmpty(args.Value))
                    return;

                switch (args.CommandId)
                {
                    case CommandId.FontFamily:
                        await editor.SetFontFamilyAsync(args.Value);
                        UpdateStatus($"Font: {args.Value}");
                        break;
                    case CommandId.FontSize:
                        await editor.SetFontSizeAsync(args.Value);
                        UpdateStatus($"Font size: {args.Value}");
                        break;
                    case CommandId.SemanticHtmlGallery:
                        if (editorPanel != null)
                            await editorPanel.ApplySemanticStyleAsync(args.Value);
                        break;
                    case CommandId.FontColorPicker:
                    case CommandId.FontColor:
                        await editor.SetFontColorAsync(args.Value);
                        UpdateStatus($"Font color: {args.Value}");
                        break;
                    case CommandId.FontBackgroundColor:
                        await editor.SetHighlightColorAsync(args.Value);
                        UpdateStatus($"Highlight: {args.Value}");
                        break;
                }
            };

            var ribbonHost = this.FindControl<Border>("RibbonHost");
            if (ribbonHost != null)
                ribbonHost.Child = ribbon;
        }

        /// <summary>
        /// Routes a command through the shell's handler chain. This is the single
        /// entry point shared by ribbon buttons and macOS menu-bar items — menus
        /// never duplicate command logic.
        /// </summary>
        public async Task ExecuteCommandAsync(CommandId commandId)
        {
            // Options / Preferences (does not require a draft session).
            if (await TryHandleOptionsCommandAsync(commandId))
                return;

            // File / document-lifecycle commands are handled by the shell.
            if (await TryHandleFileCommandAsync(commandId))
                return;

            // Account setup / publish commands are handled by the shell.
            if (await TryHandlePublishCommandAsync(commandId))
                return;

            // Insert-tab commands that need a dialog (table/video/emoticon/paste).
            if (await TryHandleInsertCommandAsync(commandId))
                return;

            // Spelling status command.
            if (await TryHandleSpellingCommandAsync(commandId))
                return;

            // Plug-in commands (informational stub on macOS).
            if (await TryHandlePluginCommandAsync(commandId))
                return;

            // Menu-only commands (About / Close Window / view switching).
            if (await TryHandleMenuCommandAsync(commandId))
                return;

            // Editor utility commands surfaced by the shell (dialogs/status).
            if (commandId == CommandId.WordCount)
            {
                await ShowWordCountAsync();
                return;
            }
            if (commandId == CommandId.FindButton)
            {
                ShowInEditorFindBar();
                return;
            }
            if (commandId == CommandId.FindAndReplace)
            {
                ShowFindReplace(showReplace: true);
                return;
            }

            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel?.WebViewEditor != null)
            {
                bool handled = await editorPanel.WebViewEditor.HandleCommandAsync(commandId);
                if (handled)
                {
                    UpdateStatus($"Applied: {commandId}");
                    return;
                }
            }

            if (editorPanel?.CommandBridge.Execute(commandId) == true)
                return;

            UpdateStatus(HandledCommands.IsHandled(commandId)
                ? $"Command: {commandId}"
                : $"Not yet available: {commandId}");
        }

        private void InitializeEditor()
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null)
            {
                editorPanel.StatusChanged += (sender, message) => UpdateStatus(message);
                editorPanel.OpenFindReplaceRequested += (sender, e) =>
                    ShowFindReplace(showReplace: true);

                if (editorPanel.WebViewEditor != null)
                {
                    editorPanel.WebViewEditor.FormatStateChanged += OnFormatStateChanged;
                    editorPanel.WebViewEditor.ContentChanged += OnEditorContentChanged;
                }
            }
        }

        private void InitializeDraftSession()
        {
            try
            {
                _draftSession = new DraftSession(DraftStoreFactory.CreateDefault());
            }
            catch (Exception ex)
            {
                // A missing/uninitialized platform context shouldn't crash the shell;
                // the File menu simply stays inert until a store is available.
                Console.WriteLine($"[OLW-Drafts] Draft store unavailable: {ex.Message}");
                return;
            }

            _titleEditor = this.FindControl<TextBox>("TitleEditor");
            if (_titleEditor != null)
            {
                _titleEditor.TextChanged += (s, e) =>
                {
                    if (_suppressDirty) return;
                    _draftSession.UpdateTitle(_titleEditor.Text ?? string.Empty);
                    UpdateWindowTitle();
                };
            }
        }

        // Test seam: lets headless tests substitute a temp-directory session and
        // inspect dirty state (e.g. close-prompt and autosave coverage).
        internal DraftSession DraftSession
        {
            get => _draftSession;
            set => _draftSession = value;
        }

        // The editor's contentChanged signal is debounced and payload-free: mark the
        // draft dirty and let save/autosave pull the full HTML via GetContentAsync.
        private void OnEditorContentChanged(object sender, EventArgs e)
        {
            if (_suppressDirty || _draftSession == null) return;
            _draftSession.MarkDirty();
            UpdateWindowTitle();
            OnEditorContentChangedForWordCount();
        }

        // ---- File / document lifecycle ----

        private async Task<bool> TryHandleFileCommandAsync(CommandId commandId)
        {
            if (_draftSession == null)
                return false;

            switch (commandId)
            {
                case CommandId.NewPost:
                    await NewDocumentAsync(isPage: false);
                    return true;
                case CommandId.NewPage:
                    await NewDocumentAsync(isPage: true);
                    return true;
                case CommandId.SavePost:
                    await SaveCurrentAsync();
                    return true;
                case CommandId.OpenDrafts:
                case CommandId.OpenPost:
                    await OpenDraftAsync();
                    return true;
                case CommandId.DeleteDraft:
                    await DeleteCurrentAsync();
                    return true;
                case CommandId.OpenDraftMRU0:
                case CommandId.OpenDraftMRU1:
                case CommandId.OpenDraftMRU2:
                case CommandId.OpenDraftMRU3:
                case CommandId.OpenDraftMRU4:
                case CommandId.OpenDraftMRU5:
                case CommandId.OpenDraftMRU6:
                case CommandId.OpenDraftMRU7:
                case CommandId.OpenDraftMRU8:
                case CommandId.OpenDraftMRU9:
                    await OpenDraftMruAsync(commandId - CommandId.OpenDraftMRU0);
                    return true;
                default:
                    return false;
            }
        }

        private async Task NewDocumentAsync(bool isPage)
        {
            if (!await ConfirmDiscardIfDirtyAsync())
                return;

            _draftSession.NewPost(isPage);
            await LoadCurrentIntoEditorAsync();
            UpdateStatus(isPage ? "New page" : "New post");
        }

        private async Task SaveCurrentAsync()
        {
            var editor = GetEditor();
            string html = editor != null ? await editor.GetContentAsync() : null;
            string title = _titleEditor?.Text ?? string.Empty;

            _draftSession.Save(title, html ?? _draftSession.Current.BodyHtml);
            UpdateWindowTitle();
            UpdateStatus($"Saved draft: {DisplayTitle()}");
        }

        private async Task OpenDraftAsync()
        {
            if (!await ConfirmDiscardIfDirtyAsync())
                return;

            var drafts = _draftSession.ListDrafts();
            var dialog = new DraftPickerDialog(drafts);
            await dialog.ShowDialog<string>(this);

            // The user can pivot from the draft list to fetching a server post.
            if (dialog.RequestedOpenFromBlog)
            {
                await OpenFromBlogAsync();
                return;
            }

            if (string.IsNullOrEmpty(dialog.SelectedDraftId))
                return;

            await OpenByIdAsync(dialog.SelectedDraftId);
        }

        private async Task OpenDraftMruAsync(int index)
        {
            var drafts = _draftSession.ListDrafts();
            if (index < 0 || index >= drafts.Count)
            {
                UpdateStatus("No draft in that slot.");
                return;
            }

            if (!await ConfirmDiscardIfDirtyAsync())
                return;

            await OpenByIdAsync(drafts[index].Id);
        }

        private async Task OpenByIdAsync(string id)
        {
            try
            {
                if (_draftSession.Open(id))
                {
                    await LoadCurrentIntoEditorAsync();
                    UpdateStatus($"Opened draft: {DisplayTitle()}");
                }
                else
                {
                    UpdateStatus("Draft not found.");
                }
            }
            catch (DraftStoreException ex)
            {
                UpdateStatus($"Could not open draft: {ex.Message}");
            }
        }

        private async Task DeleteCurrentAsync()
        {
            if (!_draftSession.Current.IsSaved)
            {
                UpdateStatus("Nothing to delete — draft not yet saved.");
                return;
            }

            bool confirmed = await ConfirmDialog.ShowConfirmAsync(
                this, "Delete Draft",
                $"Delete the draft \u201c{DisplayTitle()}\u201d? This cannot be undone.");
            if (!confirmed)
                return;

            _draftSession.Delete(_draftSession.Current.Id);
            await LoadCurrentIntoEditorAsync();
            UpdateStatus("Draft deleted.");
        }

        // Returns true when it is safe to proceed (saved, discarded, or not dirty);
        // false when the user cancelled.
        private async Task<bool> ConfirmDiscardIfDirtyAsync()
        {
            if (!_draftSession.IsDirty)
                return true;

            ConfirmResult choice = await ConfirmDialog.ShowUnsavedChangesAsync(this, DisplayTitle());
            switch (choice)
            {
                case ConfirmResult.Save:
                    await SaveCurrentAsync();
                    return true;
                case ConfirmResult.Discard:
                    return true;
                default:
                    return false;
            }
        }

        // Pushes the current document's title + body into the shell without marking dirty.
        private async Task LoadCurrentIntoEditorAsync()
        {
            _suppressDirty = true;
            try
            {
                if (_titleEditor != null)
                    _titleEditor.Text = _draftSession.Current.Title ?? string.Empty;

                var editor = GetEditor();
                if (editor != null)
                    await editor.SetContentAsync(_draftSession.Current.BodyHtml ?? string.Empty);

                _draftSession.Current.IsDirty = false;
                UpdateWindowTitle();
            }
            finally
            {
                _suppressDirty = false;
            }
        }

        private WebViewEditor GetEditor() =>
            this.FindControl<EditorPanel>("EditorPanel")?.WebViewEditor;

        private string DisplayTitle()
        {
            string title = _draftSession?.Current.Title;
            return string.IsNullOrWhiteSpace(title) ? "(untitled post)" : title;
        }

        private void UpdateWindowTitle()
        {
            if (_draftSession == null) return;
            string dirtyMark = _draftSession.IsDirty ? " \u2022" : string.Empty;
            Title = $"{DisplayTitle()}{dirtyMark} — Open Live Writer";
        }

        // Reflects the editor's current selection formatting on the ribbon's
        // toggle buttons as the caret moves.
        private void OnFormatStateChanged(object sender, FormatState state)
        {
            if (_ribbon == null || state == null)
                return;

            _ribbon.SetToggleState(CommandId.Bold, state.Bold);
            _ribbon.SetToggleState(CommandId.Italic, state.Italic);
            _ribbon.SetToggleState(CommandId.Underline, state.Underline);
            _ribbon.SetToggleState(CommandId.Strikethrough, state.Strikethrough);
            _ribbon.SetToggleState(CommandId.Subscript, state.Subscript);
            _ribbon.SetToggleState(CommandId.Superscript, state.Superscript);
            _ribbon.SetToggleState(CommandId.Bullets, state.UnorderedList);
            _ribbon.SetToggleState(CommandId.Numbers, state.OrderedList);
            _ribbon.SetToggleState(CommandId.AlignLeft, state.AlignLeft);
            _ribbon.SetToggleState(CommandId.AlignCenter, state.AlignCenter);
            _ribbon.SetToggleState(CommandId.AlignRight, state.AlignRight);
            _ribbon.SetToggleState(CommandId.Justify, state.AlignFull);
            _ribbon.SetToggleState(CommandId.Blockquote,
                string.Equals(state.BlockTag, "blockquote", StringComparison.OrdinalIgnoreCase));

            // Reflect the caret's font family/size in the ribbon Font combos.
            _ribbon.SetComboSelection(CommandId.FontFamily, state.FontFamily);
            _ribbon.SetComboSelection(CommandId.FontSize, state.FontSize);

            // Reflect the caret's block style (p/h1-h6/pre) in the Styles combo.
            // Unknown tags (e.g. blockquote) clear the selection via SetComboSelection.
            _ribbon.SetComboSelection(CommandId.SemanticHtmlGallery, state.BlockTag);

            // Show/hide the contextual ribbon tab (Table/Picture/Video/Map/Tag Tools)
            // that matches the caret's current selection context.
            _ribbon.ActivateContextualTabGroup(ContextualTabResolver.Resolve(state));
        }

        private FindReplaceDialog _findDialog;

        /// <summary>Shows the thin in-editor find bar (Cmd/Ctrl+F / Find ribbon).</summary>
        private void ShowInEditorFindBar()
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            editorPanel?.ShowFindBar();
        }

        // Opens (or re-focuses) the Find & Replace dialog for Replace All / advanced find.
        // Plain Find uses the in-editor bar instead so it does not obscure the post.
        private void ShowFindReplace(bool showReplace)
        {
            if (!showReplace)
            {
                ShowInEditorFindBar();
                return;
            }

            if (_findDialog != null)
            {
                try { _findDialog.Activate(); return; }
                catch { _findDialog = null; }
            }

            _findDialog = new FindReplaceDialog(
                onFindNext: async req =>
                {
                    var editor = GetEditor();
                    if (editor != null)
                        await editor.FindNextAsync(req.Query, req.MatchCase);
                },
                onReplaceAll: async req =>
                {
                    var editor = GetEditor();
                    if (editor == null) return;
                    int n = await editor.ReplaceAllAsync(req.Query, req.Replacement, req.MatchCase, req.WholeWord);
                    UpdateStatus($"Replaced {n} occurrence(s).");
                },
                showReplace: true,
                onReplace: async req =>
                {
                    // Replace the currently selected match, then advance to the
                    // next one (standard find-and-replace stepping).
                    var editor = GetEditor();
                    if (editor == null) return;
                    bool replaced = await editor.ReplaceCurrentAsync(req.Query, req.Replacement, req.MatchCase);
                    await editor.FindNextAsync(req.Query, req.MatchCase);
                    UpdateStatus(replaced ? "Replaced 1 occurrence." : "No selected match to replace.");
                });

            _findDialog.Closed += (s, e) => _findDialog = null;
            _findDialog.Show(this);
        }

        // Computes word/character/paragraph stats from the current editor body and
        // shows them in a modal (parity with the Windows Word Count dialog).
        private async Task ShowWordCountAsync()
        {
            var editor = GetEditor();
            string html = editor != null ? await editor.GetContentAsync() : null;
            var counter = new WordCounter(html ?? string.Empty);
            UpdateStatus($"Word count: {counter.Words} words, {counter.Chars} characters");
            await WordCountDialog.ShowAsync(this, counter);
        }

        private void UpdateStatus(string message)
        {
            var statusText = this.FindControl<TextBlock>("StatusText");
            if (statusText != null)
                statusText.Text = message;
        }
    }
}
