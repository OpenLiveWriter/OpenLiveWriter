// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Composes the print document for the current post: the same neutral article
    /// composition as <see cref="PreviewRenderer"/> (title heading + body) plus a
    /// print stylesheet with page margins and page-break rules. Pure/deterministic so
    /// the composition is headless-testable; how the document is then printed (native
    /// print panel, PDF handoff, or browser handoff) is <see cref="PrintCoordinator"/>'s job.
    /// </summary>
    public static class PrintRenderer
    {
        /// <summary>
        /// Print-only tweaks layered over the preview stylesheet: real page margins,
        /// avoid splitting images/tables/quotes across pages, and keep headings with
        /// the paragraph that follows them.
        /// </summary>
        public const string PrintStyle =
            "@media print{" +
            "@page{margin:1.8cm;}" +
            "article{max-width:none;padding:0;}" +
            "article img,article table,article blockquote,article pre{page-break-inside:avoid;}" +
            "article h1,article h2,article h3,article h4{page-break-after:avoid;}" +
            "}";

        /// <summary>
        /// Builds the standalone print HTML document for the given editor body. The
        /// extended-entry break marker is stripped (inherited from the preview
        /// composition) so the whole post prints as one continuous article.
        /// </summary>
        /// <param name="bodyHtml">The editor body HTML (may be null/empty).</param>
        /// <param name="title">Optional post title rendered as a leading heading.</param>
        public static string BuildPrintDocument(string bodyHtml, string title = null) =>
            PreviewRenderer.BuildPreviewDocument(bodyHtml, title, PrintStyle);
    }
}
