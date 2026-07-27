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

        /// <summary>Display width in px, or null when the field is blank (natural size).</summary>
        public int? WidthPx { get; set; }

        /// <summary>Display height in px, or null when the field is blank (natural size).</summary>
        public int? HeightPx { get; set; }
    }

    /// <summary>
    /// The Picture properties dialog for the Picture Tools contextual tab:
    /// alt text and title, Link To (none / source picture / web address), size
    /// (width/height with aspect-ratio lock, blank = natural size), and layout
    /// (alignment, uniform margin, border). Mirrors the Picture Properties
    /// dialog of Windows Live Writer at a basic level.
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
        private readonly NumericUpDown _widthSpinner;
        private readonly NumericUpDown _heightSpinner;
        private readonly CheckBox _lockAspectCheck;
        private readonly int _naturalWidth;
        private readonly int _naturalHeight;
        private bool _syncingSize;

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

            // Size (Windows parity): width/height prefilled with the current
            // display dims, blank means natural size; Lock aspect ratio (on by
            // default) auto-computes the other dimension from the natural dims.
            _naturalWidth = initial?.NaturalWidth ?? 0;
            _naturalHeight = initial?.NaturalHeight ?? 0;
            _widthSpinner = new NumericUpDown
            {
                Name = "ImageWidthSpinner",
                Minimum = 1, Maximum = 10000, Increment = 1, Width = 112,
                PlaceholderText = "auto",
                Value = initial != null && initial.Width > 0 ? initial.Width : (decimal?)null
            };
            _heightSpinner = new NumericUpDown
            {
                Name = "ImageHeightSpinner",
                Minimum = 1, Maximum = 10000, Increment = 1, Width = 112,
                PlaceholderText = "auto",
                Value = initial != null && initial.Height > 0 ? initial.Height : (decimal?)null
            };
            _lockAspectCheck = new CheckBox
            {
                Name = "ImageLockAspectCheck",
                Content = "Lock aspect ratio",
                IsChecked = true,
                VerticalAlignment = VerticalAlignment.Center
            };
            _widthSpinner.ValueChanged += (s, e) => SyncLinkedSize(fromWidth: true);
            _heightSpinner.ValueChanged += (s, e) => SyncLinkedSize(fromWidth: false);

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
                    BorderColor = WebViewEditor.NormalizeColor(_borderColorBox.Text) ?? "#999999",
                    WidthPx = _widthSpinner.Value.HasValue ? (int?)_widthSpinner.Value.Value : null,
                    HeightPx = _heightSpinner.Value.HasValue ? (int?)_heightSpinner.Value.Value : null
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Alt text:", _altBox);
            AddRow(grid, 1, "Title:", _titleBox);
            AddRow(grid, 2, "Link to:", _linkCombo);
            AddRow(grid, 3, "Address:", _linkUrlBox);
            AddRow(grid, 4, "Alignment:", _alignmentCombo);

            var sizeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            sizeRow.Children.Add(_widthSpinner);
            sizeRow.Children.Add(new TextBlock
            {
                Text = "×",
                VerticalAlignment = VerticalAlignment.Center
            });
            sizeRow.Children.Add(_heightSpinner);
            AddRow(grid, 5, "Size (px):", sizeRow);

            _lockAspectCheck.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            Grid.SetRow(_lockAspectCheck, 6);
            Grid.SetColumn(_lockAspectCheck, 1);
            grid.Children.Add(_lockAspectCheck);

            AddRow(grid, 7, "Margin (px):", _marginSpinner);

            var borderRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            borderRow.Children.Add(_borderWidthSpinner);
            borderRow.Children.Add(new TextBlock
            {
                Text = "Color:",
                VerticalAlignment = VerticalAlignment.Center
            });
            borderRow.Children.Add(_borderColorBox);
            AddRow(grid, 8, "Border (px):", borderRow);

            var note = new TextBlock
            {
                Text = "Border width 0 removes the border.",
                FontSize = 11,
                Opacity = 0.6,
                Margin = new global::Avalonia.Thickness(0, 2, 0, 0)
            };
            Grid.SetRow(note, 9);
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
            Grid.SetRow(buttonRow, 10);
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

        // Aspect-ratio lock: editing one size field recomputes the other from
        // the natural dims (blank when the natural dims are unknown or the
        // edited field was cleared). _syncingSize guards against recursion.
        private void SyncLinkedSize(bool fromWidth)
        {
            if (_syncingSize || _lockAspectCheck.IsChecked != true)
                return;

            decimal? source = fromWidth ? _widthSpinner.Value : _heightSpinner.Value;
            int? linked = null;
            if (source.HasValue && source.Value >= 1)
            {
                linked = fromWidth
                    ? ImageEditBuilder.HeightForWidth(_naturalWidth, _naturalHeight, (int)source.Value)
                    : ImageEditBuilder.WidthForHeight(_naturalWidth, _naturalHeight, (int)source.Value);
            }

            _syncingSize = true;
            try
            {
                if (fromWidth) _heightSpinner.Value = linked;
                else _widthSpinner.Value = linked;
            }
            finally
            {
                _syncingSize = false;
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
