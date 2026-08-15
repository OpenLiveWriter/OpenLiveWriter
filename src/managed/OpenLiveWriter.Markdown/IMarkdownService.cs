// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Markdown
{
    /// <summary>
    /// Converts between GitHub Flavored Markdown and HTML for the editor publish pipeline.
    /// </summary>
    public interface IMarkdownService
    {
        /// <summary>
        /// Renders GFM Markdown to HTML using Markdig.
        /// </summary>
        string ToHtml(string markdown);

        /// <summary>
        /// Converts HTML to GFM Markdown, preserving unknown elements as raw HTML blocks.
        /// </summary>
        string ToMarkdown(string html);
    }
}
