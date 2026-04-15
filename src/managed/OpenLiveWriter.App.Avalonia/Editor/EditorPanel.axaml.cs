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

        private void SwitchView(string view)
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
                _webViewEditor?.GetContent(html =>
                {
                    global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (sourceEditor != null)
                            sourceEditor.Text = FormatHtml(html ?? "");
                    });
                });
                StatusChanged?.Invoke(this, "Source view");
            }
            else if (view == "edit")
            {
                // If coming from source view, push HTML back to WebView
                var source = this.FindControl<TextBox>("SourceEditor");
                if (source != null && !string.IsNullOrEmpty(source.Text))
                {
                    _webViewEditor?.SetContent(source.Text);
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
            if (BoldButton != null) BoldButton.Click += (s, e) => ExecuteFormatCommand("bold");
            if (ItalicButton != null) ItalicButton.Click += (s, e) => ExecuteFormatCommand("italic");
            if (UnderlineButton != null) UnderlineButton.Click += (s, e) => ExecuteFormatCommand("underline");
            if (StrikethroughButton != null) StrikethroughButton.Click += (s, e) => ExecuteFormatCommand("strikethrough");
            if (LinkButton != null) LinkButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Link: Not yet implemented");
            if (ImageButton != null) ImageButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Image: Not yet implemented");
            if (BulletListButton != null) BulletListButton.Click += (s, e) => ExecuteFormatCommand("bulletlist");
            if (NumberListButton != null) NumberListButton.Click += (s, e) => ExecuteFormatCommand("numberlist");

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
            _commandBridge.RegisterHandler(CommandId.Bold, () => ExecuteFormatCommand("bold"));
            _commandBridge.RegisterHandler(CommandId.Italic, () => ExecuteFormatCommand("italic"));
            _commandBridge.RegisterHandler(CommandId.Underline, () => ExecuteFormatCommand("underline"));
            _commandBridge.RegisterHandler(CommandId.Strikethrough, () => ExecuteFormatCommand("strikethrough"));
            _commandBridge.RegisterHandler(CommandId.InsertLink, () => StatusChanged?.Invoke(this, "Insert Link: Not yet implemented"));
            _commandBridge.RegisterHandler(CommandId.Bullets, () => ExecuteFormatCommand("bulletlist"));
            _commandBridge.RegisterHandler(CommandId.Numbers, () => ExecuteFormatCommand("numberlist"));
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
                            ExecuteFormatCommand("bold");
                            e.Handled = true;
                            break;
                        case Key.I:
                            ExecuteFormatCommand("italic");
                            e.Handled = true;
                            break;
                        case Key.U:
                            ExecuteFormatCommand("underline");
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

        private void ExecuteFormatCommand(string format)
        {
            if (_webViewEditor == null) return;

            switch (format)
            {
                case "bold": _webViewEditor.ExecuteBold(); break;
                case "italic": _webViewEditor.ExecuteItalic(); break;
                case "underline": _webViewEditor.ExecuteUnderline(); break;
                case "strikethrough": _webViewEditor.ExecuteStrikethrough(); break;
                case "bulletlist": _webViewEditor.ExecuteUnorderedList(); break;
                case "numberlist": _webViewEditor.ExecuteOrderedList(); break;
            }

            StatusChanged?.Invoke(this, $"Applied {format} formatting");
        }
    }
}
