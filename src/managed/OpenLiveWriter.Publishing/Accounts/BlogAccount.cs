// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text.Json.Serialization;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Cross-platform blog account metadata — the persisted description of a weblog
    /// the user publishes to. This is the macOS counterpart to the Windows
    /// <c>BlogAccount</c> / <c>BlogCredentials</c> pair, minus the WinForms/MSHTML
    /// detection stack.
    ///
    /// IMPORTANT: this object carries NO secret. The account password lives in the
    /// platform credential store (macOS Keychain), keyed by <see cref="Id"/>; only
    /// the non-secret metadata below is serialized to JSON. See <see cref="FileAccountStore"/>
    /// and <see cref="ICredentialStore"/>.
    /// </summary>
    public sealed class BlogAccount
    {
        /// <summary>
        /// Stable account identifier. Empty until first saved (assigned by the store),
        /// and used as the credential-store key so the password can be located again.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Friendly name shown in the blog selector (e.g. "My WordPress blog").</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>The blog's public homepage / service URL (informational).</summary>
        public string HomepageUrl { get; set; } = string.Empty;

        /// <summary>
        /// The API endpoint the transport posts to (e.g. the MetaWeblog XML-RPC URL
        /// such as <c>https://example.com/xmlrpc.php</c>). Entered manually for now
        /// (see the provider auto-detection TODO).
        /// </summary>
        public string ApiEndpointUrl { get; set; } = string.Empty;

        /// <summary>Server-side blog identifier passed to <c>metaWeblog.newPost</c>.</summary>
        public string BlogId { get; set; } = string.Empty;

        /// <summary>Account username (not a secret; the password is in credential storage).</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Provider type. <see cref="DefaultProviderType"/> (MetaWeblog) and
        /// <see cref="WordPressProviderType"/> are implemented on macOS;
        /// Atom/Blogger require the fuller BlogClient port.
        /// </summary>
        public string ProviderType { get; set; } = DefaultProviderType;

        /// <summary>Whether the provider supports publishing pages (not just posts).</summary>
        public bool SupportsPages { get; set; } = true;

        /// <summary>Whether the provider supports inline categories.</summary>
        public bool SupportsCategories { get; set; } = true;

        /// <summary>Whether the provider supports the extended-entry (more) break.</summary>
        public bool SupportsExtendedEntries { get; set; } = true;

        /// <summary>The default provider type.</summary>
        public const string DefaultProviderType = "MetaWeblog";

        /// <summary>The WordPress provider type (MetaWeblog-compatible + wp.* extensions).</summary>
        public const string WordPressProviderType = "WordPress";

        /// <summary>Display label preferring the friendly name, falling back to the URL.</summary>
        [JsonIgnore]
        public string DisplayLabel =>
            !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName
            : !string.IsNullOrWhiteSpace(HomepageUrl) ? HomepageUrl
            : !string.IsNullOrWhiteSpace(ApiEndpointUrl) ? ApiEndpointUrl
            : "(unnamed blog)";

        /// <summary>Returns a shallow copy so callers can edit without mutating the stored instance.</summary>
        public BlogAccount Clone() => new BlogAccount
        {
            Id = Id,
            DisplayName = DisplayName,
            HomepageUrl = HomepageUrl,
            ApiEndpointUrl = ApiEndpointUrl,
            BlogId = BlogId,
            Username = Username,
            ProviderType = ProviderType,
            SupportsPages = SupportsPages,
            SupportsCategories = SupportsCategories,
            SupportsExtendedEntries = SupportsExtendedEntries
        };
    }
}
