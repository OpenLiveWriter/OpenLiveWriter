// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.App.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeRibbon();
            InitializeEditor();
        }

        private void InitializeRibbon()
        {
            // Create the ribbon configuration from the data model
            var config = DefaultRibbonConfiguration.Create();

            // Create and configure the Avalonia ribbon control
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(config);

            // Wire up command execution: try the WebViewEditor first via
            // HandleCommand, then try the editor's command bridge, then
            // fall back to status bar feedback for unhandled commands
            ribbon.CommandExecuted += (sender, commandId) =>
            {
                var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
                if (editorPanel != null)
                {
                    // Try direct WebView command handling
                    if (editorPanel.WebViewEditor != null && editorPanel.WebViewEditor.HandleCommand(commandId))
                        return;

                    // Try the command bridge (handles non-formatting commands like Undo/Redo)
                    if (editorPanel.CommandBridge.Execute(commandId))
                        return;
                }

                // Handle non-editor commands with status bar feedback
                UpdateStatus($"Command: {commandId}");
            };

            // Insert the ribbon into the host border
            var ribbonHost = this.FindControl<Border>("RibbonHost");
            if (ribbonHost != null)
                ribbonHost.Child = ribbon;
        }

        private void InitializeEditor()
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null)
            {
                editorPanel.StatusChanged += (sender, message) =>
                {
                    UpdateStatus(message);
                };
            }
        }

        private void UpdateStatus(string message)
        {
            var statusText = this.FindControl<TextBlock>("StatusText");
            if (statusText != null)
                statusText.Text = message;
        }
    }
}
