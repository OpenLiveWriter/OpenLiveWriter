// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Splits editor HTML into main/extended contents at the extended-entry break,
    /// mirroring the Windows <c>BlogPost.ExtendedEntryBreak</c> behavior.
    /// </summary>
    public static class ExtendedEntry
    {
        /// <summary>The "more" marker that separates main from extended contents.</summary>
        public const string BreakMarker = BlogPost.ExtendedEntryBreak;

        public static (string Main, string Extended) Split(string html)
        {
            html ??= string.Empty;
            int idx = html.IndexOf(BreakMarker, StringComparison.Ordinal);
            if (idx < 0)
                return (html, string.Empty);

            string main = html.Substring(0, idx);
            string extended = html.Substring(idx + BreakMarker.Length);
            return (main, extended);
        }
    }

    /// <summary>
    /// Cross-platform port of the Windows <c>OpenLiveWriter.Extensibility.BlogClient.BlogPost</c>
    /// model — restricted to the fields needed for the minimal MetaWeblog publish
    /// path. Title/contents are scrubbed of invalid XML characters exactly like the
    /// Windows model, and <see cref="Contents"/> splits at the extended-entry break.
    /// </summary>
    public class BlogPost
    {
        /// <summary>The "more" text comment that separates main from extended contents.</summary>
        public const string ExtendedEntryBreak = "<!--more-->";

        public string Id { get; set; } = string.Empty;

        public bool IsNew => Id == string.Empty;

        public bool IsPage { get; set; }

        private string _title = string.Empty;
        public string Title
        {
            get => XmlCharacterHelper.RemoveInvalidXmlChars(_title);
            set => _title = XmlCharacterHelper.RemoveInvalidXmlChars(value);
        }

        private string _mainContents = string.Empty;
        private string _extendedContents = string.Empty;

        /// <summary>Body before the extended-entry break (MetaWeblog description).</summary>
        public string MainContents => _mainContents;

        /// <summary>Body after the extended-entry break (MetaWeblog mt_text_more).</summary>
        public string ExtendedContents => _extendedContents;

        /// <summary>
        /// The full contents (main + extended, joined by the extended-entry break),
        /// scrubbed of invalid XML characters. Setting this property splits the
        /// value at the break into main/extended.
        /// </summary>
        public string Contents
        {
            get
            {
                string contents = _extendedContents.Length > 0
                    ? string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", MainContents, ExtendedEntryBreak, ExtendedContents)
                    : MainContents;

                if (contents != null)
                    contents = XmlCharacterHelper.RemoveInvalidXmlChars(contents);

                return contents;
            }
            set => SetContents(XmlCharacterHelper.RemoveInvalidXmlChars(value));
        }

        public IList<string> Categories { get; } = new List<string>();

        private string _keywords = string.Empty;

        /// <summary>
        /// Comma-separated post keywords/tags (sent as MetaWeblog <c>mt_keywords</c>).
        /// Scrubbed of invalid XML characters like the title/contents.
        /// </summary>
        public string Keywords
        {
            get => _keywords;
            set => _keywords = XmlCharacterHelper.RemoveInvalidXmlChars(value) ?? string.Empty;
        }

        public bool IsPublished { get; set; } = true;

        /// <summary>Sets the main and extended contents of the post directly.</summary>
        public void SetContents(string mainContents, string extendedContents)
        {
            _mainContents = mainContents ?? string.Empty;
            _extendedContents = extendedContents ?? string.Empty;
        }

        /// <summary>
        /// Sets the contents of the post. If the content contains the extended-entry
        /// break it is automatically split into the main and extended contents.
        /// </summary>
        private void SetContents(string contents)
        {
            var (main, extended) = ExtendedEntry.Split(contents ?? string.Empty);
            _mainContents = main;
            _extendedContents = extended;
        }
    }
}
