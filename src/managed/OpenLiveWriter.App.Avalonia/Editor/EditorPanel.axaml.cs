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

        public event EventHandler<string> StatusChanged;

        public EditorPanel()
        {
            InitializeComponent();
            _commandBridge = new CommandBridge();
            SetupToolbarButtons();
            SetupKeyboardShortcuts();
            RegisterCommandBridgeHandlers();
        }

        public CommandBridge CommandBridge => _commandBridge;

        public string Title
        {
            get => ""; // Will be bound to title field in M4
            set { }
        }

        public string EditorContent
        {
            get => ContentEditor?.Text ?? "";
            set { if (ContentEditor != null) ContentEditor.Text = value; }
        }

        private void SetupToolbarButtons()
        {
            if (BoldButton != null) BoldButton.Click += (s, e) => ApplyFormatting("bold");
            if (ItalicButton != null) ItalicButton.Click += (s, e) => ApplyFormatting("italic");
            if (UnderlineButton != null) UnderlineButton.Click += (s, e) => ApplyFormatting("underline");
            if (StrikethroughButton != null) StrikethroughButton.Click += (s, e) => ApplyFormatting("strikethrough");
            if (LinkButton != null) LinkButton.Click += (s, e) => ApplyFormatting("link");
            if (ImageButton != null) ImageButton.Click += (s, e) => StatusChanged?.Invoke(this, "Insert Image: Not yet implemented (coming in M4)");
            if (BulletListButton != null) BulletListButton.Click += (s, e) => ApplyFormatting("bulletlist");
            if (NumberListButton != null) NumberListButton.Click += (s, e) => ApplyFormatting("numberlist");
        }

        private void RegisterCommandBridgeHandlers()
        {
            _commandBridge.RegisterHandler(CommandId.Bold, () => ApplyFormatting("bold"));
            _commandBridge.RegisterHandler(CommandId.Italic, () => ApplyFormatting("italic"));
            _commandBridge.RegisterHandler(CommandId.Underline, () => ApplyFormatting("underline"));
            _commandBridge.RegisterHandler(CommandId.Strikethrough, () => ApplyFormatting("strikethrough"));
            _commandBridge.RegisterHandler(CommandId.InsertLink, () => ApplyFormatting("link"));
            _commandBridge.RegisterHandler(CommandId.Bullets, () => ApplyFormatting("bulletlist"));
            _commandBridge.RegisterHandler(CommandId.Numbers, () => ApplyFormatting("numberlist"));
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
                            ApplyFormatting("bold");
                            e.Handled = true;
                            break;
                        case Key.I:
                            ApplyFormatting("italic");
                            e.Handled = true;
                            break;
                        case Key.U:
                            ApplyFormatting("underline");
                            e.Handled = true;
                            break;
                        case Key.K:
                            ApplyFormatting("link");
                            e.Handled = true;
                            break;
                    }
                }
            };
        }

        private void ApplyFormatting(string format)
        {
            if (ContentEditor == null) return;

            int selStart = ContentEditor.SelectionStart;
            int selEnd = ContentEditor.SelectionEnd;
            string selectedText = "";

            if (selEnd > selStart && ContentEditor.Text != null)
            {
                selectedText = ContentEditor.Text.Substring(selStart, selEnd - selStart);
            }

            string wrapped = format switch
            {
                "bold" => $"**{selectedText}**",
                "italic" => $"*{selectedText}*",
                "underline" => $"__{selectedText}__",
                "strikethrough" => $"~~{selectedText}~~",
                "link" => $"[{selectedText}](url)",
                "bulletlist" => $"* {selectedText}",
                "numberlist" => $"1. {selectedText}",
                _ => selectedText
            };

            if (selEnd > selStart && ContentEditor.Text != null)
            {
                ContentEditor.Text = ContentEditor.Text.Remove(selStart, selEnd - selStart).Insert(selStart, wrapped);
                ContentEditor.SelectionStart = selStart;
                ContentEditor.SelectionEnd = selStart + wrapped.Length;
            }

            StatusChanged?.Invoke(this, $"Applied {format} formatting");
        }
    }
}
