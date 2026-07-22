// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Net.Http;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// WordPress XML-RPC transport. WordPress speaks the MetaWeblog API plus the
    /// <c>wp.*</c> extensions (pages, etc.), and the base
    /// <see cref="MetaWeblogXmlRpcClient"/> already implements both, so today this
    /// subclass carries no overrides — it exists so accounts can record
    /// <c>ProviderType = WordPress</c> faithfully (matching RSD detection) and so any
    /// future WordPress-specific payload shaping has a home without disturbing the
    /// generic MetaWeblog path. Mirrors the role of the Windows
    /// <c>OpenLiveWriter.BlogClient.Clients.WordPressClient</c>.
    /// </summary>
    public class WordPressXmlRpcClient : MetaWeblogXmlRpcClient
    {
        public WordPressXmlRpcClient(
            string endpointUrl,
            string username,
            string password,
            IBlogClientOptions options = null,
            string userAgent = null,
            HttpClient httpClient = null)
            : base(endpointUrl, username, password, options, userAgent, httpClient)
        {
        }
    }
}
