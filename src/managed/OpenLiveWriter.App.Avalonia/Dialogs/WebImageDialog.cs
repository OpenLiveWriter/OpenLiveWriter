// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="WebImageDialog"/> when the user inserts a
    /// picture from the web. The URL stays remote — no base64 embedding — so the
    /// publish pipeline (ImagePublisher, which only rewrites data-URIs) leaves it alone.
    /// </summary>
    public class WebImageDialogResult
    {
        public string Url { get; set; }
        public string AltText { get; set; }

        /// <summary>Optional display width in pixels; null when the user left it blank.</summary>
        public int? WidthPx { get; set; }
    }

    /// <summary>
    /// A modal dialog for inserting a picture from the web: image URL (required,
    /// absolute http/https), optional alt text, and an optional display width in
    /// pixels. Mirrors the "From the Web" half of the Windows Insert Picture split.
    /// </summary>
    public class WebImageDialog : Window
    {
        private readonly TextBox _urlBox;
        private readonly TextBox _altBox;
        private readonly TextBox _widthBox;
        private readonly Button _insertButton;

        public WebImageDialogResult Result { get; private set; }

        public WebImageDialog(string initialUrl = null)
        {
            Title = "Insert Picture from the Web";
            Width = 440;
            MinWidth = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _urlBox = new TextBox { PlaceholderText = "https://", Text = initialUrl ?? "https://" };
            _altBox = new TextBox();
            _widthBox = new TextBox { PlaceholderText = "pixels (optional)" };

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
                Result = new WebImageDialogResult
                {
                    Url = _urlBox.Text?.Trim(),
                    AltText = _altBox.Text,
                    WidthPx = ParseWidth(_widthBox.Text)
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            // Enable Insert only when a valid absolute http(s) URL is present and the
            // width field is either blank or a sane positive pixel count.
            _urlBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) UpdateInsertEnabled(); };
            _widthBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) UpdateInsertEnabled(); };

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Address:", _urlBox);
            AddRow(grid, 2, "Alt text:", _altBox);
            AddRow(grid, 4, "Width:", _widthBox);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_insertButton);
            Grid.SetRow(buttonRow, 5);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
            UpdateInsertEnabled();
        }

        private void UpdateInsertEnabled() =>
            _insertButton.IsEnabled = IsValidHttpUrl(_urlBox.Text) && IsValidWidth(_widthBox.Text);

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

        /// <summary>
        /// Stricter than <see cref="LinkDialog.IsValidUrl"/>: a web image must be an
        /// absolute http/https URL with a non-empty host (relative paths and other
        /// schemes can't render as remote images).
        /// </summary>
        internal static bool IsValidHttpUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            return Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                   !string.IsNullOrEmpty(uri.Host);
        }

        /// <summary>
        /// Blank is valid (no width). Otherwise a positive integer pixel count is
        /// required; non-numeric/zero/negative entries are rejected.
        /// </summary>
        internal static bool IsValidWidth(string width) =>
            string.IsNullOrWhiteSpace(width) || ParseWidth(width).HasValue;

        /// <summary>Parses an optional pixel width; null when blank or unparseable.</summary>
        internal static int? ParseWidth(string width)
        {
            if (string.IsNullOrWhiteSpace(width))
                return null;
            return int.TryParse(width.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int px) && px > 0
                ? px
                : (int?)null;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<WebImageDialogResult> ShowAsync(Window owner)
        {
            var dialog = new WebImageDialog();
            if (owner != null)
                return await dialog.ShowDialog<WebImageDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
