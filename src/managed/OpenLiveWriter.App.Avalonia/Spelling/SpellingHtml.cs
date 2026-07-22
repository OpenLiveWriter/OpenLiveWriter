// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;

namespace OpenLiveWriter.App.Avalonia.Spelling
{
    /// <summary>
    /// HTML-aware single-word replacement for the spelling flow. Mirrors
    /// <see cref="Editor.TextFinder"/>'s tag-skipping walk (anything inside
    /// <c>&lt;...&gt;</c> is copied verbatim) but replaces exactly one occurrence —
    /// the 0-based <paramref name="occurrenceOrdinal"/>-th whole-word, case-sensitive
    /// match in the text content — so the Spelling dialog's "Change" button can fix
    /// the one instance being reviewed without touching identical words elsewhere.
    /// Pure and deterministic for headless fixture tests.
    /// </summary>
    public static class SpellingHtml
    {
        /// <summary>
        /// Replaces the <paramref name="occurrenceOrdinal"/>-th (0-based) whole-word,
        /// case-sensitive occurrence of <paramref name="word"/> in the text content of
        /// <paramref name="html"/> with <paramref name="replacement"/>. Returns the HTML
        /// unchanged (and <paramref name="replaced"/> false) when there is no such
        /// occurrence. Markup — including words inside attribute values — is never altered.
        /// </summary>
        public static string ReplaceOccurrence(
            string html, string word, string replacement, int occurrenceOrdinal, out bool replaced)
        {
            replaced = false;
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(word) || occurrenceOrdinal < 0)
                return html;

            replacement ??= string.Empty;

            int seen = 0;
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
                    continue;
                }

                int next = html.IndexOf('<', i);
                if (next < 0) next = html.Length;

                if (!replaced)
                {
                    int match = FindWholeWord(html, i, next, word, ref seen, occurrenceOrdinal);
                    if (match >= 0)
                    {
                        sb.Append(html, i, match - i);
                        sb.Append(replacement);
                        sb.Append(html, match + word.Length, next - (match + word.Length));
                        replaced = true;
                        i = next;
                        continue;
                    }
                }

                sb.Append(html, i, next - i);
                i = next;
            }
            return sb.ToString();
        }

        // Finds the (targetOrdinal - alreadySeen)-th whole-word occurrence of word in
        // html[start..end); advances seen by the number of occurrences examined.
        private static int FindWholeWord(
            string html, int start, int end, string word, ref int seen, int targetOrdinal)
        {
            int i = start;
            while (i + word.Length <= end)
            {
                int idx = html.IndexOf(word, i, end - i, System.StringComparison.Ordinal);
                if (idx < 0)
                    return -1;

                if (IsWholeWord(html, idx, word.Length))
                {
                    if (seen == targetOrdinal)
                        return idx;
                    seen++;
                }
                i = idx + 1;
            }
            return -1;
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
