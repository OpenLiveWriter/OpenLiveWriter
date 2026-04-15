// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    public enum MessageDialogResult { OK, Cancel, Yes, No }
    public enum MessageDialogButtons { OK, OKCancel, YesNo }

    public partial class MessageDialog : Window
    {
        private MessageDialogResult _result = MessageDialogResult.Cancel;

        public MessageDialog()
        {
            InitializeComponent();
            PrimaryButton.Click += OnPrimaryButtonClick;
            SecondaryButton.Click += OnSecondaryButtonClick;
        }

        private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
        {
            _result = MessageDialogResult.OK;
            Close();
        }

        private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
        {
            _result = MessageDialogResult.Cancel;
            Close();
        }

        public static async Task<MessageDialogResult> Show(
            Window owner,
            string title,
            string body,
            MessageDialogButtons buttons = MessageDialogButtons.OK)
        {
            var dialog = new MessageDialog();
            dialog.TitleText.Text = title;
            dialog.BodyText.Text = body;

            switch (buttons)
            {
                case MessageDialogButtons.OKCancel:
                    dialog.SecondaryButton.IsVisible = true;
                    break;
                case MessageDialogButtons.YesNo:
                    dialog.PrimaryButton.Content = "Yes";
                    dialog.SecondaryButton.Content = "No";
                    dialog.SecondaryButton.IsVisible = true;
                    dialog._result = MessageDialogResult.No;
                    break;
            }

            // For YesNo, primary button should return Yes
            if (buttons == MessageDialogButtons.YesNo)
            {
                dialog.PrimaryButton.Click -= dialog.OnPrimaryButtonClick;
                dialog.PrimaryButton.Click += (s, e) =>
                {
                    dialog._result = MessageDialogResult.Yes;
                    dialog.Close();
                };

                dialog.SecondaryButton.Click -= dialog.OnSecondaryButtonClick;
                dialog.SecondaryButton.Click += (s, e) =>
                {
                    dialog._result = MessageDialogResult.No;
                    dialog.Close();
                };
            }

            await dialog.ShowDialog(owner);
            return dialog._result;
        }
    }
}
