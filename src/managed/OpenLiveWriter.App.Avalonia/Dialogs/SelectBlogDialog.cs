// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Modal picker to choose the current blog from the configured accounts. Returns the
    /// chosen account id, or null if cancelled. Parallels <c>DraftPickerDialog</c>.
    /// </summary>
    public class SelectBlogDialog : Window
    {
        private readonly ListBox _list;

        public string SelectedAccountId { get; private set; }

        public SelectBlogDialog(IReadOnlyList<BlogAccount> accounts, string currentId)
        {
            Title = "Select Blog";
            Width = 400;
            Height = 300;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _list = new ListBox();
            var items = (accounts ?? new List<BlogAccount>())
                .Select(a => new Item(a))
                .ToList();
            _list.ItemsSource = items;

            Item preselect = items.FirstOrDefault(i =>
                string.Equals(i.Account.Id, currentId, System.StringComparison.Ordinal));
            if (preselect != null)
                _list.SelectedItem = preselect;

            var selectButton = new Button { Content = "Select", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            selectButton.Click += (s, e) =>
            {
                SelectedAccountId = (_list.SelectedItem as Item)?.Account.Id;
                Close(SelectedAccountId);
            };
            cancelButton.Click += (s, e) => Close(null);
            _list.DoubleTapped += (s, e) =>
            {
                if (_list.SelectedItem is Item item)
                {
                    SelectedAccountId = item.Account.Id;
                    Close(SelectedAccountId);
                }
            };

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(selectButton);

            var root = new DockPanel { Margin = new global::Avalonia.Thickness(16) };
            DockPanel.SetDock(buttonRow, Dock.Bottom);
            root.Children.Add(buttonRow);
            root.Children.Add(_list);

            Content = root;
        }

        public static async Task<string> ShowAsync(
            Window owner, IReadOnlyList<BlogAccount> accounts, string currentId)
        {
            var dialog = new SelectBlogDialog(accounts, currentId);
            if (owner == null)
                return null;
            return await dialog.ShowDialog<string>(owner);
        }

        private sealed class Item
        {
            public Item(BlogAccount account) => Account = account;
            public BlogAccount Account { get; }
            public override string ToString()
            {
                string label = Account.DisplayLabel;
                if (!string.IsNullOrWhiteSpace(Account.Username))
                    label += $" ({Account.Username})";
                return label;
            }
        }
    }
}
