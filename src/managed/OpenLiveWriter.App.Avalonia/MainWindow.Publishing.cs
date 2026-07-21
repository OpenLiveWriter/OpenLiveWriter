// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Accounts;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
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
                _accountService = AccountServiceFactory.CreateDefault(CreatePublishingHttpClient);
            }
            catch (Exception ex)
            {
                // A missing/uninitialized platform context or credential store shouldn't
                // crash the shell; publishing simply stays inert until it's available.
                Console.WriteLine($"[OLW-Accounts] Account service unavailable: {ex.Message}");
                return;
            }

            RefreshBlogSelector();
            UpdateStatusBarExtras();
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
            UpdateBlogStatusLabel();
            UpdateStatus($"Current blog: {_accountService.CurrentAccount?.DisplayLabel}");
            UpdateStatusBarExtras();
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
                case CommandId.ShowCategoryPopup:
                    await ChooseCategoriesAsync();
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
            var fetcher = new HttpRsdFetcher(CreatePublishingHttpClient());
            var verifier = new MetaWeblogConnectionVerifier(CreatePublishingHttpClient);
            AccountDialogResult result = await AccountDialog.ShowAsync(this, fetcher: fetcher, verifier: verifier);
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

        // Fetches the current blog's categories (degrading gracefully to manual entry if
        // the provider returns none or the fetch fails) and lets the user pick which apply
        // to the current post. The selection is stored on the draft so it flows into the
        // newPost struct on publish.
        private async Task ChooseCategoriesAsync()
        {
            BlogAccount account = _accountService.CurrentAccount;
            IReadOnlyList<BlogPostCategory> available = Array.Empty<BlogPostCategory>();

            if (account != null && !string.IsNullOrEmpty(_accountService.GetPassword(account.Id)))
            {
                try
                {
                    IBlogClient client = _accountService.CreateClient(account);
                    available = await client.GetCategoriesAsync(account.BlogId)
                        ?? Array.Empty<BlogPostCategory>();
                }
                catch (Exception ex)
                {
                    // Never block category selection on a transport hiccup — fall back to
                    // manual entry in the dialog.
                    UpdateStatus($"Could not fetch categories: {ex.Message}");
                }
            }

            IEnumerable<string> current = _draftSession?.Current.Categories ?? new List<string>();
            List<string> chosen = await CategoryDialog.ShowAsync(this, available, current);
            if (chosen == null)
                return; // cancelled

            if (_draftSession != null)
            {
                _draftSession.Current.Categories = chosen;
                _draftSession.Current.IsDirty = true;
            }

            UpdateStatus(chosen.Count > 0
                ? $"Categories: {string.Join(", ", chosen)}"
                : "No categories selected.");
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
            string keywords = PostDocument.JoinKeywords(_draftSession?.Current.Keywords);

            if (!await ConfirmPublishRemindersAsync(title, categories))
                return;

            // Re-publishing an already-published document (same blog) edits the same server
            // post rather than creating a duplicate.
            string existingPostId = null;
            if (_draftSession != null &&
                string.Equals(_draftSession.Current.BlogId, account.BlogId, StringComparison.Ordinal))
            {
                existingPostId = _draftSession.Current.PublishedPostId;
                if (string.IsNullOrEmpty(existingPostId))
                    existingPostId = null;
            }

            bool isEdit = !string.IsNullOrEmpty(existingPostId);
            UpdateStatus(isEdit
                ? (publish ? "Updating published post\u2026" : "Updating draft\u2026")
                : (publish ? "Publishing\u2026" : "Posting as draft\u2026"));
            try
            {
                IBlogClient client = _accountService.CreateClient(account);
                string postId = await editor.PublishAsync(
                    client, account.BlogId, existingPostId, title, publish, categories, keywords);

                if (_draftSession != null)
                {
                    _draftSession.Current.BlogId = account.BlogId;
                    _draftSession.Current.PublishedPostId = postId ?? string.Empty;
                    _draftSession.Current.IsPublished = publish;
                }

                string verb = isEdit
                    ? (publish ? "Updated" : "Updated draft")
                    : (publish ? "Published" : "Posted as draft");
                UpdateStatus($"{verb} to {account.DisplayLabel} (post id {postId}).");
                await MessageDialog.ShowAsync(this, verb,
                    publish
                        ? $"Your post was published to \u201c{account.DisplayLabel}\u201d."
                        : $"Your post was saved as a draft on \u201c{account.DisplayLabel}\u201d.");

                ApplyPublishFollowUp(account, publish);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Publish failed: {ex.Message}");
                await MessageDialog.ShowAsync(this, "Publish Failed",
                    $"Could not publish to \u201c{account.DisplayLabel}\u201d:\n\n{ex.Message}");
            }
        }

        // Post-publish follow-ups driven by the General-tab preferences:
        // "View post after publishing" (publish only, never for server drafts) and
        // "Close window after publishing". Split into predicates so the preference
        // mapping is testable headlessly.
        private void ApplyPublishFollowUp(BlogAccount account, bool publish)
        {
            var prefs = _preferences ?? AppPreferences.CreateDefault();

            if (ShouldViewPostAfterPublish(prefs, publish, account))
                BrowserLauncher.Open(account.HomepageUrl);

            if (ShouldCloseAfterPublish(prefs))
                Close(); // the normal unsaved-changes prompt still runs if the draft is dirty
        }

        // MetaWeblog newPost/editPost returns only a post id, so the honest behavior is
        // opening the blog's homepage in the default browser.
        internal static bool ShouldViewPostAfterPublish(AppPreferences prefs, bool publish, BlogAccount account)
        {
            return publish
                && prefs?.ViewPostAfterPublish == true
                && !string.IsNullOrWhiteSpace(account?.HomepageUrl);
        }

        internal static bool ShouldCloseAfterPublish(AppPreferences prefs) =>
            prefs?.CloseWindowOnPublish == true;

        /// <summary>
        /// Enforces General-tab publishing reminders before transmit. Returns false when
        /// the user cancels.
        /// </summary>
        private async Task<bool> ConfirmPublishRemindersAsync(string title, string[] categories)
        {
            var prefs = _preferences ?? AppPreferences.CreateDefault();

            if (prefs.TitleReminder && string.IsNullOrWhiteSpace(title))
            {
                await MessageDialog.ShowAsync(this, "Title Required",
                    "Please enter a title for your post before publishing.");
                return false;
            }

            if (prefs.CategoryReminder && (categories == null || categories.Length == 0))
            {
                bool proceed = await ConfirmDialog.ShowConfirmAsync(
                    this,
                    "Categories",
                    "No categories are selected. Publish anyway?");
                if (!proceed)
                    return false;
            }

            return true;
        }
    }
}
