// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
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
        }

        private void InitializeRibbon()
        {
            // Create the ribbon configuration from the data model
            var config = DefaultRibbonConfiguration.Create();

            // Create and configure the Avalonia ribbon control
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(config);

            // Wire up command execution for status bar feedback
            ribbon.CommandExecuted += (s, commandId) =>
            {
                var statusText = this.FindControl<TextBlock>("StatusText");
                if (statusText != null)
                    statusText.Text = $"Command: {commandId}";
            };

            // Insert the ribbon into the host border
            var ribbonHost = this.FindControl<Border>("RibbonHost");
            if (ribbonHost != null)
                ribbonHost.Child = ribbon;
        }
    }
}
