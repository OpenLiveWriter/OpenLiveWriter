// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Composes the read-only "Preview" document shown in the editor's Preview view.
    /// The preview renders the current post body as it would look published, wrapping
    /// it in a neutral, centered "article" layout (a stand-in for a blog theme) so the
    /// author sees a realistic reading view rather than the raw editing surface.
    ///
    /// The HTML composition is deliberately separated from the live WebView display so
    /// it can be asserted headlessly (the actual on-screen render stays behind a live
    /// WebView backend). Pure and deterministic.
    /// </summary>
    public static class PreviewRenderer
    {
        /// <summary>
        /// A neutral article stylesheet applied to the preview so the post reads like
        /// published content: a comfortable measure, readable type, and standard
        /// element styling. Intentionally theme-agnostic.
        /// </summary>
        public const string PreviewStyle =
            "*{margin:0;padding:0;box-sizing:border-box;}" +
            "body{font-family:Georgia,'Times New Roman',serif;font-size:18px;line-height:1.7;" +
            "color:#222;background:#fff;padding:0;}" +
            "article{max-width:680px;margin:0 auto;padding:40px 24px;}" +
            "article h1{font-size:2.2em;font-weight:700;margin:0.6em 0 0.3em;line-height:1.2;}" +
            "article h2{font-size:1.7em;font-weight:700;margin:0.6em 0 0.3em;}" +
            "article h3{font-size:1.35em;font-weight:700;margin:0.6em 0 0.3em;}" +
            "article h4,article h5,article h6{font-weight:700;margin:0.6em 0 0.3em;}" +
            "article p{margin:0 0 1em;}" +
            "article a{color:#0066cc;}" +
            "article img{max-width:100%;height:auto;}" +
            "article blockquote{border-left:4px solid #ddd;padding-left:16px;margin:1em 0;color:#555;font-style:italic;}" +
            "article pre{background:#f5f5f5;padding:12px 16px;border-radius:4px;overflow-x:auto;" +
            "font-family:'SF Mono',Monaco,Consolas,monospace;font-size:0.85em;}" +
            "article code{background:#f0f0f0;padding:2px 6px;border-radius:3px;" +
            "font-family:'SF Mono',Monaco,Consolas,monospace;font-size:0.85em;}" +
            "article ul,article ol{padding-left:1.4em;margin:0 0 1em;}" +
            "article table{border-collapse:collapse;width:100%;margin:1em 0;}" +
            "article td,article th{border:1px solid #ddd;padding:8px 12px;}" +
            "article th{background:#f5f5f5;}" +
            "article hr{border:none;border-top:1px solid #ddd;margin:1.5em 0;}" +
            "article iframe{max-width:100%;}" +
            ".olw-preview-embed{position:relative;margin:1em 0;}";

        /// <summary>
        /// Builds the full standalone preview HTML document for the given editor body.
        /// The body is inserted verbatim (it is already HTML from the editor). The
        /// extended-entry break marker (<c>&lt;!--more--&gt;</c>) is stripped so the
        /// preview shows the whole post as a continuous read (parity with the Windows
        /// preview, which renders the joined content).
        /// </summary>
        /// <param name="bodyHtml">The editor body HTML (may be null/empty).</param>
        /// <param name="title">Optional post title rendered as a leading heading.</param>
        /// <param name="additionalStyle">Optional extra stylesheet appended after
        /// <see cref="PreviewStyle"/> (used by the print composition for @media rules).</param>
        public static string BuildPreviewDocument(string bodyHtml, string title = null, string additionalStyle = null)
        {
            string body = StripMoreMarker(bodyHtml ?? string.Empty);

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>\n<html>\n<head>\n");
            sb.Append("<meta charset=\"utf-8\">\n");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
            sb.Append("<title>").Append(EscapeTitle(title)).Append("</title>\n");
            sb.Append("<style>").Append(PreviewStyle).Append("</style>\n");
            if (!string.IsNullOrEmpty(additionalStyle))
                sb.Append("<style>").Append(additionalStyle).Append("</style>\n");
            sb.Append("</head>\n<body>\n<article>");

            if (!string.IsNullOrWhiteSpace(title))
                sb.Append("<h1 class=\"olw-preview-title\">").Append(EscapeTitle(title)).Append("</h1>");

            sb.Append(body);
            sb.Append("</article>\n</body>\n</html>");
            return sb.ToString();
        }

        /// <summary>
        /// Removes the extended-entry break marker so the preview renders the joined
        /// (main + extended) content as one continuous article.
        /// </summary>
        internal static string StripMoreMarker(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
            return html.Replace("<!--more-->", string.Empty);
        }

        private static string EscapeTitle(string s) =>
            string.IsNullOrEmpty(s)
                ? "Preview"
                : s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
