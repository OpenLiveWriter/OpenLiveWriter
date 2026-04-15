// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Input;
using System;
using OpenLiveWriter.App.Avalonia.Commands;
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
            var editorHost = this.FindControl<ContentControl>("EditorHost");
            if (editorHost != null)
            {
                editorHost.Content = _webViewEditor;
            }
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

        private static string FormatHtml(string html)
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
            if (LinkButton != null) LinkButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Link: Not yet implemented");
            if (ImageButton != null) ImageButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Image: Not yet implemented");
            if (BulletListButton != null) BulletListButton.Click += async (s, e) => await ExecuteFormatCommandAsync("bulletlist");
            if (NumberListButton != null) NumberListButton.Click += async (s, e) => await ExecuteFormatCommandAsync("numberlist");

            if (HeadingCombo != null)
            {
                HeadingCombo.SelectionChanged += (s, e) =>
                {
                    if (HeadingCombo.SelectedIndex <= 0) return;
                    var tag = HeadingCombo.SelectedIndex switch
                    {
                        1 => "h1",
                        2 => "h2",
                        3 => "h3",
                        _ => "p"
                    };
                    _webViewEditor?.SetBlockFormat(tag);
                    StatusChanged?.Invoke(this, $"Applied {tag} formatting");
                };
            }
        }

        private void RegisterCommandBridgeHandlers()
        {
            _commandBridge.RegisterHandler(CommandId.Bold, () => _ = ExecuteFormatCommandAsync("bold"));
            _commandBridge.RegisterHandler(CommandId.Italic, () => _ = ExecuteFormatCommandAsync("italic"));
            _commandBridge.RegisterHandler(CommandId.Underline, () => _ = ExecuteFormatCommandAsync("underline"));
            _commandBridge.RegisterHandler(CommandId.Strikethrough, () => _ = ExecuteFormatCommandAsync("strikethrough"));
            _commandBridge.RegisterHandler(CommandId.InsertLink, () => StatusChanged?.Invoke(this, "Insert Link: Not yet implemented"));
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
                            StatusChanged?.Invoke(this, "Insert Link: Not yet implemented");
                            e.Handled = true;
                            break;
                    }
                }
            };
        }

        private async System.Threading.Tasks.Task ExecuteFormatCommandAsync(string format)
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
