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
            var config = DefaultRibbonConfiguration.Create();
            var ribbon = new AvaloniaRibbonControl();
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
