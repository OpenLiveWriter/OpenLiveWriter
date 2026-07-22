// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>Link choice for a picture in <see cref="ImagePropertiesDialog"/>.</summary>
    public enum ImageLinkChoice
    {
        /// <summary>The picture is not a hyperlink (any existing link is removed).</summary>
        None,

        /// <summary>Link to the picture's own source URL (web pictures only).</summary>
        Source,

        /// <summary>Link to a custom web address.</summary>
        Url
    }

    /// <summary>
    /// Result returned from <see cref="ImagePropertiesDialog"/> when the user
    /// accepts with OK.
    /// </summary>
    public class ImagePropertiesDialogResult
    {
        public string AltText { get; set; }
        public string Title { get; set; }
        public ImageLinkChoice LinkChoice { get; set; }
        public string LinkUrl { get; set; }
        public string Alignment { get; set; }
        public int MarginPx { get; set; }
        public int BorderWidthPx { get; set; }
        public string BorderColor { get; set; }
    }

    /// <summary>
    /// The Picture properties dialog for the Picture Tools contextual tab:
    /// alt text and title, Link To (none / source picture / web address), and
    /// layout (alignment, uniform margin, border). Mirrors the Picture
    /// Properties dialog of Windows Live Writer at a basic level.
    /// </summary>
    public class ImagePropertiesDialog : Window
    {
        private static readonly string[] LinkChoices = { "No link", "Source picture", "Web address" };
        private static readonly string[] Alignments = { "Inline", "Left", "Right", "Center" };

        private readonly TextBox _altBox;
        private readonly TextBox _titleBox;
        private readonly ComboBox _linkCombo;
        private readonly TextBox _linkUrlBox;
        private readonly ComboBox _alignmentCombo;
        private readonly NumericUpDown _marginSpinner;
        private readonly NumericUpDown _borderWidthSpinner;
        private readonly TextBox _borderColorBox;

        public ImagePropertiesDialogResult Result { get; private set; }

        public ImagePropertiesDialog(ImageFormatState initial)
        {
            Title = "Picture Properties";
            Width = 460;
            MinWidth = 380;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _altBox = new TextBox { Text = initial?.Alt ?? string.Empty };
            _titleBox = new TextBox { Text = initial?.Title ?? string.Empty };

            _linkCombo = new ComboBox { MinWidth = 140 };
            foreach (string choice in LinkChoices)
                _linkCombo.Items.Add(new ComboBoxItem { Content = choice });
            _linkUrlBox = new TextBox { PlaceholderText = "https://", IsEnabled = false };

            _alignmentCombo = new ComboBox { MinWidth = 140 };
            foreach (string alignment in Alignments)
                _alignmentCombo.Items.Add(new ComboBoxItem { Content = alignment });

            _marginSpinner = new NumericUpDown
            {
                Minimum = 0, Maximum = 100, Increment = 1, Width = 112,
                Value = initial?.MarginPx ?? 0
            };
            _borderWidthSpinner = new NumericUpDown
            {
                Minimum = 0, Maximum = 20, Increment = 1, Width = 112,
                Value = initial?.BorderWidthPx ?? 0
            };
            _borderColorBox = new TextBox
            {
                Text = initial?.BorderColor ?? "#999999",
                Width = 100
            };

            // Link To prefill: none when unlinked, source when the link targets the
            // picture's own src, otherwise a custom web address.
            ImageLinkChoice initialChoice = InitialLinkChoice(initial);
            _linkCombo.SelectedIndex = (int)initialChoice;
            if (initialChoice == ImageLinkChoice.Url)
            {
                _linkUrlBox.Text = initial?.LinkHref ?? string.Empty;
                _linkUrlBox.IsEnabled = true;
            }
            _linkCombo.SelectionChanged += (s, e) =>
                _linkUrlBox.IsEnabled = _linkCombo.SelectedIndex == (int)ImageLinkChoice.Url;

            _alignmentCombo.SelectedIndex = AlignmentIndex(initial?.Alignment);

            var okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };
            okButton.Click += (s, e) =>
            {
                Result = new ImagePropertiesDialogResult
                {
                    AltText = _altBox.Text ?? string.Empty,
                    Title = _titleBox.Text ?? string.Empty,
                    LinkChoice = (ImageLinkChoice)Math.Max(0, _linkCombo.SelectedIndex),
                    LinkUrl = _linkUrlBox.Text?.Trim(),
                    Alignment = ImageEditBuilder.NormalizeAlignment(
                        ((ComboBoxItem)_alignmentCombo.SelectedItem)?.Content as string),
                    MarginPx = (int)(_marginSpinner.Value ?? 0),
                    BorderWidthPx = (int)(_borderWidthSpinner.Value ?? 0),
                    BorderColor = WebViewEditor.NormalizeColor(_borderColorBox.Text) ?? "#999999"
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Alt text:", _altBox);
            AddRow(grid, 1, "Title:", _titleBox);
            AddRow(grid, 2, "Link to:", _linkCombo);
            AddRow(grid, 3, "Address:", _linkUrlBox);
            AddRow(grid, 4, "Alignment:", _alignmentCombo);
            AddRow(grid, 5, "Margin (px):", _marginSpinner);

            var borderRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            borderRow.Children.Add(_borderWidthSpinner);
            borderRow.Children.Add(new TextBlock
            {
                Text = "Color:",
                VerticalAlignment = VerticalAlignment.Center
            });
            borderRow.Children.Add(_borderColorBox);
            AddRow(grid, 6, "Border (px):", borderRow);

            var note = new TextBlock
            {
                Text = "Border width 0 removes the border.",
                FontSize = 11,
                Opacity = 0.6,
                Margin = new global::Avalonia.Thickness(0, 2, 0, 0)
            };
            Grid.SetRow(note, 7);
            Grid.SetColumn(note, 1);
            grid.Children.Add(note);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(okButton);
            Grid.SetRow(buttonRow, 8);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
        }

        /// <summary>
        /// The Link To choice implied by an image state: none when unlinked,
        /// source when the link targets the picture's own source, else custom URL.
        /// Pure/deterministic so the mapping is unit-testable.
        /// </summary>
        internal static ImageLinkChoice InitialLinkChoice(ImageFormatState initial)
        {
            if (initial == null || string.IsNullOrEmpty(initial.LinkHref))
                return ImageLinkChoice.None;
            return string.Equals(initial.LinkHref, initial.Src, StringComparison.OrdinalIgnoreCase)
                ? ImageLinkChoice.Source
                : ImageLinkChoice.Url;
        }

        /// <summary>
        /// Resolves the href to apply for a dialog result: null removes the link,
        /// Source links to the picture's own src (web pictures only — embedded
        /// data-URI pictures have no meaningful source URL).
        /// </summary>
        internal static string ResolveLinkUrl(ImagePropertiesDialogResult result, ImageFormatState initial)
        {
            if (result == null)
                return null;
            switch (result.LinkChoice)
            {
                case ImageLinkChoice.Source:
                    return initial != null && initial.HasRemoteSource ? initial.Src : null;
                case ImageLinkChoice.Url:
                    return string.IsNullOrWhiteSpace(result.LinkUrl) ? null : result.LinkUrl.Trim();
                default:
                    return null;
            }
        }

        private static int AlignmentIndex(string alignment)
        {
            switch (ImageEditBuilder.NormalizeAlignment(alignment))
            {
                case "left": return 1;
                case "right": return 2;
                case "center": return 3;
                default: return 0;
            }
        }

        private static void AddRow(Grid grid, int row, string label, Control field)
        {
            var text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new global::Avalonia.Thickness(0, 4, 8, 4),
                MinWidth = 80
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            field.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            // Text fields stretch the column; pickers/spinners keep natural width.
            if (!(field is TextBox))
                field.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(field, row);
            Grid.SetColumn(field, 1);
            grid.Children.Add(field);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/>, prefilled from
        /// the selected image's current state, and returns the user's input or
        /// null if cancelled.
        /// </summary>
        public static async Task<ImagePropertiesDialogResult> ShowAsync(Window owner, ImageFormatState initial)
        {
            var dialog = new ImagePropertiesDialog(initial);
            if (owner != null)
                return await dialog.ShowDialog<ImagePropertiesDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
