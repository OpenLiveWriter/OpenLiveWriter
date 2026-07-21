// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>A find/replace request captured from the dialog fields.</summary>
    public class FindReplaceRequest
    {
        public string Query { get; set; }
        public string Replacement { get; set; }
        public bool MatchCase { get; set; }
        public bool WholeWord { get; set; }
    }

    /// <summary>
    /// A non-modal Find &amp; Replace panel: a search field, a replacement field,
    /// match-case / whole-word options, and Find Next / Replace / Replace All
    /// actions. The dialog owns no editor logic — it raises requests via callbacks
    /// so the host can drive the editor (keeping search/replace logic in the
    /// testable <c>TextFinder</c> / <c>WebViewEditor</c>). The single-Replace
    /// button appears only when an <c>onReplace</c> callback is supplied.
    /// </summary>
    public class FindReplaceDialog : Window
    {
        private readonly TextBox _findBox;
        private readonly TextBox _replaceBox;
        private readonly CheckBox _matchCase;
        private readonly CheckBox _wholeWord;

        private readonly Func<FindReplaceRequest, Task> _onFindNext;
        private readonly Func<FindReplaceRequest, Task> _onReplace;
        private readonly Func<FindReplaceRequest, Task> _onReplaceAll;

        public FindReplaceDialog(
            Func<FindReplaceRequest, Task> onFindNext,
            Func<FindReplaceRequest, Task> onReplaceAll,
            bool showReplace = true,
            Func<FindReplaceRequest, Task> onReplace = null)
        {
            _onFindNext = onFindNext;
            _onReplace = onReplace;
            _onReplaceAll = onReplaceAll;

            Title = showReplace ? "Find and Replace" : "Find";
            Width = 420;
            MinWidth = 360;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _findBox = new TextBox { PlaceholderText = "Find what" };
            _replaceBox = new TextBox { PlaceholderText = "Replace with" };
            _matchCase = new CheckBox { Content = "Match case" };
            _wholeWord = new CheckBox { Content = "Whole word" };

            var findNextButton = new Button { Content = "Find Next", IsDefault = true, MinWidth = 90 };
            var replaceButton = new Button { Content = "Replace", MinWidth = 90 };
            var replaceAllButton = new Button { Content = "Replace All", MinWidth = 90 };
            var closeButton = new Button { Content = "Close", IsCancel = true, MinWidth = 80 };

            findNextButton.Click += async (s, e) => await RaiseAsync(_onFindNext);
            replaceButton.Click += async (s, e) => await RaiseAsync(_onReplace);
            replaceAllButton.Click += async (s, e) => await RaiseAsync(_onReplaceAll);
            closeButton.Click += (s, e) => Close();

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Find:", _findBox);
            if (showReplace)
                AddRow(grid, 1, "Replace:", _replaceBox);

            var optionsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            optionsRow.Children.Add(_matchCase);
            optionsRow.Children.Add(_wholeWord);
            Grid.SetRow(optionsRow, 2);
            Grid.SetColumn(optionsRow, 1);
            grid.Children.Add(optionsRow);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(findNextButton);
            if (showReplace)
            {
                if (_onReplace != null)
                    buttonRow.Children.Add(replaceButton);
                buttonRow.Children.Add(replaceAllButton);
            }
            buttonRow.Children.Add(closeButton);
            Grid.SetRow(buttonRow, 3);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
        }

        /// <summary>The current field values as a request object.</summary>
        internal FindReplaceRequest CurrentRequest() => new()
        {
            Query = _findBox.Text ?? string.Empty,
            Replacement = _replaceBox.Text ?? string.Empty,
            MatchCase = _matchCase.IsChecked == true,
            WholeWord = _wholeWord.IsChecked == true
        };

        private async Task RaiseAsync(Func<FindReplaceRequest, Task> handler)
        {
            if (handler == null) return;
            var request = CurrentRequest();
            if (string.IsNullOrEmpty(request.Query)) return;
            await handler(request);
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
    }
}
