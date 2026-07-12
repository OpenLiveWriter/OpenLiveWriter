// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="TableDialog"/> when the user inserts a table.
    /// </summary>
    public class TableDialogResult
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool HeaderRow { get; set; }
        public string Width { get; set; }
    }

    /// <summary>
    /// A modal dialog for inserting a table: rows, columns, an optional header row,
    /// and an optional width. Mirrors the fields of the Windows "Insert Table"
    /// dialog at a basic level.
    /// </summary>
    public class TableDialog : Window
    {
        private readonly NumericUpDown _rowsBox;
        private readonly NumericUpDown _columnsBox;
        private readonly CheckBox _headerCheck;
        private readonly TextBox _widthBox;

        public TableDialogResult Result { get; private set; }

        public TableDialog()
        {
            Title = "Insert Table";
            Width = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _rowsBox = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 2, Increment = 1, FormatString = "0" };
            _columnsBox = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 2, Increment = 1, FormatString = "0" };
            _headerCheck = new CheckBox { Content = "Include header row", IsChecked = true };
            _widthBox = new TextBox { PlaceholderText = "e.g. 100% or 500" };

            var insertButton = new Button { Content = "Insert", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            insertButton.Click += (s, e) =>
            {
                Result = new TableDialogResult
                {
                    Rows = (int)(_rowsBox.Value ?? 2),
                    Columns = (int)(_columnsBox.Value ?? 2),
                    HeaderRow = _headerCheck.IsChecked == true,
                    Width = _widthBox.Text
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Rows:", _rowsBox);
            AddRow(grid, 1, "Columns:", _columnsBox);
            AddRow(grid, 2, "Width:", _widthBox);

            Grid.SetRow(_headerCheck, 3);
            Grid.SetColumn(_headerCheck, 1);
            _headerCheck.Margin = new global::Avalonia.Thickness(0, 4, 0, 0);
            grid.Children.Add(_headerCheck);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(insertButton);
            Grid.SetRow(buttonRow, 4);
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
                MinWidth = 70
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            field.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            field.HorizontalAlignment = HorizontalAlignment.Left;
            if (field is NumericUpDown) field.Width = 120;
            Grid.SetRow(field, row);
            Grid.SetColumn(field, 1);
            grid.Children.Add(field);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<TableDialogResult> ShowAsync(Window owner)
        {
            var dialog = new TableDialog();
            if (owner != null)
                return await dialog.ShowDialog<TableDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
