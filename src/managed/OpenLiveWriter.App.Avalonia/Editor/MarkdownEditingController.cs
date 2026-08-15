// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Markdown;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Pure conversion helpers for Markdown editing mode: maps between the canonical
    /// Markdown body and the HTML the Design WebView edits. Kept UI-free so view
    /// sync rules are unit-testable without Avalonia or a WebView backend.
    /// </summary>
    public sealed class MarkdownEditingController
    {
        /// <summary>
        /// Tooltip shown on disabled font family/size ribbon controls in Markdown mode.
        /// </summary>
        public const string FontFamilySizeDisabledTooltip =
            "Font family and size are not available in Markdown mode because Markdown does not encode visual fonts.";

        private readonly IMarkdownService _markdown;

        public MarkdownEditingController(IMarkdownService markdown)
        {
            _markdown = markdown;
        }

        /// <summary>True when the editor is editing Markdown as the canonical body.</summary>
        public bool IsMarkdownMode { get; private set; }

        /// <summary>Sets Markdown mode from a <see cref="ContentFormat"/> value.</summary>
        public void SetContentFormat(ContentFormat format) =>
            IsMarkdownMode = format == ContentFormat.Markdown;

        /// <summary>Enables or disables Markdown mode directly.</summary>
        public void SetMarkdownMode(bool enabled) => IsMarkdownMode = enabled;

        /// <summary>
        /// Returns HTML suitable for the Design WebView. In Markdown mode the input is
        /// treated as Markdown; otherwise the input is returned unchanged (HTML passthrough).
        /// </summary>
        public string HtmlFromCanonical(string markdownOrHtml)
        {
            markdownOrHtml ??= string.Empty;
            return IsMarkdownMode ? _markdown.ToHtml(markdownOrHtml) : markdownOrHtml;
        }

        /// <summary>
        /// Returns the canonical body from Design HTML. In Markdown mode the result is
        /// Markdown; otherwise the HTML is returned unchanged.
        /// </summary>
        public string CanonicalFromHtml(string html)
        {
            html ??= string.Empty;
            return IsMarkdownMode ? _markdown.ToMarkdown(html) : html;
        }
    }
}
