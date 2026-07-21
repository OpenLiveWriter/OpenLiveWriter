// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Verifies a blog endpoint + credentials with a lightweight live call. The seam
    /// (patterned after <see cref="IRsdHttpFetcher"/>) keeps the account dialog's
    /// "Test Connection" path unit-testable without a network: tests inject a fake,
    /// the app uses <see cref="MetaWeblogConnectionVerifier"/>.
    /// </summary>
    public interface IBlogConnectionVerifier
    {
        /// <summary>
        /// Completes normally when the endpoint accepts the credentials; throws
        /// (<see cref="BlogClientPublishException"/> on an XML-RPC fault, transport
        /// exceptions on network errors) otherwise.
        /// </summary>
        Task VerifyAsync(string endpointUrl, string username, string password,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Default <see cref="IBlogConnectionVerifier"/> backed by
    /// <see cref="MetaWeblogXmlRpcClient.VerifyCredentialsAsync"/> (a
    /// <c>blogger.getUsersBlogs</c> round-trip).
    /// </summary>
    public sealed class MetaWeblogConnectionVerifier : IBlogConnectionVerifier
    {
        private readonly Func<HttpClient> _httpClientFactory;

        /// <param name="httpClientFactory">
        /// Optional factory for proxy-aware <see cref="HttpClient"/> instances; when
        /// null the transport uses its built-in shared client.
        /// </param>
        public MetaWeblogConnectionVerifier(Func<HttpClient> httpClientFactory = null)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task VerifyAsync(string endpointUrl, string username, string password,
            CancellationToken cancellationToken)
        {
            var client = new MetaWeblogXmlRpcClient(
                endpointUrl: endpointUrl ?? string.Empty,
                username: username ?? string.Empty,
                password: password ?? string.Empty,
                httpClient: _httpClientFactory?.Invoke());
            return client.VerifyCredentialsAsync(cancellationToken);
        }
    }
}
