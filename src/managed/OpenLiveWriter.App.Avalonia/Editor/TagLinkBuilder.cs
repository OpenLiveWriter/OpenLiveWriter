// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Builds a block of tag links using the Open Live Writer convention:
    /// <c>rel="tag"</c> microformat anchors joined by a separator, optionally prefixed
    /// with a caption. This mirrors the Windows <c>TagProvider.GenerateHtmlForTags</c>
    /// (<c>&lt;a href="{base}{tag}" rel="tag"&gt;{tag}&lt;/a&gt;</c>) minus the WinForms
    /// provider-management UI.
    ///
    /// The historical default providers (Technorati, del.icio.us, …) are dead, so the
    /// default destination is the blog-relative <c>/tag/</c> space that WordPress, Ghost
    /// and most modern engines resolve; the base URL is overridable. Composition is pure
    /// and unit-testable without a live WebView.
    /// </summary>
    public static class TagLinkBuilder
    {
        /// <summary>Blog-relative tag base used when the caller doesn't override it.</summary>
        public const string DefaultBaseUrl = "/tag/";

        /// <summary>Separator inserted between tag anchors.</summary>
        public const string DefaultSeparator = ", ";

        /// <summary>Caption placed before the tag anchors.</summary>
        public const string DefaultCaption = "Tags: ";

        /// <summary>
        /// Builds the tag-links block for the given tags. Tags are trimmed, empties
        /// dropped, and duplicates removed case-insensitively (preserving order).
        /// Returns null when no usable tags remain.
        /// </summary>
        public static string BuildTagLinksHtml(IEnumerable<string> tags,
            string baseUrl = DefaultBaseUrl, string caption = DefaultCaption,
            string separator = DefaultSeparator)
        {
            List<string> clean = Normalize(tags);
            if (clean.Count == 0)
                return null;

            baseUrl ??= DefaultBaseUrl;
            separator ??= DefaultSeparator;

            var sb = new StringBuilder();
            sb.Append("<p class=\"olw-tags\">");
            if (!string.IsNullOrEmpty(caption))
                sb.Append(EscapeText(caption));

            for (int i = 0; i < clean.Count; i++)
            {
                if (i > 0)
                    sb.Append(EscapeText(separator));
                sb.Append(BuildTagAnchor(clean[i], baseUrl));
            }

            sb.Append("</p>");
            return sb.ToString();
        }

        /// <summary>Builds a single <c>rel="tag"</c> anchor for the tag.</summary>
        internal static string BuildTagAnchor(string tag, string baseUrl)
        {
            string href = (baseUrl ?? DefaultBaseUrl) + Uri.EscapeDataString(tag);
            return "<a href=\"" + EscapeAttr(href) + "\" rel=\"tag\">" + EscapeText(tag) + "</a>";
        }

        /// <summary>
        /// Splits raw dialog input (comma- and/or newline-separated) into trimmed,
        /// de-duplicated tag tokens preserving first-seen order.
        /// </summary>
        public static List<string> ParseTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();
            return Normalize(input.Split(new[] { ',', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries));
        }

        private static List<string> Normalize(IEnumerable<string> tags)
        {
            var result = new List<string>();
            if (tags == null)
                return result;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in tags)
            {
                string t = raw?.Trim();
                if (string.IsNullOrEmpty(t))
                    continue;
                if (seen.Add(t))
                    result.Add(t);
            }
            return result;
        }

        private static string EscapeAttr(string s) =>
            s?.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";

        private static string EscapeText(string s) =>
            s?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
    }
}
