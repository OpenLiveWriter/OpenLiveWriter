// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.App.Avalonia.Theming
{
    /// <summary>
    /// Pure HTML → <see cref="BlogThemeStyle"/> extraction. Finds every
    /// <c>&lt;link rel="stylesheet"&gt;</c> tag (resolving relative, root-relative, and
    /// protocol-relative hrefs against the homepage URL) and every inline
    /// <c>&lt;style&gt;</c> block. Regex-based and tolerant, mirroring the parsing style
    /// of <c>RsdServiceDetector</c> — no network, fully fixture-testable.
    /// </summary>
    public static class ThemeStyleExtractor
    {
        // Matches each <link ...> tag; attributes are parsed out separately.
        private static readonly Regex LinkTagRegex = new Regex(
            @"<link\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex StyleBlockRegex = new Regex(
            @"<style\b[^>]*>(?<css>.*?)</style\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex AttributeRegex = new Regex(
            "(?<name>[\\w:-]+)\\s*=\\s*(?:\"(?<v1>[^\"]*)\"|'(?<v2>[^']*)'|(?<v3>[^\\s\"'>]+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // A URI scheme prefix such as "http:", "https:", "ftp:". Used to decide whether a
        // URL is absolute — we can't rely on Uri.TryCreate(UriKind.Absolute) because on
        // Unix a leading-slash path is (mis)parsed as an absolute file URI.
        private static readonly Regex SchemeRegex = new Regex(
            "^[a-zA-Z][a-zA-Z0-9+.-]*:", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Extracts the theme's stylesheet links and inline style blocks from
        /// <paramref name="homepageHtml"/>. Never returns null and never throws on
        /// malformed markup — the result is simply <see cref="BlogThemeStyle.IsEmpty"/>
        /// when nothing usable is found.
        /// </summary>
        public static BlogThemeStyle Extract(string homepageHtml, string homepageUrl)
        {
            var theme = new BlogThemeStyle { SourceUrl = homepageUrl ?? string.Empty };
            if (string.IsNullOrEmpty(homepageHtml))
                return theme;

            var stylesheetUrls = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match linkMatch in LinkTagRegex.Matches(homepageHtml))
            {
                var attrs = ParseAttributes(linkMatch.Value);
                if (!attrs.TryGetValue("rel", out string rel) || !HasToken(rel, "stylesheet"))
                    continue;
                if (!attrs.TryGetValue("href", out string href) || string.IsNullOrWhiteSpace(href))
                    continue;

                string absolute = ResolveUrl(homepageUrl, href.Trim());
                if (string.IsNullOrEmpty(absolute) || seen.Contains(absolute))
                    continue;

                seen.Add(absolute);
                stylesheetUrls.Add(absolute);
            }

            var inlineStyles = new List<string>();
            foreach (Match styleMatch in StyleBlockRegex.Matches(homepageHtml))
            {
                string css = styleMatch.Groups["css"].Value.Trim();
                if (css.Length > 0)
                    inlineStyles.Add(css);
            }

            theme.StylesheetUrls = stylesheetUrls;
            theme.InlineStyles = inlineStyles;
            return theme;
        }

        // True when <paramref name="rel"/> contains the token (rel is a space-separated
        // token list, e.g. "alternate stylesheet" — only plain "stylesheet" participates
        // in the cascade by default, but alternates are still the theme's own CSS and
        // harmless to include in a preview).
        private static bool HasToken(string rel, string token)
        {
            foreach (string part in (rel ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(part, token, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static Dictionary<string, string> ParseAttributes(string tag)
        {
            var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in AttributeRegex.Matches(tag))
            {
                string name = m.Groups["name"].Value;
                string value = m.Groups["v1"].Success ? m.Groups["v1"].Value
                    : m.Groups["v2"].Success ? m.Groups["v2"].Value
                    : m.Groups["v3"].Value;
                attrs[name] = value;
            }
            return attrs;
        }

        /// <summary>Resolves <paramref name="url"/> against <paramref name="baseUrl"/> when relative.</summary>
        private static string ResolveUrl(string baseUrl, string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            if (SchemeRegex.IsMatch(url))
                return url; // already absolute (has a scheme)
            if (!string.IsNullOrEmpty(baseUrl) &&
                Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
                Uri.TryCreate(baseUri, url, out Uri combined))
            {
                return combined.ToString();
            }
            return url;
        }
    }
}
