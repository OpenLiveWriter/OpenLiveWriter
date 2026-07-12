// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Simple category selection dialog: a checklist of the categories the blog reported
    /// (via <c>metaWeblog.getCategories</c>), pre-checked for the currently selected ones,
    /// plus a free-text field to add categories the provider didn't list. Degrades
    /// gracefully when the provider returns no categories — the checklist is replaced with
    /// a hint and the user can still type category names.
    ///
    /// Returns the chosen category names, or null if cancelled.
    /// </summary>
    public class CategoryDialog : Window
    {
        private readonly List<CheckBox> _checkBoxes = new List<CheckBox>();
        private readonly TextBox _customBox;

        public List<string> SelectedCategories { get; private set; }

        public CategoryDialog(IReadOnlyList<BlogPostCategory> available, IEnumerable<string> selectedNames)
        {
            Title = "Categories";
            Width = 360;
            Height = 420;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var selected = new HashSet<string>(
                selectedNames?.Where(n => !string.IsNullOrWhiteSpace(n)) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            var listPanel = new StackPanel { Spacing = 4 };
            if (available != null && available.Count > 0)
            {
                foreach (BlogPostCategory category in available)
                {
                    var cb = new CheckBox
                    {
                        Content = category.Name,
                        IsChecked = selected.Contains(category.Name),
                        Tag = category.Name
                    };
                    _checkBoxes.Add(cb);
                    listPanel.Children.Add(cb);
                }
            }
            else
            {
                listPanel.Children.Add(new TextBlock
                {
                    Text = "This blog didn't report any categories. You can type category "
                         + "names below (separate multiple with commas).",
                    TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 12,
                    Foreground = new global::Avalonia.Media.SolidColorBrush(
                        global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66))
                });
            }

            // Custom categories default to any currently-selected names the provider list
            // didn't cover, so an edit round-trips them.
            var known = new HashSet<string>(
                (available ?? new List<BlogPostCategory>()).Select(c => c.Name),
                StringComparer.OrdinalIgnoreCase);
            string customDefault = string.Join(", ",
                selected.Where(s => !known.Contains(s)));

            _customBox = new TextBox
            {
                PlaceholderText = "Add categories (comma-separated)",
                Text = customDefault
            };

            var okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            okButton.Click += (s, e) =>
            {
                SelectedCategories = CollectSelection();
                Close(SelectedCategories);
            };
            cancelButton.Click += (s, e) => Close(null);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(okButton);

            var root = new DockPanel { Margin = new global::Avalonia.Thickness(16) };
            DockPanel.SetDock(buttonRow, Dock.Bottom);

            var customLabel = new TextBlock
            {
                Text = "Other categories:",
                Margin = new global::Avalonia.Thickness(0, 12, 0, 4)
            };
            DockPanel.SetDock(customLabel, Dock.Bottom);
            DockPanel.SetDock(_customBox, Dock.Bottom);

            root.Children.Add(buttonRow);
            root.Children.Add(_customBox);
            root.Children.Add(customLabel);
            root.Children.Add(new ScrollViewer { Content = listPanel });

            Content = root;
        }

        private List<string> CollectSelection()
        {
            IEnumerable<string> checkedNames = _checkBoxes
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag as string);

            return MergeSelection(checkedNames, _customBox.Text);
        }

        /// <summary>
        /// Merges checked category names with a comma/newline-separated custom string,
        /// trimming, dropping blanks, and de-duplicating case-insensitively while keeping
        /// first-seen order. Pure so it can be unit-tested without the UI.
        /// </summary>
        internal static List<string> MergeSelection(IEnumerable<string> checkedNames, string customText)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void Add(string name)
            {
                string trimmed = name?.Trim();
                if (string.IsNullOrEmpty(trimmed)) return;
                if (seen.Add(trimmed))
                    result.Add(trimmed);
            }

            foreach (string name in checkedNames ?? Enumerable.Empty<string>())
                Add(name);

            if (!string.IsNullOrWhiteSpace(customText))
            {
                foreach (string part in customText.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    Add(part);
            }

            return result;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the chosen
        /// category names, or null if cancelled / headless (null owner).
        /// </summary>
        public static async Task<List<string>> ShowAsync(
            Window owner, IReadOnlyList<BlogPostCategory> available, IEnumerable<string> selectedNames)
        {
            var dialog = new CategoryDialog(available, selectedNames);
            if (owner == null)
                return null;
            return await dialog.ShowDialog<List<string>>(owner);
        }
    }
}
