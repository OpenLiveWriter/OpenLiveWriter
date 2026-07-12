// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Builds a responsive video embed block from a video URL or a pasted embed
    /// snippet. This is the modern web-embed replacement for the Windows
    /// "video from service / from file" paths, which relied on now-defunct Flash and
    /// video-service (Soapbox/YouTube-upload) APIs. Instead of uploading media we
    /// insert a standards-based responsive <c>&lt;iframe&gt;</c> (16:9) that plays
    /// the hosted video.
    ///
    /// URL normalization (YouTube watch/short/shorts → embed, Vimeo → player) and the
    /// embed-HTML composition are pure/deterministic so they are testable without a
    /// live WebView.
    /// </summary>
    public static class VideoEmbedBuilder
    {
        private static readonly Regex IframeSrc = new Regex(
            "<iframe[^>]*\\ssrc\\s*=\\s*[\"']([^\"']+)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YouTubeId = new Regex(
            "(?:youtube\\.com/(?:watch\\?[^#]*\\bv=|embed/|shorts/|v/)|youtu\\.be/)([A-Za-z0-9_-]{6,})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex VimeoId = new Regex(
            "(?:vimeo\\.com/(?:video/)?|player\\.vimeo\\.com/video/)(\\d{6,})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Builds a responsive embed block for the given input. Accepts a plain video
        /// URL (YouTube/Vimeo/generic http(s)) or a pasted <c>&lt;iframe&gt;</c> embed
        /// snippet (its <c>src</c> is extracted and re-wrapped so arbitrary/unsafe
        /// attributes are dropped). Returns null when nothing embeddable is found.
        /// </summary>
        public static string BuildEmbedHtml(string urlOrEmbed)
        {
            string src = ResolveEmbedSrc(urlOrEmbed);
            return src == null ? null : WrapResponsive(src);
        }

        /// <summary>
        /// Resolves the embed <c>src</c> URL from the input without building the
        /// wrapper: extracts + normalizes a pasted iframe's src, or normalizes a plain
        /// URL. Returns null when the input is not embeddable.
        /// </summary>
        internal static string ResolveEmbedSrc(string urlOrEmbed)
        {
            if (string.IsNullOrWhiteSpace(urlOrEmbed))
                return null;

            string input = urlOrEmbed.Trim();

            // Pasted embed snippet: pull out the iframe src and normalize it.
            Match iframe = IframeSrc.Match(input);
            if (iframe.Success)
            {
                string extracted = iframe.Groups[1].Value.Trim();
                if (extracted.StartsWith("//", StringComparison.Ordinal))
                    extracted = "https:" + extracted;
                return NormalizeToEmbedUrl(extracted) ?? (IsHttpUrl(extracted) ? extracted : null);
            }

            // Plain URL: normalize known providers, otherwise accept any http(s) URL.
            return NormalizeToEmbedUrl(input) ?? (IsHttpUrl(input) ? input : null);
        }

        /// <summary>
        /// Converts a known video-service watch/share URL to its embeddable player
        /// URL: YouTube (watch / youtu.be / shorts / embed) → <c>youtube.com/embed/ID</c>;
        /// Vimeo → <c>player.vimeo.com/video/ID</c>. Returns null for URLs that are not
        /// a recognized video service (callers may still embed a generic URL directly).
        /// </summary>
        internal static string NormalizeToEmbedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            Match yt = YouTubeId.Match(url);
            if (yt.Success)
                return "https://www.youtube.com/embed/" + yt.Groups[1].Value;

            Match vimeo = VimeoId.Match(url);
            if (vimeo.Success)
                return "https://player.vimeo.com/video/" + vimeo.Groups[1].Value;

            return null;
        }

        // Wraps an embed src in a responsive 16:9 container. Attributes use explicit
        // values (allowfullscreen="true") so the markup stays XML/publish well-formed.
        private static string WrapResponsive(string src)
        {
            string safeSrc = EscapeAttr(src);
            return
                "<div class=\"olw-video\" style=\"position:relative;padding-bottom:56.25%;height:0;overflow:hidden;max-width:100%;\">" +
                "<iframe src=\"" + safeSrc + "\" " +
                "style=\"position:absolute;top:0;left:0;width:100%;height:100%;border:0;\" " +
                "frameborder=\"0\" allowfullscreen=\"true\" " +
                "allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture\">" +
                "</iframe></div>";
        }

        private static bool IsHttpUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri u) &&
            (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);

        private static string EscapeAttr(string s) =>
            s?.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
    }
}
