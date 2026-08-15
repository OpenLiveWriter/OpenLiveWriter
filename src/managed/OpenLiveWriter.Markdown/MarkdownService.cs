// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Markdig;

namespace OpenLiveWriter.Markdown
{
    /// <summary>
    /// GFM Markdown ↔ HTML conversion for Source/Design view boundaries and publish.
    /// </summary>
    public sealed class MarkdownService : IMarkdownService
    {
        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        private readonly HtmlToMarkdownConverter _htmlConverter = new HtmlToMarkdownConverter();

        /// <inheritdoc />
        public string ToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            return global::Markdig.Markdown.ToHtml(markdown, Pipeline);
        }

        /// <inheritdoc />
        public string ToMarkdown(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            return _htmlConverter.Convert(html);
        }
    }
}
