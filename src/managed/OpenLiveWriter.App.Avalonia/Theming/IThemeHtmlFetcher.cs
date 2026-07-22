// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Theming
{
    /// <summary>
    /// HTTP fetch seam for theme harvesting — the same testability pattern as
    /// <c>IRsdHttpFetcher</c>, async here because the shell's command handlers are
    /// async. Implementations return null on failure rather than throwing so a
    /// network miss degrades to the neutral preview.
    /// </summary>
    public interface IThemeHtmlFetcher
    {
        /// <summary>Fetches the text at <paramref name="url"/>, or null on any failure.</summary>
        Task<string> FetchAsync(string url);
    }

    /// <summary>
    /// Default <see cref="IThemeHtmlFetcher"/> backed by the shell's proxy-aware
    /// <see cref="HttpClient"/> (from <c>PublishingHttpClientFactory</c>). Redirects are
    /// followed by the client itself; a timeout bounds the wait so the Preview view is
    /// never held up by a slow blog. Never throws — failures return null.
    /// </summary>
    public sealed class HttpThemeHtmlFetcher : IThemeHtmlFetcher
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private readonly HttpClient _httpClient;
        private readonly TimeSpan _timeout;

        public HttpThemeHtmlFetcher(HttpClient httpClient, TimeSpan? timeout = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _timeout = timeout ?? DefaultTimeout;
        }

        public async Task<string> FetchAsync(string url)
        {
            try
            {
                using var cts = new CancellationTokenSource(_timeout);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("User-Agent", "OpenLiveWriter");
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token)
                    .ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException
                || ex is OperationCanceledException || ex is IOException || ex is UriFormatException)
            {
                return null;
            }
        }
    }
}
