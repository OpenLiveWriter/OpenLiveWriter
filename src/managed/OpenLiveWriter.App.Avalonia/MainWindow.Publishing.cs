// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Accounts;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;
using OpenLiveWriter.Ribbon.Avalonia.Controls;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Account setup + publishing behavior for the shell: wires the AddWeblog /
    /// ConfigureWeblog / Accounts / SelectBlog commands and the PostAndPublish /
    /// PostAsDraft flow onto the cross-platform <see cref="BlogAccountService"/>.
    /// </summary>
    public partial class MainWindow
    {
        private void InitializeAccounts()
        {
            try
            {
                _accountService = AccountServiceFactory.CreateDefault();
            }
            catch (Exception ex)
            {
                // A missing/uninitialized platform context or credential store shouldn't
                // crash the shell; publishing simply stays inert until it's available.
                Console.WriteLine($"[OLW-Accounts] Account service unavailable: {ex.Message}");
                return;
            }

            RefreshBlogSelector();
        }

        // Fills the ribbon's blog-selector dropdown from the stored accounts and
        // reflects the current selection.
        private void RefreshBlogSelector()
        {
            if (_ribbon == null || _accountService == null)
                return;

            var items = _accountService.ListAccounts()
                .Select(a => new RibbonGalleryItem(a.Id, BlogSelectorLabel(a)))
                .ToList();

            _ribbon.SetDropDownItems(CommandId.SelectBlog, items, _accountService.CurrentAccount?.Id);
        }

        private static string BlogSelectorLabel(BlogAccount account)
        {
            string label = account.DisplayLabel;
            if (!string.IsNullOrWhiteSpace(account.Username))
                label += $" ({account.Username})";
            return label;
        }

        // Blog selector dropdown selection → set current account (persisted).
        private void OnBlogSelectorChanged(string accountId)
        {
            if (_accountService == null || string.IsNullOrEmpty(accountId))
                return;

            _accountService.SetCurrentAccount(accountId);
            UpdateStatus($"Current blog: {_accountService.CurrentAccount?.DisplayLabel}");
        }

        private async Task<bool> TryHandlePublishCommandAsync(CommandId commandId)
        {
            if (_accountService == null)
                return false;

            switch (commandId)
            {
                case CommandId.AddWeblog:
                    await AddAccountAsync();
                    return true;
                case CommandId.ConfigureWeblog:
                case CommandId.Accounts:
                    await ManageAccountsAsync();
                    return true;
                case CommandId.SelectBlog:
                    await SelectBlogAsync();
                    return true;
                case CommandId.PostAndPublish:
                    await PublishCurrentAsync(publish: true);
                    return true;
                case CommandId.PostAsDraft:
                case CommandId.PostAsDraftAndEditOnline:
                    await PublishCurrentAsync(publish: false);
                    return true;
                default:
                    return false;
            }
        }

        private async Task AddAccountAsync()
        {
            AccountDialogResult result = await AccountDialog.ShowAsync(this);
            if (result?.Account == null)
                return;

            BlogAccount saved = _accountService.SaveAccount(result.Account, result.Password);
            RefreshBlogSelector();
            UpdateStatus($"Added blog account: {saved.DisplayLabel}");
        }

        private async Task ManageAccountsAsync()
        {
            await AccountManagerDialog.ShowAsync(this, _accountService);
            RefreshBlogSelector();
            BlogAccount current = _accountService.CurrentAccount;
            UpdateStatus(current != null ? $"Current blog: {current.DisplayLabel}" : "No blog selected.");
        }

        private async Task SelectBlogAsync()
        {
            var accounts = _accountService.ListAccounts();
            if (accounts.Count == 0)
            {
                await MessageDialog.ShowAsync(this, "No Blog Accounts",
                    "No blog accounts are configured yet. Add one from the Blog Account tab first.");
                return;
            }

            string id = await SelectBlogDialog.ShowAsync(this, accounts, _accountService.CurrentAccount?.Id);
            if (string.IsNullOrEmpty(id))
                return;

            _accountService.SetCurrentAccount(id);
            RefreshBlogSelector();
            UpdateStatus($"Current blog: {_accountService.CurrentAccount?.DisplayLabel}");
        }

        // Core publish flow. publish=true → PostAndPublish; publish=false → post as a
        // server-side draft (PostAsDraft / PostAsDraftAndEditOnline). Body comes from the
        // live editor content via WebViewEditor.PublishAsync.
        private async Task PublishCurrentAsync(bool publish)
        {
            BlogAccount account = _accountService.CurrentAccount;
            if (account == null)
            {
                string message = _accountService.HasAccounts
                    ? "No blog is selected. Choose a blog from the blog selector first."
                    : "No blog account is configured. Add a blog account from the Blog Account tab first.";
                await MessageDialog.ShowAsync(this, "Cannot Publish", message);
                return;
            }

            string password = _accountService.GetPassword(account.Id);
            if (string.IsNullOrEmpty(password))
            {
                await MessageDialog.ShowAsync(this, "Cannot Publish",
                    $"No stored password for \u201c{account.DisplayLabel}\u201d. Re-open the account settings and re-enter it.");
                return;
            }

            WebViewEditor editor = GetEditor();
            if (editor == null)
            {
                UpdateStatus("Editor not ready.");
                return;
            }

            string title = _titleEditor?.Text ?? _draftSession?.Current.Title ?? string.Empty;
            string[] categories = _draftSession?.Current.Categories?.ToArray() ?? Array.Empty<string>();

            UpdateStatus(publish ? "Publishing\u2026" : "Posting as draft\u2026");
            try
            {
                IBlogClient client = _accountService.CreateClient(account);
                string postId = await editor.PublishAsync(client, account.BlogId, title, publish, categories);

                if (_draftSession != null)
                {
                    _draftSession.Current.BlogId = account.BlogId;
                    _draftSession.Current.PublishedPostId = postId ?? string.Empty;
                    _draftSession.Current.IsPublished = publish;
                }

                string verb = publish ? "Published" : "Posted as draft";
                UpdateStatus($"{verb} to {account.DisplayLabel} (post id {postId}).");
                await MessageDialog.ShowAsync(this, verb,
                    publish
                        ? $"Your post was published to \u201c{account.DisplayLabel}\u201d."
                        : $"Your post was saved as a draft on \u201c{account.DisplayLabel}\u201d.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Publish failed: {ex.Message}");
                await MessageDialog.ShowAsync(this, "Publish Failed",
                    $"Could not publish to \u201c{account.DisplayLabel}\u201d:\n\n{ex.Message}");
            }
        }
    }
}
