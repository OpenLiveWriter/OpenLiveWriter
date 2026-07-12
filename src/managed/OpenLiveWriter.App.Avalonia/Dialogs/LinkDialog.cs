// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="LinkDialog"/> when the user inserts a link.
    /// </summary>
    public class LinkDialogResult
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public bool OpenInNewWindow { get; set; }
    }

    /// <summary>
    /// A modal dialog for inserting a hyperlink: URL (required), optional display
    /// text and title, and an "open in new window" option. Mirrors the fields of the
    /// Windows "Insert Hyperlink" dialog at a basic level.
    /// </summary>
    public class LinkDialog : Window
    {
        private readonly TextBox _urlBox;
        private readonly TextBox _textBox;
        private readonly TextBox _titleBox;
        private readonly CheckBox _newWindowCheck;
        private readonly Button _insertButton;

        public LinkDialogResult Result { get; private set; }

        public LinkDialog(string initialText = null)
        {
            Title = "Insert Hyperlink";
            Width = 440;
            MinWidth = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _urlBox = new TextBox { PlaceholderText = "https://", Text = "https://" };
            _textBox = new TextBox { Text = initialText ?? string.Empty };
            _titleBox = new TextBox();
            _newWindowCheck = new CheckBox { Content = "Open in new window", IsChecked = false };

            _insertButton = new Button
            {
                Content = "Insert",
                IsDefault = true,
                MinWidth = 80,
                IsEnabled = false
            };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _insertButton.Click += (s, e) =>
            {
                Result = new LinkDialogResult
                {
                    Url = _urlBox.Text?.Trim(),
                    Text = _textBox.Text,
                    Title = _titleBox.Text,
                    OpenInNewWindow = _newWindowCheck.IsChecked == true
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            // Enable Insert only when a non-trivial URL is present.
            _urlBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                    _insertButton.IsEnabled = IsValidUrl(_urlBox.Text);
            };

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Address:", _urlBox);
            AddRow(grid, 2, "Text:", _textBox);
            AddRow(grid, 4, "Title:", _titleBox);

            Grid.SetRow(_newWindowCheck, 6);
            Grid.SetColumn(_newWindowCheck, 1);
            _newWindowCheck.Margin = new global::Avalonia.Thickness(0, 4, 0, 0);
            grid.Children.Add(_newWindowCheck);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_insertButton);
            Grid.SetRow(buttonRow, 7);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
        }

        private static void AddRow(Grid grid, int row, string label, Control field)
        {
            var text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new global::Avalonia.Thickness(0, 4, 8, 4),
                MinWidth = 60
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            field.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            Grid.SetRow(field, row);
            Grid.SetColumn(field, 1);
            grid.Children.Add(field);
        }

        internal static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            var trimmed = url.Trim();
            return trimmed.Length > "https://".Length ||
                   (!trimmed.Equals("https://", StringComparison.OrdinalIgnoreCase) &&
                    !trimmed.Equals("http://", StringComparison.OrdinalIgnoreCase) &&
                    trimmed.Length > 0);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<LinkDialogResult> ShowAsync(Window owner, string initialText = null)
        {
            var dialog = new LinkDialog(initialText);
            if (owner != null)
                return await dialog.ShowDialog<LinkDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
