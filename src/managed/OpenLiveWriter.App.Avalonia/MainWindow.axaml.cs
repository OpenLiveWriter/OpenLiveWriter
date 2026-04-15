// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using OpenLiveWriter.App.Avalonia.Dialogs;

namespace OpenLiveWriter.App.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AboutButton.Click += OnAboutButtonClick;
        }

        private async void OnAboutButtonClick(object sender, RoutedEventArgs e)
        {
            await AboutDialog.Show(this);
        }
    }
}
