// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Net;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Cross-platform port of the Windows <c>OpenLiveWriter.HtmlEditor.WordCounter</c>
    /// contract: converts editor HTML to plain text and reports word, character
    /// (with and without spaces), and paragraph counts. Pure and WinForms/MSHTML-free
    /// so it is fully unit-testable headlessly.
    ///
    /// The counting regexes mirror the originals so the counts match the Windows
    /// Word Count dialog:
    ///  - words:               runs of non-whitespace excluding parentheses
    ///  - chars:               everything except newline/CR/tab (spaces count)
    ///  - chars without space: non-whitespace characters
    ///  - paragraphs:          blank-line-separated blocks (+1), 0 when empty
    /// </summary>
    public sealed class WordCounter
    {
        private static readonly Regex WordRegex = new(@"[^\n\r\t\s()]+", RegexOptions.Compiled);
        private static readonly Regex CharRegex = new(@"[^\n\r\t]", RegexOptions.Compiled);
        private static readonly Regex CharNoSpaceRegex = new(@"\S", RegexOptions.Compiled);
        private static readonly Regex ParagraphRegex = new(@"(\n){1,2}\s*", RegexOptions.Compiled);

        public WordCounter(string html)
        {
            PlainText = HtmlToPlainText(html ?? string.Empty);
            Words = WordRegex.Matches(PlainText).Count;
            Chars = CharRegex.Matches(PlainText).Count;
            CharsWithoutSpaces = CharNoSpaceRegex.Matches(PlainText).Count;
            Paragraphs = PlainText.Length == 0 ? 0 : ParagraphRegex.Matches(PlainText).Count + 1;
        }

        /// <summary>The plain-text extraction the counts are computed from.</summary>
        public string PlainText { get; }

        public int Words { get; }
        public int Chars { get; }
        public int CharsWithoutSpaces { get; }
        public int Paragraphs { get; }

        /// <summary>
        /// Converts editor HTML to plain text: block-level ends and <c>&lt;br&gt;</c>
        /// become line breaks, remaining tags are stripped, HTML entities are decoded,
        /// and line endings are normalized to <c>\n</c>. Deterministic for testing.
        /// </summary>
        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            string s = html;
            // Line breaks -> newline.
            s = Regex.Replace(s, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            // Block-level element ends -> newline (paragraph boundary).
            s = Regex.Replace(s, @"</(p|div|h[1-6]|li|ul|ol|blockquote|pre|tr|table)\s*>",
                "\n", RegexOptions.IgnoreCase);
            // Drop all remaining tags.
            s = Regex.Replace(s, @"<[^>]+>", string.Empty);
            // Decode entities (&amp; &nbsp; &#160; ...).
            s = WebUtility.HtmlDecode(s);
            // Normalize line endings.
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");

            return s.Trim();
        }
    }
}
