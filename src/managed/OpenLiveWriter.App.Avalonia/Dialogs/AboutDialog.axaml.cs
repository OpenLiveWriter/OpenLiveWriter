// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"Version {version?.ToString(3) ?? "0.0.0"}";
        }

        public static new async Task Show(Window owner)
        {
            var dialog = new AboutDialog();
            await dialog.ShowDialog(owner);
        }
    }
}
