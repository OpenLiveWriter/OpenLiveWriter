// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// UI-agnostic orchestrator over the account store + credential store. Adds/updates
    /// accounts (persisting metadata to the store and the password to the credential
    /// store), tracks the current account, builds transport clients, and runs the
    /// account-aware publish flow. Fully testable headlessly — no WinForms/WebView and
    /// no live Keychain (inject an <see cref="InMemoryCredentialStore"/> and a fake client
    /// factory).
    /// </summary>
    public sealed class BlogAccountService
    {
        private readonly IAccountStore _accounts;
        private readonly ICredentialStore _credentials;
        private readonly Func<BlogAccount, string, IBlogClient> _clientFactory;
        private readonly Func<HttpClient> _httpClientFactory;

        /// <param name="clientFactory">
        /// Builds an <see cref="IBlogClient"/> from an account + password. Defaults to
        /// <see cref="BlogClientFactory.CreateClient(BlogAccount, string, System.Net.Http.HttpClient)"/>;
        /// tests inject a factory returning a fake client.
        /// </param>
        /// <param name="httpClientFactory">
        /// Optional factory for proxy-aware <see cref="HttpClient"/> instances used by the
        /// default client factory. When null, transports use their built-in shared client.
        /// </param>
        public BlogAccountService(
            IAccountStore accounts,
            ICredentialStore credentials,
            Func<BlogAccount, string, IBlogClient> clientFactory = null,
            Func<HttpClient> httpClientFactory = null)
        {
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _httpClientFactory = httpClientFactory;
            _clientFactory = clientFactory ?? ((a, p) =>
                BlogClientFactory.CreateClient(a, p, _httpClientFactory?.Invoke()));
        }

        /// <summary>Raised after the account list or current selection changes.</summary>
        public event EventHandler AccountsChanged;

        /// <summary>All stored accounts (corrupt entries skipped), ordered by display label.</summary>
        public IReadOnlyList<BlogAccount> ListAccounts() => _accounts.List();

        /// <summary>Whether at least one account is configured.</summary>
        public bool HasAccounts => _accounts.List().Count > 0;

        /// <summary>
        /// Persists <paramref name="account"/> (assigning an id if new) and stores its
        /// <paramref name="password"/> in the credential store (only when non-null; pass
        /// null to leave an existing password untouched during a metadata-only update).
        /// Makes the saved account current if none was selected. Returns the saved account.
        /// </summary>
        public BlogAccount SaveAccount(BlogAccount account, string password)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            BlogAccount saved = _accounts.Save(account);
            if (password != null)
                _credentials.Store(saved.Id, saved.Username ?? string.Empty, password);

            if (string.IsNullOrEmpty(_accounts.CurrentAccountId))
                _accounts.CurrentAccountId = saved.Id;

            AccountsChanged?.Invoke(this, EventArgs.Empty);
            return saved;
        }

        /// <summary>Loads an account by id (null if missing).</summary>
        public BlogAccount GetAccount(string id) => _accounts.Load(id);

        /// <summary>Deletes an account and its stored credential.</summary>
        public void DeleteAccount(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            _accounts.Delete(id);
            _credentials.Delete(id);
            AccountsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The current (last-selected) account, or null if none is selected or the
        /// selected id no longer resolves to a stored account.
        /// </summary>
        public BlogAccount CurrentAccount
        {
            get
            {
                string id = _accounts.CurrentAccountId;
                if (string.IsNullOrEmpty(id)) return null;
                try { return _accounts.Load(id); }
                catch (AccountStoreException) { return null; }
            }
        }

        /// <summary>Sets the current account by id (persisted). No-op if the id is unknown.</summary>
        public void SetCurrentAccount(string id)
        {
            if (string.IsNullOrEmpty(id) || !_accounts.Exists(id))
                return;
            _accounts.CurrentAccountId = id;
            AccountsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Retrieves the stored password for an account id (null if none).</summary>
        public string GetPassword(string accountId)
        {
            var cred = _credentials.Retrieve(accountId);
            return cred?.Password;
        }

        /// <summary>
        /// Builds a transport client for <paramref name="account"/> using its stored
        /// password. Throws <see cref="BlogAccountException"/> when no password is stored.
        /// </summary>
        public IBlogClient CreateClient(BlogAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            var cred = _credentials.Retrieve(account.Id);
            if (cred == null)
                throw new BlogAccountException($"No stored password for account '{account.DisplayLabel}'.");
            return _clientFactory(account, cred.Value.Password);
        }

        /// <summary>
        /// Full account-aware publish flow used by the shell's Publish / Post-as-draft
        /// commands. Resolves the current account + credential, builds a client, pushes
        /// <paramref name="editorHtml"/> through the cross-platform publish pipeline, and
        /// records the returned server post id back on <paramref name="document"/>.
        ///
        /// <paramref name="editorHtml"/> is supplied by the caller so the body can come
        /// from the live editor content. Returns a result describing success or the reason
        /// publishing could not proceed (no account / no credential); never throws for the
        /// "not configured" cases so the UI can prompt gracefully. Transport errors from
        /// the client bubble up as exceptions for the caller to surface.
        /// </summary>
        public async Task<PublishOutcome> PublishAsync(PostDocument document, string editorHtml, bool publish)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            BlogAccount account = CurrentAccount;
            if (account == null)
                return PublishOutcome.NoAccount();

            var cred = _credentials.Retrieve(account.Id);
            if (cred == null)
                return PublishOutcome.NoCredential(account);

            IBlogClient client = _clientFactory(account, cred.Value.Password);

            string[] categories = document.Categories?
                .Where(c => !string.IsNullOrEmpty(c))
                .ToArray() ?? Array.Empty<string>();

            // When the document was already published to this blog, edit the same server
            // post rather than creating a duplicate. Republishing to a different blog
            // creates a fresh post.
            string existingPostId =
                string.Equals(document.BlogId, account.BlogId, StringComparison.Ordinal)
                    ? document.PublishedPostId
                    : null;

            string postId = await EditorContentPublisher.PublishOrEditAsync(
                client, account.BlogId, existingPostId, document.Title ?? string.Empty,
                editorHtml ?? string.Empty, publish, categories,
                isPage: document.IsPage).ConfigureAwait(false);

            document.BlogId = account.BlogId;
            document.PublishedPostId = postId;
            document.IsPublished = publish;

            return PublishOutcome.Ok(account, postId);
        }
    }

    /// <summary>Thrown for account/credential resolution problems (not transport faults).</summary>
    public class BlogAccountException : Exception
    {
        public BlogAccountException(string message) : base(message) { }
    }

    /// <summary>Result of a publish attempt through <see cref="BlogAccountService.PublishAsync"/>.</summary>
    public sealed class PublishOutcome
    {
        public enum ResultStatus { Success, NoAccountConfigured, NoCredential }

        public ResultStatus Status { get; private set; }
        public BlogAccount Account { get; private set; }
        public string PostId { get; private set; }

        public bool Succeeded => Status == ResultStatus.Success;

        public static PublishOutcome Ok(BlogAccount account, string postId) =>
            new PublishOutcome { Status = ResultStatus.Success, Account = account, PostId = postId };

        public static PublishOutcome NoAccount() =>
            new PublishOutcome { Status = ResultStatus.NoAccountConfigured };

        public static PublishOutcome NoCredential(BlogAccount account) =>
            new PublishOutcome { Status = ResultStatus.NoCredential, Account = account };
    }
}
