// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="MapDialog"/> when the user inserts a map.
    /// </summary>
    public class MapDialogResult
    {
        /// <summary>A place name / caption (used for a search link or as the map caption).</summary>
        public string Label { get; set; }

        /// <summary>Latitude/longitude text (e.g. "37.7749, -122.4194"); optional.</summary>
        public string Coordinates { get; set; }

        /// <summary>Zoom level for the embedded map.</summary>
        public int Zoom { get; set; } = MapEmbedBuilder.DefaultZoom;
    }

    /// <summary>
    /// A modal dialog for inserting a map. The Windows Bing/Virtual Earth map picker is
    /// long dead; this collects a place name and/or coordinates that
    /// <see cref="MapEmbedBuilder"/> turns into an OpenStreetMap embed (no API key).
    /// </summary>
    public class MapDialog : Window
    {
        private readonly TextBox _labelBox;
        private readonly TextBox _coordsBox;
        private readonly NumericUpDown _zoom;
        private readonly Button _insertButton;

        public MapDialogResult Result { get; private set; }

        public MapDialog()
        {
            Title = "Insert Map";
            Width = 460;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _labelBox = new TextBox { PlaceholderText = "Place name or caption (e.g. Golden Gate Bridge)" };
            _coordsBox = new TextBox { PlaceholderText = "Latitude, Longitude (e.g. 37.8199, -122.4783)" };
            _zoom = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 19,
                Increment = 1,
                Value = MapEmbedBuilder.DefaultZoom,
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            _insertButton = new Button { Content = "Insert", IsDefault = true, MinWidth = 80, IsEnabled = false };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _insertButton.Click += (s, e) =>
            {
                Result = new MapDialogResult
                {
                    Label = _labelBox.Text?.Trim(),
                    Coordinates = _coordsBox.Text?.Trim(),
                    Zoom = (int)(_zoom.Value ?? MapEmbedBuilder.DefaultZoom)
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            void UpdateEnabled(object s, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
            {
                if (e.Property == TextBox.TextProperty)
                    _insertButton.IsEnabled =
                        !string.IsNullOrWhiteSpace(_labelBox.Text) ||
                        !string.IsNullOrWhiteSpace(_coordsBox.Text);
            }
            _labelBox.PropertyChanged += UpdateEnabled;
            _coordsBox.PropertyChanged += UpdateEnabled;

            var stack = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = "Place name:" });
            stack.Children.Add(_labelBox);
            stack.Children.Add(new TextBlock { Text = "Coordinates (optional):" });
            stack.Children.Add(_coordsBox);
            var zoomRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
            zoomRow.Children.Add(new TextBlock { Text = "Zoom:", VerticalAlignment = VerticalAlignment.Center });
            zoomRow.Children.Add(_zoom);
            stack.Children.Add(zoomRow);
            stack.Children.Add(new TextBlock
            {
                Text = "With coordinates an interactive OpenStreetMap is embedded; " +
                       "with only a place name a map search link is inserted.",
                FontSize = 11,
                Foreground = global::Avalonia.Media.Brushes.Gray,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_insertButton);
            stack.Children.Add(buttonRow);

            Content = stack;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<MapDialogResult> ShowAsync(Window owner)
        {
            var dialog = new MapDialog();
            if (owner != null)
                return await dialog.ShowDialog<MapDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
