// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Globalization;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Modal statistics dialog mirroring the Windows Word Count form: words,
    /// characters (with and without spaces), and paragraphs for the current
    /// document.
    /// </summary>
    public class WordCountDialog : Window
    {
        public WordCountDialog(WordCounter counter)
        {
            Title = "Word Count";
            Width = 300;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            AddStat(grid, 0, "Words", counter.Words);
            AddStat(grid, 1, "Characters (with spaces)", counter.Chars);
            AddStat(grid, 2, "Characters (no spaces)", counter.CharsWithoutSpaces);
            AddStat(grid, 3, "Paragraphs", counter.Paragraphs);

            var close = new Button
            {
                Content = "Close",
                IsDefault = true,
                IsCancel = true,
                MinWidth = 80,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            close.Click += (s, e) => Close();
            Grid.SetRow(close, 4);
            Grid.SetColumn(close, 0);
            Grid.SetColumnSpan(close, 2);
            grid.Children.Add(close);

            Content = grid;
        }

        private static void AddStat(Grid grid, int row, string label, int value)
        {
            var labelText = new TextBlock
            {
                Text = label,
                Margin = new global::Avalonia.Thickness(0, 3, 16, 3)
            };
            Grid.SetRow(labelText, row);
            Grid.SetColumn(labelText, 0);
            grid.Children.Add(labelText);

            var valueText = new TextBlock
            {
                Text = value.ToString("N0", CultureInfo.CurrentCulture),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new global::Avalonia.Thickness(0, 3, 0, 3)
            };
            Grid.SetRow(valueText, row);
            Grid.SetColumn(valueText, 1);
            grid.Children.Add(valueText);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/>. A null owner
        /// (headless) is a no-op so the counting logic stays testable without a UI.
        /// </summary>
        public static async Task ShowAsync(Window owner, WordCounter counter)
        {
            var dialog = new WordCountDialog(counter);
            if (owner == null) return;
            await dialog.ShowDialog(owner);
        }
    }
}
