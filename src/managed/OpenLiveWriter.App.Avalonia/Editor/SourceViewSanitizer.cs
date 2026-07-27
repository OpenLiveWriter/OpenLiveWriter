// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Source-view display helper: inline base64 <c>data:</c> URIs (embedded images)
    /// are megabytes of single-line text that make the Source editor unusable (a
    /// multi-MB single line stalls text layout so the pane renders blank). For
    /// display, each long data URI is replaced with a short <c>data-olw-img:N</c>
    /// token; the full values are kept and re-expanded when the source is pushed
    /// back into the editor, so round-tripping loses nothing. Tokens the user
    /// deletes stay deleted (their choice); tokens they duplicate duplicate the
    /// image (same as duplicating the <c>&lt;img&gt;</c> tag itself).
    /// </summary>
    internal static class SourceViewSanitizer
    {
        public const string TokenPrefix = "data-olw-img:";

        // Only elide payloads that would actually hurt editing; small icons stay inline.
        private const int MinElideLength = 200;

        private static readonly Regex DataSrcRegex = new Regex(
            "src=\"(data:[^\"]{200,})\"",
            RegexOptions.Compiled);

        /// <summary>
        /// Replaces long data-URI <c>src</c> values with <c>data-olw-img:N</c> tokens
        /// and returns the sanitized text; <paramref name="fullUris"/> receives the
        /// original values in token order.
        /// </summary>
        public static string ElideDataUris(string html, List<string> fullUris)
        {
            fullUris?.Clear();
            if (string.IsNullOrEmpty(html) || fullUris == null)
                return html;

            return DataSrcRegex.Replace(html, match =>
            {
                fullUris.Add(match.Groups[1].Value);
                return $"src=\"{TokenPrefix}{fullUris.Count - 1}\"";
            });
        }

        /// <summary>
        /// Re-expands <c>data-olw-img:N</c> tokens in edited source text back to the
        /// full data URIs captured by <see cref="ElideDataUris"/>. Unknown or stale
        /// token indices are left as-is (never throws).
        /// </summary>
        public static string RestoreDataUris(string sourceText, IReadOnlyList<string> fullUris)
        {
            if (string.IsNullOrEmpty(sourceText) || fullUris == null || fullUris.Count == 0)
                return sourceText;

            var sb = new StringBuilder(sourceText.Length);
            int pos = 0;
            while (pos < sourceText.Length)
            {
                int idx = sourceText.IndexOf(TokenPrefix, pos, System.StringComparison.Ordinal);
                if (idx < 0)
                {
                    sb.Append(sourceText, pos, sourceText.Length - pos);
                    break;
                }

                sb.Append(sourceText, pos, idx - pos);
                int numStart = idx + TokenPrefix.Length;
                int numEnd = numStart;
                while (numEnd < sourceText.Length && char.IsDigit(sourceText[numEnd]))
                    numEnd++;

                if (numEnd > numStart &&
                    int.TryParse(sourceText.Substring(numStart, numEnd - numStart), out int tokenIndex) &&
                    tokenIndex >= 0 && tokenIndex < fullUris.Count)
                {
                    sb.Append(fullUris[tokenIndex]);
                }
                else
                {
                    // Stale/unknown token — keep it verbatim rather than guessing.
                    sb.Append(sourceText, idx, numEnd - idx);
                }
                pos = numEnd;
            }
            return sb.ToString();
        }
    }
}
