// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    public partial class InputDialog : Window
    {
        private bool _confirmed;

        public InputDialog()
        {
            InitializeComponent();
            OKButton.Click += (s, e) => { _confirmed = true; Close(); };
            CancelButton.Click += (s, e) => Close();
        }

        public static async Task<string> Show(Window owner, string prompt, string defaultValue = "")
        {
            var dialog = new InputDialog();
            dialog.PromptText.Text = prompt;
            dialog.InputTextBox.Text = defaultValue;
            dialog.InputTextBox.SelectAll();

            await dialog.ShowDialog(owner);
            return dialog._confirmed ? dialog.InputTextBox.Text : null;
        }
    }
}
