// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using System;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    public partial class EditorPanel : UserControl
    {
        private readonly CommandBridge _commandBridge;
        private WebViewEditor _webViewEditor;

        public event EventHandler<string> StatusChanged;

        public EditorPanel()
        {
            InitializeComponent();
            _commandBridge = new CommandBridge();
            InitializeWebViewEditor();
            SetupToolbarButtons();
            SetupKeyboardShortcuts();
            RegisterCommandBridgeHandlers();
        }

        public CommandBridge CommandBridge => _commandBridge;

        public WebViewEditor WebViewEditor => _webViewEditor;

        public string Title
        {
            get => ""; // Will be bound to title field later
            set { }
        }

        public string EditorContent
        {
            get
            {
                // For async content retrieval, callers should use GetContentAsync
                return "";
            }
            set
            {
                _webViewEditor?.SetContent(value ?? "");
            }
        }

        private void InitializeWebViewEditor()
        {
            _webViewEditor = new WebViewEditor();
            var editorHost = this.FindControl<ContentControl>("EditorHost");
            if (editorHost != null)
            {
                editorHost.Content = _webViewEditor;
            }

            _webViewEditor.FormatStateChanged += (sender, state) =>
            {
                // Future: update toolbar toggle states based on cursor position
            };

            _webViewEditor.ContentChanged += (sender, e) =>
            {
                // Future: mark document as dirty, update word count, etc.
            };
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
            // Route ribbon commands through the WebViewEditor
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

        /// <summary>
        /// Dispatch a formatting command to the WebViewEditor via execCommand.
        /// </summary>
        private void ExecuteFormatCommand(string format)
        {
            if (_webViewEditor == null) return;

            switch (format)
            {
                case "bold":
                    _webViewEditor.ExecuteBold();
                    break;
                case "italic":
                    _webViewEditor.ExecuteItalic();
                    break;
                case "underline":
                    _webViewEditor.ExecuteUnderline();
                    break;
                case "strikethrough":
                    _webViewEditor.ExecuteStrikethrough();
                    break;
                case "bulletlist":
                    _webViewEditor.ExecuteUnorderedList();
                    break;
                case "numberlist":
                    _webViewEditor.ExecuteOrderedList();
                    break;
            }

            StatusChanged?.Invoke(this, $"Applied {format} formatting");
        }
    }
}
