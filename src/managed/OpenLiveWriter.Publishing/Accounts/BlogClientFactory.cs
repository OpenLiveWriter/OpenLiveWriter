// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net.Http;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Constructs an <see cref="IBlogClient"/> transport from a <see cref="BlogAccount"/>
    /// plus its (separately-stored) password. The MetaWeblog and WordPress XML-RPC
    /// providers are supported; other providers (Atom/Blogger) require the fuller
    /// BlogClient port.
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

            var options = new BlogClientOptions
            {
                SupportsExtendedEntries = account.SupportsExtendedEntries,
                SupportsCategoriesInline = account.SupportsCategories
            };

            string provider = account.ProviderType ?? BlogAccount.DefaultProviderType;
            if (string.Equals(provider, BlogAccount.WordPressProviderType, StringComparison.OrdinalIgnoreCase))
            {
                return new WordPressXmlRpcClient(
                    endpointUrl: account.ApiEndpointUrl ?? string.Empty,
                    username: account.Username ?? string.Empty,
                    password: password ?? string.Empty,
                    options: options,
                    httpClient: httpClient);
            }

            if (string.Equals(provider, BlogAccount.DefaultProviderType, StringComparison.OrdinalIgnoreCase))
            {
                return new MetaWeblogXmlRpcClient(
                    endpointUrl: account.ApiEndpointUrl ?? string.Empty,
                    username: account.Username ?? string.Empty,
                    password: password ?? string.Empty,
                    options: options,
                    httpClient: httpClient);
            }

            throw new NotSupportedException(
                $"Provider '{provider}' is not supported on macOS yet. " +
                $"Only '{BlogAccount.DefaultProviderType}' and '{BlogAccount.WordPressProviderType}' are implemented.");
        }
    }
}
