// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.App.Avalonia
{
    public partial class MainWindow : Window
    {
        private AvaloniaRibbonControl _ribbon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRibbon();
            InitializeEditor();
        }

        private void InitializeRibbon()
        {
            var config = DefaultRibbonConfiguration.Create();
            var ribbon = new AvaloniaRibbonControl();
            _ribbon = ribbon;
            ribbon.LoadConfiguration(config);

            // Wire ribbon commands — use async handler for proper await chain
            ribbon.CommandExecuted += async (sender, commandId) =>
            {
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

                UpdateStatus($"Command: {commandId}");
            };

            ribbon.ComboSelectionChanged += async (sender, args) =>
            {
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
                }
            };

            var ribbonHost = this.FindControl<Border>("RibbonHost");
            if (ribbonHost != null)
                ribbonHost.Child = ribbon;
        }

        private void InitializeEditor()
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null)
            {
                editorPanel.StatusChanged += (sender, message) => UpdateStatus(message);

                if (editorPanel.WebViewEditor != null)
                    editorPanel.WebViewEditor.FormatStateChanged += OnFormatStateChanged;
            }
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
        }

        private void UpdateStatus(string message)
        {
            var statusText = this.FindControl<TextBlock>("StatusText");
            if (statusText != null)
                statusText.Text = message;
        }
    }
}
