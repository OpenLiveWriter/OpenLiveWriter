// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// The "Paste Special" content sanitizers: paste-as-plain-text (strip all markup)
    /// and paste-as-clean-HTML (keep a safe subset of tags/attributes, dropping
    /// scripts, styles, classes, ids, event handlers, and other foreign cruft that
    /// pasting from Word/web pages typically drags in). Both transforms are
    /// pure/deterministic so they are unit-testable without a live WebView.
    /// </summary>
    public static class PasteCleaner
    {
        // Inline/flow tags kept when cleaning HTML.
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "b", "strong", "i", "em", "u", "s", "strike", "sub", "sup",
            "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "ul", "ol", "li",
            "a", "img", "pre", "code", "table", "thead", "tbody", "tr", "td", "th", "hr",
            "span", "div"
        };

        // Attributes kept per tag; anything else (style/class/id/on*) is dropped.
        private static readonly Dictionary<string, HashSet<string>> AllowedAttributes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "title" },
                ["img"] = new(StringComparer.OrdinalIgnoreCase) { "src", "alt" },
            };

        private static readonly Regex ScriptStyle = new(
            @"<(script|style)\b[^>]*>[\s\S]*?</\1\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Comments = new(@"<!--[\s\S]*?-->", RegexOptions.Compiled);
        private static readonly Regex Tag = new(@"<(/?)([a-zA-Z][a-zA-Z0-9]*)([^>]*?)(/?)>", RegexOptions.Compiled);
        private static readonly Regex AttrPair = new(
            "([a-zA-Z_:][-a-zA-Z0-9_:.]*)\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s\"'>]+)",
            RegexOptions.Compiled);
        private static readonly Regex AnyTag = new("<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex Whitespace = new(@"[ \t\f\v\u00A0]+", RegexOptions.Compiled);

        /// <summary>
        /// Converts pasted HTML to plain text: drops all tags and decodes entities.
        /// Block-level boundaries become newlines so the text keeps its shape.
        /// </summary>
        public static string ToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string s = ScriptStyle.Replace(html, string.Empty);
            s = Comments.Replace(s, string.Empty);

            // Preserve paragraph/line breaks as newlines before stripping tags.
            s = Regex.Replace(s, @"<\s*(br|/p|/div|/li|/h[1-6]|/tr)\s*/?>", "\n", RegexOptions.IgnoreCase);
            s = AnyTag.Replace(s, string.Empty);
            s = WebUtility.HtmlDecode(s);

            // Collapse runs of spaces but keep newlines; trim trailing spaces per line.
            var lines = s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                string collapsed = Whitespace.Replace(line, " ").Trim();
                if (collapsed.Length > 0)
                    sb.Append(collapsed).Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>
        /// Cleans pasted HTML to a safe subset: removes scripts/styles/comments,
        /// drops tags outside the whitelist (keeping their text), and strips all
        /// attributes except a small per-tag allow-list (e.g. <c>a[href]</c>,
        /// <c>img[src]</c>). Disallowed <c>javascript:</c> URLs are dropped.
        /// </summary>
        public static string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string s = ScriptStyle.Replace(html, string.Empty);
            s = Comments.Replace(s, string.Empty);

            return Tag.Replace(s, m =>
            {
                bool isClose = m.Groups[1].Value == "/";
                string name = m.Groups[2].Value.ToLowerInvariant();
                string attrs = m.Groups[3].Value;
                bool selfClose = m.Groups[4].Value == "/";

                if (!AllowedTags.Contains(name))
                    return string.Empty; // drop the tag, keep inner content

                if (isClose)
                    return "</" + name + ">";

                string keptAttrs = BuildAllowedAttributes(name, attrs);
                return "<" + name + keptAttrs + (selfClose ? " />" : ">");
            });
        }

        private static string BuildAllowedAttributes(string tag, string attrs)
        {
            if (string.IsNullOrWhiteSpace(attrs) || !AllowedAttributes.TryGetValue(tag, out var allowed))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (Match m in AttrPair.Matches(attrs))
            {
                string attrName = m.Groups[1].Value;
                if (!allowed.Contains(attrName))
                    continue;

                string rawValue = m.Groups[2].Value;
                string value = rawValue.Length >= 2 && (rawValue[0] == '"' || rawValue[0] == '\'')
                    ? rawValue.Substring(1, rawValue.Length - 2)
                    : rawValue;

                // Reject dangerous URL schemes.
                if ((attrName.Equals("href", StringComparison.OrdinalIgnoreCase) ||
                     attrName.Equals("src", StringComparison.OrdinalIgnoreCase)) &&
                    value.TrimStart().StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    continue;

                sb.Append(' ').Append(attrName.ToLowerInvariant()).Append("=\"")
                  .Append(value.Replace("\"", "&quot;")).Append('"');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Builds an editor insertion payload from plain text: HTML-escapes the text
        /// and turns line breaks into <c>&lt;br&gt;</c> so multi-line pastes keep
        /// their shape.
        /// </summary>
        public static string BuildPlainTextInsertion(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            string escaped = text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
            return escaped.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br />");
        }
    }
}
