// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Markdown;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Selects the post body string to transmit based on the document's canonical
    /// format and the blog account's publish format.
    /// </summary>
    public static class PublishBodyResolver
    {
        /// <summary>
        /// Returns the string to feed into <see cref="EditorContentPublisher.BuildPost"/>
        /// / transport as <see cref="BlogPost.Contents"/>.
        /// </summary>
        public static string Resolve(string canonicalBody, ContentFormat bodyFormat, ContentFormat publishFormat,
            IMarkdownService markdown)
        {
            canonicalBody ??= string.Empty;

            if (bodyFormat == ContentFormat.Html && publishFormat == ContentFormat.Html)
                return canonicalBody;

            if (bodyFormat == ContentFormat.Markdown && publishFormat == ContentFormat.Html)
            {
                if (markdown == null)
                    throw new ArgumentNullException(nameof(markdown));
                return markdown.ToHtml(canonicalBody);
            }

            if (bodyFormat == ContentFormat.Markdown && publishFormat == ContentFormat.Markdown)
                return canonicalBody;

            if (bodyFormat == ContentFormat.Html && publishFormat == ContentFormat.Markdown)
            {
                if (markdown == null)
                    throw new ArgumentNullException(nameof(markdown));
                return markdown.ToMarkdown(canonicalBody);
            }

            return canonicalBody;
        }
    }
}
