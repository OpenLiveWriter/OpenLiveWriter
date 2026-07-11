// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Input;
using System;
using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    public partial class EditorPanel : UserControl
    {
        private readonly CommandBridge _commandBridge;
        private WebViewEditor _webViewEditor;
        private string _currentView = "edit"; // "edit", "source", "preview"

        public event EventHandler<string> StatusChanged;

        public EditorPanel()
        {
            InitializeComponent();
            _commandBridge = new CommandBridge();
            InitializeWebViewEditor();
            SetupToolbarButtons();
            SetupViewToggle();
            SetupKeyboardShortcuts();
            RegisterCommandBridgeHandlers();
        }

        public CommandBridge CommandBridge => _commandBridge;
        public WebViewEditor WebViewEditor => _webViewEditor;

        private void InitializeWebViewEditor()
        {
            _webViewEditor = new WebViewEditor();
            _webViewEditor.FormatStateChanged += OnFormatStateChanged;
            var editorHost = this.FindControl<ContentControl>("EditorHost");
            if (editorHost != null)
            {
                editorHost.Content = _webViewEditor;
            }
        }

        // Reflects the editor's current selection formatting on the toolbar
        // toggle buttons as the caret moves.
        private void OnFormatStateChanged(object sender, FormatState state)
        {
            if (state == null) return;
            if (BoldButton != null) BoldButton.IsChecked = state.Bold;
            if (ItalicButton != null) ItalicButton.IsChecked = state.Italic;
            if (UnderlineButton != null) UnderlineButton.IsChecked = state.Underline;
            if (StrikethroughButton != null) StrikethroughButton.IsChecked = state.Strikethrough;
            if (BulletListButton != null) BulletListButton.IsChecked = state.UnorderedList;
            if (NumberListButton != null) NumberListButton.IsChecked = state.OrderedList;
        }

        private void SetupViewToggle()
        {
            if (EditViewButton != null)
                EditViewButton.Click += (s, e) => SwitchView("edit");
            if (SourceViewButton != null)
                SourceViewButton.Click += (s, e) => SwitchView("source");
            if (PreviewViewButton != null)
                PreviewViewButton.Click += (s, e) => SwitchView("preview");
        }

        private async void SwitchView(string view)
        {
            _currentView = view;

            // Update toggle states
            if (EditViewButton != null) EditViewButton.IsChecked = (view == "edit");
            if (SourceViewButton != null) SourceViewButton.IsChecked = (view == "source");
            if (PreviewViewButton != null) PreviewViewButton.IsChecked = (view == "preview");

            var editorHost = this.FindControl<ContentControl>("EditorHost");
            var sourceEditor = this.FindControl<TextBox>("SourceEditor");
            var previewHost = this.FindControl<ContentControl>("PreviewHost");

            if (editorHost != null) editorHost.IsVisible = (view == "edit");
            if (sourceEditor != null) sourceEditor.IsVisible = (view == "source");
            if (previewHost != null) previewHost.IsVisible = (view == "preview");

            if (view == "source")
            {
                // Get HTML from WebView and show in source editor
                if (_webViewEditor != null)
                {
                    var html = await _webViewEditor.GetContentAsync();
                    if (sourceEditor != null)
                        sourceEditor.Text = FormatHtml(html ?? "");
                }
                StatusChanged?.Invoke(this, "Source view");
            }
            else if (view == "edit")
            {
                // If coming from source view, push HTML back to WebView
                var source = this.FindControl<TextBox>("SourceEditor");
                if (source != null && !string.IsNullOrEmpty(source.Text) && _webViewEditor != null)
                {
                    await _webViewEditor.SetContentAsync(source.Text);
                }
                StatusChanged?.Invoke(this, "Edit view");
            }
            else if (view == "preview")
            {
                StatusChanged?.Invoke(this, "Preview view");
            }
        }

        internal static string FormatHtml(string html)
        {
            // Basic HTML formatting for readability in source view
            if (string.IsNullOrEmpty(html)) return html;
            return html
                .Replace("><", ">\n<")
                .Replace("<br>", "<br>\n")
                .Replace("<br/>", "<br/>\n")
                .Replace("<br />", "<br />\n");
        }

        private void SetupToolbarButtons()
        {
            if (BoldButton != null) BoldButton.Click += async (s, e) => await ExecuteFormatCommandAsync("bold");
            if (ItalicButton != null) ItalicButton.Click += async (s, e) => await ExecuteFormatCommandAsync("italic");
            if (UnderlineButton != null) UnderlineButton.Click += async (s, e) => await ExecuteFormatCommandAsync("underline");
            if (StrikethroughButton != null) StrikethroughButton.Click += async (s, e) => await ExecuteFormatCommandAsync("strikethrough");
            if (LinkButton != null) LinkButton.Click += async (s, e) => await ShowInsertLinkDialogAsync();
            if (ImageButton != null) ImageButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Image: Not yet implemented");
            if (BulletListButton != null) BulletListButton.Click += async (s, e) => await ExecuteFormatCommandAsync("bulletlist");
            if (NumberListButton != null) NumberListButton.Click += async (s, e) => await ExecuteFormatCommandAsync("numberlist");

            if (HeadingCombo != null)
            {
                HeadingCombo.SelectionChanged += (s, e) =>
                {
                    if (HeadingCombo.SelectedIndex < 0) return;
                    var tag = MapHeadingIndexToTag(HeadingCombo.SelectedIndex);
                    _webViewEditor?.SetBlockFormat(tag);
                    StatusChanged?.Invoke(this, $"Applied {tag} formatting");
                };
            }
        }

        /// <summary>
        /// Maps the <c>HeadingCombo</c> selection index to the <c>formatBlock</c>
        /// tag. Delegates to <see cref="SemanticHtmlStyles"/> so the toolbar combo
        /// and the ribbon SemanticHtmlGallery stay in sync and the mapping is
        /// unit-testable without a live WebView. Index 0 (Normal) maps to a plain
        /// paragraph; 1-6 to h1-h6; 7 to preformatted.
        /// </summary>
        internal static string MapHeadingIndexToTag(int index) =>
            SemanticHtmlStyles.TagForIndex(index);

        /// <summary>
        /// Applies a semantic block style by tag (e.g. from the ribbon
        /// SemanticHtmlGallery). Ignores unknown tags.
        /// </summary>
        internal async Task ApplySemanticStyleAsync(string tag)
        {
            if (_webViewEditor == null || !SemanticHtmlStyles.IsKnownTag(tag))
                return;
            await _webViewEditor.SetBlockFormatAsync(tag);
            SyncHeadingComboToTag(tag);
            StatusChanged?.Invoke(this, $"Applied {tag} formatting");
        }

        // Reflects an applied block tag back onto the toolbar combo selection.
        private void SyncHeadingComboToTag(string tag)
        {
            if (HeadingCombo == null) return;
            for (int i = 0; i < SemanticHtmlStyles.Styles.Count; i++)
            {
                if (string.Equals(SemanticHtmlStyles.Styles[i].Tag, tag,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    HeadingCombo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void RegisterCommandBridgeHandlers()
        {
            _commandBridge.RegisterHandler(CommandId.Bold, () => _ = ExecuteFormatCommandAsync("bold"));
            _commandBridge.RegisterHandler(CommandId.Italic, () => _ = ExecuteFormatCommandAsync("italic"));
            _commandBridge.RegisterHandler(CommandId.Underline, () => _ = ExecuteFormatCommandAsync("underline"));
            _commandBridge.RegisterHandler(CommandId.Strikethrough, () => _ = ExecuteFormatCommandAsync("strikethrough"));
            _commandBridge.RegisterHandler(CommandId.InsertLink, () => _ = ShowInsertLinkDialogAsync());
            _commandBridge.RegisterHandler(CommandId.Bullets, () => _ = ExecuteFormatCommandAsync("bulletlist"));
            _commandBridge.RegisterHandler(CommandId.Numbers, () => _ = ExecuteFormatCommandAsync("numberlist"));
            _commandBridge.RegisterHandler(CommandId.Undo, () => StatusChanged?.Invoke(this, "Undo"));
            _commandBridge.RegisterHandler(CommandId.Redo, () => StatusChanged?.Invoke(this, "Redo"));
        }

        private void SetupKeyboardShortcuts()
        {
            this.KeyDown += (s, e) =>
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Meta) || e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    switch (e.Key)
                    {
                        case Key.B:
                            _ = ExecuteFormatCommandAsync("bold");
                            e.Handled = true;
                            break;
                        case Key.I:
                            _ = ExecuteFormatCommandAsync("italic");
                            e.Handled = true;
                            break;
                        case Key.U:
                            _ = ExecuteFormatCommandAsync("underline");
                            e.Handled = true;
                            break;
                        case Key.K:
                            _ = ShowInsertLinkDialogAsync();
                            e.Handled = true;
                            break;
                    }
                }
            };
        }

        private async Task ShowInsertLinkDialogAsync()
        {
            if (_webViewEditor == null) return;

            var owner = TopLevel.GetTopLevel(this) as Window;
            var result = await LinkDialog.ShowAsync(owner);
            if (result == null || string.IsNullOrWhiteSpace(result.Url))
                return;

            await _webViewEditor.InsertLinkAsync(result.Url, result.Text, result.Title, result.OpenInNewWindow);
            StatusChanged?.Invoke(this, $"Inserted link: {result.Url}");
        }

        private async Task ExecuteFormatCommandAsync(string format)
        {
            if (_webViewEditor == null) return;

            switch (format)
            {
                case "bold": await _webViewEditor.ExecuteBoldAsync(); break;
                case "italic": await _webViewEditor.ExecuteItalicAsync(); break;
                case "underline": await _webViewEditor.ExecuteUnderlineAsync(); break;
                case "strikethrough": await _webViewEditor.ExecuteStrikethroughAsync(); break;
                case "bulletlist": await _webViewEditor.ExecuteUnorderedListAsync(); break;
                case "numberlist": await _webViewEditor.ExecuteOrderedListAsync(); break;
            }

            StatusChanged?.Invoke(this, $"Applied {format} formatting");
        }
    }
}
