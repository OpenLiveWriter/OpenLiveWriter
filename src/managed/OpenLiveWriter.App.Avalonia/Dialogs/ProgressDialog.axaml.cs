// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Threading;
using System;
using System.Threading;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    public partial class ProgressDialog : Window
    {
        private CancellationTokenSource _cts;

        public ProgressDialog()
        {
            InitializeComponent();
            _cts = new CancellationTokenSource();
            CancelButton.Click += (s, e) =>
            {
                _cts.Cancel();
                CancelButton.IsEnabled = false;
                CancelButton.Content = "Cancelling...";
            };
        }

        public CancellationToken CancellationToken => _cts.Token;

        public void UpdateProgress(string message, int percentComplete, string detail = null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MessageText.Text = message;
                ProgressBar.Value = percentComplete;
                if (detail != null) DetailText.Text = detail;
            });
        }

        public void SetIndeterminate(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MessageText.Text = message;
                ProgressBar.IsIndeterminate = true;
            });
        }
    }
}
