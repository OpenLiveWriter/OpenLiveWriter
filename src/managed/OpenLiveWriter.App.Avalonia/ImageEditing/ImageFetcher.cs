// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.ImageEditing
{
    /// <summary>
    /// Fetch seam for pictures that need pixel baking — remote (web) pictures over
    /// HTTP and local (file://) pictures from disk — the same testability pattern as
    /// <c>IThemeHtmlFetcher</c>. Implementations return null on failure rather than
    /// throwing so a miss degrades to a status-bar message.
    /// </summary>
    public interface IImageFetcher
    {
        /// <summary>Fetches the bytes at <paramref name="url"/>, or null on any failure.</summary>
        Task<byte[]> FetchAsync(string url);
    }

    /// <summary>
    /// Default <see cref="IImageFetcher"/>: <c>file://</c> URLs are read from disk
    /// (inserted pictures live in the draft's media folder until publish); anything
    /// else is fetched with the shell's proxy-aware <see cref="HttpClient"/> (from
    /// <c>PublishingHttpClientFactory</c>). A timeout bounds the wait so Picture
    /// Tools never hangs on a slow host. Never throws — failures return null.
    /// </summary>
    public sealed class HttpImageFetcher : IImageFetcher
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private readonly HttpClient _httpClient;
        private readonly TimeSpan _timeout;

        public HttpImageFetcher(HttpClient httpClient, TimeSpan? timeout = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _timeout = timeout ?? DefaultTimeout;
        }

        public async Task<byte[]> FetchAsync(string url)
        {
            if (IsFileUri(url, out string localPath))
                return ReadLocalFile(localPath);

            try
            {
                using var cts = new CancellationTokenSource(_timeout);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("User-Agent", "OpenLiveWriter");
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token)
                    .ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException
                || ex is OperationCanceledException || ex is IOException || ex is UriFormatException)
            {
                return null;
            }
        }

        /// <summary>True when <paramref name="url"/> is a file:// URI that resolves to a local path.</summary>
        internal static bool IsFileUri(string url, out string localPath)
        {
            localPath = null;
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.IsFile)
            {
                localPath = uri.LocalPath;
                return true;
            }
            return false;
        }

        private static byte[] ReadLocalFile(string localPath)
        {
            try
            {
                return File.ReadAllBytes(localPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                || ex is System.Security.SecurityException || ex is NotSupportedException)
            {
                return null;
            }
        }
    }
}
