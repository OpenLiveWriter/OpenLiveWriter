// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Modal list of saved drafts (most-recent first) for the Open Drafts command.
    /// Double-click or Open returns the selected draft id; Cancel returns null. An
    /// "Open from Blog…" button lets the user pivot to fetching a post from the
    /// server instead (signaled via <see cref="RequestedOpenFromBlog"/>).
    /// </summary>
    public class DraftPickerDialog : Window
    {
        private readonly ListBox _list;
        private readonly Button _openButton;

        /// <summary>The id of the draft the user chose, or null if cancelled.</summary>
        public string SelectedDraftId { get; private set; }

        /// <summary>
        /// True when the user chose "Open from Blog…" rather than a local draft —
        /// the caller should route to the open-from-blog flow instead.
        /// </summary>
        public bool RequestedOpenFromBlog { get; private set; }

        public DraftPickerDialog(IReadOnlyList<DraftInfo> drafts)
        {
            Title = "Open Draft";
            Width = 480;
            Height = 360;
            MinWidth = 360;
            MinHeight = 280;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _list = new ListBox { Margin = new global::Avalonia.Thickness(0, 0, 0, 12) };
            foreach (var draft in drafts ?? Array.Empty<DraftInfo>())
            {
                _list.Items.Add(new ListBoxItem
                {
                    Content = $"{draft.DisplayTitle}   \u2014   {FormatModified(draft.DateModifiedUtc)}",
                    Tag = draft.Id
                });
            }

            _openButton = new Button { Content = "Open", IsDefault = true, MinWidth = 80, IsEnabled = false };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };
            var fromBlogButton = new Button { Content = "Open from Blog\u2026", MinWidth = 120 };

            _list.SelectionChanged += (s, e) => _openButton.IsEnabled = _list.SelectedItem != null;
            _list.DoubleTapped += (s, e) => { if (_list.SelectedItem != null) Confirm(); };
            _openButton.Click += (s, e) => Confirm();
            fromBlogButton.Click += (s, e) =>
            {
                RequestedOpenFromBlog = true;
                Close(null);
            };
            cancelButton.Click += (s, e) => Close(null);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };
            buttonRow.Children.Add(fromBlogButton);
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_openButton);

            var layout = new DockPanel { Margin = new global::Avalonia.Thickness(16) };
            var header = new TextBlock
            {
                Text = (drafts == null || drafts.Count == 0)
                    ? "No saved drafts."
                    : "Select a draft to open:",
                Margin = new global::Avalonia.Thickness(0, 0, 0, 8)
            };
            DockPanel.SetDock(header, Dock.Top);
            DockPanel.SetDock(buttonRow, Dock.Bottom);
            layout.Children.Add(header);
            layout.Children.Add(buttonRow);
            layout.Children.Add(_list);

            Content = layout;
        }

        private void Confirm()
        {
            SelectedDraftId = (_list.SelectedItem as ListBoxItem)?.Tag as string;
            Close(SelectedDraftId);
        }

        private static string FormatModified(DateTime utc) =>
            utc == default
                ? "unsaved"
                : utc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);

        /// <summary>
        /// Shows the picker modally and returns the chosen draft id, or null if
        /// cancelled / no owner window.
        /// </summary>
        public static async Task<string> ShowAsync(Window owner, IReadOnlyList<DraftInfo> drafts)
        {
            var dialog = new DraftPickerDialog(drafts);
            if (owner == null) return null;
            return await dialog.ShowDialog<string>(owner);
        }
    }
}
