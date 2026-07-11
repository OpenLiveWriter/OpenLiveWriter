// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Pure, WebView-independent search/replace logic backing the editor's Find and
    /// Find &amp; Replace features. The live in-page highlight (window.find) runs
    /// inside the WKWebView and is verified separately/live; the matching and
    /// replacement contract here is deterministic and unit-tested headlessly.
    ///
    /// Replacement can operate on plain text (<see cref="ReplaceAll"/>) or on HTML
    /// while leaving tags untouched (<see cref="ReplaceAllInHtml"/>), so applying a
    /// Replace All to editor content does not corrupt markup (element/attribute text
    /// inside <c>&lt;...&gt;</c> is copied verbatim).
    /// </summary>
    public static class TextFinder
    {
        /// <summary>Returns the start indices of every non-overlapping match in <paramref name="text"/>.</summary>
        public static IReadOnlyList<int> FindAll(string text, string query, bool matchCase, bool wholeWord)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return result;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int i = 0;
            while (i <= text.Length - query.Length)
            {
                int idx = text.IndexOf(query, i, comparison);
                if (idx < 0) break;

                if (!wholeWord || IsWholeWord(text, idx, query.Length))
                {
                    result.Add(idx);
                    i = idx + query.Length;
                }
                else
                {
                    i = idx + 1;
                }
            }
            return result;
        }

        /// <summary>Counts non-overlapping matches in plain text.</summary>
        public static int Count(string text, string query, bool matchCase, bool wholeWord) =>
            FindAll(text, query, matchCase, wholeWord).Count;

        /// <summary>
        /// Returns the index of the next match at or after <paramref name="startIndex"/>,
        /// optionally wrapping to the start. Returns -1 when there is no match.
        /// </summary>
        public static int IndexOfNext(string text, string query, int startIndex,
            bool matchCase, bool wholeWord, bool wrap)
        {
            var matches = FindAll(text, query, matchCase, wholeWord);
            foreach (int idx in matches)
            {
                if (idx >= startIndex)
                    return idx;
            }
            return wrap && matches.Count > 0 ? matches[0] : -1;
        }

        /// <summary>Replaces every match in plain text, reporting how many were replaced.</summary>
        public static string ReplaceAll(string text, string query, string replacement,
            bool matchCase, bool wholeWord, out int count)
        {
            count = 0;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return text;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            replacement ??= string.Empty;

            var sb = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                int idx = text.IndexOf(query, i, comparison);
                if (idx < 0)
                {
                    sb.Append(text, i, text.Length - i);
                    break;
                }

                if (wholeWord && !IsWholeWord(text, idx, query.Length))
                {
                    // Not a standalone word — copy through this char and keep scanning.
                    sb.Append(text, i, idx - i + 1);
                    i = idx + 1;
                    continue;
                }

                sb.Append(text, i, idx - i);
                sb.Append(replacement);
                count++;
                i = idx + query.Length;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Replaces matches found in the text content of <paramref name="html"/>,
        /// copying anything inside angle-bracket tags verbatim so markup is never
        /// altered. Reports the replacement count.
        /// </summary>
        public static string ReplaceAllInHtml(string html, string query, string replacement,
            bool matchCase, bool wholeWord, out int count)
        {
            count = 0;
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(query))
                return html;

            var sb = new StringBuilder(html.Length);
            int i = 0;
            while (i < html.Length)
            {
                if (html[i] == '<')
                {
                    int end = html.IndexOf('>', i);
                    if (end < 0)
                    {
                        sb.Append(html, i, html.Length - i);
                        break;
                    }
                    sb.Append(html, i, end - i + 1); // tag copied verbatim
                    i = end + 1;
                }
                else
                {
                    int next = html.IndexOf('<', i);
                    if (next < 0) next = html.Length;
                    string segment = html.Substring(i, next - i);
                    sb.Append(ReplaceAll(segment, query, replacement, matchCase, wholeWord, out int segCount));
                    count += segCount;
                    i = next;
                }
            }
            return sb.ToString();
        }

        /// <summary>Counts matches in the text content of <paramref name="html"/> (tags excluded).</summary>
        public static int CountInHtml(string html, string query, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(query))
                return 0;

            int count = 0;
            int i = 0;
            while (i < html.Length)
            {
                if (html[i] == '<')
                {
                    int end = html.IndexOf('>', i);
                    if (end < 0) break;
                    i = end + 1;
                }
                else
                {
                    int next = html.IndexOf('<', i);
                    if (next < 0) next = html.Length;
                    count += Count(html.Substring(i, next - i), query, matchCase, wholeWord);
                    i = next;
                }
            }
            return count;
        }

        private static bool IsWholeWord(string text, int index, int length)
        {
            bool beforeOk = index == 0 || !IsWordChar(text[index - 1]);
            int after = index + length;
            bool afterOk = after >= text.Length || !IsWordChar(text[after]);
            return beforeOk && afterOk;
        }

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
    }
}
