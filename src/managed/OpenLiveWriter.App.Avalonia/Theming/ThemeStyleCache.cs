// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Theming
{
    /// <summary>
    /// Per-account theme cache in front of <see cref="ThemeStyleExtractor"/>: fetches the
    /// blog homepage through the injectable <see cref="IThemeHtmlFetcher"/> seam, extracts
    /// the theme styles, and remembers them in memory plus (optionally) on disk under the
    /// platform app-data dir so restarts don't re-fetch. A fetch records
    /// <see cref="BlogThemeStyle.FetchedUtc"/>; "Update Theme" passes
    /// <c>forceRefresh: true</c> to bypass the cache and re-harvest.
    ///
    /// Failure contract: a fetch/parse/IO miss returns null and never throws, and a
    /// failed refresh leaves any previously cached entry untouched (the cache is never
    /// poisoned by a network hiccup). Callers degrade to the neutral preview on null.
    /// </summary>
    public sealed class ThemeStyleCache
    {
        private const string CacheExtension = ".oltheme.json";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly IThemeHtmlFetcher _fetcher;
        private readonly string _cacheDirectory; // null = memory only
        private readonly ConcurrentDictionary<string, BlogThemeStyle> _memory =
            new ConcurrentDictionary<string, BlogThemeStyle>(StringComparer.Ordinal);

        /// <param name="fetcher">Homepage fetcher (tests inject a fake).</param>
        /// <param name="cacheDirectory">
        /// Optional disk-cache directory (created lazily on first save). Null for an
        /// in-memory-only cache; tests pass a temp dir.
        /// </param>
        public ThemeStyleCache(IThemeHtmlFetcher fetcher, string cacheDirectory = null)
        {
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _cacheDirectory = cacheDirectory;
        }

        /// <summary>
        /// Returns the cached theme for <paramref name="accountKey"/>, fetching the
        /// homepage on a cache miss (or when <paramref name="forceRefresh"/> is set).
        /// A cached entry is reused only while the account's
        /// <paramref name="homepageUrl"/> still matches the one the theme was harvested
        /// from. Returns null when nothing is cached and the fetch fails or yields no
        /// usable styles.
        /// </summary>
        public async Task<BlogThemeStyle> GetThemeAsync(
            string accountKey, string homepageUrl, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(homepageUrl))
                return null;

            string key = CacheKey(accountKey, homepageUrl);

            if (!forceRefresh)
            {
                if (_memory.TryGetValue(key, out BlogThemeStyle cached) && Matches(cached, homepageUrl))
                    return cached;

                BlogThemeStyle fromDisk = LoadFromDisk(key);
                if (fromDisk != null && Matches(fromDisk, homepageUrl))
                {
                    _memory[key] = fromDisk;
                    return fromDisk;
                }
            }

            BlogThemeStyle fetched = await FetchAndExtractAsync(homepageUrl).ConfigureAwait(false);
            if (fetched == null)
                return null; // fetch failed: keep any previous cache entry, callers fall back to neutral

            // An empty (stylesheet-less) result is a successful harvest — cache it too so
            // repeated previews of a theme-less blog don't re-fetch every time. Callers
            // distinguish "fetch failed" (null) from "no stylesheets found" (IsEmpty).

            _memory[key] = fetched;
            SaveToDisk(key, fetched);
            return fetched;
        }

        private async Task<BlogThemeStyle> FetchAndExtractAsync(string homepageUrl)
        {
            string html;
            try
            {
                html = await _fetcher.FetchAsync(homepageUrl).ConfigureAwait(false);
            }
            catch
            {
                return null; // a misbehaving fetcher must not break Preview
            }

            if (string.IsNullOrEmpty(html))
                return null;

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, homepageUrl);
            theme.FetchedUtc = DateTime.UtcNow;
            return theme;
        }

        private static bool Matches(BlogThemeStyle theme, string homepageUrl) =>
            theme != null &&
            string.Equals(theme.SourceUrl, homepageUrl, StringComparison.OrdinalIgnoreCase);

        private static string CacheKey(string accountKey, string homepageUrl) =>
            !string.IsNullOrWhiteSpace(accountKey) ? accountKey.Trim() : homepageUrl;

        // ---- Optional disk cache (one JSON file per account key) ----

        private BlogThemeStyle LoadFromDisk(string key)
        {
            if (_cacheDirectory == null)
                return null;

            try
            {
                string path = PathForKey(key);
                if (!File.Exists(path))
                    return null;

                var record = JsonSerializer.Deserialize<CacheRecord>(File.ReadAllText(path));
                if (record == null)
                    return null;

                return new BlogThemeStyle
                {
                    SourceUrl = record.SourceUrl ?? string.Empty,
                    FetchedUtc = record.FetchedUtc,
                    StylesheetUrls = record.StylesheetUrls ?? Array.Empty<string>(),
                    InlineStyles = record.InlineStyles ?? Array.Empty<string>()
                };
            }
            catch
            {
                return null; // corrupt or unreadable cache file — treat as a miss
            }
        }

        private void SaveToDisk(string key, BlogThemeStyle theme)
        {
            if (_cacheDirectory == null)
                return;

            try
            {
                Directory.CreateDirectory(_cacheDirectory);
                var record = new CacheRecord
                {
                    SourceUrl = theme.SourceUrl,
                    FetchedUtc = theme.FetchedUtc,
                    StylesheetUrls = theme.StylesheetUrls,
                    InlineStyles = theme.InlineStyles
                };
                string json = JsonSerializer.Serialize(record, SerializerOptions);

                string finalPath = PathForKey(key);
                string tempPath = finalPath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
                File.Move(tempPath, finalPath);
            }
            catch
            {
                // Disk persistence is a nice-to-have; the memory cache still serves.
            }
        }

        private string PathForKey(string key) =>
            Path.Combine(_cacheDirectory, SanitizeFileName(key) + CacheExtension);

        // Account keys are normally GUIDs, but a homepage URL can stand in — keep only
        // filename-safe characters so the cache file name is always valid.
        private static string SanitizeFileName(string key)
        {
            var sb = new StringBuilder(key.Length);
            foreach (char c in key)
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            return sb.ToString();
        }

        private sealed class CacheRecord
        {
            public string SourceUrl { get; set; }
            public DateTime FetchedUtc { get; set; }
            public IReadOnlyList<string> StylesheetUrls { get; set; }
            public IReadOnlyList<string> InlineStyles { get; set; }
        }
    }
}
