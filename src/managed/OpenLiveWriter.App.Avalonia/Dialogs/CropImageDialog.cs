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
    /// Result returned from <see cref="CropImageDialog"/> when the user accepts
    /// with OK. Pixel coordinates in the source image's space (clamped to the
    /// image bounds).
    /// </summary>
    public class CropImageDialogResult
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// The Picture Tools crop dialog: numeric X/Y/width/height in source pixels
    /// with the image's dimensions shown and a scaled preview of the picture
    /// being cropped. (An interactive rubber-band crop inside the WebView is
    /// deliberately out of scope.) Defaults to the full image.
    /// </summary>
    public class CropImageDialog : Window
    {
        private readonly int _imageWidth;
        private readonly int _imageHeight;
        private readonly NumericUpDown _xSpinner;
        private readonly NumericUpDown _ySpinner;
        private readonly NumericUpDown _widthSpinner;
        private readonly NumericUpDown _heightSpinner;

        public CropImageDialogResult Result { get; private set; }

        public CropImageDialog(byte[] imageBytes, int imageWidth, int imageHeight)
        {
            _imageWidth = Math.Max(1, imageWidth);
            _imageHeight = Math.Max(1, imageHeight);

            Title = "Crop Picture";
            Width = 440;
            MinWidth = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _xSpinner = new NumericUpDown
            {
                Minimum = 0, Maximum = _imageWidth - 1, Increment = 1, Width = 118, Value = 0
            };
            _ySpinner = new NumericUpDown
            {
                Minimum = 0, Maximum = _imageHeight - 1, Increment = 1, Width = 118, Value = 0
            };
            _widthSpinner = new NumericUpDown
            {
                Minimum = 1, Maximum = _imageWidth, Increment = 1, Width = 118, Value = _imageWidth
            };
            _heightSpinner = new NumericUpDown
            {
                Minimum = 1, Maximum = _imageHeight, Increment = 1, Width = 118, Value = _imageHeight
            };

            var okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };
            okButton.Click += (s, e) =>
            {
                var rect = ImageEditorService.ClampCrop(
                    (int)(_xSpinner.Value ?? 0), (int)(_ySpinner.Value ?? 0),
                    (int)(_widthSpinner.Value ?? _imageWidth), (int)(_heightSpinner.Value ?? _imageHeight),
                    _imageWidth, _imageHeight);
                Result = new CropImageDialogResult
                {
                    X = rect.Left,
                    Y = rect.Top,
                    Width = rect.Width,
                    Height = rect.Height
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            var layout = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 10 };

            // Scaled preview of the picture being cropped (cheap: the bytes are
            // already in memory for baking).
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
                    // Undecodable preview bytes — the numeric crop still works.
                }
            }

            layout.Children.Add(new TextBlock
            {
                Text = $"Image size: {_imageWidth} \u00d7 {_imageHeight} px",
                FontSize = 12,
                Opacity = 0.7
            });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                RowDefinitions = new RowDefinitions("Auto,Auto")
            };
            AddField(grid, 0, "Left (X):", _xSpinner, "Width:", _widthSpinner);
            AddField(grid, 1, "Top (Y):", _ySpinner, "Height:", _heightSpinner);
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

        private static void AddField(Grid grid, int row, string label, Control field,
            string label2, Control field2)
        {
            var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            rowPanel.Children.Add(new TextBlock
            {
                Text = label,
                MinWidth = 64,
                VerticalAlignment = VerticalAlignment.Center
            });
            rowPanel.Children.Add(field);
            rowPanel.Children.Add(new TextBlock
            {
                Text = label2,
                Margin = new global::Avalonia.Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            rowPanel.Children.Add(field2);
            Grid.SetRow(rowPanel, row);
            grid.Children.Add(rowPanel);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// chosen crop rectangle (source-image pixels), or null if cancelled.
        /// </summary>
        public static async Task<CropImageDialogResult> ShowAsync(
            Window owner, byte[] imageBytes, int imageWidth, int imageHeight)
        {
            var dialog = new CropImageDialog(imageBytes, imageWidth, imageHeight);
            if (owner != null)
                return await dialog.ShowDialog<CropImageDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
