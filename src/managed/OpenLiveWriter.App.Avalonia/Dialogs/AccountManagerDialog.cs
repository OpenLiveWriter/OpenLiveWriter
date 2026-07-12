// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Manage-accounts dialog: lists configured blog accounts and lets the user add,
    /// edit, delete, and choose the current one. Operates directly on the supplied
    /// <see cref="BlogAccountService"/> (metadata to the account store, passwords to the
    /// credential store). This is the reachable entry point for account management given
    /// the ribbon's blog-selector dropdown is presentation-only today.
    /// </summary>
    public class AccountManagerDialog : Window
    {
        private readonly BlogAccountService _service;
        private readonly ListBox _list;
        private readonly Button _editButton;
        private readonly Button _deleteButton;
        private readonly Button _setCurrentButton;

        public AccountManagerDialog(BlogAccountService service)
        {
            _service = service;

            Title = "Blog Accounts";
            Width = 460;
            Height = 340;
            MinWidth = 400;
            MinHeight = 280;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _list = new ListBox { Margin = new global::Avalonia.Thickness(0, 0, 0, 8) };
            _list.SelectionChanged += (s, e) => UpdateButtons();

            var addButton = new Button { Content = "Add\u2026", MinWidth = 90 };
            _editButton = new Button { Content = "Edit\u2026", MinWidth = 90, IsEnabled = false };
            _deleteButton = new Button { Content = "Delete", MinWidth = 90, IsEnabled = false };
            _setCurrentButton = new Button { Content = "Set as Current", MinWidth = 110, IsEnabled = false };
            var closeButton = new Button { Content = "Close", IsCancel = true, MinWidth = 90 };

            addButton.Click += async (s, e) => await AddAsync();
            _editButton.Click += async (s, e) => await EditAsync();
            _deleteButton.Click += async (s, e) => await DeleteAsync();
            _setCurrentButton.Click += (s, e) => SetCurrent();
            closeButton.Click += (s, e) => Close();

            var actions = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Margin = new global::Avalonia.Thickness(8, 0, 0, 0)
            };
            actions.Children.Add(addButton);
            actions.Children.Add(_editButton);
            actions.Children.Add(_deleteButton);
            actions.Children.Add(_setCurrentButton);

            var body = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };
            Grid.SetColumn(_list, 0);
            Grid.SetColumn(actions, 1);
            body.Children.Add(_list);
            body.Children.Add(actions);

            var root = new DockPanel { Margin = new global::Avalonia.Thickness(16) };
            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            footer.Children.Add(closeButton);
            DockPanel.SetDock(footer, Dock.Bottom);
            root.Children.Add(footer);
            root.Children.Add(body);

            Content = root;

            Refresh();
        }

        private void Refresh()
        {
            string currentId = _service?.CurrentAccount?.Id;
            var items = _service?.ListAccounts() ?? new List<BlogAccount>();

            _list.ItemsSource = items
                .Select(a => new AccountListItem(a, string.Equals(a.Id, currentId, System.StringComparison.Ordinal)))
                .ToList();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool hasSelection = _list.SelectedItem is AccountListItem;
            _editButton.IsEnabled = hasSelection;
            _deleteButton.IsEnabled = hasSelection;
            _setCurrentButton.IsEnabled = hasSelection;
        }

        private BlogAccount Selected => (_list.SelectedItem as AccountListItem)?.Account;

        private async Task AddAsync()
        {
            AccountDialogResult result = await AccountDialog.ShowAsync(this);
            if (result?.Account == null)
                return;
            _service.SaveAccount(result.Account, result.Password);
            Refresh();
        }

        private async Task EditAsync()
        {
            BlogAccount selected = Selected;
            if (selected == null) return;

            AccountDialogResult result = await AccountDialog.ShowAsync(this, selected);
            if (result?.Account == null)
                return;
            _service.SaveAccount(result.Account, result.Password);
            Refresh();
        }

        private async Task DeleteAsync()
        {
            BlogAccount selected = Selected;
            if (selected == null) return;

            bool confirmed = await ConfirmDialog.ShowConfirmAsync(
                this, "Delete Account",
                $"Remove the blog account \u201c{selected.DisplayLabel}\u201d? "
                + "Its saved password will also be removed.");
            if (!confirmed)
                return;

            _service.DeleteAccount(selected.Id);
            Refresh();
        }

        private void SetCurrent()
        {
            BlogAccount selected = Selected;
            if (selected == null) return;
            _service.SetCurrentAccount(selected.Id);
            Refresh();
        }

        /// <summary>Shows the manager modally over <paramref name="owner"/> (no-op if null).</summary>
        public static async Task ShowAsync(Window owner, BlogAccountService service)
        {
            if (owner == null)
                return;
            var dialog = new AccountManagerDialog(service);
            await dialog.ShowDialog(owner);
        }

        private sealed class AccountListItem
        {
            public AccountListItem(BlogAccount account, bool isCurrent)
            {
                Account = account;
                IsCurrent = isCurrent;
            }

            public BlogAccount Account { get; }
            public bool IsCurrent { get; }

            public override string ToString()
            {
                string label = Account.DisplayLabel;
                if (!string.IsNullOrWhiteSpace(Account.Username))
                    label += $" ({Account.Username})";
                return IsCurrent ? "\u2714 " + label : label;
            }
        }
    }
}
