// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net.Http;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Constructs an <see cref="IBlogClient"/> transport from a <see cref="BlogAccount"/>
    /// plus its (separately-stored) password. Today only the MetaWeblog XML-RPC client is
    /// produced; other providers (Atom/WordPress/Blogger) require the fuller BlogClient port.
    /// </summary>
    public static class BlogClientFactory
    {
        /// <summary>
        /// Builds a transport client for <paramref name="account"/>. The
        /// <paramref name="password"/> is supplied by the caller (retrieved from the
        /// credential store) and never read from the account metadata.
        /// </summary>
        /// <exception cref="ArgumentNullException">account is null.</exception>
        /// <exception cref="NotSupportedException">provider type is not implemented.</exception>
        public static IBlogClient CreateClient(BlogAccount account, string password, HttpClient httpClient = null)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            string provider = account.ProviderType ?? BlogAccount.DefaultProviderType;
            if (!string.Equals(provider, BlogAccount.DefaultProviderType, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(
                    $"Provider '{provider}' is not supported on macOS yet. Only '{BlogAccount.DefaultProviderType}' is implemented.");
            }

            var options = new BlogClientOptions
            {
                SupportsExtendedEntries = account.SupportsExtendedEntries,
                SupportsCategoriesInline = account.SupportsCategories
            };

            return new MetaWeblogXmlRpcClient(
                endpointUrl: account.ApiEndpointUrl ?? string.Empty,
                username: account.Username ?? string.Empty,
                password: password ?? string.Empty,
                options: options,
                httpClient: httpClient);
        }
    }
}
