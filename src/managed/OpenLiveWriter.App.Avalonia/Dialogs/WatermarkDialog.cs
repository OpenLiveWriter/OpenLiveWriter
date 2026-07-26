// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media.Imaging;
using OpenLiveWriter.App.Avalonia.ImageEditing;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="WatermarkDialog"/> when the user accepts
    /// with OK. Opacity is a percent (0-100); position anchors the text.
    /// </summary>
    public class WatermarkDialogResult
    {
        public string Text { get; set; }
        public int SizePx { get; set; }
        public int OpacityPercent { get; set; }
        public WatermarkPosition Position { get; set; }
    }

    /// <summary>
    /// The Picture Tools watermark dialog: text, font size (px), opacity
    /// (0-100%) and position (the five anchors Windows Live Writer offered),
    /// with a scaled preview of the picture being watermarked. The watermark is
    /// baked into the pixels by <see cref="ImageEditorService.AddTextWatermark"/>
    /// after OK.
    /// </summary>
    public class WatermarkDialog : Window
    {
        private static readonly (string Label, WatermarkPosition Position)[] Positions =
        {
            ("Top left", WatermarkPosition.TopLeft),
            ("Top right", WatermarkPosition.TopRight),
            ("Bottom left", WatermarkPosition.BottomLeft),
            ("Bottom right", WatermarkPosition.BottomRight),
            ("Center", WatermarkPosition.Center),
        };

        private readonly TextBox _textBox;
        private readonly NumericUpDown _sizeSpinner;
        private readonly NumericUpDown _opacitySpinner;
        private readonly ComboBox _positionCombo;

        public WatermarkDialogResult Result { get; private set; }

        public WatermarkDialog(byte[] imageBytes, string initialText = null)
        {
            Title = "Watermark";
            Width = 440;
            MinWidth = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _textBox = new TextBox { PlaceholderText = "Watermark text", Text = initialText };
            _sizeSpinner = new NumericUpDown
            {
                Minimum = 6, Maximum = 200, Increment = 1, Width = 118, Value = 24
            };
            _opacitySpinner = new NumericUpDown
            {
                Minimum = 0, Maximum = 100, Increment = 5, Width = 118, Value = 60
            };
            _positionCombo = new ComboBox { Width = 160 };
            foreach (var (label, _) in Positions)
                _positionCombo.Items.Add(label);
            _positionCombo.SelectedIndex = 3; // Bottom right, like Windows.

            var okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };
            okButton.Click += (s, e) =>
            {
                string text = _textBox.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    Close(null);
                    return;
                }

                Result = new WatermarkDialogResult
                {
                    Text = text,
                    SizePx = (int)(_sizeSpinner.Value ?? 24),
                    OpacityPercent = (int)(_opacitySpinner.Value ?? 60),
                    Position = Positions[Math.Max(0, _positionCombo.SelectedIndex)].Position
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            var layout = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 10 };

            // Scaled preview of the picture being watermarked (cheap: the bytes
            // are already in memory for baking).
            if (imageBytes != null && imageBytes.Length > 0)
            {
                try
                {
                    layout.Children.Add(new Border
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Child = new Image
                        {
                            Source = new Bitmap(new MemoryStream(imageBytes)),
                            MaxWidth = 400,
                            MaxHeight = 220
                        }
                    });
                }
                catch (Exception)
                {
                    // Undecodable preview bytes — the watermark fields still work.
                }
            }

            layout.Children.Add(new TextBlock { Text = "Text:" });
            layout.Children.Add(_textBox);

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
                RowSpacing = 8
            };
            AddField(grid, 0, "Size (px):", _sizeSpinner);
            AddField(grid, 1, "Opacity (%):", _opacitySpinner);
            AddField(grid, 2, "Position:", _positionCombo);
            layout.Children.Add(grid);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(okButton);
            layout.Children.Add(buttonRow);

            Content = layout;
        }

        private static void AddField(Grid grid, int row, string label, Control field)
        {
            var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            rowPanel.Children.Add(new TextBlock
            {
                Text = label,
                MinWidth = 84,
                VerticalAlignment = VerticalAlignment.Center
            });
            rowPanel.Children.Add(field);
            Grid.SetRow(rowPanel, row);
            grid.Children.Add(rowPanel);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// chosen watermark settings, or null if cancelled (or the text is empty).
        /// </summary>
        public static async Task<WatermarkDialogResult> ShowAsync(Window owner, byte[] imageBytes)
        {
            var dialog = new WatermarkDialog(imageBytes);
            if (owner != null)
                return await dialog.ShowDialog<WatermarkDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
